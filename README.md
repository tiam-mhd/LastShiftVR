# LastShiftVR — Ward 13 (Unity Windows PCVR Prototype)

A **greybox horror prototype**, with three collectible fuses, a flashlight, a monster chase, a locked exit, and win/loss states. Built with Unity 6, OpenXR and C#. Designed to run on Windows and on Meta Quest 2/3/3S using Quest Link.

**No compiled EXE, full Unity URP project configuration, pre-generated scene, or finished art is supplied. Unity installation and hardware testing are still required.** The scene and materials are created in Unity using the editor menu.

For step-by-step instructions **in Persian**, see [README_FA.md](README_FA.md).

## Fast preview without VR hardware
1. Install Unity 6.1+ (Windows Build Support), then in Unity Hub create a new **Universal 3D (URP)** project.
2. Install Input System, XR Plug-in Management, Unity OpenXR and XR Hands using Package Manager. Accept input system restart prompts.
3. Download this repository and copy **only** its `Assets/LastShift` folder into your new project's `Assets` folder. Do not replace the new URP project's `ProjectSettings` and `Packages` with the incomplete source-only example configuration.
4. Wait for Unity to compile without red Console errors.
5. Choose **Tools > Last Shift > Generate Prototype Ward 13**; the generated scene is saved under `Assets/LastShift/Scenes/Ward13_Prototype.unity`.
6. Press **Play** in the Unity Game view. Without XR headset, DesktopPreview automatically activates. Press **F8** to force desktop mode if required.
7. **WASD** walk, **Mouse** look, **E / Left Click** interact, **F** toggle flashlight, **Left / Right Arrow** snap-turn, **Escape** unlock the cursor.

## Quest Link PCVR
1. Install and run Meta Quest Link for Windows, connect your Quest by USB Link or Air Link, and launch Link in the headset.
2. Set Meta Quest Link as the active OpenXR runtime.
3. Unity > Edit > Project Settings > XR Plug-in Management > **PC, Mac & Linux Standalone** > enable OpenXR. Under OpenXR > Interaction Profiles, select **Oculus Touch Controller Profile**.
4. Press Play with Quest Link active. Or choose Build Profiles > Windows x86_64 > Build and run the generated EXE with Link active.
5. Controllers: **left stick** move, **right stick** snap-turn, **right trigger** interact, **right grip** flashlight.
6. Optional hand tracking: pinch interactions only when XR Hands tracking data is actually provided. Official Quest Link Windows hand tracking support is limited to Unity Editor testing; **do not assume hand tracking works in a standalone EXE**.

## Repository layout
- `Assets/LastShift/Scripts/Runtime`: gameplay state, enemy, flashlight, input, rig, optional hand interactions, desktop preview.
- `Assets/LastShift/Editor/LastShiftSceneBuilder.cs`: builds a basic Ward 13 scene and stores its generated materials.
- `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt`: **reference only**, not a full URP template project.
- `README_FA.md`: complete Persian usage and troubleshooting guide.
- `.gitignore`: Unity ignores.

## Constraints
- The scene contains primitive cubes and a capsule enemy, not finished artwork.
- No physical hand meshes or realistic hand-grabbing are included.
- No compiled Windows build is attached. The project has not been compiled/tested in Unity in this environment.
- If you find a red Console error, copy its entire message for diagnosis.

## Official documentation
- https://developers.meta.com/vr/documentation/unity/unity-link/
- https://developers.meta.com/vr/documentation/unity/unity-handtracking-overview/
- https://developers.meta.com/vr/documentation/unity/unity-xr-plugin/
