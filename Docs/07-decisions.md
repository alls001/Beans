# Decisions

Use one entry per durable decision. Update the existing entry when the decision changes unless history matters.

### DEC-2026-04-12-01 - Keep the repo instruction layer small

Status: Active

Decision: `AGENTS.md` stays short and durable. Workflow detail lives in [`00-codex-workflow.md`](./00-codex-workflow.md) and task-specific detail lives in task specs.

Why: This matches current Codex guidance and keeps repo context from turning into a noisy instruction dump.

Affected docs: `AGENTS.md`, [`00-codex-workflow.md`](./00-codex-workflow.md), task specs

### DEC-2026-04-12-02 - Default to manual Unity verification

Status: Active

Decision: Use [`09-testing.md`](./09-testing.md) as the default gameplay verification handoff.

Why: This environment has no Unity editor bridge, no active automated Unity test suite, and no reliable local CLI compile loop.

Affected docs and systems: gameplay tasks, [`08-user-input.md`](./08-user-input.md), [`09-testing.md`](./09-testing.md)

### DEC-2026-04-12-03 - Boss MVP structure

Status: Active

Decision: The boss MVP uses a dedicated `LevelBoss` scene, a shadow-only telegraph, one stomp attack, vulnerability after every stomp, and the existing plant exit after the boss dies. No boss health bar in the first pass.

Why: This fits the current scene-driven architecture and minimizes risky refactors.

Affected docs and systems: [`05-first-boss-plan.md`](./05-first-boss-plan.md), `Level01`, `LevelBoss`, boss scripts

### DEC-2026-04-12-04 - Use `mukstoo` as the default short-lived working branch

Status: Active

Decision: Stay on `mukstoo` for normal solo sessions. Use separate feature branches or worktrees only for risky or truly parallel work.

Why: This keeps the prototype lightweight while still leaving room for escalation later.

Affected docs and systems: `AGENTS.md`, [`00-codex-workflow.md`](./00-codex-workflow.md), local git workflow

### DEC-2026-04-12-05 - Insert LevelBoss between Level01 and Level02

Status: Active

Decision: Preserve the current scene flow and insert `LevelBoss` between `Level01` and `Level02`, keeping other scenes unchanged.

Why: It honors the existing progression while minimizing merge conflicts with other scene work.

Affected docs and systems: [`05-first-boss-plan.md`](./05-first-boss-plan.md), `Level01`, `LevelBoss`, `EditorBuildSettings.asset`

### DEC-2026-04-13-01 - Boss MVP visual behavior

Status: Active

Decision: The foot starts visible at ground level at a fixed spawn point, rises offscreen, then drops to the locked shadow. The shadow starts under the foot, follows the player briefly after the foot is offscreen, then locks shortly before impact, and scales during the warning window to stay readable.

Why: This matches the desired readability and player expectations from the boss behavior notes.

Affected docs and systems: `BossFootController`, `BossShadowTelegraph`, [`10-boss-behavior-description.md`](./10-boss-behavior-description.md)

### DEC-2026-04-13-02 - Boss visuals align to player scale and sorting

Status: Active

Decision: Scale the foot and shadow from the player's sprite bounds and use the player's sorting layer as the visual baseline, but derive boss floor contact from the arena floor collider instead of the player's Y position.

Why: Player-relative scale and sorting help readability, but player-relative Y positioning caused the boss to treat the player's body height as ground.

Affected docs and systems: `BossFootController`, `BossShadowTelegraph`, [`11-boss-implementation-plan.md`](./11-boss-implementation-plan.md)

### DEC-2026-04-13-03 - Ground lookup must ignore the player collider

Status: Active

Decision: Boss floor alignment should use the arena floor collider directly, or a fallback that excludes the player layer, instead of a generic downward raycast.

Why: The previous ground ray hit `Player` first, which made the boss use the player's body height as "ground."

Affected docs and systems: `BossFootController`, [`13-boss-visual-debug.md`](./13-boss-visual-debug.md)

### DEC-2026-04-13-04 - Boss motion should use explicit path distances, not sprite-bounds visibility

Status: Active

Decision: The foot rise/drop path should be driven by explicit grounded and offscreen target positions, and the next loop should restart from the landing point instead of snapping back to an older spawn point.

Why: Bounds-based offscreen checks and state-entry snaps were producing fake teleports and unreadable motion.

