# Testing Queue

Use this file whenever a task needs Unity/editor/manual verification. Write results directly under the matching `User result:` field. If a code change makes an old result obsolete, clear the old result and move the test back to `Pending` or `Re-test`.

## Pending

### BOSS-S3-001 - Boss should only be hittable during the vulnerable window

Run after: Boss MVP Session 3

Scene: `Assets/_Scenes/LevelBoss.unity`

Setup:

1. Open `LevelBoss`.
2. Press Play.
3. Start the fight and get close enough to attack safely after a stomp.

Steps:

1. Try to attack the boss before the stomp lands.
2. Try again while the boss is supposed to be vulnerable, including from different sides around the grounded foot.
3. Try once more after the boss has retreated.

Expected result:

- Attacks outside the punish window do not damage the boss.
- Attacks during the vulnerable window do damage the boss.
- The grounded vulnerable foot can be hit from around it instead of only from the toe/front side.
- The vulnerable window feels visible enough to understand.

Failure clues:

- The boss takes damage all the time.
- The boss never takes damage.
- Hits register on invisible or fully retreated states.
- The Console never shows a boss damage message during the punish window.

If it fails, capture:

- A short description of when the hit did or did not register.
- Any Console messages related to health or damage.

User result:

### BOSS-S3-002 - Boss death should unlock the plant victory exit

Run after: Boss MVP Session 3

Scene: `Assets/_Scenes/LevelBoss.unity`

Setup:

1. Open `LevelBoss`.
2. Press Play.
3. Defeat the boss completely.

Steps:

1. Watch what happens when the boss reaches zero health.
2. Check whether the attack loop stops.
3. Check whether the plant grows or becomes usable.
4. Walk the player into the plant after victory.

Expected result:

- The boss stops attacking after death.
- The boss logs its defeat and disappears from the arena.
- The plant becomes the victory exit.
- Touching the plant loads the chosen post-boss scene.

Failure clues:

- The boss keeps attacking after death.
- The plant never grows or never becomes usable.
- The scene transition after victory is wrong or missing.

If it fails, capture:

- The Console errors.
- A screenshot of the plant and boss state after the supposed kill.

User result:

## Deferred Follow-up

### BOSS-S2-003 - Shadow readability and grounded foot push should feel correct across the arena

Run after: Boss MVP Session 2 follow-up fixes

Scene: `Assets/_Scenes/LevelBoss.unity`

Setup:

1. Open `LevelBoss`.
2. If the scene still looks unchanged after a code update, close and reopen the Unity Editor once.
3. Press Play.
4. Start moving through both the left side and right side of the arena.

Steps:

1. Let the boss begin a stomp cycle while you stay on the left half of the arena.
2. Confirm the shadow is visible in the Game view there.
3. Cross to the right half of the arena and repeat.
4. Confirm the shadow is still visible there before the stomp lands.
5. Let the grounded foot collide with the player on purpose once.
6. Check whether the player is pushed quickly toward screen-front/down instead of drifting in random directions.

Expected result:

- The telegraph is readable on both the left and right sides of the arena.
- The shadow appears dark enough to read against the ground.
- The foot still rises and drops correctly after the visual fix.
- Grounded foot collision feels smaller and cleaner than before.
- If the foot body pushes the player, the push is quick and mostly toward screen-front/down.

Failure clues:

- The shadow is only readable on one side of the arena.
- The shadow becomes invisible when the player moves to the right half.
- The foot body still lets the player overlap too deeply.
- The push is too slow or throws the player sideways/backward in inconsistent directions.

If it fails, capture:

- A screenshot from each side of the arena if the shadow behaves differently.
- A short note on how the foot push felt.

User result:
Deferred after user feedback on 2026-04-13.

Current state:

- Shadow still disappears on the right side of the arena.
- Grounded collision improved enough to stop blocking the MVP.
- Resume from `Docs/13-boss-visual-debug.md` after the boss-hit milestone is playable.

## Passed/Archived

### BOSS-S1-001 - Level01 clear should lead to LevelBoss

User result:
Result: Passed. `Level01` completed and loaded `LevelBoss` as expected.

### BOSS-S1-002 - LevelBoss should load as a boss scene, not a normal wave scene

User result:
Result: Passed. No enemy waves spawned in `LevelBoss`. Console logs were from `Level01`.

### BOSS-S2-000 - Boss idle preview should exist before Play

User result:
Result: Passed. Foot preview placement looked correct and the shadow appeared under it in the latest check.

### BOSS-S2-001 - Shadow telegraph should lock and grow before the stomp lands

User result:
Result: Passed overall. The stomp loop, target lock, and foot motion were working. Remaining follow-up tuning moved to `BOSS-S2-003` for the shadow visibility edge case and grounded push feel.

### BOSS-S2-002 - Stomp damage should reward dodging

User result:
Result: Passed. Staying in the stomp damaged the player, and moving out avoided the damage.
