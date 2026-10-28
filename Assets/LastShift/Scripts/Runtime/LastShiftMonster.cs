using UnityEngine;

namespace LastShift
{
    public sealed class LastShiftMonster : MonoBehaviour
    {
        public float chaseSpeed = 1.08f;
        public float captureDistance = 1.12f;
        public float acceleration = 1.3f;

        private LastShiftGame game;
        private FlashlightBehaviour torch;
        private Transform head;
        private PatientPathChase path;
        private bool hunting;
        private float currentSpeed;

        public void Wake(LastShiftGame owner, FlashlightBehaviour source)
        {
            game = owner;
            torch = source;
            if (Camera.main != null) head = Camera.main.transform;
            path = GetComponent<PatientPathChase>();
            currentSpeed = .3f;
            hunting = true;
            gameObject.SetActive(true);
            enabled = true;
        }

        public void StopChasing() { hunting = false; }

        private void Update()
        {
            if (!hunting || game == null || game.HasEnded) return;
            if (head == null && Camera.main != null) head = Camera.main.transform;
            if (head == null) return;

            var playerPosition = head.position;
            var flat = playerPosition - transform.position;
            flat.y = 0f;
            if (flat.magnitude < captureDistance)
            {
                game.Caught();
                return;
            }

            bool stunned = torch != null &&
                torch.HoldsMonster(transform.position + Vector3.up * 1.13f, transform);
            if (stunned)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, Time.deltaTime * 4.3f);
                return;
            }

            var destination = path != null ? path.Target(transform.position) : playerPosition;
            destination.y = transform.position.y;
            var direction = destination - transform.position;

            currentSpeed = Mathf.MoveTowards(currentSpeed, chaseSpeed, Time.deltaTime * acceleration);
            if (direction.sqrMagnitude > .015f)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                    destination, currentSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction.normalized, Vector3.up),
                    Time.deltaTime * 4f);
            }
        }
    }
}