Affected docs and systems: `BossFootController`, [`13-boss-visual-debug.md`](./13-boss-visual-debug.md)

### DEC-2026-04-13-05 - Boss start spacing and shadow hierarchy should be stable in-editor

Status: Active

Decision: The boss idle spawn should be separated from the player using the actual player/foot visual width, and `BossShadow` should remain a stable root scene object instead of changing hierarchy at runtime.

Why: A tiny fixed spawn offset caused visual overlap with the player, and runtime hierarchy changes made the scene harder to debug.

Affected docs and systems: `BossFootController`, `BossShadowTelegraph`, `LevelBoss`, [`13-boss-visual-debug.md`](./13-boss-visual-debug.md)

### DEC-2026-04-13-06 - Shadow telegraph should be lifted along the floor normal

Status: Active

Decision: The boss telegraph should be rendered slightly above the floor along the detected ground normal, not just nudged by world-Y.

Why: A telegraph can be "correctly positioned" in scene space but still become unreadable in the Game view if it sits coplanar with the floor or uses a flipped ground normal.

Affected docs and systems: `BossShadowTelegraph`, `BossFootController`, `LevelBoss`, [`13-boss-visual-debug.md`](./13-boss-visual-debug.md)

### DEC-2026-04-13-07 - Serialized boss tuning in the scene must stay synced with script tuning

Status: Active

Decision: When `LevelBoss` already has serialized boss components, scene YAML values must be updated alongside code tuning instead of assuming script field defaults will win.

Why: Unity scene serialization was keeping older speed and rise values, which made the tested behavior lag behind the intended boss tuning.

Affected docs and systems: `LevelBoss`, `BossFootController`, [`09-testing.md`](./09-testing.md), [`13-boss-visual-debug.md`](./13-boss-visual-debug.md)

### DEC-2026-04-13-08 - Shadow telegraph should sort just below the player, not far below the scene

Status: Active

Decision: The boss shadow should render only slightly below the player sorting order instead of using a large negative offset.

Why: A large negative sorting offset made the telegraph easier to lose behind arena art on some parts of the map even when the logic was correct.

Affected docs and systems: `BossFootController`, `BossShadowTelegraph`, `LevelBoss`, [`13-boss-visual-debug.md`](./13-boss-visual-debug.md)

### DEC-2026-04-13-09 - Grounded foot push should use a fixed forward arena direction

Status: Active

Decision: When the grounded foot pushes the player, the push should be fast and biased toward screen-front/down instead of relying on raw collision normals or radial stomp knockback.

Why: Generic physics push and radial damage knockback felt too slow and too inconsistent for this boss presentation.

Affected docs and systems: `BossFootController`, boss collision feel, [`09-testing.md`](./09-testing.md)

### DEC-2026-04-13-10 - Boss runtime sorting should sit above decorative foreground overlays

Status: Active

Decision: In `LevelBoss`, the player, shadow, and foot should use a raised runtime sorting stack so the telegraph does not disappear behind decorative cloud sprites that already use the `player` sorting layer at higher orders.

Why: The scene contains foreground cloud sprites on the same sorting layer around order `20`, which can visually hide a correct shadow on one side of the arena.

Affected docs and systems: `BossFootController`, `LevelBoss`, [`13-boss-visual-debug.md`](./13-boss-visual-debug.md)

### DEC-2026-04-13-11 - Boss hurtbox should reuse the grounded foot collider and only become attack-targetable during the punish window

Status: Active

Decision: Reuse the boss foot's grounded body as the MVP hurtbox, but switch the boss onto the `Enemy` layer only during `Vulnerable` so player attacks can connect only in the punish window.

Why: This keeps the first boss pass simple and readable without adding a second authored hurtbox object before the core loop is proven.

Affected docs and systems: `BossFootController`, `HealthSystem`, `PlayerController3D`, [`09-testing.md`](./09-testing.md)

### DEC-2026-04-13-12 - Boss controller should stay authoritative while future animation remains a visual layer

Status: Active

Decision: Keep `BossFootController` as the gameplay and state authority and treat future boss animation as a child-visual concern rather than moving fight logic into animation timing.

Why: This matches the rest of the project's pattern, where gameplay scripts own behavior and animation components mostly respond to script-driven state changes.

Affected docs and systems: `BossFootController`, future boss art pipeline, `QuadSpriteAnimator`, [`06-todos.md`](./06-todos.md)
