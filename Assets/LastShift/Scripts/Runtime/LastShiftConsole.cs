using UnityEngine;
namespace LastShift {
public sealed class LastShiftConsole:MonoBehaviour,IVRUse {
 public void Use(LastShiftGame game){if(game!=null)game.TryRestorePower();}
}
}