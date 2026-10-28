# LastShiftVR — The Patient v0.3 (Windows PCVR / Meta Quest Link)

A Unity 6 **playable-source horror prototype**, not a pre-built or hardware-tested Windows EXE.

## What's new in v0.3

- The **actual character artwork supplied by the user** is stored at
  `Assets/LastShift/Resources/GhostGirlReference.png` and displayed during play
  as an alpha-transparent **2D camera-facing impostor**. This is **not** an animated,
  rigged humanoid 3D model. Replace `GhostGirlVisual` with a licensed rigged
  model for full 3D appearance and animation.
- New walkable Ward 12 (Treatment) and Ward 14 (Surgery) connected to the central corridor.
- Procedural aged concrete, stained tile and corroded steel texture generation
  during scene creation, along with surgical props and reactive atmosphere.
- Three highlighted pickup stations and a three-slot power panel.
- One use per slot, then one more for the breaker switch and escape door.
- First-fuse apparition, second-fuse chase, flashlight stun, 3D procedural ghost cues.
- On-screen desktop HUD and a built-in Unity editor scene validator.

## Install into your existing Unity 6 Universal 3D project

**BACK UP** your existing scene first: this generator overwrites the generated
`Ward13_Prototype.unity` scene, not your other authored scenes.

1. Update your local copy of **`Assets/LastShift`** from this repository. The
   other repo folders are NOT a complete URP project setup; do not overwrite
   your working project's `ProjectSettings` or `Packages`.
2. Check that Input System, XR Hands, XR Plugin Management, and OpenXR packages
   are installed in Unity. Use Assets > Refresh and fix red Console compile errors.
3. Select **Tools > Last Shift > Generate Ward 13 - The Patient v0.3**.
   Do NOT use the v0.2 generator for this level.
4. Select **Tools > Last Shift > Validate Ward 13 v0.3**.
5. Open Game view > Play. With no XR headset, F8 forces Desktop Preview.
6. Controls: WASD move, mouse look, E/Left Mouse interact, F torch, Esc cursor.
   On Quest Link: left/right sticks move/turn, right Trigger interact, right Grip torch.
7. Pick up three orange fuses (corridor, treatment, surgery). The ghost appears
   after the first and starts hunting after the second. Go back to POWER BOX,
   interact three times to insert the fuses, then once more to flip the breaker.
   Interact with EXIT to win.

## Runtime and build

Standalone Windows x86_64, OpenXR enabled for PC/Mac/Linux Standalone, Oculus
Touch Controller profile configured, Meta Quest Link set as active OpenXR
runtime, launch Link from the headset. Windows EXE hand tracking is **not
guaranteed** via Quest Link; controllers are the supported default.

## Repository state and important limitations

- This repo includes scripts, the user-supplied character reference texture,
  runtime systems and editor scene generator; scene + generated materials are
  built on your own machine via the above editor menu.
- No precompiled Windows build is provided, no verified Quest device test was
  possible here, and Unity editor compilation must still be checked locally.
- The enemy picture is a 2D stand-in in 3D space, not 3D rigging or animation.
- Fuse pickup uses point-and-click/Trigger into inventory; panel insertion uses
  sequential interaction. True physics grabbing/insertion is future work.
- The procedural textures and simple props are a step above the blockout,
  **not photorealistic scanned assets**.

[راهنمای جامع فارسی](README_FA.md)
