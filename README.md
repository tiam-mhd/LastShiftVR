# Ghost Girl Animated — integration update

The repository contains the actual **`shabannn.fbx` (~21.9 MB)** at
`Assets/LastShift/Characters/ThePatient/Model/shabannn.fbx`.
The final Unity scene builder now auto-detects that file without renaming it, instantiates its actual renderable 3D geometry, fits and grounds the model, and converts incompatible materials to URP/Lit while preserving any imported textures.

**Automatic animation mapping:** The importer queries actual `AnimationClip` subassets of this FBX and any optional animation files, maps matching names to the ten game states, and logs exact mapping/fallbacks in Unity Console. The actual embedded clip count and names were not independently inspectable from this environment; the Unity import report is authoritative.

**Visibility safety:** The real FBX can be visible and chase even if fewer than four clips exist. If animation data is absent, there is only a modest whole-body fallback; the game does not claim artist-quality skeletal walk/run/attack without the necessary clips. Environmental horror remains enabled.

**Scene rebuild is required** after copying the updated `Assets/LastShift` folder into the Unity project. In Unity use only:
`Tools > Last Shift > Build Final Ward 13 - Real Patient FBX`.

Creator attribution: **Ghost Girl Animated by Shaban Hafizsalim (shabaninteractive)**,
[Sketchfab model](https://sketchfab.com/3d-models/ghost-girl-animated-1091b3ee172f4951aaeafa2581f9de1d), CC BY 4.0.
See `Assets/LastShift/Characters/ThePatient/ATTRIBUTION.md`. Keep credit with distributed builds.

---

# LastShiftVR — Real 3D Character Integration (Windows PCVR)

Unity 6 + URP + OpenXR horror game prototype for Windows. Play through Meta Quest Link with Quest 2 / 3 / 3S, using controllers; keyboard/mouse desktop preview also works.

## No more fake 3D
The old image-extruded portrait mesh, 2D billboard, standalone scan-volume OBJ/MTL, and synthetic bone-pose animation scripts have been **removed**. The original character PNG remains only as an artist's reference, not an in-game renderer.

**A finished photorealistic character model is NOT bundled.** The real 3D asset and authentic animation clips must be supplied separately. The game will never spawn an invisible lethal enemy when required assets are missing.

## New pipeline

Expected files:
- `Assets/LastShift/Characters/ThePatient/Model/ThePatient.fbx` OR `ThePatient.prefab`
- `Assets/LastShift/Characters/ThePatient/Animations/`: clips named **Idle, Walk, Run, Attack** (required), plus Stare, Frightened, Scare, Appear, Vanish, Stunned (optional)
- `Assets/LastShift/Resources/GhostGirlReference.png`: reference image only

Unity model import requires:
- an actual rigged **SkinnedMeshRenderer**
- **Humanoid** rig with a valid Avatar
- appropriately retargeted actual AnimationClips
- correct URP/PBR materials and real 360-degree geometry

`PatientCharacterImporter.cs` searches the specified directory for an FBX or prefab, imports clips (including FBX subclips), generates `ThePatient.controller`, and binds the model and Animator to existing enemy AI. `PatientCharacterRig.cs` plays those named states via cross-fading, with no fabricated motions.

## Open in Unity
1. Install Unity 6, create a new Universal 3D project, and install Input System, XR Plug-in Management, OpenXR Plugin and XR Hands. Windows Build Support for Windows EXE.
2. Copy **only** `Assets/LastShift` from this repo to your Unity project; preserve its working `Packages` and `ProjectSettings`.
3. **Delete stale local files** if you previously installed the fake 3D version: `GhostGirlVisual.cs`, `PatientAnimationDriver.cs`, `ThePatient_ScanVolume.obj`, `ThePatient_ScanVolume.mtl`, and `GeneratedMeshes/ThePatient_3D_Rigged.asset`.
4. Add your real `ThePatient.fbx` and animation clips to the specified Character folders. Select FBX in Unity Inspector > Rig > Humanoid > Create From This Model > Apply. Import animation FBXs as Humanoid.
5. From Unity menu choose the **only final production action**:

   `Tools > Last Shift > Build Final Ward 13 - Real Patient FBX`

6. Open/Play the generated `Assets/LastShift/Scenes/Ward13_Prototype.unity`. If the model/clips are missing, the AI chase is disabled but the fuse gameplay and lighting still function.

### Controls
- Desktop: WASD, mouse, E interact, F flashlight, Esc free cursor.
- Meta PCVR: Quest Link with active OpenXR runtime; left stick move, right stick snap-turn, right trigger interact, right grip flashlight.
- Hand tracking on Windows EXE is NOT guaranteed by Quest Link. Controllers remain supported.

### Rig requirements
See `Characters/ThePatient/ART_BRIEF.md` for likeness, materials, full 360-degree topology, realistic hair/clothing and animation requirements.

## Status
This repository contains **source and import integration**, not an artist-made character FBX, animations or a prebuilt Windows EXE. Unity compilation and on-device testing are required. The single frontal reference image cannot guarantee a perfect unseen-side/back likeness.

[Persian instructions / راهنمای فارسی](README_FA.md)
