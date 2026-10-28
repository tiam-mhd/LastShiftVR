using UnityEngine;

namespace LastShift
{
    public enum PatientAction
    {
        Idle, Stare, Walk, Run, Frightened, Scare, Attack, Appear, Vanish, Stunned
    }

    /// <summary>
    /// Procedural skeletal animation. No store-bought clips required:
    /// walk, run, frightened recoil, stare, jumpscare, appear, vanish and idle
    /// are applied to the seven-bone skinned model with blended interpolation.
    /// </summary>
    public sealed class PatientAnimationDriver : MonoBehaviour
    {
        public GhostGirlVisual visual;
        public LastShiftMonster enemy;
        public PatientAction currentAction = PatientAction.Idle;
        public float animationSpeed = 1f;
        public float blendRate = 6.5f;
        public float walkCycle = 1.7f;
        public float runCycle = 3.8f;

        private PatientAction overrideAction;
        private float overrideUntil;
        private float cycle;
        private Vector3 lastPosition;
        private float currentVelocity;
        private bool initialized;

        private void Awake()
        {
            if (visual == null) visual = GetComponent<GhostGirlVisual>();
            if (enemy == null) enemy = GetComponent<LastShiftMonster>();
            lastPosition = transform.position;
        }

        public void Play(PatientAction action, float duration = 1f)
        {
            overrideAction = action;
            overrideUntil = Time.time + Mathf.Max(.15f, duration);
        }

        private void LateUpdate()
        {
            if (visual == null || visual.hipsBone == null) return;
            if (!initialized)
            {
                lastPosition = transform.position;
                initialized = true;
            }

            Vector3 delta = transform.position - lastPosition;
            lastPosition = transform.position;
            float rawVelocity = Time.deltaTime > .001f ?
                Vector3.ProjectOnPlane(delta, Vector3.up).magnitude / Time.deltaTime : 0f;
            currentVelocity = Mathf.Lerp(currentVelocity, rawVelocity, Time.deltaTime * 5f);

            bool isHunting = enemy != null && enemy.IsHunting;
            bool isAttacking = enemy != null && enemy.IsAttacking;
            bool isStunned = enemy != null && enemy.IsStunned;

            if (isAttacking) currentAction = PatientAction.Attack;
            else if (Time.time < overrideUntil) currentAction = overrideAction;
            else if (visual.opacity < .20f) currentAction = PatientAction.Vanish;
            else if (isStunned) currentAction = PatientAction.Frightened;
            else if (isHunting && currentVelocity > 1.35f) currentAction = PatientAction.Run;
            else if (isHunting && currentVelocity > .12f) currentAction = PatientAction.Walk;
            else currentAction = PatientAction.Idle;

            float frequency = currentAction == PatientAction.Run ? runCycle : walkCycle;
            cycle += Time.deltaTime * frequency * animationSpeed * 4f;
            float s = Mathf.Sin(cycle);
            float s2 = Mathf.Sin(cycle + Mathf.PI);
            float t = Time.time * animationSpeed;
            float chestPitch = 0f, headRoll = 0f, headPitch = 0f;
            float leftArmPitch = 0f, rightArmPitch = 0f;
            float leftArmRoll = -4f, rightArmRoll = 4f;
            float leftLegPitch = 0f, rightLegPitch = 0f;
            float hipRoll = 0f;

            switch (currentAction)
            {
                case PatientAction.Walk:
                    chestPitch = 8f;
                    hipRoll = s * 2.5f;
                    headRoll = Mathf.Sin(t * 1.2f) * 3f;
                    leftArmPitch = s2 * 22f + 10f;
                    rightArmPitch = s * 22f + 10f;
                    leftLegPitch = s * 24f;
                    rightLegPitch = s2 * 24f;
                    break;

                case PatientAction.Run:
                    chestPitch = 18f;
                    hipRoll = s * 5.5f;
                    headPitch = -10f;
                    leftArmPitch = s2 * 45f + 28f;
                    rightArmPitch = s * 45f + 28f;
                    leftArmRoll = -24f;
                    rightArmRoll = 24f;
                    leftLegPitch = s * 42f;
                    rightLegPitch = s2 * 42f;
                    break;

                case PatientAction.Stare:
                    chestPitch = 3f;
                    headRoll = 18f + Mathf.Sin(t * 1.1f) * 2f;
                    headPitch = -8f;
                    leftArmPitch = 5f;
                    rightArmPitch = 5f;
                    break;

                case PatientAction.Frightened:
                case PatientAction.Stunned:
                    chestPitch = -20f + Mathf.Sin(t * 8f) * 5f;
                    headRoll = Mathf.Sin(t * 11f) * 5f;
                    headPitch = -24f;
                    leftArmPitch = -55f;
                    rightArmPitch = -55f;
                    leftArmRoll = -48f;
                    rightArmRoll = 48f;
                    break;

                case PatientAction.Scare:
                case PatientAction.Attack:
                    chestPitch = -32f;
                    headPitch = -27f;
                    headRoll = Mathf.Sin(t * 10f) * 6f;
                    leftArmPitch = -85f;
                    rightArmPitch = -85f;
                    leftArmRoll = -29f;
                    rightArmRoll = 29f;
                    break;

                case PatientAction.Appear:
                    headRoll = Mathf.Sin(t * 1.2f) * 15f;
                    chestPitch = 7f;
                    leftArmPitch = 18f;
                    rightArmPitch = 18f;
                    break;

                case PatientAction.Vanish:
                    chestPitch = 16f;
                    headPitch = -28f;
                    leftArmPitch = 28f;
                    rightArmPitch = 28f;
                    leftArmRoll = -12f;
                    rightArmRoll = 12f;
                    break;

                default:
                    chestPitch = Mathf.Sin(t * .9f) * 2.2f;
                    headRoll = Mathf.Sin(t * .49f) * 5.5f;
                    leftArmPitch = Mathf.Sin(t * .65f) * 4f + 3f;
                    rightArmPitch = Mathf.Sin(t * .72f + .9f) * 4f + 3f;
                    break;
            }

            float alpha = Mathf.Clamp01(Time.deltaTime * blendRate);
            Rotate(visual.hipsBone, 0f, 0f, hipRoll, alpha);
            Rotate(visual.chestBone, chestPitch, Mathf.Sin(t * .31f) * 2f, 0f, alpha);
            Rotate(visual.headBone, headPitch, 0f, headRoll, alpha);
            Rotate(visual.leftArmBone, leftArmPitch, 0f, leftArmRoll, alpha);
            Rotate(visual.rightArmBone, rightArmPitch, 0f, rightArmRoll, alpha);
            Rotate(visual.leftLegBone, leftLegPitch, 0f, 0f, alpha);
            Rotate(visual.rightLegBone, rightLegPitch, 0f, 0f, alpha);
        }

        private static void Rotate(Transform bone, float x, float y, float z, float blend)
        {
            if (bone == null) return;
            bone.localRotation = Quaternion.Slerp(bone.localRotation,
                Quaternion.Euler(x, y, z), blend);
        }
    }
}
