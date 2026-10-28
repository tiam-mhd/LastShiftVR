using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace LastShift
{
    /// <summary>
    /// Preview the greybox with a keyboard and mouse when no headset is connected.
    /// Press F8 to override automatic selection, Escape to unlock mouse,
    /// and click Game view to relock.
    /// </summary>
    public sealed class DesktopPreview : MonoBehaviour
    {
        public Transform head;
        public Transform leftController;
        public Transform rightController;
        public float eyeHeight = 1.65f;
        public float mouseSensitivity = 0.10f;
        public bool forceDesktopPreview;
        private float pitch;
        private bool wasActive;

        public static bool IsPreviewActive { get; private set; }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f8Key.wasPressedThisFrame)
                forceDesktopPreview = !forceDesktopPreview;

            bool active = forceDesktopPreview || !XRSettings.isDeviceActive;
            IsPreviewActive = active;

            if (active != wasActive)
            {
                Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !active;
                wasActive = active;
            }
            if (!active || head == null) return;

            head.localPosition = new Vector3(0f, eyeHeight, 0f);

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 d = mouse.delta.ReadValue() * mouseSensitivity;
                transform.Rotate(0f, d.x, 0f, Space.Self);
                pitch = Mathf.Clamp(pitch - d.y, -85f, 85f);
                head.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }

            if (rightController != null)
            {
                rightController.position = head.position + head.TransformDirection(new Vector3(.23f, -.24f, .30f));
                rightController.rotation = head.rotation;
            }
            if (leftController != null)
            {
                leftController.position = head.position + head.TransformDirection(new Vector3(-.23f, -.24f, .30f));
                leftController.rotation = head.rotation;
            }
        }

        private void OnDisable()
        {
            IsPreviewActive = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
