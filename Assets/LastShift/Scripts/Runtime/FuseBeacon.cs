using UnityEngine;

namespace LastShift
{
    /// <summary>Small readable pickup animation for the three fuse objects.</summary>
    public sealed class FuseBeacon : MonoBehaviour
    {
        public float bobHeight = .055f;
        public float spinDegreesPerSecond = 40f;
        private Vector3 start;
        private void Start() { start = transform.localPosition; }
        private void Update()
        {
            transform.localPosition = start + Vector3.up * (Mathf.Sin(Time.time * 2.8f) * bobHeight);
            transform.Rotate(Vector3.up, spinDegreesPerSecond * Time.deltaTime, Space.World);
        }
    }
}