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
        public float firstSightingSeconds = 5f;
        public float openingDelay = 2.5f;
        public bool CharacterReady => monster != null && rig != null && rig.IsReady;

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
                Debug.LogWarning("[LastShift] Ghost event skipped: real model / animation clips not installed.");
                return;
            }

            if (count == 1)
            {
                if (sighting != null) StopCoroutine(sighting);
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
            float z = -2.25f;
            if (Camera.main != null)
                z = Mathf.Clamp(Camera.main.transform.position.z + 6.5f, -2.25f, 11.5f);
            monster.transform.position = new Vector3(0f, 0f, z);
            monster.gameObject.SetActive(true);
            monster.enabled = false;
            rig.SetVisible(true);
            rig.PlayAction(PatientMotion.Appear, firstSightingSeconds * .45f);
            PlayAtMonster(stinger);

            yield return new WaitForSeconds(Mathf.Max(.15f, firstSightingSeconds - .9f));
            if (CharacterReady)
                rig.PlayAction(PatientMotion.Vanish, .8f);
            yield return new WaitForSeconds(.85f);

            if (!hunting && monster != null)
            {
                rig.SetVisible(false);
                monster.gameObject.SetActive(false);
                monster.enabled = true;
            }
            sighting = null;
        }

        private void StartHunt()
        {
            if (!CharacterReady) return;
            hunting = true;
            monster.enabled = true;
            monster.transform.position = new Vector3(0f, 0f, 14f);
            monster.gameObject.SetActive(true);
            rig.SetVisible(true);
            monster.Wake(game, game.flashlight);
            rig.PlayAction(PatientMotion.Scare, 1f);
            PlayAtMonster(stinger);
            nextBreath = Time.time + 2f;
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
