# Boss MVP Implementation Plan (Conceptual)

This is the high-level plan for how we implement the minimal boss behavior in Unity 6, without code details.

## Core Pieces

1. **Boss Controller Script**
   - Owns the attack loop state machine.
   - Picks the target position at telegraph start.
   - Triggers the telegraph, stomp, and damage checks on timers.

2. **Shadow Telegraph**
   - A SpriteRenderer on a ground-aligned GameObject.
   - Exists under the foot in the idle pose and remains there while the foot rises.
   - Follows the player for a short chase window once the foot is offscreen.
   - Locks in place before impact.
   - Scales during the warning window and stays readable against the floor.
   - Is positioned from the arena floor collider with a tiny Y offset for visibility.

3. **Foot Visual**
   - A SpriteRenderer on a separate GameObject.
   - Starts visible at ground level.
   - Rises vertically from its spawn point (no horizontal tracking).
   - Moves through an explicit rise path to an offscreen target instead of relying on sprite bounds.
   - Drops onto the locked shadow position.
   - Scaled large relative to the player.
   - Rendered above the player and above the shadow.
   - Starts the next loop from its landing point instead of snapping back to an old start point.

4. **Stomp Damage**
   - Use a physics overlap centered on the stomp target.
   - The shadow is the warning and the impact zone.
   - If the player is inside the zone at impact, apply damage.

## Rendering / Visual Setup

- Use the player's sorting layer as the baseline.
- Shadow order: below player, above ground.
- Foot order: above player.
- Shadow and foot share the same locked X/Z target at impact.
- Ground Y is derived from the arena floor collider, not from the player's body height.
- If edit-time preview matters, the boss controller must also resolve the player and floor outside Play mode.

## Unity-Specific Notes

- Perspective camera + 2D sprites means scale and Y placement matter a lot.
- A "missing" sprite is often just below the ground plane or behind the camera.
- Sorting layer and order are more reliable than Z depth for 2D sprites in 3D scenes.

## Debugging Checklist (Before Writing New Code)

1. Select `BossFoot` and verify scale, sorting order, and pre-Play idle pose.
2. Select `BossShadow` and verify scale, sorting order, Y offset, and readability.
3. Verify sprite import type is `Sprite (2D and UI)` with alpha transparency.
4. Confirm the saved scene serialization matches the current boss script field list.

## Open Questions / Missing Info

- Whether stomp impact should add player pushback in the MVP or wait for the next pass.
