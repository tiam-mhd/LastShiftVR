using UnityEngine;
using UnityEngine.XR;
namespace LastShift {
public sealed class PCVRRig:MonoBehaviour {
 public Camera headCamera;
 public Transform leftController,rightController;
 private void Update(){
  if(headCamera!=null)SetPose(InputDevices.GetDeviceAtXRNode(XRNode.Head),headCamera.transform);
  if(leftController!=null)SetPose(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand),leftController);
  if(rightController!=null)SetPose(InputDevices.GetDeviceAtXRNode(XRNode.RightHand),rightController);
 }
 private static void SetPose(InputDevice device,Transform node){
  if(!device.isValid)return;
  if(device.TryGetFeatureValue(CommonUsages.devicePosition,out Vector3 p))node.localPosition=p;
  if(device.TryGetFeatureValue(CommonUsages.deviceRotation,out Quaternion q))node.localRotation=q;
 }
}
}
