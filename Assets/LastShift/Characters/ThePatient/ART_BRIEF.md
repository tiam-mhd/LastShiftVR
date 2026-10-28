# The Patient — artist / Blender / character pipeline brief

**Reference**: `Assets/LastShift/Resources/GhostGirlReference.png`
The user wants the SAME character, not a different generic ghost.

Front-facing design: pale blue-gray skin, extremely long black hair strands masking the eyes,
distressed dirty white long-sleeved loose nightgown, torn cuffs and hem, bare feet, lean
silhouette. Preserve the visible facial and clothing details and overall proportions.

Produce a truly modeled character:
- Full 360-degree topology for face, scalp, torso, hands and feet; no photo extrusion or plane/card body
- Mesh hair locks or groom cards attached to a real scalp with believable volume
- Separately modeled flowing dress with intentional topology, UVs and PBR maps
- Retopology, UV maps, eye sockets and correct eyelid/face anatomy
- Humanoid skeleton; safe shoulder/elbow/knee deformation
- Original high-quality textures for front and inferred back/side
- Rigged and skinned FBX with actual animation clips
- Realistic movement: Idle, Walk, Run, Attack; optional Stare, Frightened, Scare, Appear, Vanish, Stunned
- Animation loops: Idle/Walk/Run, in-place locomotion
- Verify in Unity 6 URP from front, side and rear; preserve identity.

**Source limitation:** only one frontal image supplied. Side/rear detail requires artistic inference.
Do not claim a lossless exact match from all angles.

Export path: `Model/ThePatient.fbx`; motion clips into `Animations/`.
Unity model import: Humanoid, valid Avatar, SkinnedMeshRenderer, Windows PCVR compatible.
