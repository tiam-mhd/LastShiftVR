using UnityEngine;
using UnityEngine.XR;
namespace LastShift {
[RequireComponent(typeof(CharacterController))]
public sealed class VRLocomotion:MonoBehaviour{
 public PCVRInput input;
 public Transform head;
 public float speed=1.5f,snapAngle=30f;
 private CharacterController capsule;private bool ready=true;
 private void Awake(){capsule=GetComponent<CharacterController>();}
 private void Update(){
  if(head==null)return;
  Vector3 local=transform.InverseTransformPoint(head.position);
  float height=Mathf.Clamp(local.y,1.05f,2.1f);
  capsule.height=height;capsule.center=new Vector3(local.x,height*.5f,local.z);
  Vector2 stick=input!=null&&input.HasLeftHand?Vector2.zero:ControllerButtons.Stick(XRNode.LeftHand);
  Vector3 f=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;
  Vector3 rt=Vector3.ProjectOnPlane(head.right,Vector3.up).normalized;
  Vector3 delta=Vector3.ClampMagnitude(f*stick.y+rt*stick.x,1)*speed*Time.deltaTime;
  // Hard limit to the actual corridor and side wards; stop at the boundary
  // instead of showing the exterior Unity skybox.
  if(!WardSpatial.Contains(head.position+delta))
    delta=Vector3.zero;
  capsule.Move(delta+Vector3.down*.7f*Time.deltaTime);
  Vector2 turn=input!=null&&input.HasRightHand?Vector2.zero:ControllerButtons.Stick(XRNode.RightHand);
  if(Mathf.Abs(turn.x)<.25f)ready=true;
  if(ready&&Mathf.Abs(turn.x)>.8f){transform.RotateAround(head.position,Vector3.up,snapAngle*Mathf.Sign(turn.x));ready=false;}
 }
}
}