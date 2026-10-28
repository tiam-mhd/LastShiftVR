using System.Collections;
using UnityEngine;

namespace LastShift
{
    /// <summary>
    /// One persistent, visible enemy. Never disables or despawns the ghost
    /// after the initial sighting. Picking up fuses escalates behavior without
    /// teleporting it or resetting its Animator while in the player's view.
    /// </summary>
    public sealed class GhostDirector : MonoBehaviour
    {
        public LastShiftGame game;
        public LastShiftMonster monster;
        public PatientCharacterRig rig;
        public Light[] crisisLights;
        public float openingDelay = 2.5f;
        public float pursuitWarningSeconds = 1.65f;
        public bool CharacterReady => monster != null && rig != null && rig.IsReady;

        private bool introduced;
        private bool hunting;
        private Coroutine pendingHunt;
        private AudioSource ghostAudio;
        private AudioClip whisper;
        private AudioClip stinger;
        private float nextBreath;

        private void Awake()
        {
            whisper = MakeTone("Patient whisper", 114f, 1.1f, .12f);
            stinger = MakeTone("Patient warning", 203f, .6f, .26f);
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(openingDelay);
            if (game == null || game.HasEnded || !CharacterReady) yield break;
            EnsureIntroduced();
            if (game.CollectedFuses >= 2) StartHunt();
        }

        public void OnFuseCountChanged(int count)
        {
            if (game == null || game.HasEnded || !CharacterReady) return;
            EnsureIntroduced();
            if (count >= 2) StartHunt();
            else if (count == 1)
            {
                if (rig != null) rig.PlayAction(PatientMotion.Stare, 1.2f);
                PlayAtMonster(whisper);
            }
        }

        private void EnsureIntroduced()
        {
            if (introduced || !CharacterReady || Camera.main == null) return;
            introduced = true;
            var cameraTransform = Camera.main.transform;
            Vector3 candidate = WardSpatial.SafeCorridorSpawn(
                cameraTransform.position, cameraTransform.forward);
            monster.transform.position = candidate;
            monster.transform.rotation = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(cameraTransform.position - candidate, Vector3.up).normalized,
                Vector3.up);

            monster.gameObject.SetActive(true);
            monster.enabled = false;
            rig.SetVisible(true);
            rig.PlayAction(PatientMotion.Stare, 1.5f);
            PlayAtMonster(stinger);
            Debug.Log("[LastShift / Patient] Persistent ghost entered ward at " +
                candidate + ". It will stay visible after this sighting.");
        }

        private void StartHunt()
        {
            if (hunting || pendingHunt != null || game == null || game.HasEnded) return;
            EnsureIntroduced();
            pendingHunt = StartCoroutine(ActivatePursuit());
        }

        private IEnumerator ActivatePursuit()
        {
            // Do not relocate the character: use the same grounded enemy.
            // This prevents an offscreen enemy suddenly causing Game Over.
            if (CharacterReady)
            {
                rig.SetVisible(true);
                rig.PlayAction(PatientMotion.Stare, pursuitWarningSeconds);
                PlayAtMonster(stinger);
            }
            yield return new WaitForSeconds(pursuitWarningSeconds);
            pendingHunt = null;
            if (!CharacterReady || game == null || game.HasEnded) yield break;
            monster.enabled = true;
            monster.Wake(game, game.flashlight);
            rig.SetVisible(true);
            hunting = true;
            nextBreath = Time.time + 2f;
            Debug.Log("[LastShift / Patient] Hunt started; existing visible FBX is pursuing.");
        }

        private void Update()
        {
            if (!hunting || !CharacterReady || game == null ||
                game.HasEnded || Camera.main == null) return;
            float d = Vector3.Distance(Camera.main.transform.position, monster.transform.position);
            if (d < 7f && Time.time > nextBreath)
            {
                PlayAtMonster(whisper);
                nextBreath = Time.time + Random.Range(2.2f, 4.4f);
            }
        }

        public void OnGameEnded()
        {
            hunting = false;
            if (pendingHunt != null)
            {
                StopCoroutine(pendingHunt);
                pendingHunt = null;
            }
            // Keep the model visible through win/loss; no mysterious disappearance.
            if (CharacterReady) rig.SetVisible(true);
        }

        private void PlayAtMonster(AudioClip clip)
        {
            if (!CharacterReady || clip == null) return;
            if (ghostAudio == null)
            {
                ghostAudio = monster.GetComponent<AudioSource>();
                if (ghostAudio == null) ghostAudio = monster.gameObject.AddComponent<AudioSource>();
                ghostAudio.spatialBlend = 1f;
                ghostAudio.rolloffMode = AudioRolloffMode.Logarithmic;
                ghostAudio.minDistance = .7f;
                ghostAudio.maxDistance = 13f;
                ghostAudio.playOnAwake = false;
            }
            ghostAudio.PlayOneShot(clip, .65f);
        }

        private static AudioClip MakeTone(string name, float frequency, float duration, float volume)
        {
            const int rate = 12000;
            int n = Mathf.RoundToInt(rate * duration);
            float[] data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float f = Mathf.Min(1f, t * 12f, (duration - t) * 5f);
                data[i] = volume * f * (Mathf.Sin(2f * Mathf.PI * frequency * t) * .6f +
                          Mathf.Sin(2f * Mathf.PI * frequency * .51f * t) * .32f);
            }
            var clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
