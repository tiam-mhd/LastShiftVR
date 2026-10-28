# LastShiftVR — Ghost Girl Animated (Windows PCVR)

A Unity 6 / URP / OpenXR VR horror prototype, played on Windows with a Meta Quest 2/3/3S via Quest Link (also includes desktop keyboard/mouse preview).

## Character asset is present

**Actual imported model:** `Assets/LastShift/Characters/ThePatient/Model/shabannn.fbx` (21,853,936 bytes).

The final Unity builder auto-detects this file, instantiates its genuine renderable 3D geometry, fits it to human scale (roughly 1.72m), grounds it, repairs incompatible imported materials to URP/Lit, scans the actual AnimationClip subassets and maps matching clips to the game's 10 character states.

**Important:** The animation count and names inside this binary FBX could not be independently examined here. At scene-build time Unity prints `[LastShift / Model]` and `[LastShift / Anim]` reports listing the actual clips and whether each state uses a native match or a fallback. If only one source clip exists, mapping ten states does not create ten distinct artist-authored actions. If there are no imported clips, the 3D model still appears and moves with basic whole-body reactions while an appropriate rig is being prepared.

The fake portrait extruder, 2D cutout and scan-volume code remain removed.

## Set up and run on Windows

1. Close Unity and back up your project.
2. Download this GitHub repository's latest `main` ZIP. Verify the `shabannn.fbx` file is included.
3. Cleanly replace the Unity project's **entire `Assets/LastShift` folder**. Do NOT merge it over previous sources or overwrite the existing Unity project's Packages/ProjectSettings. `tools/Sync-LastShift.ps1` can back up and replace it.
4. Open your Unity 6 Universal 3D project with the required XR/Input packages. Wait for the FBX import and C# compilation; fix any red Console errors before continuing.
5. Run the **only** custom menu entry:
   `Tools > Last Shift > Build Final Ward 13 - Real Patient FBX`
6. In the scene Hierarchy expand `THE WATCHER`; there should be a child `The Patient - Ghost Girl Animated (real FBX)`. The parent is intentionally inactive in edit mode.
7. Press Play. An opening sighting is scheduled about 2.5 seconds after start. Collecting the second fuse starts the active hunt. The environmental scare system remains intact.

### Animation map

The director and AI request `Idle, Walk, Run, Stare, Frightened, Scare, Attack, Appear, Vanish, Stunned`. The importer searches the actual FBX's AnimationClips and any optional clips under `Characters/ThePatient/Animations`. Matching names are used first, then an available clip may be reused as a fallback. Without motion clips, the actual 3D mesh has only a minimal whole-body fallback.

### Player controls

Desktop: WASD move, mouse look, E interact, F flashlight. Meta PCVR: left stick move, right stick snap turn, right trigger interact, right grip flashlight. Windows standalone Quest Link hand tracking availability depends on runtime support; controller fallback is maintained.

### Troubleshooting

- No model: regenerate the scene; look for `[LastShift / Model]` and `[Ward13]` Console messages. The FBX must appear under `THE WATCHER`.
- Pink/missing textures: check `[LastShift / Materials]` warnings, then extract/relink FBX textures under its Unity Materials tab.
- Missing skeletal animations: check `[LastShift / Anim]` clip names and add compatible animation FBXs; re-run the final builder.
- Old DEBUG menus: your old `Assets/LastShift` folder was merged rather than replaced; use the clean sync script.
- The source has not been compiled or tested on actual Unity/Quest hardware in the authoring environment.

### License and credits

**Ghost Girl Animated** by **Shaban Hafizsalim (shabaninteractive)** on [Sketchfab](https://sketchfab.com/3d-models/ghost-girl-animated-1091b3ee172f4951aaeafa2581f9de1d), licensed **CC BY 4.0**. Retain the credit in a distributed game; see [ATTRIBUTION.md](Assets/LastShift/Characters/ThePatient/ATTRIBUTION.md).

[راهنمای فارسی](README_FA.md)
