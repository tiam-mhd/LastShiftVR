# Animation bindings for Ghost Girl Animated

The importer first reads **actual AnimationClip subassets of `Model/shabannn.fbx`**.
It also scans this folder for `.anim` and `.fbx` animation files.

State mappings:
- Idle: idle / breathe / stand / default / Take 001
- Walk: walk / creep / move / locomotion
- Run: run / sprint / chase
- Stare: stare / look / watch
- Frightened/Stunned: frightened / recoil / flinch / hit
- Scare: scare / scream / jump
- Attack: attack / strike / grab / lunge
- Appear/Vanish: appear / spawn / disappear / fade

These are aliases, not promises that such clips exist in the downloaded asset.
The Unity Console logs each real imported clip and which state used a native match
versus a fallback. If you want distinct realistic walking, running, attacking
and disappearing motions, supply authentic retarget-compatible `.anim` or FBX clips
with these names. If source clips are absent, the real 3D model still remains visible.

Motion is kept in-place where appropriate because LastShiftMonster controls world translation.
