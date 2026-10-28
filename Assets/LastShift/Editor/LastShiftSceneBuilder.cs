#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using LastShift;

public static class LastShiftSceneBuilder {
 const string root="Assets/LastShift";
 // Internal helper; no legacy/test menu entry.
 public static void Build(){
  if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  if(!AssetDatabase.IsValidFolder(root+"/Scenes"))AssetDatabase.CreateFolder(root,"Scenes");
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var floor=MaterialFor("Floor",new Color(.13f,.15f,.18f));
  var wall=MaterialFor("Wall",new Color(.21f,.22f,.25f));
  var metal=MaterialFor("Metal",new Color(.10f,.10f,.12f));
  var fuseMat=MaterialFor("Fuses",new Color(.85f,.49f,.08f));
  var danger=MaterialFor("Watcher",new Color(.03f,.04f,.06f));
  var green=MaterialFor("Unlocked",Color.green);
  var red=MaterialFor("Locked",new Color(.5f,.08f,.08f));
  var world=new GameObject("WARD 13").transform;
  Cube("Floor",new Vector3(0,-.15f,2),new Vector3(5.2f,.3f,28),floor,world);
  Cube("Ceiling",new Vector3(0,3.15f,2),new Vector3(5.2f,.3f,28),metal,world);
  Cube("Wall left",new Vector3(-2.6f,1.5f,2),new Vector3(.3f,3.2f,28),wall,world);
  Cube("Wall right",new Vector3(2.6f,1.5f,2),new Vector3(.3f,3.2f,28),wall,world);
  Cube("Back wall",new Vector3(0,1.5f,16),new Vector3(5.2f,3.2f,.3f),wall,world);
  Cube("Entry wall",new Vector3(0,1.5f,-12),new Vector3(5.2f,3.2f,.3f),wall,world);
  for(int i=0;i<7;i++){
   var lightGO=new GameObject("Emergency light "+i);
   lightGO.transform.position=new Vector3(0,2.8f,-8+i*3.6f);
   var l=lightGO.AddComponent<Light>();
   l.type=LightType.Point;l.color=(i%2==0)?Color.red:Color.cyan;
   l.intensity=.5f;l.range=5f;l.shadows=LightShadows.None;
  }
  foreach(float z in new float[]{-4.6f,2.2f,8.2f}){
   Cube("Stretcher",new Vector3(-1.65f,.55f,z),new Vector3(.75f,.2f,1.6f),metal,world);
  }
  var owner=new GameObject("GAME SYSTEMS");
  var game=owner.AddComponent<LastShiftGame>();
  var input=owner.AddComponent<PCVRInput>();
  var pointer=owner.AddComponent<PCVRHandInteractor>();
  pointer.input=input;pointer.game=game;
  var torch=owner.AddComponent<FlashlightBehaviour>();
  torch.input=input;game.flashlight=torch;
  Vector3[] places={
   new Vector3(-.9f,1.05f,-1.7f),
   new Vector3(1.05f,1.05f,5.1f),
   new Vector3(-.9f,1.05f,11)
  };
  for(int i=0;i<places.Length;i++){
   Cube("FUSE "+(i+1),places[i],new Vector3(.3f,.4f,.2f),fuseMat,world).AddComponent<LastShiftFuse>();
  }
  Cube("POWER BOX",new Vector3(1.6f,1.28f,-8.5f),new Vector3(.55f,.75f,.25f),green,world)
   .AddComponent<LastShiftConsole>();
  var door=Cube("EXIT DOOR",new Vector3(0,1.2f,-11.55f),new Vector3(1.6f,2.3f,.18f),red,world);
  var doorScript=door.AddComponent<LastShiftDoor>();
  doorScript.doorRenderer=door.GetComponent<Renderer>();
  doorScript.unlockedMaterial=green;
  game.exitDoor=doorScript;
  var board=new GameObject("Mission Board");
  board.transform.SetParent(world);
  board.transform.position=new Vector3(-2.15f,1.98f,-9.35f);
  var text=board.AddComponent<TextMesh>();
  text.fontSize=40;text.characterSize=.08f;text.color=Color.green;
  game.objectiveBoard=text;
  var monster=new GameObject("THE WATCHER");
  monster.transform.position=new Vector3(0,0,14.2f);
  game.monster=monster.AddComponent<LastShiftMonster>();
  var body=GameObject.CreatePrimitive(PrimitiveType.Capsule);
  body.transform.SetParent(monster.transform,false);
  body.transform.localPosition=new Vector3(0,1.3f,0);
  body.transform.localScale=new Vector3(.65f,1.45f,.65f);
  body.GetComponent<Renderer>().sharedMaterial=danger;
  monster.SetActive(false);
  var rig=new GameObject("PCVR Player Rig");
  rig.transform.position=new Vector3(0,0,-8);
  rig.AddComponent<CharacterController>();
  var tracked=rig.AddComponent<PCVRRig>();
  var move=rig.AddComponent<VRLocomotion>();
  var head=new GameObject("Tracked Head");
  head.tag="MainCamera";head.transform.SetParent(rig.transform,false);
  var cam=head.AddComponent<Camera>();
  head.AddComponent<AudioListener>();
  tracked.headCamera=cam;
  input.head=cam.transform;
  move.head=cam.transform;move.input=input;
  var left=new GameObject("Left Controller");
  left.transform.SetParent(rig.transform,false);
  var right=new GameObject("Right Controller");
  right.transform.SetParent(rig.transform,false);
  tracked.leftController=left.transform;
  tracked.rightController=right.transform;
  input.leftController=left.transform;
  input.rightController=right.transform;
  pointer.rigRoot=rig.transform;
  torch.rightHand=right.transform;
  var desktop = rig.AddComponent<DesktopPreview>();
  desktop.head=cam.transform;
  desktop.leftController=left.transform;
  desktop.rightController=right.transform;
  string path=root+"/Scenes/Ward13_Prototype.unity";
  EditorSceneManager.MarkSceneDirty(scene);
  EditorSceneManager.SaveScene(scene,path);
  var scenes=EditorBuildSettings.scenes.ToList();
  scenes.RemoveAll(s=>s.path==path);
  scenes.Insert(0,new EditorBuildSettingsScene(path,true));
  EditorBuildSettings.scenes=scenes.ToArray();
  Selection.activeGameObject=rig;
  AssetDatabase.SaveAssets();
  Debug.Log("[LastShift] PCVR prototype generated. Enable Standalone OpenXR and launch Quest Link.");
 }
 static GameObject Cube(string name,Vector3 p,Vector3 scale,Material m,Transform root){
  var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
  go.name=name;go.transform.SetParent(root);
  go.transform.position=p;go.transform.localScale=scale;
  go.GetComponent<Renderer>().sharedMaterial=m;return go;
 }
 static Material MaterialFor(string name,Color c){
  const string dir="Assets/LastShift/GeneratedMaterials";
  if(!AssetDatabase.IsValidFolder(dir))
   AssetDatabase.CreateFolder("Assets/LastShift","GeneratedMaterials");
  string assetPath=dir+"/"+name.Replace(" ","_")+".mat";
  var saved=AssetDatabase.LoadAssetAtPath<Material>(assetPath);
  if(saved!=null)return saved;
  var shader=Shader.Find("Universal Render Pipeline/Lit");
  if(shader==null)shader=Shader.Find("Standard");
  var m=new Material(shader){name=name};
  if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);
  if(m.HasProperty("_Color"))m.SetColor("_Color",c);
  AssetDatabase.CreateAsset(m,assetPath);
  return m;
 }
}
#endif
