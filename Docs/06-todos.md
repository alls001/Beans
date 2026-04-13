# Todos

Use this file as the live queue. Keep active items concrete, with one next step each.

## Now

### WF-001 - Keep the workflow docs as the source of truth

Spec: [Codex Workflow](./00-codex-workflow.md)

Status: Ongoing

Next step: Use these docs in the next implementation session and keep them updated whenever the boss work changes the plan, tests, or required user actions.

### BOSS-S3 - Boss MVP Session 3: vulnerability and death flow

Spec: [First Boss Fight Plan](./05-first-boss-plan.md)

Status: In progress

Next step: Make the boss hittable only during the vulnerable window, hook boss health cleanly, and stop the loop / grow the plant when the boss dies.

### TEST-BOSS-S3 - Run Session 3 manual checks

Spec: [Testing Queue](./09-testing.md)

Status: Waiting on user

Next step: Run `BOSS-S3-001`, then run `BOSS-S3-002` if the boss can be killed in the same pass.

## Next

### BOSS-S2-VIS - Revisit boss shadow and collision polish

Spec: [Boss Visual Debug](./13-boss-visual-debug.md)

Status: Deferred until the boss damage milestone is playable

Next step: Resume from the diagnostic notes in `13-boss-visual-debug.md`, starting with targeted shadow-occlusion logging on the right side and the next collision polish pass.

## Later

### BOSS-S4 - Boss MVP Session 4: tuning and cleanup

Spec: [First Boss Fight Plan](./05-first-boss-plan.md)

Status: Later

Next step: Tune timings, damage, and hit count; clean scene wiring; refresh the manual test pass; and archive stable checks after boss damage and death are both reliable.

### BOSS-S4-ANIM - Align boss visuals with the project's animation pipeline

Spec: [First Boss Fight Plan](./05-first-boss-plan.md)

Status: Later

Next step: If production boss art needs animated states, keep `BossFootController` as the root state machine but move foot and shadow visuals to animator-driven child renderers, ideally following the existing `QuadSpriteAnimator` pattern used by enemies.

### TEST-EDITMODE - Consider light automated tests after the boss MVP stabilizes

Spec: [First Boss Fight Plan](./05-first-boss-plan.md)

Status: Later

Next step: Add EditMode tests only for pure helper logic or boss state logic if the manual boss loop is already stable.

## Done Recently

### WF-000 - Create the Codex workflow baseline

Done: Added `AGENTS.md`, the operating manual, the live todo list, the decisions log, the user-input queue, the testing queue, and the task-doc folder guide.

Date: 2026-04-12

### BOSS-S1 - Boss MVP Session 1: scene scaffold

Done: Added `LevelBoss` to build settings, routed `Level01 -> LevelBoss`, set `LevelBoss -> Level02`, and disabled the wave spawner in `LevelBoss`.

Date: 2026-04-12

### BOSS-S2 - Boss MVP Session 2: core stomp loop

Done: Added the boss foot controller, telegraph, stomp timing, and stomp damage, and wired placeholder foot and shadow sprites in `LevelBoss`.

Date: 2026-04-12
