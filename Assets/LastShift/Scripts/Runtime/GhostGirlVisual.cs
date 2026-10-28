using UnityEngine;
using UnityEngine.Rendering;

namespace LastShift
{
    /// <summary>
    /// A visible two-sided, camera-facing 2D stand-in made from the user's character image.
    /// It is not a rigged 3D character. The mesh has front and back triangles so
    /// backface culling cannot hide the patient after she turns around.
    /// </summary>
    public sealed class GhostGirlVisual : MonoBehaviour
    {
        public float height = 1.83f;
        public float hoverAmplitude = .027f;
        [Range(0f, 1f)] public float opacity = 1f;
        public bool facePlayer = true;
        // Set by Ward13PatientBuilder so URP shader is retained in Windows builds.
        public Material preparedMaterial;

        private Transform billboard;
        private Material runtimeMaterial;
        private Transform viewer;
        private Mesh mesh;
        private bool created;

        private void Awake() { EnsureVisual(); }

        public void EnsureVisual()
        {
            if (created) return;
            created = true;
            var texture = Resources.Load<Texture2D>("GhostGirlReference");
            if (texture == null)
            {
                Debug.LogError("[LastShift] GhostGirlReference texture missing. Check Assets/LastShift/Resources/GhostGirlReference.png.");
                MakeFallback();
                return;
            }

            var go = new GameObject("THE PATIENT - visible two-sided artwork");
            go.transform.SetParent(transform, false);
            billboard = go.transform;
            billboard.localScale = new Vector3(height * (float)texture.width / texture.height, height, 1f);
            mesh = DoubleSidedCard();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");

            if (preparedMaterial != null) runtimeMaterial = new Material(preparedMaterial);
            else if (shader != null) runtimeMaterial = new Material(shader);
            if (runtimeMaterial == null)
            {
                Debug.LogError("[LastShift] No compatible shader for GhostGirlVisual.");
                MakeFallback();
                return;
            }

            runtimeMaterial.name = "The Patient | Runtime Transparent";
            if (runtimeMaterial.HasProperty("_BaseMap")) runtimeMaterial.SetTexture("_BaseMap", texture);
            if (runtimeMaterial.HasProperty("_MainTex")) runtimeMaterial.SetTexture("_MainTex", texture);
            if (runtimeMaterial.HasProperty("_Surface"))
            {
                runtimeMaterial.SetFloat("_Surface", 1f); // URP transparent
                runtimeMaterial.SetFloat("_Blend", 0f);
                runtimeMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                runtimeMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                runtimeMaterial.SetFloat("_ZWrite", 0f);
                runtimeMaterial.SetFloat("_Cull", 0f);
                runtimeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                runtimeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }
            runtimeMaterial.renderQueue = (int)RenderQueue.Transparent;
            renderer.sharedMaterial = runtimeMaterial;
            SetOpacity(opacity);
            Debug.Log("[LastShift] The Patient billboard ready: " + texture.width + "x" + texture.height +
                      ", shader " + runtimeMaterial.shader.name);
        }

        private static Mesh DoubleSidedCard()
        {
            var result = new Mesh { name = "Patient two-sided visible card" };
            result.vertices = new[] {
                new Vector3(-.5f,0,0), new Vector3(.5f,0,0),
                new Vector3(.5f,1,0), new Vector3(-.5f,1,0),
                new Vector3(-.5f,0,0), new Vector3(.5f,0,0),
                new Vector3(.5f,1,0), new Vector3(-.5f,1,0)
            };
            result.uv = new[] {
                new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1),
                new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)
            };
            result.triangles = new[] {0,1,2,0,2,3, 4,6,5,4,7,6};
            result.RecalculateNormals();
            result.RecalculateBounds();
            return result;
        }

        public void SetOpacity(float alpha)
        {
            opacity = Mathf.Clamp01(alpha);
            if (runtimeMaterial == null) return;
            var color = new Color(1f, 1f, 1f, opacity);
            if (runtimeMaterial.HasProperty("_BaseColor")) runtimeMaterial.SetColor("_BaseColor", color);
            if (runtimeMaterial.HasProperty("_Color")) runtimeMaterial.SetColor("_Color", color);
        }

        private void LateUpdate()
        {
            if (!created) EnsureVisual();
            if (billboard == null) return;
            if (viewer == null && Camera.main != null) viewer = Camera.main.transform;
            if (facePlayer && viewer != null)
            {
                Vector3 toCamera = viewer.position - transform.position;
                toCamera.y = 0f;
                if (toCamera.sqrMagnitude > .01f)
                    billboard.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
            }
            billboard.position = transform.position + Vector3.up *
                (Mathf.Sin(Time.time * 1.12f) * hoverAmplitude);
        }

        private void MakeFallback()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Visible ghost placeholder - texture or shader missing";
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.up;
            go.transform.localScale = new Vector3(.55f, .9f, .55f);
            var rend = go.GetComponent<Renderer>();
            if (rend != null) rend.material.color = new Color(.95f,.85f,.88f);
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
            if (mesh != null) Destroy(mesh);
        }
    }
}
