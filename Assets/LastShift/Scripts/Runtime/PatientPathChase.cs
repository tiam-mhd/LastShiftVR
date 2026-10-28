using UnityEngine;

namespace LastShift
{
    /// <summary>
    /// Optional simple waypoint router for the two side wards.
    /// This is NOT NavMesh, and is only intended for the fixed generated layout.
    /// </summary>
    public sealed class PatientPathChase : MonoBehaviour
    {
        public Transform player;
        public Vector3 rightWardDoor = new Vector3(1.75f, 0f, 1.85f);
        public Vector3 leftWardDoor = new Vector3(-1.75f, 0f, 7.25f);
        public float maxRoomX = 2.85f;

        public Vector3 Target(Vector3 currentPos)
        {
            if (player == null && Camera.main != null)
                player = Camera.main.transform;
            if (player == null) return currentPos;
            var p = player.position;
            p.y = currentPos.y;

            // Door openings at corridor side walls; route via doorway for room targets.
            if (p.x > maxRoomX)
            {
                var doorway = rightWardDoor; doorway.y = currentPos.y;
                if (currentPos.x <= 2.9f && Vector3.Distance(currentPos, doorway) > .65f)
                    return doorway;
            }
            else if (p.x < -maxRoomX)
            {
                var doorway = leftWardDoor; doorway.y = currentPos.y;
                if (currentPos.x >= -2.9f && Vector3.Distance(currentPos, doorway) > .65f)
                    return doorway;
            }
            else if (currentPos.x > maxRoomX)
            {
                var doorway = rightWardDoor; doorway.y = currentPos.y;
                if (Vector3.Distance(currentPos, doorway) > .55f) return doorway;
            }
            else if (currentPos.x < -maxRoomX)
            {
                var doorway = leftWardDoor; doorway.y = currentPos.y;
                if (Vector3.Distance(currentPos, doorway) > .55f) return doorway;
            }
            return p;
        }
    }
}