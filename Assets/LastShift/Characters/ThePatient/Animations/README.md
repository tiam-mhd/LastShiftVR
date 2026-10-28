# Required actual animation clips

Supply artist-authored / retargetable Unity AnimationClips named EXACTLY:
- `Idle` (required)
- `Walk` (required; in-place)
- `Run` (required; in-place)
- `Attack` (required)
Optional supported:
- `Stare`
- `Frightened`
- `Scare`
- `Appear`
- `Vanish`
- `Stunned`

Valid inputs: `.anim` clips or FBX files containing clips with exactly those names.
Set the individual animation FBX files' Rig to Humanoid, Avatar Definition 'Copy From Other Avatar' when retargeting.

Loop Idle/Walk/Run. Make Attack, Scare, Appear and Vanish non-looping.
The runtime uses Animator.CrossFadeInFixedTime to activate states.

No animations are fabricated in code. Without the four required clips
or a valid Humanoid avatar, **the enemy remains disabled** and cannot
kill the player while invisible.
