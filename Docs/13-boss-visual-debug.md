# Boss Visual Debug (Temporary)

## Newest Findings

### Deferred after latest test pass

- `BOSS-S2-003` is still failing on one specific visual edge case: the shadow disappears on the right side of the arena.
- Grounded foot collision is improved enough to stop blocking the MVP, even though it is not perfect yet.
- This follow-up is intentionally parked until right after the boss-hit milestone so we stop bouncing between polish issues and can finish the first playable loop.

### Next diagnostic starting point for the shadow bug

- Add targeted logging for all visible `SpriteRenderer`s on the same sorting layer as the shadow that have an order greater than or equal to the shadow order while the player is on the right side.
- If sorting still looks inconclusive, temporarily render the telegraph through a different path for one debug pass:
  - dedicated sorting layer, or
  - a simple quad/material instead of the current sprite telegraph path
- If collision polish becomes urgent again, split the grounded foot into:
  - one cleaner push body, and
  - one separate hurtbox / stomp logic volume

### What we already tried on the shadow issue

- Raise the whole runtime sorting stack above the decorative foreground cloud orders.
- Lift the telegraph along the ground normal instead of only world-Y.
- Increase telegraph darkness and visibility.
- Keep the shadow root stable in the scene instead of reparenting it at runtime.

### What we learned from those tries

- The shadow logic is broadly correct now because the stomp loop still works and the telegraph can be seen on the left side.
- The remaining failure is much more likely to be a localized render-stack or occluder issue than a general boss-state or position bug.

### Current test state

- `BOSS-S2-000`: passed in the latest check
- `BOSS-S2-002`: passed in the latest check
- `BOSS-S2-001`: passed overall
- Remaining follow-up moved to `BOSS-S2-003`

### What is still wrong right now

- The shadow is now working overall, but it appears to be readable only on the left side of the arena and can still disappear on the right.
- The grounded foot collision is acceptable for now, but it can still be polished later if the feel becomes a problem again.

### Original leading hypothesis for the right-side shadow bug

- `LevelBoss` contains several decorative cloud sprites on the same `player` sorting layer at order `20`.
- The player was still effectively living around order `0`, with the shadow just below that.
- So the shadow could be logically correct, but still render behind that foreground cloud stack on some parts of the arena, especially the right side where those overlays are more present.

That hypothesis was strong enough to justify the sorting-stack fix, but it was not sufficient on its own, which is why the next pass should inspect specific right-side occluders and render-state interactions instead of assuming one global sorting bug.

## Why These Problems Were Happening

### 1. The scene was still overriding newer boss tuning

The script defaults had been increased, but `LevelBoss.unity` was still serializing older values for:

- rise speed
- drop speed
- rise distance
- chase duration
- chase speed

So Unity could keep playing an older-feeling version even after the code was updated.

### 2. The shadow was likely "correct" but still too close to the floor to read

The shadow could already be sitting in the right gameplay position while still being hard to see because:

- it was almost coplanar with the floor
- the floor normal could make the sprite face the wrong way
- the telegraph art is soft enough that tiny placement errors make it disappear into the ground

### 3. The collision work was only half done

The controller already had physical-foot fields, but the collider/body setup was not fully active in the motion/state flow yet.

That meant:

- the foot could look correct
- the stomp could damage correctly
- but the grounded foot still did not reliably occupy space as a body in the arena

## What We Changed In This Pass

### Shadow readability

- Lift the shadow along the detected ground normal instead of only adding world-Y.
- Stop using the temporary debug red and switch back to a darker telegraph.
- Raise the telegraph slightly more above the floor plane.
- Sort the telegraph just below the player instead of far below the player layer stack.
- Keep the ground-facing direction upright even if the sampled normal is flipped.
- Raise the whole boss/player/shadow runtime sorting stack above the decorative foreground cloud orders used in `LevelBoss`.

### Motion tuning

- Increase rise speed.
- Increase rise height a lot so the foot should fully clear the screen.
- Increase chase speed a lot.
- Increase chase duration so the foot has more time to stay above the player before lock.
- Keep the drop fast and threatening.

### Physical foot

- Finish the physical foot setup with a runtime `BoxCollider` and kinematic `Rigidbody`.
- Auto-size the collider from the foot sprite.
- Enable the collider during grounded and stomp-relevant states, and disable it while the foot is up/offscreen.
- Shrink the collider so the grounded body feels less exaggerated.
- Push colliding players quickly toward screen-front/down instead of relying on generic collision resolution.
- Stop using radial damage knockback for stomp hits and apply a boss-specific forward push instead.

### Scene sync

- Update the serialized `LevelBoss` boss values so Unity stops replaying stale speed and rise numbers.

## What Improved Before This Pass

- The floor lookup was fixed: the boss now uses the floor instead of treating the player as ground.
- The start pose stopped overlapping as badly as before.
- The foot rise/drop path became readable instead of pure pop-in/pop-out.
- The stomp damage rule already worked well enough to pass the dodge test.

## Why This Next Try Should Work Better

- The shadow is no longer only "mathematically correct"; it is being pushed visually above the floor plane on purpose.
- The rise/chase values in code and the scene are now aligned, so the tested boss should match the intended tuning.
- The foot now has an actual physical body path instead of only a visual sprite path.
- The telegraph should stop disappearing on the right if the cause really was foreground cloud sprites on the same sorting layer.

## Current Expected Behavior In `LevelBoss`

1. Before Play:
   - the foot previews on the ground
   - the shadow exists under it
2. At scene start:
   - the foot is grounded and idle for a readable punish window
   - the player should not begin inside the foot
3. Rise:
   - the foot rises fast enough to feel intentional
   - it should leave the visible Game frame instead of hovering half onscreen
4. Chase:
   - the shadow moves toward the player fast enough to catch up
   - the foot stays above that same target path
5. Lock:
   - the shadow stops and becomes the stomp point
6. Drop:
   - the foot slams down at the locked point
   - if the player stayed in the zone, damage happens
   - if the player escaped, no damage happens
7. Grounded recovery:
   - the foot stays on the landing spot for a punish window
   - the player should not be able to stand inside it for free

## Still Open After This Pass

- Confirm that the darker telegraph stays readable on both sides of the arena instead of only on the left.
- Confirm that the grounded foot push now feels smaller, faster, and consistently toward screen-front/down.
- Boss hurt feedback and vulnerable-only damage are still the next gameplay milestone, not part of this visual pass.
