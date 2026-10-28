using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using XRCommonUsages = UnityEngine.XR.CommonUsages;

namespace LastShift
{
    public static class ControllerButtons
    {
        public static bool RightTrigger()
        {
            if (DesktopPreview.IsPreviewActive)
            {
                var keyboard = Keyboard.current;
                var mouse = Mouse.current;
                return (keyboard != null && keyboard.eKey.isPressed) ||
                       (mouse != null && mouse.leftButton.isPressed);
            }
            var device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            return device.isValid &&
                device.TryGetFeatureValue(XRCommonUsages.triggerButton, out bool value) && value;
        }

        public static bool RightGrip()
        {
            if (DesktopPreview.IsPreviewActive)
                return Keyboard.current != null && Keyboard.current.fKey.isPressed;
            var device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            return device.isValid &&
                device.TryGetFeatureValue(XRCommonUsages.gripButton, out bool value) && value;
        }

        public static Vector2 Stick(XRNode hand)
        {
            if (DesktopPreview.IsPreviewActive)
            {
                var k = Keyboard.current;
                if (k == null) return Vector2.zero;
                if (hand == XRNode.LeftHand)
                    return new Vector2(
                        (k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0),
                        (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0));
                if (hand == XRNode.RightHand)
                    return new Vector2(
                        (k.rightArrowKey.isPressed ? 1 : 0) - (k.leftArrowKey.isPressed ? 1 : 0), 0f);
                return Vector2.zero;
            }
            var device = InputDevices.GetDeviceAtXRNode(hand);
            return device.isValid && device.TryGetFeatureValue(XRCommonUsages.primary2DAxis, out Vector2 v)
                ? v : Vector2.zero;
        }
    }
}
