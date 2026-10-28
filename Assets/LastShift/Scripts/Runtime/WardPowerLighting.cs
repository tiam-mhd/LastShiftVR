using UnityEngine;

namespace LastShift
{
    /// <summary>
    /// Couples fuse pickup / insertion / breaker events to visible lighting.
    /// The three circuits activate one by one; the main breaker restores the ward.
    /// All values are animated at runtime (not baked-only effects).
    /// </summary>
    public sealed class WardPowerLighting : MonoBehaviour
    {
        public Light[] corridorLights;
        public Light[] emergencyLights;
        public Light[] circuitLights;  // index 0=entrance, 1=treatment, 2=surgery
        public Renderer[] circuitIndicators;
        public Color restoredColor = new Color(.8f, .93f, 1f);
        public Color unpoweredAmbient = new Color(.045f, .057f, .063f);
        public Color poweredAmbient = new Color(.21f, .245f, .26f);
        public float restoredCorridorIntensity = 1.9f;
        public float circuitOnIntensity = 3.5f;
        public float emergencyIntensity = .85f;
        public float transitionSpeed = 2.6f;

        private int installed;
        private bool powered;
        private float pulseTime;
        private Color[] originalColors;
        private Color initialFog;
        private bool initialized;
        private MaterialPropertyBlock block;
        private int appliedIndicatorState = -1;

        private void Start()
        {
            block = new MaterialPropertyBlock();
            originalColors = new Color[corridorLights == null ? 0 : corridorLights.Length];
            for (int i = 0; i < originalColors.Length; i++)
            {
                if (corridorLights[i] == null) continue;
                originalColors[i] = corridorLights[i].color;
                var flicker = corridorLights[i].GetComponent<HospitalFlicker>();
                if (flicker != null) flicker.enabled = false; // no competing scripts on intensity
            }
            if (emergencyLights != null)
                foreach (var light in emergencyLights)
                {
                    if (light == null) continue;
                    var flicker = light.GetComponent<HospitalFlicker>();
                    if (flicker != null) flicker.enabled = false;
                }
            initialFog = RenderSettings.fogColor;
            initialized = true;
            ApplyImmediate();
        }

        public void OnFuseCollected(int count)
        {
            // Distinct bright spark / partial brownout from taking a cartridge.
            pulseTime = 1.2f;
            Debug.Log("[LastShift Lighting] Fuse pickup: voltage flicker " + count + "/3");
        }

        public void OnFuseInserted(int count)
        {
            installed = Mathf.Clamp(count, 0, 3);
            pulseTime = 1.35f;
            Debug.Log("[LastShift Lighting] Circuit " + installed + "/3 energized.");
        }

        public void OnBreakerRestored()
        {
            installed = 3;
            powered = true;
            pulseTime = 2.1f;
            Debug.Log("[LastShift Lighting] Main breaker ON - ward power restored.");
        }

        private void ApplyImmediate()
        {
            if (corridorLights != null)
                foreach (var l in corridorLights) if (l != null) l.intensity = .12f;
            if (circuitLights != null)
                foreach (var l in circuitLights) if (l != null) l.intensity = 0f;
            if (emergencyLights != null)
                foreach (var l in emergencyLights) if (l != null) l.intensity = emergencyIntensity;
            RenderSettings.ambientLight = unpoweredAmbient;
        }

        private void Update()
        {
            if (!initialized) return;
            float dt = Time.deltaTime;
            pulseTime = Mathf.Max(0f, pulseTime - dt);
            // Avoid 30Hz full-ward light changes, shadow atlas churn and high-frequency
            // VR flicker. A slow 5Hz voltage pulse is sufficient for the scare.
            float flash = pulseTime > 0f ? (Mathf.Sin(Time.time * 10f) * .5f + .5f)
                    * Mathf.Clamp01(pulseTime * 1.35f) : 0f;
            float t = Mathf.Clamp01(dt * transitionSpeed);

            if (corridorLights != null)
                for (int i = 0; i < corridorLights.Length; i++)
                {
                    var l = corridorLights[i];
                    if (l == null) continue;
                    float destination = powered ? restoredCorridorIntensity :
                        (.11f + flash * .55f);
                    l.intensity = Mathf.Lerp(l.intensity, destination, Mathf.Clamp01(dt * 9f));
                    l.color = Color.Lerp(l.color,
                        powered ? restoredColor : originalColors[i], t);
                }

            if (emergencyLights != null)
                foreach (var l in emergencyLights)
                {
                    if (l == null) continue;
                    l.intensity = Mathf.Lerp(l.intensity,
                        powered ? 0f : emergencyIntensity + flash * .6f, t);
                }

            if (circuitLights != null)
                for (int i = 0; i < circuitLights.Length; i++)
                {
                    var l = circuitLights[i];
                    if (l == null) continue;
                    bool enabledCircuit = powered || i < installed;
                    float desired = enabledCircuit ? circuitOnIntensity : 0f;
                    if (enabledCircuit && pulseTime > 0f)
                        desired += flash * .45f;
                    l.intensity = Mathf.Lerp(l.intensity, desired, Mathf.Clamp01(dt * 4.8f));
                }

            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight,
                powered ? poweredAmbient : unpoweredAmbient + Color.white * (flash * .032f), t);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor,
                powered ? new Color(.085f, .11f, .13f) : initialFog, t);

            int indicatorState = powered ? 4 : installed;
            if (circuitIndicators != null && indicatorState != appliedIndicatorState)
            {
                // Unity may reload this scene/component without rerunning Start
                // (e.g. disabled domain/scene reload in Editor). Never pass null
                // into Renderer.GetPropertyBlock or spam exceptions every frame.
                if (block == null) block = new MaterialPropertyBlock();
                appliedIndicatorState = indicatorState;
                for (int i = 0; i < circuitIndicators.Length; i++)
                {
                    var r = circuitIndicators[i];
                    if (r == null) continue;
                    // Reuse the same block but reset unrelated properties
                    // between renderers to avoid leaking another circuit state.
                    block.Clear();
                    Color col = powered ? Color.green : i < installed
                        ? new Color(1f,.56f,.09f) : new Color(.26f,.04f,.035f);
                    r.GetPropertyBlock(block);
                    block.SetColor("_BaseColor", col);
                    block.SetColor("_Color", col);
                    if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_EmissionColor"))
                        block.SetColor("_EmissionColor", col * (powered || i < installed ? 1.8f : .15f));
                    r.SetPropertyBlock(block);
                }
            }
        }
    }
}
