using UnityEngine;

namespace LastShift
{
    /// <summary>
    /// Floor-plan limits of the generated Ward 13 level. Keeps movement in the
    /// playable hospital rather than walking into the default Unity skybox.
    /// Geometry/CharacterController collision remains authoritative.
    /// </summary>
    public static class WardSpatial
    {
        public static bool InCorridor(Vector3 p) =>
            p.x >= -2.30f && p.x <= 2.30f && p.z >= -11.45f && p.z <= 15.5f;
        public static bool InTreatment(Vector3 p) =>
            p.x >= 2.20f && p.x <= 7.96f && p.z >= -1.08f && p.z <= 4.82f;
        public static bool InSurgery(Vector3 p) =>
            p.x <= -2.20f && p.x >= -7.96f && p.z >= 4.32f && p.z <= 10.18f;

        public static bool Contains(Vector3 p) =>
            InCorridor(p) || InTreatment(p) || InSurgery(p);

        public static Vector3 SafeCorridorSpawn(Vector3 player, Vector3 gaze)
        {
            // Spawn roughly five meters in the direction the player is facing,
            // inside the actual corridor, instead of always at the far back wall.
            float towardZ = gaze.z >= 0f ? 1f : -1f;
            if (Mathf.Abs(gaze.z) < .25f) towardZ = player.z > 7f ? -1f : 1f;
            float z = Mathf.Clamp(player.z + towardZ * 5.2f, -9.4f, 12.5f);
            if (InTreatment(player)) return new Vector3(3.12f, 0f, 1.85f);
            if (InSurgery(player)) return new Vector3(-3.12f, 0f, 7.25f);
            return new Vector3(0f, 0f, z);
        }
    }

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
