using UnityEngine;
namespace LastShift {
public sealed class FlashlightBehaviour:MonoBehaviour {
 public PCVRInput input;
 public Transform rightHand;
 [Range(0,100)]public float battery=100f;
 public float drainPerSecond=.22f,range=12f;
 public bool IsOn{get;private set;}=true;
 private bool previous;
 private Light lamp;
 private void Start(){
  if(rightHand==null)rightHand=input!=null?input.rightController:null;
  if(rightHand==null&&Camera.main!=null)rightHand=Camera.main.transform;
  if(rightHand==null)return;
  var obj=new GameObject("LastShift Flashlight");
  obj.transform.SetParent(rightHand,false);
  obj.transform.localPosition=new Vector3(0,-.02f,.1f);
  lamp=obj.AddComponent<Light>();lamp.type=LightType.Spot;
  lamp.spotAngle=47;lamp.range=range;lamp.intensity=3;
  lamp.shadows=LightShadows.None;
 }
 private void Update(){
  bool pressed=input!=null?input.TorchPressed:ControllerButtons.RightGrip();
  if(pressed&&!previous&&battery>0)IsOn=!IsOn;
  previous=pressed;
  if(IsOn){battery=Mathf.Max(0,battery-drainPerSecond*Time.deltaTime);if(battery<=0)IsOn=false;}
  if(lamp!=null)lamp.enabled=IsOn;
 }
 public bool HoldsMonster(Vector3 p,Transform root){
  if(!IsOn||lamp==null)return false;
  Vector3 delta=p-lamp.transform.position;
  if(delta.magnitude>range||delta.magnitude<.15f)return false;
  Vector3 direction=delta.normalized;
  if(Vector3.Dot(lamp.transform.forward,direction)<.93f)return false;
  return Physics.Raycast(lamp.transform.position,direction,out var hit,delta.magnitude+.5f)
    &&(hit.transform==root||hit.transform.IsChildOf(root));
 }
}
}