using UnityEngine;
namespace LastShift {
public sealed class PCVRHandInteractor : MonoBehaviour {
 public PCVRInput input;
 public LastShiftGame game;
 public Transform rigRoot;
 public float handReach=.35f, rayReach=3.2f, teleportReach=5f;
 public LayerMask interactionLayers=~0;
 private bool previous;
 private void Update(){
  if(input==null||game==null||game.HasEnded)return;
  bool down=input.InteractPressed;
  if(down&&!previous){
   bool used=input.HasRightHand?UseHand():UseRay();
   if(!used&&input.HasRightHand)Teleport();
  }
  previous=down;
 }
 private bool UseHand(){
  foreach(var col in Physics.OverlapSphere(input.RightIndexPosition,handReach,interactionLayers,QueryTriggerInteraction.Ignore))
   if(Use(col))return true;
  return false;
 }
 private bool UseRay(){
  Transform src=input.rightController!=null?input.rightController:input.head;
  return src!=null&&Physics.Raycast(src.position,src.forward,out var hit,rayReach,interactionLayers,QueryTriggerInteraction.Ignore)&&Use(hit.collider);
 }
 private bool Use(Collider col){
  foreach(var b in col.GetComponentsInParent<MonoBehaviour>())
   if(b is IVRUse action){action.Use(game);return true;}
  return false;
 }
 private void Teleport(){
  if(input.head==null||rigRoot==null)return;
  if(!Physics.Raycast(input.head.position,input.head.forward,out var hit,teleportReach,interactionLayers,QueryTriggerInteraction.Ignore))return;
  if(Vector3.Dot(hit.normal,Vector3.up)<.88f || hit.point.y>rigRoot.position.y+.25f)return;
  Vector3 d=hit.point-input.head.position;
  rigRoot.position+=new Vector3(d.x,0,d.z);
 }
}
}
