# Boss MVP Behavior Description (Plain Language)

## What We Are Building (Minimum Version)

The first boss fight is a readable stomp loop built around a giant foot and a shadow telegraph:

1. The fight starts with the foot already visible on the ground.
   - The foot is large and unmistakable: about 2x to 3x the player size.
   - The foot is idle for a short time so the player can approach it.
   - The spawn point is fixed (not following the player) and can be slightly offset from the player's start.
   - The grounded foot should occupy space so the player is not standing inside it for free.
   - In the editor, we also want this idle pose previewed before Play when possible.
2. The foot rises to prepare a stomp.
   - It rises fast enough to feel like a wind-up, slow enough to read.
   - It moves upward along a readable path instead of popping out of view.
   - It goes offscreen while the shadow remains under the original foot location.
3. The shadow starts following the player once the foot is offscreen.
   - It moves from the foot's original spot toward the player instead of teleporting directly there.
   - It tracks the player for a short time.
4. The shadow locks in place shortly before impact.
   - The movement is smooth, not a teleport.
5. The shadow scales during the warning.
   - It becomes easier to read before impact instead of staying too faint.
6. The foot slams down onto the locked shadow.
7. If the player is still inside the shadow, the player takes damage.
8. If the player dodged out in time, nothing happens.
9. The foot pauses on the ground for a short punish window.
10. The next loop begins from where the foot landed unless a later design change says otherwise.
11. The loop repeats until the boss is defeated.

This is the smallest version that still feels like a boss: a visible threat, a clear telegraph, and a dodge decision.

## How It Should Feel (Player Experience)

The player should think:

- "A stomp is coming - I can see the shadow."
- "I have a short window to dodge."
- "If I dodge correctly, I'm safe and can counter."

The fight should feel:

- fair (shadow shows the exact impact area)
- readable (foot and shadow are always visible when needed)
- simple but tense (single repeated threat that still demands attention)

## Current Implementation Status

- The stomp timing and damage logic run.
- The floor lookup is now using the arena floor instead of the player.
- The main open work is visual continuity: preview before Play, exact idle placement, readable rise/drop speed, sole-to-floor contact, and shadow readability.

So we are still inside the **visual clarity and motion readability** phase of the MVP.

## Tuning Notes (Not Blocking)

- Foot size: should end up around 2x to 3x the player.
- Shadow size: roughly the player's width.
- Shadow layering: below player, above ground.
- Foot layering: above player.
- Idle and rise timing: long enough that the player can actually see the motion path.
