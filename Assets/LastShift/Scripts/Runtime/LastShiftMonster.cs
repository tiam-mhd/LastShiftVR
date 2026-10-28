using System.Collections;
using UnityEngine;

namespace LastShift
{
    /// <summary>
    /// Real FBX horror enemy. Moves through real door openings, respects
    /// world geometry, cannot walk into the Unity sky, and never teleports
    /// into the camera or across walls to display a jumpscare.
    /// </summary>
    public sealed class LastShiftMonster : MonoBehaviour
    {
        public float chaseSpeed = 1.05f;
        public float runSpeed = 1.68f;
        public float runTriggerDistance = 4.75f;
        public float captureDistance = 1.10f;
        public float acceleration = 1.6f;
        public float revealSeconds = 1.25f;

        private LastShiftGame game;
        private FlashlightBehaviour torch;
        private Transform head;
        private PatientPathChase path;
        private PatientCharacterRig rig;
        private bool hunting;
        private bool attacking;
        private float currentSpeed;
        private Coroutine captureRoutine;
        private readonly RaycastHit[] obstacleHits = new RaycastHit[24];

        public bool IsHunting => hunting && !attacking;
        public bool IsAttacking => attacking;
        public bool IsStunned { get; private set; }
        public float CurrentSpeed => currentSpeed;

        public void Wake(LastShiftGame owner, FlashlightBehaviour source)
        {
            game = owner;
            torch = source;
            head = Camera.main != null ? Camera.main.transform : null;
            path = GetComponent<PatientPathChase>();
            rig = GetComponent<PatientCharacterRig>();

            if (rig == null || !rig.IsReady)
            {
                Debug.LogWarning("[LastShift] Cannot activate enemy without real visible FBX mesh.");
                hunting = false;
                enabled = false;
                return;
            }

            currentSpeed = 0f;
            attacking = false;
            IsStunned = false;
            hunting = true;
            gameObject.SetActive(true);
            enabled = true;
            rig.SetVisible(true);
        }

        public void StopChasing()
        {
            hunting = false;
            currentSpeed = 0f;
            IsStunned = false;
            if (!attacking && captureRoutine != null)
            {
                StopCoroutine(captureRoutine);
                captureRoutine = null;
            }
        }

        private void Update()
        {
            if (!hunting || attacking || game == null || game.HasEnded) return;
            if (head == null && Camera.main != null) head = Camera.main.transform;
            if (head == null || !WardSpatial.Contains(head.position)) return;

            Vector3 offset = head.position - transform.position;
            offset.y = 0f;

            // A capture requires an actual, unobstructed approach; no capture through walls.
            if (offset.magnitude <= captureDistance &&
                IsVisibleToPlayer() && HasClearSight())
            {
                captureRoutine = StartCoroutine(CaptureReveal());
                return;
            }

            bool stunned = torch != null &&
                torch.HoldsMonster(transform.position + Vector3.up * 1.12f, transform);
            IsStunned = stunned;
            if (stunned)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, Time.deltaTime * 4f);
                return;
            }

            Vector3 target = path != null ? path.Target(transform.position) : head.position;
            target.y = transform.position.y;
            Vector3 direction = target - transform.position;
            float desiredSpeed = offset.magnitude < runTriggerDistance ? runSpeed : chaseSpeed;
            currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed,
                Time.deltaTime * acceleration);
            if (direction.sqrMagnitude < .025f) return;

            direction.Normalize();
            float step = currentSpeed * Time.deltaTime;
            if (TryMove(direction * step) ||
                TryMove(Vector3.Project(direction, Vector3.right) * step) ||
                TryMove(Vector3.Project(direction, Vector3.forward) * step))
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction, Vector3.up), Time.deltaTime * 3.5f);
            }
        }

        private bool TryMove(Vector3 delta)
        {
            if (delta.sqrMagnitude < .0000001f) return false;
            Vector3 next = transform.position + delta;
            if (!WardSpatial.Contains(next)) return false;

            Vector3 direction = delta.normalized;
            float distance = delta.magnitude;
            var origin = transform.position + Vector3.up * .95f;
            int hits = Physics.SphereCastNonAlloc(origin, .27f, direction, obstacleHits,
                distance + .045f, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hits; i++)
            {
                Collider col = obstacleHits[i].collider;
                if (col == null) continue;
                if (col.transform == transform || col.transform.IsChildOf(transform))
                    continue;
                if (head != null && col.transform.IsChildOf(head.root)) continue;
                return false;
            }

            transform.position = next;
            return true;
        }

        private bool IsVisibleToPlayer()
        {
            if (head == null || rig == null || !rig.IsReady) return false;
            Camera camera = Camera.main;
            if (camera == null) return false;
            if (!rig.HasVisibleMesh()) return false;
            // Don't kill from behind the player, behind the camera, or
            // while the enemy is completely off screen.
            Vector3 at = transform.position + Vector3.up * 1.15f;
            Vector3 p = camera.WorldToViewportPoint(at);
            return p.z > .15f && p.x >= .08f && p.x <= .92f &&
                   p.y >= .08f && p.y <= .92f;
        }

        private bool HasClearSight()
        {
            if (head == null) return false;
            Vector3 origin = transform.position + Vector3.up * 1.30f;
            Vector3 vector = head.position - origin;
            float distance = vector.magnitude;
            if (distance < .05f) return true;
            int hits = Physics.RaycastNonAlloc(new Ray(origin, vector / distance), obstacleHits,
                distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            // RaycastNonAlloc does not guarantee nearest-hit order. Any true
            // blocking collider before the player's head must cancel capture.
            for (int i = 0; i < hits; i++)
            {
                Collider col = obstacleHits[i].collider;
                if (col == null || col.transform == transform || col.transform.IsChildOf(transform))
                    continue;
                if (col.transform.IsChildOf(head.root)) continue;
                if (obstacleHits[i].distance + .05f < distance)
                    return false;
            }
            return true;
        }

        private IEnumerator CaptureReveal()
        {
            attacking = true;
            hunting = false;
            if (rig == null || !rig.IsReady)
            {
                attacking = false;
                yield break;
            }
            currentSpeed = 0f;
            rig.SetVisible(true);
            rig.PlayAction(PatientMotion.Attack, revealSeconds);

            // Remain in the hallway/ward at the actual contact position.
            // The old version moved the monster directly into the camera view
            // and accidentally displayed it outside the hospital walls.
            float elapsed = 0f;
            while (elapsed < revealSeconds)
            {
                if (game == null || game.HasEnded || head == null) break;
                Vector3 toPlayer = head.position - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > .02f)
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(toPlayer.normalized), Time.deltaTime * 8f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (game != null && !game.HasEnded)
            {
                Vector3 playerDelta = head != null ? head.position - transform.position : Vector3.zero;
                playerDelta.y = 0f;
                if (playerDelta.magnitude <= captureDistance + .5f &&
                    IsVisibleToPlayer() && HasClearSight())
                {
                    game.Caught();
                }
                else
                {
                    // During the reveal the player may back away or look away.
                    // Cancel the kill instead of showing GAME OVER with no ghost.
                    attacking = false;
                    hunting = true;
                    rig.SetVisible(true);
                    Debug.Log("[LastShift / Patient] Capture cancelled: ghost was not visibly in front of player.");
                }
            }
            captureRoutine = null;
        }
    }
}