using UnityEngine;

namespace LastShift
{
    public sealed class PCVRHandInteractor : MonoBehaviour
    {
        public PCVRInput input;
        public LastShiftGame game;
        public Transform rigRoot;
        public float handReach = .35f, rayReach = 3.5f, teleportReach = 5f;
        public LayerMask interactionLayers = ~0;
        public string CurrentTarget { get; private set; }
        private bool previous;

        private void Update()
        {
            if (input == null || game == null || game.HasEnded)
            {
                CurrentTarget = null;
                return;
            }

            CurrentTarget = null;
            var source = RaySource();
            if (source != null && Physics.Raycast(source.position, source.forward, out var hit,
                 rayReach, interactionLayers, QueryTriggerInteraction.Ignore))
            {
                IVRUse item = FindUsable(hit.collider);
                if (item != null)
                {
                    CurrentTarget = item is LastShiftFuse ? "TAKE FUSE" :
                                    item is LastShiftConsole ? "ACTIVATE POWER BOX" :
                                    item is LastShiftDoor ? "OPEN EXIT" : "INTERACT";
                }
            }

            bool down = input.InteractPressed;
            if (down && !previous)
            {
                // Interaction is interaction only. Previous code teleported the
                // player on ANY failed hand pinch, sometimes outside Ward 13,
                // leaving a flat camera gradient instead of hospital geometry.
                if (input.HasRightHand) UseHand();
                else UseRay();
            }
            previous = down;
        }

        private Transform RaySource()
        {
            // Desktop crosshair must be aligned with the player's view, not the offset hand.
            if (DesktopPreview.IsPreviewActive && input != null) return input.head;
            return input != null && input.rightController != null
                ? input.rightController
                : input != null ? input.head : null;
        }

        private bool UseHand()
        {
            foreach (var col in Physics.OverlapSphere(input.RightIndexPosition,
                         handReach, interactionLayers, QueryTriggerInteraction.Ignore))
                if (Use(col)) return true;
            return false;
        }

        private bool UseRay()
        {
            var source = RaySource();
            return source != null &&
                   Physics.Raycast(source.position, source.forward, out var hit,
                       rayReach, interactionLayers, QueryTriggerInteraction.Ignore) &&
                   Use(hit.collider);
        }

        private static IVRUse FindUsable(Collider col)
        {
            if (col == null) return null;
            foreach (var b in col.GetComponentsInParent<MonoBehaviour>())
                if (b is IVRUse action) return action;
            return null;
        }
        private bool Use(Collider col)
        {
            var usable = FindUsable(col);
            if (usable == null) return false;
            usable.Use(game);
            return true;
        }

    }
}
