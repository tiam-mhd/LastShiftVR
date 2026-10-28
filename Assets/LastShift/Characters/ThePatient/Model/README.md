# Imported model — Ghost Girl Animated

**Actual file already in this repository:** `shabannn.fbx` (about 21.9 MB).

Creator: Shaban Hafizsalim / shabaninteractive  
Original: https://sketchfab.com/3d-models/ghost-girl-animated-1091b3ee172f4951aaeafa2581f9de1d  
License: CC BY 4.0. Credit information: `../ATTRIBUTION.md`.

The final Ward 13 builder automatically locates `shabannn.fbx`; you do **not** need to rename it.
It instantiates the genuine MeshRenderer/SkinnedMeshRenderer meshes and fits them to about 1.72 m.

The builder scans the actual AnimationClip subassets imported from the FBX and any optional extra
clips in `../Animations/`, logs their real names, and maps them to Idle/Walk/Run/Attack/etc.
If a state is missing, it reuses the closest imported motion where possible. If no source
animations exist, the genuine 3D mesh remains visible and can pursue with minimal whole-body
motion; this is **not** a substitute for sculpted skeletal walking/running/attack animations.

If Unity warns about missing diffuse textures, extract/relink textures in the FBX Materials
tab and ensure URP Lit materials are assigned. Source file is not modified.
