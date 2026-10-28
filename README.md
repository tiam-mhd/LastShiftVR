# LastShiftVR — The Patient 3D / Windows PCVR

A Unity 6 + URP + OpenXR survival-horror prototype for Windows PCVR, using Meta Quest 2/3/3S through Quest Link. Also includes keyboard/mouse desktop preview.

## Latest release source: The Patient 3D
The original user-supplied character image is the front texture of a **volumetric Unity skinned mesh** with **seven articulated bones**. Procedural C# animations cover idle, stare, walking, running, recoil from light, scare, attack, appearing and vanishing.

**Important model limitation:** The original reference supplies only a frontal view. Front texture details are retained; geometry depth, sides and the reverse are inferred. This is a playable prototype reconstruction, not a lossless 360-degree realistic sculpt or a mocap animation package.

The repository also includes the standalone **static** model `Assets/LastShift/Models/ThePatient_ScanVolume.obj` and its `.mtl`, referencing the original `Resources/GhostGirlReference.png` texture. The **animated** mesh is generated and serialized by Unity as `Assets/LastShift/GeneratedMeshes/ThePatient_3D_Rigged.asset` when you build the scene.

## Setup on Windows

1. Install Unity 6 (URP/Universal 3D project, Windows Build Support).
2. From Unity Registry install **Input System**, **XR Plug-in Management**, **OpenXR Plugin** and **XR Hands**.
3. Download GitHub ZIP and copy **only** `Assets/LastShift` to an existing Unity Universal 3D project's `Assets`. Keep that project's original `Packages` and `ProjectSettings`.
4. Wait for error-free C# compilation after `Assets > Refresh`.
5. In Unity select the **one and only** production menu:
   `Tools > Last Shift > Build Final Ward 13 - The Patient 3D`
6. Open `Assets/LastShift/Scenes/Ward13_Prototype.unity` (opened by builder) and press Play.

**Desktop:** WASD move, mouse look, E interact, F flashlight. **PCVR Quest Link:** left stick move, right stick snap-turn, right trigger interact, right grip flashlight.

The 3D character appears during scripted events and chases the player. The rig is saved in the generated scene and can be inspected by temporarily enabling `THE WATCHER` in the Hierarchy (when not in Play). The character is hidden initially by design.

## Game loop
Collect three fuses, escape the Patient, insert them into the power cabinet one at a time, flip the breaker, then open the exit. Fuse pickups flicker the electrical system, each inserted fuse powers a circuit and the breaker restores ward illumination.

## Only final menus
Legacy generators and debug/test/validation entries are hidden or removed from `Tools > Last Shift`. There is one final scene generation entry.

## Caveats
- This is a prototype. Procedural bone poses do not match the quality of artist-authored animations.
- Existing environment art is procedural and not photorealistic.
- No prebuilt .exe is included. Code has not been compiled or tested on an actual Unity + Quest hardware setup in the authoring environment.
- Windows standalone Quest Link hand tracking is not guaranteed; controllers remain supported.
- Unity can regenerate the scene and locally generated 3D mesh; **back up manual scene changes** before re-running the generator.

[Persian setup and full notes](README_FA.md)
