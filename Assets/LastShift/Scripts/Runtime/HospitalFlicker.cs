using UnityEngine;

namespace LastShift
{
    public sealed class HospitalFlicker : MonoBehaviour
    {
        public Light target;
        public float baseIntensity = .85f;
        public float flickerChancePerSecond = .45f;
        private float duration;
        private void Update()
        {
            if (target == null) return;
            if (duration > 0f)
            {
                duration -= Time.deltaTime;
                target.intensity = baseIntensity * Random.Range(.05f, .55f);
            }
            else
            {
                target.intensity = baseIntensity;
                if (Random.value < flickerChancePerSecond * Time.deltaTime)
                    duration = Random.Range(.08f, .24f);
            }
        }
    }
}