using UnityEngine;
namespace LastShift {
public interface IVRUse { void Use(LastShiftGame game); }
public sealed class LastShiftFuse : MonoBehaviour, IVRUse {
 public bool WasTaken{get;private set;}
 public void Use(LastShiftGame game){ if(game!=null&&!WasTaken)game.FuseFound(this); }
 public void MarkTaken(){WasTaken=true;gameObject.SetActive(false);}
}
}