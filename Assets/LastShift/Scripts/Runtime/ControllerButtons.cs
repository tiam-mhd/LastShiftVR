using UnityEngine;
using UnityEngine.XR;
namespace LastShift {
public static class ControllerButtons {
 public static bool RightTrigger() {
  var d=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
  return d.isValid && d.TryGetFeatureValue(CommonUsages.triggerButton,out bool v) && v;
 }
 public static bool RightGrip() {
  var d=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
  return d.isValid && d.TryGetFeatureValue(CommonUsages.gripButton,out bool v) && v;
 }
 public static Vector2 Stick(XRNode hand) {
  var d=InputDevices.GetDeviceAtXRNode(hand);
  return d.isValid && d.TryGetFeatureValue(CommonUsages.primary2DAxis,out Vector2 v) ? v : Vector2.zero;
 }
}
}
