using UnityEngine;
namespace LastShift {
public enum GamePhase { Searching,Hunted,Powered,Won,Lost }
public sealed class LastShiftGame:MonoBehaviour {
 public TextMesh objectiveBoard;
 public LastShiftMonster monster;
 public LastShiftDoor exitDoor;
 public FlashlightBehaviour flashlight;
 public GamePhase Phase{get;private set;}=GamePhase.Searching;
 public int CollectedFuses{get;private set;}
 public const int RequiredFuses=3;
 public bool PowerRestored{get;private set;}
 public bool HasEnded=>Phase==GamePhase.Won||Phase==GamePhase.Lost;
 private void Start(){Board("Find three fuses. Trigger or pinch to interact.");}
 public void FuseFound(LastShiftFuse item){
  if(HasEnded||item==null||item.WasTaken)return;
  item.MarkTaken();CollectedFuses++;
  if(CollectedFuses==2&&monster!=null){
   Phase=GamePhase.Hunted;monster.Wake(this,flashlight);
   Board("IT IS AWAKE. Light can slow it. Find the final fuse.");return;
  }
  Board(CollectedFuses==RequiredFuses?"Return to the POWER BOX.":"Fuse found: "+CollectedFuses+"/3");
 }
 public void TryRestorePower(){
  if(HasEnded)return;
  if(CollectedFuses<RequiredFuses){Board("Power box needs "+(RequiredFuses-CollectedFuses)+" fuses.");return;}
  PowerRestored=true;Phase=GamePhase.Powered;
  if(exitDoor!=null)exitDoor.SetUnlocked(true);
  Board("Power restored! Reach the EXIT DOOR.");
 }
 public void TryEscape(){
  if(HasEnded)return;
  if(!PowerRestored){Board("Door locked. Restore power.");return;}
  Phase=GamePhase.Won;if(monster!=null)monster.StopChasing();
  Board("YOU ESCAPED — YOU WIN!");
 }
 public void Caught(){
  if(HasEnded)return;
  Phase=GamePhase.Lost;if(monster!=null)monster.StopChasing();
  Board("THE WATCHER CAUGHT YOU — GAME OVER");
 }
 private void Board(string message){
  string text="LAST SHIFT / WARD 13\nFUSES "+CollectedFuses+"/3\nPOWER "+(PowerRestored?"ON":"OFF")+"\n\n"+message;
  if(objectiveBoard!=null)objectiveBoard.text=text;
  Debug.Log("[LastShift] "+message);
 }
}
}