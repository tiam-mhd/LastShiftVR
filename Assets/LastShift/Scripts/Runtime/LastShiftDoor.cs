using UnityEngine;
namespace LastShift {
public sealed class LastShiftDoor:MonoBehaviour,IVRUse {
 public bool IsUnlocked{get;private set;}
 public Renderer doorRenderer;
 public Material unlockedMaterial;
 public void SetUnlocked(bool value){
  IsUnlocked=value;
  if(value&&doorRenderer!=null&&unlockedMaterial!=null)doorRenderer.sharedMaterial=unlockedMaterial;
 }
 public void Use(LastShiftGame game){if(game!=null)game.TryEscape();}
}
}