using System.Collections;
using UnityEngine;

namespace LastShift
{
    /// <summary>Staged, non-camera-shaking VR horror encounters.</summary>
    public sealed class GhostDirector : MonoBehaviour
    {
        public LastShiftGame game;
        public LastShiftMonster monster;
        public GhostGirlVisual visual;
        public Light[] crisisLights;
        public float firstSightingSeconds = 6.5f;
        public float openingDelay = 2.5f;

        private Coroutine sighting;
        private AudioSource ghostAudio;
        private float nextBreath;
        private AudioClip whisper;
        private AudioClip stinger;
        private bool hunting;

        private void Awake()
        {
            whisper = Tone("The Patient - whisper", 114f, 1.2f, .12f);
            stinger = Tone("The Patient - sting", 203f, .65f, .30f);
        }

        private IEnumerator Start()
        {
            // A non-hostile early sighting confirms the actual character art is visible
            // before any fuses are collected. It fades out before the real chase.
            yield return new WaitForSeconds(openingDelay);
            if (game != null && !game.HasEnded && game.CollectedFuses == 0 && !hunting)
            {
                Debug.Log("[LastShift] Opening sighting: The Patient should appear in the corridor.");
                sighting = StartCoroutine(FirstSighting());
            }
        }

        public void OnFuseCountChanged(int count)
        {
            if (game == null || game.HasEnded || monster == null) return;
            if (count == 1)
            {
                if (sighting != null) StopCoroutine(sighting);
                sighting = StartCoroutine(FirstSighting());
            }
            else if (count == 2)
            {
                if (sighting != null) StopCoroutine(sighting);
                StartHunt();
            }
        }

        private IEnumerator FirstSighting()
        {
            // At ~6 meters from the entrance, visible in flashlight range.
            float z = -2.25f;
            if (Camera.main != null)
                z = Mathf.Clamp(Camera.main.transform.position.z + 6.5f, -2.25f, 11.5f);
            monster.transform.position = new Vector3(0f, 0f, z);
            monster.gameObject.SetActive(true);
            monster.enabled = false;
            if (visual != null) { visual.EnsureVisual(); visual.SetOpacity(0f); }
            PlayAtMonster(stinger);
            float t = 0f;

            while (t < firstSightingSeconds)
            {
                t += Time.deltaTime;
                float visibility = Mathf.Clamp01(t * 2f)
                    * Mathf.Clamp01((firstSightingSeconds - t) * 2.5f);
                if (visual != null) visual.SetOpacity(visibility);
                yield return null;
            }
            if (visual != null) visual.SetOpacity(1f);
            monster.gameObject.SetActive(false);
            monster.enabled = true;
            sighting = null;
        }

        private void StartHunt()
        {
            hunting = true;
            monster.enabled = true;
            if (visual != null) visual.SetOpacity(1f);
            monster.transform.position = new Vector3(0f, 0f, 14f);
            monster.Wake(game, game.flashlight);
            PlayAtMonster(stinger);
            nextBreath = Time.time + 2f;
        }

        private void Update()
        {
            if (!hunting || monster == null || game == null || game.HasEnded || Camera.main == null)
                return;
            float dist = Vector3.Distance(Camera.main.transform.position, monster.transform.position);
            if (dist < 7f && Time.time > nextBreath)
            {
                PlayAtMonster(whisper);
                nextBreath = Time.time + Random.Range(2f, 4.3f);
            }
            if (crisisLights != null)
                foreach (var light in crisisLights)
                    if (light != null && dist < 6f)
                        light.intensity = Mathf.Lerp(light.intensity,
                            .5f + Mathf.Sin(Time.time * 19f) * .25f, Time.deltaTime * 4f);
        }

        public void OnGameEnded()
        {
            hunting = false;
            if (sighting != null) StopCoroutine(sighting);
        }

        private void PlayAtMonster(AudioClip clip)
        {
            if (monster == null || clip == null) return;
            if (ghostAudio == null)
            {
                ghostAudio = monster.gameObject.AddComponent<AudioSource>();
                ghostAudio.spatialBlend = 1f;
                ghostAudio.rolloffMode = AudioRolloffMode.Logarithmic;
                ghostAudio.minDistance = .6f;
                ghostAudio.maxDistance = 12f;
                ghostAudio.playOnAwake = false;
            }
            ghostAudio.PlayOneShot(clip, .65f);
        }

        private static AudioClip Tone(string title, float frequency, float length, float amplitude)
        {
            const int rate = 16000;
            int n = Mathf.RoundToInt(rate * length);
            float[] samples = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float fade = Mathf.Min(1f, t * 12f, (length - t) * 5f);
                samples[i] = amplitude * fade * (
                    Mathf.Sin(2f * Mathf.PI * frequency * t) * .63f +
                    Mathf.Sin(2f * Mathf.PI * (frequency * .51f) * t) * .35f);
            }
            var result = AudioClip.Create(title, n, 1, rate, false);
            result.SetData(samples, 0);
            return result;
        }
    }
}