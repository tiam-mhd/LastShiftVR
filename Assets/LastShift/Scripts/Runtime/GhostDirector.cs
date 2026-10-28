using System.Collections;
using UnityEngine;

namespace LastShift
{
    /// <summary>VR-safe scripted encounters for the imported REAL 3D patient.</summary>
    public sealed class GhostDirector : MonoBehaviour
    {
        public LastShiftGame game;
        public LastShiftMonster monster;
        public PatientCharacterRig rig;
        public Light[] crisisLights;
        public float firstSightingSeconds = 5.8f;
        public float openingDelay = 2.5f;
        public bool CharacterReady => monster != null && rig != null && rig.IsReady;

        private Coroutine sighting;
        private AudioSource ghostAudio;
        private float nextBreath;
        private AudioClip whisper;
        private AudioClip stinger;
        private bool hunting;
        private bool witnessed;

        private void Awake()
        {
            whisper = Tone("The Patient - whisper", 114f, 1.2f, .12f);
            stinger = Tone("The Patient - sting", 203f, .65f, .30f);
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(openingDelay);
            if (CharacterReady && game != null && !game.HasEnded &&
                game.CollectedFuses == 0 && !hunting)
                sighting = StartCoroutine(FirstSighting());
        }

        public void OnFuseCountChanged(int count)
        {
            if (game == null || game.HasEnded || monster == null) return;
            if (!CharacterReady)
            {
                Debug.LogWarning("[LastShift] Ghost encounter skipped: real FBX mesh not configured.");
                return;
            }

            if (count == 1)
            {
                // Don't restart the same sighting: doing so used to teleport the
                // ghost unpredictably while the player was watching.
                if (!witnessed && sighting == null)
                    sighting = StartCoroutine(FirstSighting());
            }
            else if (count == 2)
            {
                if (sighting != null) StopCoroutine(sighting);
                sighting = null;
                StartHunt();
            }
        }

        private IEnumerator FirstSighting()
        {
            if (!CharacterReady) yield break;
            var cam = Camera.main;
            if (cam == null) yield break;
            witnessed = true;
            monster.transform.position = WardSpatial.SafeCorridorSpawn(
                cam.transform.position, cam.transform.forward);
            monster.gameObject.SetActive(true);
            monster.enabled = false;
            rig.SetVisible(true);
            rig.PlayAction(PatientMotion.Stare, 1.8f);
            PlayAtMonster(stinger);

            // Do not vanish in direct view. Wait for a real player look-away.
            float elapsed = 0f;
            while (!hunting && game != null && !game.HasEnded)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= firstSightingSeconds && !IsGhostInView())
                    break;
                yield return null;
            }
            if (!hunting && monster != null && game != null && !game.HasEnded)
            {
                rig.SetVisible(false);
                monster.gameObject.SetActive(false);
            }
            sighting = null;
        }

        private bool IsGhostInView()
        {
            var cam = Camera.main;
            if (cam == null || monster == null) return false;
            Vector3 delta = monster.transform.position + Vector3.up * 1.25f -
                            cam.transform.position;
            return delta.sqrMagnitude < 180f &&
                   Vector3.Dot(cam.transform.forward, delta.normalized) > .28f;
        }

        private void StartHunt()
        {
            if (!CharacterReady) return;
            hunting = true;
            if (Camera.main == null) return;
            var cam = Camera.main.transform;
            // Reuse the existing witnessed spawn; teleport only if truly hidden.
            bool wasVisible = monster.gameObject.activeSelf && IsGhostInView();
            if (!wasVisible)
                monster.transform.position = WardSpatial.SafeCorridorSpawn(
                    cam.position, cam.forward);
            monster.gameObject.SetActive(true);
            monster.enabled = false;
            rig.SetVisible(true);
            rig.PlayAction(PatientMotion.Stare, 1.7f);
            PlayAtMonster(stinger);
            nextBreath = Time.time + 2f;
            StartCoroutine(BeginPursuit());
        }

        private IEnumerator BeginPursuit()
        {
            // Give the player a readable, stationary sighting before the chase,
            // instead of spawning an enemy out of sight at a fixed far-end position.
            yield return new WaitForSeconds(1.6f);
            if (!CharacterReady || game == null || game.HasEnded) yield break;
            rig.SetVisible(true);
            monster.enabled = true;
            monster.Wake(game, game.flashlight);
            rig.PlayAction(PatientMotion.Scare, .9f);
        }

        private void Update()
        {
            if (!hunting || !CharacterReady || game == null ||
                game.HasEnded || Camera.main == null)
                return;
            float dist = Vector3.Distance(Camera.main.transform.position, monster.transform.position);
            if (dist < 7f && Time.time > nextBreath)
            {
                PlayAtMonster(whisper);
                nextBreath = Time.time + Random.Range(2f, 4.3f);
            }
            if (crisisLights == null) return;
            foreach (var light in crisisLights)
                if (light != null && dist < 6f)
                    light.intensity = Mathf.Lerp(light.intensity,
                        .5f + Mathf.Sin(Time.time * 19f) * .25f, Time.deltaTime * 4f);
        }

        public void OnGameEnded()
        {
            hunting = false;
            if (sighting != null)
            {
                StopCoroutine(sighting);
                sighting = null;
            }
        }

        private void PlayAtMonster(AudioClip clip)
        {
            if (monster == null || clip == null || !CharacterReady) return;
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
                samples[i] = amplitude * fade *
                    (Mathf.Sin(2f * Mathf.PI * frequency * t) * .63f +
                     Mathf.Sin(2f * Mathf.PI * (frequency * .51f) * t) * .35f);
            }
            var result = AudioClip.Create(title, n, 1, rate, false);
            result.SetData(samples, 0);
            return result;
        }
    }
}
