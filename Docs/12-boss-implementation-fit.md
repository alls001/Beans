# Boss MVP Fit Check (Behavior vs Implementation)

## Desired Behavior Recap

- Foot starts visible on the ground and is large.
- Shadow starts under the foot, then moves toward the player, follows briefly, locks, and grows before impact.
- Foot stomps at the locked shadow.
- Player is damaged only if still inside the impact area.
- The loop restarts from the landing point instead of teleporting sideways.

## What Our Implementation Is Built To Do

- A state loop drives idle ground -> rise offscreen -> shadow chase -> shadow lock -> stomp -> vulnerable.
- Shadow position is grounded to the arena floor and tied to the locked stomp target.
- Foot position rises through an explicit path and drops on impact.
- Damage uses an overlap check at the target position.
- Sorting and scale are computed relative to the player.

## Why This Should Match the Desired Behavior

- **Shadow under the player**: we set its sorting order below the player and position it at the arena floor Y, not at player-body height.
- **Foot above the player**: we set its sorting order above the player and scale it relative to the player size.
- **Telegraph matches impact**: the stomp impact uses the same locked target the shadow ends on.
- **Visibility**: sprites are assigned to the same sorting layer as the player, and the rise/drop path is explicit instead of inferred from sprite bounds.

## Known Risk Areas

- If the player sprite is not the only SpriteRenderer under the player prefab, we might pick the wrong renderer for size or sorting.
- If the saved scene YAML is stale, the repo can still look wrong even when the live editor scene is better.
- Transparent padding in the foot art can still make sole-to-floor contact look wrong even when the ground lookup is correct.

## How We Will Confirm Fit

- Re-run `BOSS-S2-000`, `BOSS-S2-001`, and `BOSS-S2-002` after visual fixes.
- Verify shadow scale and foot scale feel reasonable and visible.
