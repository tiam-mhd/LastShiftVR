using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastShift
{
    /// <summary>
    /// Volumetric, skinned reconstruction from the supplied single-view portrait.
    /// The front UV projection preserves the exact artwork's visible details.
    /// Depth, sides and the back are inferred because the reference has one view.
    /// This is a genuine 3D skinned mesh (NOT a single camera-facing quad).
    /// </summary>
    public sealed class GhostGirlVisual : MonoBehaviour
    {
        [Range(1.6f, 2.1f)] public float height = 1.83f;
        [Range(0f, 1f)] public float opacity = 1f;
        public float hoverAmplitude = .018f;
        public bool facePlayer = true;
        public Material preparedMaterial;
        public Material hairMaterial; // dark rear volume; original front texture remains untouched

        [NonSerialized] public Transform hipsBone;
        [NonSerialized] public Transform chestBone;
        [NonSerialized] public Transform headBone;
        [NonSerialized] public Transform leftArmBone;
        [NonSerialized] public Transform rightArmBone;
        [NonSerialized] public Transform leftLegBone;
        [NonSerialized] public Transform rightLegBone;

        private Transform model;
        private SkinnedMeshRenderer skin;
        private Material instanceMaterial;
        private Mesh generatedMesh;
        private Transform viewer;
        private bool created;
        private MeshRenderer[] rearHairRenderers;
        private float timeOffset;

        public Mesh GeneratedMesh => generatedMesh;
        public SkinnedMeshRenderer ModelRenderer => skin;

        private const int Columns = 58;
        private const int Rows = 104;

        private void Awake() { EnsureVisual(); }

        public void EnsureVisual()
        {
            if (created) return;
            if (TryUseSavedRig()) return;
            created = true;
            timeOffset = UnityEngine.Random.Range(0f, 5f);

            Texture2D source = Resources.Load<Texture2D>("GhostGirlReference");
            if (source == null)
            {
                Debug.LogError("[LastShift] GhostGirlReference.png missing. The 3D rig cannot be textured.");
                CreateFallback();
                return;
            }

            model = new GameObject("THE PATIENT | 3D skinned model").transform;
            model.SetParent(transform, false);
            model.localPosition = Vector3.zero;

            hipsBone = Bone("01 / pelvis", model, new Vector3(0f, .47f, 0f));
            chestBone = Bone("02 / spine and chest", hipsBone, new Vector3(0f, .67f, 0f));
            headBone = Bone("03 / head and hair", chestBone, new Vector3(0f, .37f, 0f));
            leftArmBone = Bone("04 / left arm", chestBone, new Vector3(-.24f, .18f, 0f));
            rightArmBone = Bone("05 / right arm", chestBone, new Vector3(.24f, .18f, 0f));
            leftLegBone = Bone("06 / left leg", hipsBone, new Vector3(-.15f, -.12f, 0f));
            rightLegBone = Bone("07 / right leg", hipsBone, new Vector3(.15f, -.12f, 0f));

            var bones = new[] {
                hipsBone, chestBone, headBone, leftArmBone, rightArmBone,
                leftLegBone, rightLegBone
            };

            bool hasAlphaData = source.isReadable;
            Color32[] pixels = null;
            if (hasAlphaData) pixels = source.GetPixels32();
            else Debug.LogWarning("[LastShift] Ghost texture is not readable: enable Read/Write on GhostGirlReference.png and regenerate scene.");

            generatedMesh = CreateVolumeMesh(source.width, source.height, pixels, bones);
            skin = model.gameObject.AddComponent<SkinnedMeshRenderer>();
            skin.sharedMesh = generatedMesh;
            skin.bones = bones;
            skin.rootBone = hipsBone;
            skin.updateWhenOffscreen = true;
            skin.localBounds = new Bounds(new Vector3(0, height * .5f, 0), new Vector3(2.8f, 3.5f, 2.5f));
            skin.shadowCastingMode = ShadowCastingMode.Off;
            skin.receiveShadows = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            if (preparedMaterial != null) instanceMaterial = Application.isPlaying ? new Material(preparedMaterial) : preparedMaterial;
            else if (shader != null) instanceMaterial = new Material(shader);
            if (instanceMaterial == null)
            {
                Debug.LogError("[LastShift] Cannot create an unlit transparent material.");
                CreateFallback();
                return;
            }

            instanceMaterial.name = "The Patient | original projected texture (runtime)";
            if (instanceMaterial.HasProperty("_BaseMap")) instanceMaterial.SetTexture("_BaseMap", source);
            if (instanceMaterial.HasProperty("_MainTex")) instanceMaterial.SetTexture("_MainTex", source);
            if (instanceMaterial.HasProperty("_Surface"))
            {
                instanceMaterial.SetFloat("_Surface", 1f);
                instanceMaterial.SetFloat("_Blend", 0f);
                instanceMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                instanceMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                instanceMaterial.SetFloat("_ZWrite", 0f);
                instanceMaterial.SetFloat("_Cull", 0f);
                instanceMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                instanceMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }
            instanceMaterial.renderQueue = (int)RenderQueue.Transparent;
            skin.sharedMaterial = instanceMaterial;
            BuildBackHair();
            SetOpacity(opacity);

            var driver = gameObject.GetComponent<PatientAnimationDriver>();
            if (driver == null) driver = gameObject.AddComponent<PatientAnimationDriver>();
            driver.visual = this;
            Debug.Log("[LastShift] TRUE 3D skinned Patient built: " + generatedMesh.vertexCount +
                " vertices, 7 bones, original reference texture " + source.width + "x" + source.height + ".");
        }

        /// <summary>Short layered rear hair locks give real profile depth without repainting the reference.</summary>
        private void BuildBackHair()
        {
            if (headBone == null || headBone.Find("3D rear hair volume") != null) return;
            var root = new GameObject("3D rear hair volume").transform;
            root.SetParent(headBone, false);
            Material mat = hairMaterial != null ? hairMaterial : preparedMaterial;
            var segments = new List<MeshRenderer>();
            for (int i = 0; i < 13; i++)
            {
                float a = (i / 12f - .5f);
                var strand = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                strand.name = "Volume hair lock " + (i + 1);
                strand.transform.SetParent(root, false);
                strand.transform.localPosition = new Vector3(a * .43f,
                    -.12f - Mathf.Abs(a) * .055f,
                    -.19f - .065f * Mathf.Cos(a * Mathf.PI));
                strand.transform.localScale = new Vector3(.035f + .011f * Mathf.Abs(a),
                    .25f + (i % 4) * .042f, .043f);
                strand.transform.localRotation = Quaternion.Euler(
                    Mathf.Sin(i * 2.3f) * 8f, 0f, -a * 13f);
                var col = strand.GetComponent<Collider>();
                if (Application.isPlaying) Destroy(col);
                else DestroyImmediate(col);
                var rend = strand.GetComponent<MeshRenderer>();
                if (mat != null) rend.sharedMaterial = mat;
                segments.Add(rend);
            }
            rearHairRenderers = segments.ToArray();
        }

        private bool TryUseSavedRig()
        {
            var existing = transform.Find("THE PATIENT | 3D skinned model");
            if (existing == null) return false;
            var found = existing.GetComponent<SkinnedMeshRenderer>();
            if (found == null || found.sharedMesh == null) return false;
            model = existing;
            skin = found;
            Transform[] savedBones = skin.bones;
            foreach (var bone in savedBones)
            {
                if (bone == null) continue;
                if (bone.name.StartsWith("01 /")) hipsBone = bone;
                else if (bone.name.StartsWith("02 /")) chestBone = bone;
                else if (bone.name.StartsWith("03 /")) headBone = bone;
                else if (bone.name.StartsWith("04 /")) leftArmBone = bone;
                else if (bone.name.StartsWith("05 /")) rightArmBone = bone;
                else if (bone.name.StartsWith("06 /")) leftLegBone = bone;
                else if (bone.name.StartsWith("07 /")) rightLegBone = bone;
            }
            created = true;
            timeOffset = UnityEngine.Random.Range(0f, 5f);
            if (Application.isPlaying && skin.sharedMaterial != null)
            {
                instanceMaterial = new Material(skin.sharedMaterial);
                skin.sharedMaterial = instanceMaterial;
            }
            else instanceMaterial = skin.sharedMaterial;
            SetOpacity(opacity);
            rearHairRenderers = headBone != null ? headBone.GetComponentsInChildren<MeshRenderer>(true) : null;
            var driver = gameObject.GetComponent<PatientAnimationDriver>();
            if (driver == null) driver = gameObject.AddComponent<PatientAnimationDriver>();
            driver.visual = this;
            Debug.Log("[LastShift] 3D Patient rig loaded from generated Unity mesh asset.");
            return true;
        }

        private Transform Bone(string label, Transform parent, Vector3 local)
        {
            Transform t = new GameObject(label).transform;
            t.SetParent(parent, false);
            t.localPosition = local;
            return t;
        }

        private static float SurfaceDepth(float u, float v)
        {
            float width = Mathf.Clamp01(1f - Mathf.Abs(u - .5f) * 1.65f);
            // Hair/head, shoulders, torso and the draping skirt have different depths.
            float torso = Mathf.SmoothStep(.09f, .18f, width);
            if (v > .73f) torso *= 1.06f; // head / bangs
            else if (v < .24f) torso *= .72f; // exposed feet
            else if (v < .62f) torso *= 1.28f; // loose dress
            return torso;
        }

        private Mesh CreateVolumeMesh(int tw, int th, Color32[] rgba, Transform[] bones)
        {
            int sx = Columns + 1, sy = Rows + 1;
            var verts = new List<Vector3>(sx * sy * 2);
            var uv = new List<Vector2>(sx * sy * 2);
            var weights = new List<BoneWeight>(sx * sy * 2);
            var tris = new List<int>(sx * sy * 12);
            bool[,] solid = new bool[Columns, Rows];

            float width = height * (float)tw / th;
            for (int y = 0; y < Rows; y++)
            for (int x = 0; x < Columns; x++)
            {
                float u = (x + .5f) / Columns;
                float v = (y + .5f) / Rows;
                solid[x, y] = AlphaAt(rgba, tw, th, u, v) > 19;
            }

            for (int y = 0; y <= Rows; y++)
            for (int x = 0; x <= Columns; x++)
            {
                float u = (float)x / Columns;
                float v = (float)y / Rows;
                float px = (u - .5f) * width;
                float py = v * height;
                float depth = SurfaceDepth(u, v);
                var w = Weights(u, v, rgba, tw, th);

                verts.Add(new Vector3(px, py, depth));
                verts.Add(new Vector3(px, py, -depth));
                uv.Add(new Vector2(u, v));
                uv.Add(new Vector2(u, v));
                weights.Add(w);
                weights.Add(w);
            }

            for (int y = 0; y < Rows; y++)
            for (int x = 0; x < Columns; x++)
            {
                if (!solid[x, y]) continue;
                int a = (y * sx + x) * 2;
                int b = a + 2;
                int d = a + sx * 2;
                int c = d + 2;
                // Front and back faces.
                tris.Add(a); tris.Add(b); tris.Add(c);
                tris.Add(a); tris.Add(c); tris.Add(d);
                tris.Add(a + 1); tris.Add(c + 1); tris.Add(b + 1);
                tris.Add(a + 1); tris.Add(d + 1); tris.Add(c + 1);

                // Close the thickness at the silhouette boundary.
                if (x == 0 || !solid[x - 1, y]) Side(tris, a, d);
                if (x == Columns - 1 || !solid[x + 1, y]) Side(tris, c, b);
                if (y == 0 || !solid[x, y - 1]) Side(tris, b, a);
                if (y == Rows - 1 || !solid[x, y + 1]) Side(tris, d, c);
            }

            var result = new Mesh { name = "The Patient - volumetric rigged portrait" };
            result.indexFormat = IndexFormat.UInt32;
            result.SetVertices(verts);
            result.SetUVs(0, uv);
            result.SetTriangles(tris, 0, true);
            result.boneWeights = weights.ToArray();
            var poses = new Matrix4x4[bones.Length];
            for (int i = 0; i < bones.Length; i++)
                poses[i] = bones[i].worldToLocalMatrix * model.localToWorldMatrix;
            result.bindposes = poses;
            result.RecalculateNormals();
            result.RecalculateBounds();
            return result;
        }

        private static void Side(List<int> triangles, int firstFront, int lastFront)
        {
            triangles.Add(firstFront);
            triangles.Add(lastFront);
            triangles.Add(lastFront + 1);
            triangles.Add(firstFront);
            triangles.Add(lastFront + 1);
            triangles.Add(firstFront + 1);
        }

        private static byte AlphaAt(Color32[] pixels, int tw, int th, float u, float v)
        {
            if (pixels == null) return 255;
            int x = Mathf.Clamp(Mathf.RoundToInt(u * (tw - 1)), 0, tw - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(v * (th - 1)), 0, th - 1);
            return pixels[y * tw + x].a;
        }

        private static BoneWeight Weights(float u, float v, Color32[] pixels, int tw, int th)
        {
            float x = u - .5f;
            // 0 pelvis, 1 chest, 2 head/hair, 3 left arm, 4 right arm, 5 left leg, 6 right leg.
            int dominant = v < .17f ? (x < 0f ? 5 : 6) : v < .55f ? 0 : 1;
            float blend = .05f;
            int secondary = 0;

            if (v > .73f && Mathf.Abs(x) < .30f)
            {
                dominant = 2;
                secondary = 1;
                blend = .08f;
            }
            else if (v > .47f && v < .80f && Mathf.Abs(x) > .25f)
            {
                dominant = x < 0 ? 3 : 4;
                secondary = 1;
                blend = .12f;
            }
            else if (v > .47f && v < .76f && Mathf.Abs(x) < .23f && pixels != null)
            {
                int ix = Mathf.Clamp(Mathf.RoundToInt(u * (tw - 1)), 0, tw - 1);
                int iy = Mathf.Clamp(Mathf.RoundToInt(v * (th - 1)), 0, th - 1);
                Color32 px = pixels[iy * tw + ix];
                // Loose dark hair strands swing with the skull, not the dress.
                if (px.r < 80 && px.g < 90 && px.b < 102)
                {
                    dominant = 2;
                    secondary = 1;
                    blend = .12f;
                }
            }
            if (dominant == secondary) blend = 0f;
            return new BoneWeight {
                boneIndex0 = dominant, weight0 = 1f - blend,
                boneIndex1 = secondary, weight1 = blend
            };
        }

        public void SetOpacity(float alpha)
        {
            opacity = Mathf.Clamp01(alpha);
            if (instanceMaterial == null) return;
            Color color = new Color(1, 1, 1, opacity);
            if (instanceMaterial.HasProperty("_BaseColor"))
                instanceMaterial.SetColor("_BaseColor", color);
            if (instanceMaterial.HasProperty("_Color"))
                instanceMaterial.SetColor("_Color", color);
            if (rearHairRenderers != null)
                foreach (var rend in rearHairRenderers)
                    if (rend != null) rend.enabled = opacity > .04f;
        }

        private void LateUpdate()
        {
            if (model == null) return;
            if (viewer == null && Camera.main != null) viewer = Camera.main.transform;
            if (facePlayer && viewer != null)
            {
                var dir = viewer.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > .03f)
                    model.rotation = Quaternion.Slerp(model.rotation,
                        Quaternion.LookRotation(dir.normalized, Vector3.up),
                        Time.deltaTime * 3.0f);
            }
            model.localPosition = new Vector3(0f,
                Mathf.Sin(Time.time * .9f + timeOffset) * hoverAmplitude, 0f);
        }

        private void CreateFallback()
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "The Patient: 3D fallback (reference missing)";
            capsule.transform.SetParent(transform, false);
            capsule.transform.localPosition = Vector3.up;
            capsule.transform.localScale = new Vector3(.6f, .9f, .4f);
            var collider = capsule.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }

        private void OnDestroy()
        {
            if (Application.isPlaying && instanceMaterial != null && instanceMaterial != preparedMaterial)
                Destroy(instanceMaterial);
            // The editor-baked mesh is a persistent Unity asset; never destroy it here.
            if (Application.isPlaying && generatedMesh != null) Destroy(generatedMesh);
        }
    }
}
