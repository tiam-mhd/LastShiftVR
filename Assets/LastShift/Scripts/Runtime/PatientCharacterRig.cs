using System;
using UnityEngine;

namespace LastShift
{
    /// <summary>State requests for an actual imported model's Animator clips.</summary>
    public enum PatientMotion
    {
        Idle = 0, Walk = 1, Run = 2, Stare = 3, Frightened = 4,
        Scare = 5, Attack = 6, Appear = 7, Vanish = 8, Stunned = 9
    }

    [DisallowMultipleComponent]
    public sealed class PatientCharacterRig : MonoBehaviour
    {
        public GameObject characterRoot;
        public Animator animator;
        [Tooltip("Bits for semantic clips found in the original imported file. Other states may reuse a source clip.")]
        public int availableMotionMask;

        private Renderer[] renderers = Array.Empty<Renderer>();
        private PatientMotion currentMotion = (PatientMotion)(-1);
        private float lastActionUntil;
        private PatientMotion actionMotion;
        private bool warnedNoAnimations;
        private Vector3 restPosition;
        private Quaternion restRotation;
        private bool initialized;

        public bool HasModel
        {
            get
            {
                if (characterRoot == null || !characterRoot.transform.IsChildOf(transform))
                    return false;
                if (renderers == null || renderers.Length == 0) CacheRenderers();
                foreach (Renderer r in renderers)
                {
                    if (r is SkinnedMeshRenderer s && s.sharedMesh != null) return true;
                    if (r is MeshRenderer mr && mr.GetComponent<MeshFilter>() != null &&
                        mr.GetComponent<MeshFilter>().sharedMesh != null) return true;
                }
                return false;
            }
        }

        // The real FBX must be visible even when it has no animation clips,
        // rather than deleting all horror events until four arbitrary clip names match.
        public bool IsReady => HasModel;

        public bool HasAnimation => animator != null && animator.runtimeAnimatorController != null &&
            animator.runtimeAnimatorController.animationClips != null &&
            animator.runtimeAnimatorController.animationClips.Length > 0;

        public bool Has(PatientMotion motion) =>
            (availableMotionMask & (1 << (int)motion)) != 0;

        public void Configure(GameObject realModel, Animator modelAnimator, int motionMask)
        {
            characterRoot = realModel;
            animator = modelAnimator;
            availableMotionMask = motionMask;
            CacheRenderers();
            initialized = false;
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
        }

        private void Awake() { CacheRenderers(); }

        private void CacheRenderers()
        {
            renderers = characterRoot != null
                ? characterRoot.GetComponentsInChildren<Renderer>(true)
                : Array.Empty<Renderer>();
        }

        public void SetVisible(bool visible)
        {
            if (!HasModel) return;
            foreach (Renderer r in renderers)
                if (r != null) r.enabled = visible;
        }

        public void PlayAction(PatientMotion motion, float seconds = 1f)
        {
            if (!IsReady) return;
            actionMotion = motion;
            lastActionUntil = Time.time + Mathf.Max(.15f, seconds);
            PlayMotion(motion);
        }

        private void Update()
        {
            if (!IsReady) return;

            if (!initialized && characterRoot != null)
            {
                restPosition = characterRoot.transform.localPosition;
                restRotation = characterRoot.transform.localRotation;
                initialized = true;
            }

            var monster = GetComponent<LastShiftMonster>();
            PatientMotion motion;

            if (monster != null && monster.IsAttacking)
                motion = PatientMotion.Attack;
            else if (Time.time < lastActionUntil)
                motion = actionMotion;
            else if (monster != null && monster.IsStunned)
                motion = PatientMotion.Frightened;
            else if (monster != null && monster.IsHunting)
            {
                motion = monster.CurrentSpeed > 1.28f ? PatientMotion.Run :
                    monster.CurrentSpeed > .1f ? PatientMotion.Walk : PatientMotion.Stare;
            }
            else motion = PatientMotion.Idle;

            PlayMotion(motion);
            // If the FBX contains no AnimationClips, give its REAL model a small
            // whole-body idle/chase response until artist-authored clips are supplied.
            // This does not fabricate anatomy or deform the mesh from a photograph.
            if (!HasAnimation) ApplyWholeBodyFallback(motion);
        }

        private void PlayMotion(PatientMotion requested)
        {
            if (!HasAnimation || animator == null) return;
            // Never use an unrelated walking clip as a scare/attack/vanish:
            // that was the reason for bizarre gestures and apparent disappearances.
            bool authentic = Has(requested) ||
                (requested == PatientMotion.Run && Has(PatientMotion.Walk));
            if (!authentic)
            {
                animator.speed = 0f; // hold the visible character's current pose
                return;
            }
            animator.speed = 1f;
            if (currentMotion == requested) return;
            string name = "Base Layer." + requested;
            int hash = Animator.StringToHash(name);
            if (!animator.HasState(0, hash)) return;
            currentMotion = requested;
            animator.CrossFadeInFixedTime(hash, .16f, 0);
        }

        private void ApplyWholeBodyFallback(PatientMotion motion)
        {
            if (!initialized || characterRoot == null) return;
            float speed = motion == PatientMotion.Run ? 10f :
                          motion == PatientMotion.Walk ? 5f : 1f;
            float amplitude = motion == PatientMotion.Run ? .038f :
                              motion == PatientMotion.Walk ? .017f : .008f;
            float bob = Mathf.Sin(Time.time * speed) * amplitude;
            float lean = motion == PatientMotion.Attack ? 10f :
                         motion == PatientMotion.Frightened ? -8f :
                         motion == PatientMotion.Run ? 4f : 0f;
            characterRoot.transform.localPosition = Vector3.Lerp(
                characterRoot.transform.localPosition,
                restPosition + Vector3.up * bob, Mathf.Clamp01(Time.deltaTime * 8f));
            characterRoot.transform.localRotation = Quaternion.Slerp(
                characterRoot.transform.localRotation,
                restRotation * Quaternion.Euler(lean, 0f, Mathf.Sin(Time.time * 1.5f) * 1.5f),
                Mathf.Clamp01(Time.deltaTime * 7f));
            if (!warnedNoAnimations)
            {
                Debug.LogWarning("[LastShift] The Patient REAL FBX is visible, but no original animation clips were found. " +
                    "Idle/Walk/Run/Attack currently use whole-body placeholders. Import real clips for skeletal motion.");
                warnedNoAnimations = true;
            }
        }
    }
}