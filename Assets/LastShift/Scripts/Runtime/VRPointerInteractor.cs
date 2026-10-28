using UnityEngine;
namespace LastShift {
/// <summary>Legacy controller ray interactor: not added by the PCVR scene generator.</summary>
public sealed class VRPointerInteractor:MonoBehaviour {
 public LastShiftGame game;
 public PCVRInput input;
 public Transform rightHand;
 public float maxDistance=3.2f;
 private bool last;
 private void Update(){
  bool pressed=input!=null ? (!input.HasRightHand && input.InteractPressed) : ControllerButtons.RightTrigger();
  if(pressed&&!last){
   Transform source=rightHand!=null?rightHand:RigFinder.FindRightHand();
   if(source==null&&Camera.main!=null)source=Camera.main.transform;
   if(source!=null&&game!=null&&!game.HasEnded
    &&Physics.Raycast(source.position,source.forward,out var hit,maxDistance)){
     foreach(var b in hit.collider.GetComponentsInParent<MonoBehaviour>()){
      if(b is IVRUse action){action.Use(game);break;}
     }
   }
  }
  last=pressed;
 }
}
}