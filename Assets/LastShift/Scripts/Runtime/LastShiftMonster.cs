using System.Collections;
using UnityEngine;

namespace LastShift
{
    /// <summary>Patient pursuit and a readable, 1.35s in-front-of-view capture reveal.</summary>
    public sealed class LastShiftMonster : MonoBehaviour
    {
        public float chaseSpeed = 1.08f;
        public float runSpeed = 1.9f;
        public float runTriggerDistance = 5f;
        public bool IsHunting => hunting && !attacking;
        public bool IsAttacking => attacking;
        public bool IsStunned { get; private set; }
        public float captureDistance = 1.12f;
        public float acceleration = 1.3f;
        public float revealSeconds = 1.35f;

        private LastShiftGame game;
        private FlashlightBehaviour torch;
        private Transform head;
        private PatientPathChase path;
        private GhostGirlVisual visual;
        private bool hunting;
        private bool attacking;
        private float currentSpeed;
        private Coroutine captureRoutine;

        public void Wake(LastShiftGame owner, FlashlightBehaviour source)
        {
            game = owner;
            torch = source;
            if (Camera.main != null) head = Camera.main.transform;
            path = GetComponent<PatientPathChase>();
            visual = GetComponent<GhostGirlVisual>();
            currentSpeed = .3f;
            attacking = false;
            IsStunned = false;
            hunting = true;
            gameObject.SetActive(true);
            enabled = true;
            if (visual != null) { visual.EnsureVisual(); visual.SetOpacity(1f); }
        }

        public void StopChasing()
        {
            hunting = false;
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
            if (head == null) return;

            Vector3 flat = head.position - transform.position;
            flat.y = 0f;
            if (flat.magnitude < captureDistance && HasClearSight())
            {
                captureRoutine = StartCoroutine(CaptureReveal());
                return;
            }

            bool stunned = torch != null &&
                torch.HoldsMonster(transform.position + Vector3.up * 1.13f, transform);
            IsStunned = stunned;
            if (stunned)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, Time.deltaTime * 4.3f);
                return;
            }

            var destination = path != null ? path.Target(transform.position) : head.position;
            destination.y = transform.position.y;
            var direction = destination - transform.position;
            float speedTarget = flat.magnitude < runTriggerDistance ? runSpeed : chaseSpeed;
            currentSpeed = Mathf.MoveTowards(currentSpeed, speedTarget, Time.deltaTime * acceleration);
            if (direction.sqrMagnitude > .015f)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                    destination, currentSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction.normalized, Vector3.up),
                    Time.deltaTime * 4f);
            }
        }

        private bool HasClearSight()
        {
            if (head == null) return false;
            Vector3 origin = transform.position + Vector3.up * 1.4f;
            Vector3 towardsHead = head.position - origin;
            if (towardsHead.sqrMagnitude < .02f) return true;
            // Ground and ward geometry block the capture; the player's controller
            // or CharacterController can be the first hit and should be accepted.
            if (!Physics.Raycast(origin, towardsHead.normalized, out var hit,
                                 towardsHead.magnitude, Physics.DefaultRaycastLayers,
                                 QueryTriggerInteraction.Ignore))
                return true;
            if (hit.transform.IsChildOf(transform)) return true;
            if (head.IsChildOf(hit.transform) || hit.transform.IsChildOf(head.root)) return true;
            return false;
        }

        private IEnumerator CaptureReveal()
        {
            attacking = true;
            hunting = false;
            var animationDriver = GetComponent<PatientAnimationDriver>();
            if (animationDriver != null) animationDriver.Play(PatientAction.Attack, revealSeconds);
            if (visual == null) visual = GetComponent<GhostGirlVisual>();
            if (visual != null) { visual.EnsureVisual(); visual.SetOpacity(1f); }

            // Do not move the VR camera: the ghost steps into the player's view instead.
            // This prevents a game-over message from covering an invisible offscreen enemy.
            float elapsed = 0f;
            while (elapsed < revealSeconds)
            {
                if (game == null || game.HasEnded || head == null) break;
                Vector3 flatForward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
                if (flatForward.sqrMagnitude < .0001f) flatForward = head.forward;
                flatForward.Normalize();
                Vector3 revealPoint = head.position + flatForward * 1.65f;
                revealPoint.y = head.position.y - 1.45f;
                transform.position = Vector3.Lerp(transform.position, revealPoint,
                    Mathf.Clamp01(Time.deltaTime * 9f));
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (game != null && !game.HasEnded) game.Caught();
            captureRoutine = null;
        }
    }
}
