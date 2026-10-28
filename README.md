# LastShiftVR — Windows PCVR Horror Prototype

Windows PCVR build with Unity 6, OpenXR, C#, Meta Quest Link and optional XR Hands integration.

## Setup
1. Install Unity 6 (6000.0.66f2+ suggested) and Windows Build Support. Create a new **Universal 3D** project.
2. Copy `Assets/LastShift` from the accompanying source archive into the Unity project.
3. Install **XR Plug-in Management**, **OpenXR**, **XR Interaction Toolkit** and **XR Hands**.
4. In XR Plug-in Management > PC, Mac & Linux Standalone, enable OpenXR and Oculus Touch Controller profile.
5. Install Meta Quest Link on Windows, connect Quest 2/3/3S, set Link as active OpenXR runtime and start Link in headset.
6. Unity menu: Tools > Last Shift > Generate Prototype Ward 13.
7. Build Profiles > Windows x86_64 > Build; launch the exe with Meta Quest Link running.

## Controls
Controller: Left stick move, right stick snap turn; right trigger interact, right grip toggle torch.
Hands when supported by the PC OpenXR runtime: right pinch near an item interact; left pinch torch; right pinch while gazing at floor to teleport.

**Important:** Quest Link hand tracking support in Unity Editor does not guarantee availability in standalone Windows executables. Controller fallback is mandatory. Hardware testing is pending.

The full Unity source archive is shipped separately. This repository holds documentation and the core PCVR input integration files. Native Windows binary is not included.
