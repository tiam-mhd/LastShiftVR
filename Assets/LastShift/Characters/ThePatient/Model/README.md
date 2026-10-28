# Real The Patient Model

Put ONE real artist-authored character file here:
- `ThePatient.fbx` — **preferred**; or
- `ThePatient.prefab` — already configured prefab with a skinned character.

A single front-facing PNG is NOT a real model. Do not place 2D impostors, scan-volume OBJ files or extruded photo meshes here.

Unity Inspector for ThePatient.fbx:
1. Rig > Animation Type = Humanoid
2. Avatar Definition = Create From This Model
3. Check the Avatar Configuration (valid green mapping)
4. Model has **SkinnedMeshRenderer** with actual face, hair, dress, hands, feet and rear geometry
5. Materials use URP/Lit where practical
6. Set scale so the character stands approximately 1.5–1.9 meters high in Unity
7. Apply; no need to write custom C# scripts.

Character likeness reference: `Assets/LastShift/Resources/GhostGirlReference.png` (reference ONLY, not runtime art).

Run the single menu item:
Tools > Last Shift > Build Final Ward 13 - Real Patient FBX

The editor imports the FBX, binds Animator, and adds a generated Controller if the clips below are available.
