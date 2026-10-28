using System.Collections;
using UnityEngine;

namespace LastShift
{
    /// <summary>
    /// Environmental horror events that function even when the real FBX character
    /// is not yet imported. No mesh impostors, camera jumps, or invisible kills.
    /// Audio is generated locally as original procedural placeholder sound.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WardHorrorEvents : MonoBehaviour
    {
        public LastShiftGame game;
        public Light scareLight;
        public Transform corridorSoundPoint;
        public Transform rearSoundPoint;
        public Transform doorPivot;
        [Range(0f, 1f)] public float intensity = .75f;

        private AudioSource corridor;
        private AudioSource rear;
        private AudioClip knock;
        private AudioClip whisper;
        private AudioClip slam;
        private Quaternion initialDoorRotation;
        private Coroutine ongoing;
        private bool ready;

        private void Start()
        {
            corridor = MakeSource(corridorSoundPoint, "Corridor horror sounds");
            rear = MakeSource(rearSoundPoint, "Rear horror sounds");
            knock = CreateSound("Three heavy knocks", 1.8f, 19);
            whisper = CreateSound("Distant breath and whisper", 2.8f, 47);
            slam = CreateSound("Sudden metal slam", 1.1f, 83);
            if (doorPivot != null) initialDoorRotation = doorPivot.localRotation;
            if (scareLight != null)
            {
                scareLight.intensity = 0f;
                scareLight.enabled = true;
                scareLight.shadows = LightShadows.None;
            }
            ready = true;
        }

        public void OnFuseTaken(int count)
        {
            if (!ready || game == null || game.HasEnded) return;
            if (ongoing != null) StopCoroutine(ongoing);
            ongoing = StartCoroutine(HorrorSequence(Mathf.Clamp(count, 1, 3)));
        }

        public void OnPowerRestored()
        {
            if (ongoing != null) StopCoroutine(ongoing);
            ongoing = null;
            if (scareLight != null) scareLight.intensity = 0f;
            if (doorPivot != null) doorPivot.localRotation = initialDoorRotation;
            if (corridor != null) corridor.PlayOneShot(slam, .2f * intensity);
        }

        private IEnumerator HorrorSequence(int stage)
        {
            if (stage == 1)
            {
                yield return new WaitForSeconds(.7f);
                if (corridor != null) corridor.PlayOneShot(knock, .58f * intensity);
                yield return Pulse(.30f, 1.5f);
            }
            else if (stage == 2)
            {
                yield return Pulse(.85f, 1.2f);
                if (rear != null) rear.PlayOneShot(whisper, .65f * intensity);
                yield return new WaitForSeconds(.8f);
                if (corridor != null) corridor.PlayOneShot(slam, .78f * intensity);
                if (doorPivot != null) yield return SlamDoor();
            }
            else
            {
                if (rear != null) rear.PlayOneShot(whisper, .75f * intensity);
                yield return new WaitForSeconds(1.4f);
                if (corridor != null) corridor.PlayOneShot(knock, .65f * intensity);
                yield return Pulse(.5f, 1.8f);
            }

            if (scareLight != null) scareLight.intensity = 0f;
            ongoing = null;
        }

        private IEnumerator Pulse(float duration, float peak)
        {
            if (scareLight == null) yield break;
            float time = 0f;
            // Slow and brief light pulse, not repeated high-frequency strobing.
            while (time < duration)
            {
                time += Time.deltaTime;
                float progress = Mathf.Clamp01(time / duration);
                scareLight.intensity = Mathf.Sin(progress * Mathf.PI) * peak * intensity;
                yield return null;
            }
            scareLight.intensity = 0f;
        }

        private IEnumerator SlamDoor()
        {
            var open = Quaternion.Euler(0f, 75f, 0f) * initialDoorRotation;
            float t = 0f;
            while (t < .38f)
            {
                t += Time.deltaTime;
                doorPivot.localRotation = Quaternion.Slerp(initialDoorRotation, open, t / .38f);
                yield return null;
            }
            yield return new WaitForSeconds(.22f);
            t = 0f;
            while (t < .15f)
            {
                t += Time.deltaTime;
                doorPivot.localRotation = Quaternion.Slerp(open, initialDoorRotation, t / .15f);
                yield return null;
            }
            doorPivot.localRotation = initialDoorRotation;
        }

        private static AudioSource MakeSource(Transform anchor, string label)
        {
            var go = new GameObject(label);
            if (anchor != null)
            {
                go.transform.SetParent(anchor, false);
                go.transform.localPosition = Vector3.zero;
            }
            var audio = go.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 1f;
            audio.rolloffMode = AudioRolloffMode.Logarithmic;
            audio.minDistance = 1f;
            audio.maxDistance = 15f;
            return audio;
        }

        private static AudioClip CreateSound(string label, float length, int seed)
        {
            const int rate = 16000;
            int n = Mathf.RoundToInt(rate * length);
            var data = new float[n];
            var rng = new System.Random(seed);
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float envelope = 0f;
                float tone = 0f;
                if (seed == 19)
                {
                    // Calculate three percussive envelopes without allocating
                    // a new array once per audio sample (~29k garbage objects).
                    float d0 = t - .13f, d1 = t - .67f, d2 = t - 1.19f;
                    if (d0 >= 0f) envelope += Mathf.Exp(-d0 * 21f);
                    if (d1 >= 0f) envelope += Mathf.Exp(-d1 * 21f);
                    if (d2 >= 0f) envelope += Mathf.Exp(-d2 * 21f);
                    tone = Mathf.Sin(2f * Mathf.PI * 73f * t) * .5f + noise * .40f;
                }
                else if (seed == 83)
                {
                    envelope = Mathf.Exp(-t * 8f);
                    tone = noise * .55f + Mathf.Sin(2f * Mathf.PI * 90f * t) * .45f;
                }
                else
                {
                    envelope = Mathf.Sin(Mathf.PI * t / length) * .60f;
                    tone = noise * .21f + Mathf.Sin(2f * Mathf.PI * (89f + Mathf.Sin(t * 3.2f) * 11f) * t) * .22f;
                }
                data[i] = Mathf.Clamp(tone * envelope * .6f, -.85f, .85f);
            }
            var clip = AudioClip.Create(label, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
