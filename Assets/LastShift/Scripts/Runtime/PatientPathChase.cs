using UnityEngine;

namespace LastShift
{
    /// <summary>
    /// Fixed-layout portal routing: corridor <-> treatment room <-> surgery room.
    /// Avoids driving the enemy directly through solid side walls.
    /// Environmental colliders are handled by LastShiftMonster safe movement.
    /// </summary>
    public sealed class PatientPathChase : MonoBehaviour
    {
        public Transform player;
        private const float DoorZRight = 1.85f;
        private const float DoorZLeft = 7.25f;

        private static Vector3 DoorInner(bool right, float y)
            => new Vector3(right ? 3.25f : -3.25f, y, right ? DoorZRight : DoorZLeft);

        private static Vector3 DoorOuter(bool right, float y)
            => new Vector3(right ? 1.25f : -1.25f, y, right ? DoorZRight : DoorZLeft);

        public Vector3 Target(Vector3 current)
        {
            if (player == null && Camera.main != null) player = Camera.main.transform;
            if (player == null) return current;

            Vector3 desired = player.position;
            desired.y = current.y;

            bool selfRight = WardSpatial.InTreatment(current);
            bool selfLeft = WardSpatial.InSurgery(current);
            bool targetRight = WardSpatial.InTreatment(desired);
            bool targetLeft = WardSpatial.InSurgery(desired);

            // Leave whichever side room we're in before crossing the hallway.
            if (selfRight && !targetRight)
            {
                Vector3 inner = DoorInner(true, current.y);
                if (Vector3.Distance(current, inner) > .35f) return inner;
                return DoorOuter(true, current.y);
            }
            if (selfLeft && !targetLeft)
            {
                Vector3 inner = DoorInner(false, current.y);
                if (Vector3.Distance(current, inner) > .35f) return inner;
                return DoorOuter(false, current.y);
            }

            // Corridor -> side ward through its actual open doorway.
            if (!selfRight && !selfLeft && targetRight)
            {
                Vector3 outer = DoorOuter(true, current.y);
                if (Vector3.Distance(current, outer) > .38f) return outer;
                return DoorInner(true, current.y);
            }
            if (!selfRight && !selfLeft && targetLeft)
            {
                Vector3 outer = DoorOuter(false, current.y);
                if (Vector3.Distance(current, outer) > .38f) return outer;
                return DoorInner(false, current.y);
            }

            return desired;
        }
    }
}
