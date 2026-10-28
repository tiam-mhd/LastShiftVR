using UnityEngine;
namespace LastShift {
public sealed class LastShiftMonster:MonoBehaviour {
 public float chaseSpeed=.95f,captureDistance=1.05f;
 private LastShiftGame game;private FlashlightBehaviour torch;private Transform head;
 private bool hunting;
 public void Wake(LastShiftGame owner,FlashlightBehaviour source){
  game=owner;torch=source;head=Camera.main!=null?Camera.main.transform:null;
  hunting=true;gameObject.SetActive(true);
 }
 public void StopChasing(){hunting=false;}
 private void Update(){
  if(!hunting||head==null||game==null||game.HasEnded)return;
  Vector3 target=new Vector3(head.position.x,transform.position.y,head.position.z);
  Vector3 direction=target-transform.position;float distance=direction.magnitude;
  if(distance<captureDistance){game.Caught();return;}
  if(torch==null||!torch.HoldsMonster(transform.position+Vector3.up*1.1f,transform))
   transform.position+=direction.normalized*chaseSpeed*Time.deltaTime;
  if(direction.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(
    transform.rotation,Quaternion.LookRotation(direction.normalized),Time.deltaTime*5f);
 }
}
}