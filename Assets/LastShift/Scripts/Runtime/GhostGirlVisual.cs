using UnityEngine;

namespace LastShift
{
    /// <summary>
    /// Uses the exact user-provided transparent character artwork as a camera-facing
    /// 2D impostor in the 3D world. This is NOT a rigged 3D model.
    /// A true 3D rig can later replace this component without changing enemy AI.
    /// </summary>
    public sealed class GhostGirlVisual : MonoBehaviour
    {
        public float height = 1.83f;
        public float hoverAmplitude = .036f;
        public float swayAmplitude = 4.0f;
        [Range(0f, 1f)] public float opacity = 1f;
        public bool facePlayer = true;

        private Transform billboard;
        private Material runtimeMaterial;
        private Transform viewer;
        private bool created;

        private void Awake() { EnsureVisual(); }

        public void EnsureVisual()
        {
            if (created) return;
            created = true;
            var texture = Resources.Load<Texture2D>("GhostGirlReference");
            if (texture == null)
            {
                Debug.LogWarning("[LastShift] GhostGirlReference missing from Assets/LastShift/Resources.");
                MakeFallback();
                return;
            }

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "THE PATIENT - supplied artwork (2D impostor)";
            quad.transform.SetParent(transform, false);
            quad.transform.localPosition = new Vector3(0f, height * .5f, 0f);
            quad.transform.localScale = new Vector3(height * texture.width / texture.height,
                height, 1f);
            var primitiveCollider = quad.GetComponent<Collider>();
            if (primitiveCollider != null) Destroy(primitiveCollider);
            billboard = quad.transform;

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            runtimeMaterial = new Material(shader) { name = "GhostGirl Runtime Transparent" };
            if (runtimeMaterial.HasProperty("_BaseMap"))
                runtimeMaterial.SetTexture("_BaseMap", texture);
            if (runtimeMaterial.HasProperty("_MainTex"))
                runtimeMaterial.SetTexture("_MainTex", texture);
            if (runtimeMaterial.HasProperty("_Surface"))
            {
                runtimeMaterial.SetFloat("_Surface", 1f);
                runtimeMaterial.SetFloat("_Blend", 0f);
                runtimeMaterial.SetFloat("_SrcBlend", 5f);  // SrcAlpha
                runtimeMaterial.SetFloat("_DstBlend", 10f); // OneMinusSrcAlpha
                runtimeMaterial.SetFloat("_ZWrite", 0f);
                runtimeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            runtimeMaterial.renderQueue = 3000;
            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = runtimeMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            SetOpacity(opacity);
        }

        public void SetOpacity(float alpha)
        {
            opacity = Mathf.Clamp01(alpha);
            if (runtimeMaterial == null) return;
            var color = new Color(.77f, .84f, .96f, opacity);
            if (runtimeMaterial.HasProperty("_BaseColor"))
                runtimeMaterial.SetColor("_BaseColor", color);
            if (runtimeMaterial.HasProperty("_Color"))
                runtimeMaterial.SetColor("_Color", color);
        }

        private void Update()
        {
            if (!created) EnsureVisual();
            if (billboard == null) return;
            if (viewer == null && Camera.main != null) viewer = Camera.main.transform;
            if (facePlayer && viewer != null)
            {
                var facing = viewer.position - transform.position;
                facing.y = 0f;
                if (facing.sqrMagnitude > .01f)
                    billboard.rotation = Quaternion.LookRotation(facing, Vector3.up);
            }

            // Subtle motion without artificial camera movement (VR comfort).
            billboard.position = transform.position + Vector3.up * (
                height * .5f + Mathf.Sin(Time.time * 1.12f) * hoverAmplitude);
            billboard.Rotate(0f, 0f, Mathf.Sin(Time.time * .83f) * swayAmplitude * Time.deltaTime,
                Space.Self);
        }

        private void MakeFallback()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Missing Ghost Texture - placeholder";
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.up;
            go.transform.localScale = new Vector3(.55f, .9f, .55f);
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
        }
    }
}