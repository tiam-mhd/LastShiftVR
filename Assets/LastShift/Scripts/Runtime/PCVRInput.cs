using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using System.Collections.Generic;
namespace LastShift {
public sealed class PCVRInput : MonoBehaviour {
 public Transform rightController,leftController,head;
 [Range(.008f,.06f)] public float pinchThreshold=.029f;
 public bool RightPinch{get;private set;}
 public bool LeftPinch{get;private set;}
 public bool HasRightHand{get;private set;}
 public bool HasLeftHand{get;private set;}
 public Vector3 RightIndexPosition{get;private set;}
 public Vector3 LeftIndexPosition{get;private set;}
 public bool InteractPressed => HasRightHand ? RightPinch : ControllerButtons.RightTrigger();
 public bool TorchPressed => HasLeftHand ? LeftPinch : ControllerButtons.RightGrip();
 private readonly List<XRHandSubsystem> systems=new List<XRHandSubsystem>();
 private float scanAt;
 private void Update(){
  if(Time.unscaledTime>=scanAt){SubsystemManager.GetSubsystems(systems);scanAt=Time.unscaledTime+1f;}
  HasRightHand=false;HasLeftHand=false;
  foreach(var s in systems){
   if(!s.running)continue;
   if(!HasRightHand && Read(s.rightHand,out bool rp,out Vector3 ri)){
    HasRightHand=true;RightPinch=rp;RightIndexPosition=ri;
   }
   if(!HasLeftHand && Read(s.leftHand,out bool lp,out Vector3 li)){
    HasLeftHand=true;LeftPinch=lp;LeftIndexPosition=li;
   }
  }
  if(!HasRightHand)RightPinch=false;
  if(!HasLeftHand)LeftPinch=false;
 }
 private bool Read(XRHand hand,out bool pinch,out Vector3 index){
  pinch=false;index=default;
  if(!hand.isTracked)return false;
  var a=hand.GetJoint(XRHandJointID.ThumbTip);
  var b=hand.GetJoint(XRHandJointID.IndexTip);
  if(!a.TryGetPose(out Pose ap)||!b.TryGetPose(out Pose bp))return false;
  Transform origin=head!=null?head.parent:null;
  index=origin!=null?origin.TransformPoint(bp.position):bp.position;
  Vector3 thumb=origin!=null?origin.TransformPoint(ap.position):ap.position;
  pinch=Vector3.Distance(index,thumb)<=pinchThreshold;
  return true;
 }
}
}
