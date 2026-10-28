using UnityEngine;
namespace LastShift {
public static class RigFinder {
 public static Transform FindRightHand() {
  var cam=Camera.main;
  if(cam==null)return null;
  var root=cam.transform.root;
  foreach(var t in root.GetComponentsInChildren<Transform>(true)){
   var n=t.name.ToLowerInvariant().Replace(" ","");
   if(n.Contains("righthandanchor")||n.Contains("rightcontroller"))return t;
  }
  return null;
 }
}
}