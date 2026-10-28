using System;
using UnityEngine;

namespace LastShift
{
    // States map to the names of REAL imported animation clips. There are no synthetic
    // mesh generators, billboards or procedural limb rotations in this pipeline.
    public enum PatientMotion
    {
        Idle = 0, Walk = 1, Run = 2, Stare = 3, Frightened = 4,
        Scare = 5, Attack = 6, Appear = 7, Vanish = 8, Stunned = 9
    }

    [DisallowMultipleComponent]
    public sealed class PatientCharacterRig : MonoBehaviour
    {
        [Tooltip("Actual artist-created 3D model instance (FBX or prefab).")]
        public GameObject characterRoot;
        public Animator animator;
        [Tooltip("Set automatically by the editor importer for imported animation states.")]
        public int availableMotionMask;

        private Renderer[] renderers = Array.Empty<Renderer>();
        private PatientMotion currentMotion = (PatientMotion)(-1);
        private float lastActionUntil;
        private PatientMotion actionMotion;
        private bool warned;

        public bool HasModel
        {
            get
            {
                if (characterRoot == null || animator == null || !characterRoot.transform.IsChildOf(transform))
                    return false;
                if (renderers == null || renderers.Length == 0) CacheRenderers();
                return renderers.Length > 0;
            }
        }

        // No invisible enemy: without an actual mesh AND required animation clips,
        // the horror director won't activate the chase.
        public bool IsReady => HasModel &&
            characterRoot.GetComponentInChildren<SkinnedMeshRenderer>(true) != null &&
            animator.runtimeAnimatorController != null &&
            Has(PatientMotion.Idle) && Has(PatientMotion.Walk) &&
            Has(PatientMotion.Run) && Has(PatientMotion.Attack);

        public bool Has(PatientMotion motion) =>
            (availableMotionMask & (1 << (int)motion)) != 0;

        public void Configure(GameObject realModel, Animator modelAnimator, int motionMask)
        {
            characterRoot = realModel;
            animator = modelAnimator;
            availableMotionMask = motionMask;
            CacheRenderers();
        }

        private void Awake()
        {
            CacheRenderers();
            if (HasModel && animator != null)
            {
                animator.applyRootMotion = false; // The AI controls world-space motion.
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
        }

        private void CacheRenderers()
        {
            renderers = characterRoot != null
                ? characterRoot.GetComponentsInChildren<Renderer>(true)
                : Array.Empty<Renderer>();
        }

        public void SetVisible(bool visible)
        {
            if (characterRoot == null) return;
            if (renderers == null || renderers.Length == 0) CacheRenderers();
            foreach (var r in renderers)
                if (r != null) r.enabled = visible;
        }

        public void PlayAction(PatientMotion motion, float seconds = 1f)
        {
            if (!IsReady) return;
            actionMotion = motion;
            lastActionUntil = Time.time + Mathf.Max(.1f, seconds);
            PlayMotion(motion);
        }

        private void Update()
        {
            if (!IsReady)
            {
                if (!warned && Application.isPlaying)
                {
                    Debug.LogWarning("[LastShift] The Patient requires a real model and Idle/Walk/Run/Attack animations. The invisible enemy has been disabled.");
                    warned = true;
                }
                return;
            }

            if (Time.time < lastActionUntil)
            {
                PlayMotion(actionMotion);
                return;
            }

            var monster = GetComponent<LastShiftMonster>();
            if (monster == null || !monster.IsHunting)
            {
                PlayMotion(PatientMotion.Idle);
                return;
            }
            if (monster.IsAttacking) PlayMotion(PatientMotion.Attack);
            else if (monster.IsStunned) PlayMotion(PatientMotion.Frightened);
            else if (monster.CurrentSpeed > 1.28f) PlayMotion(PatientMotion.Run);
            else if (monster.CurrentSpeed > .12f) PlayMotion(PatientMotion.Walk);
            else PlayMotion(PatientMotion.Stare);
        }

        private void PlayMotion(PatientMotion requested)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            PatientMotion motion = Has(requested) ? requested : PatientMotion.Idle;
            if (currentMotion == motion) return;
            currentMotion = motion;
            animator.CrossFadeInFixedTime(motion.ToString(), .16f, 0);
        }
    }
}
