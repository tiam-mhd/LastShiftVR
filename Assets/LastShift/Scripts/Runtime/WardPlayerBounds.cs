using UnityEngine;

namespace LastShift
{
    /// <summary>
    /// Extra fail-safe for desktop testing. Do not forcibly move the headset in
    /// VR, where abrupt tracking-origin changes can cause discomfort.
    /// </summary>
    public sealed class WardPlayerBounds : MonoBehaviour
    {
        public Transform head;
        private Vector3 lastGood;
        private bool initialized;
        private float nextWarning;

        private void LateUpdate()
        {
            if (!DesktopPreview.IsPreviewActive || head == null) return;
            Vector3 pos = head.position;
            if (!initialized)
            {
                lastGood = pos;
                initialized = true;
            }

            if (WardSpatial.Contains(pos) && pos.y >= .5f && pos.y <= 3f)
            {
                lastGood = pos;
                return;
            }

            Vector3 correction = lastGood - pos;
            correction.y = Mathf.Abs(pos.y - lastGood.y) > 1.5f ? lastGood.y - pos.y : 0f;
            transform.position += correction;
            if (Time.time >= nextWarning)
            {
                Debug.LogWarning("[LastShift] Prevented player from leaving the playable hospital.");
                nextWarning = Time.time + 5f;
            }
        }
    }
}
