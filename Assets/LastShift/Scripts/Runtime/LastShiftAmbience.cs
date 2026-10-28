using UnityEngine;
using System;

namespace LastShift
{
    /// <summary>Original procedurally generated, low-volume atmospheric drone for the prototype.</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class LastShiftAmbience : MonoBehaviour
    {
        private void Start()
        {
            const int sampleRate = 16000;
            const int seconds = 4;
            float[] samples = new float[sampleRate * seconds];
            var random = new System.Random(13);
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)sampleRate;
                float rumble = .65f * Mathf.Sin(2f * Mathf.PI * 44f * t);
                float pulse = .25f * Mathf.Sin(2f * Mathf.PI * 55f * t);
                float air = (float)(random.NextDouble() * 2.0 - 1.0) * .05f;
                samples[i] = (rumble + pulse + air) * .14f;
            }
            AudioClip clip = AudioClip.Create("WARD 13 - generated ambient drone",
                samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            var speaker = GetComponent<AudioSource>();
            speaker.clip = clip;
            speaker.loop = true;
            speaker.playOnAwake = false;
            speaker.spatialBlend = 0f;
            speaker.volume = .23f;
            speaker.Play();
        }
    }
}