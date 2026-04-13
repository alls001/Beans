# First Boss Fight Plan

> Status note (2026-04-13): this file is still the high-level MVP scope doc, but the current source of truth for the active boss loop is now [`10-boss-behavior-description.md`](./10-boss-behavior-description.md), [`11-boss-implementation-plan.md`](./11-boss-implementation-plan.md), [`12-boss-implementation-fit.md`](./12-boss-implementation-fit.md), and [`13-boss-visual-debug.md`](./13-boss-visual-debug.md). Use this file for milestone scope and repo-touch points, not the latest visual timing details.

## Project Facts That Matter For This Task

I re-checked the project specifically with the boss fight in mind. These are the facts that should drive the decision:

- The game is currently very scene-driven. The usual flow is `waves -> plant grows -> player touches plant -> next scene`.
- `LevelBoss.unity` already exists, but right now it is effectively a clone of `Level02.unity` / `LevelTesttiro player.unity`, not a real boss scene yet.
- `LevelBoss` is not currently in `EditorBuildSettings.asset`, so it is not part of the active build flow.
- `RandomSpawnerEnemy` is currently responsible for ending a normal level and calling `PlantGrowthController.GrowPlant()`.
- `PlayerController3D` only damages a `HealthSystem` found on the exact collider hit by the overlap check, not on the collider's parent.
- `HealthSystem` currently knows how to notify `PlayerController3D` and `EnemyBase`, but not a future boss controller.
- The player's melee attacks target the `Enemy` layer, so the boss hurtbox must be hittable through that same layer or we must change the player hit logic.
- `PlantGrowthController` is already a good victory gate for the boss scene too. We do not need a brand-new exit system if we keep the boss in a separate scene.
- `StageAnnouncementUI` already supports custom text through `ShowAnnouncement(...)`, so a simple boss intro can reuse the existing UI system.
- There are no obvious existing boss-foot art assets, shadow telegraph assets, boss UI assets, or boss-specific scripts yet.

## My Current Recommendation

For the first implementation, I recommend this route:

- Use a dedicated `LevelBoss` scene.
- Reuse the current arena / environment instead of building new map content.
- Route `Level01 -> LevelBoss`.
- Let the boss scene be a self-contained fight with no regular enemy waves.
- After the boss dies, reuse the plant climb to go either to `Level02` or to `Creditos`, depending on whether the boss is the end of the current demo.

I recommend this boss behavior for the MVP:

- The foot starts visible on the ground.
- The foot idles briefly so the player can read it and attack it.
- The foot rises upward and leaves the screen.
- The shadow starts under the foot, then follows the player, then locks.
- The shadow grows during the warning window.
- The foot stomps at the locked shadow point.
- The stomp deals damage if the player is still inside the impact zone.
- After each stomp, the foot stays briefly vulnerable on the ground.
- The next loop begins from where the foot landed.

I recommend these initial design decisions for the first pass:

- Separate boss scene: yes.
- Boss starts vulnerable before first stomp: no.
- Boss vulnerable after every stomp: yes, for MVP.
- Boss vulnerable only on miss: later.
- Visible foot at top of the screen while tracking: later.
- Boss health bar: not required for MVP.
- Fancy appearance animation: not required for MVP.

Why this is the best first version:

- It fits the current scene-based architecture.
- It avoids rewriting the wave/plant flow in `Level01`.
- It gives you a fully playable boss loop quickly.
- It leaves room to upgrade the fight later without throwing away the first implementation.

## Bare Minimum

## Goal

Have a complete and playable first boss fight as fast and as cleanly as possible:

- player enters boss scene
- boss attacks
- player can dodge
- boss can be hit
- boss can die
- player can leave the scene after winning

## Recommended Bare-Minimum Design

- Dedicated `LevelBoss` scene.
- No normal enemy waves in that scene.
- One boss attack only: stomp.
- One telegraph only: growing shadow on the floor.
- One punish rule only: boss becomes vulnerable after each stomp.
- One simple boss state loop:
  - `IdleGround`
  - `RiseOffscreen`
  - `ShadowChase`
  - `ShadowLock`
  - `Stomp`
  - `Vulnerable`
  - repeat
- No boss health bar yet.
- No special intro cutscene yet.
- No "miss-only" logic yet.
- No multi-attack series yet.

This is the cleanest MVP because it teaches the fight immediately and does not depend on advanced timing rules.

## Bare-Minimum Implementation Plan

### 1. Use a separate boss scene

The fastest route is:

- add `LevelBoss` to build settings
- change `Level01` so its plant leads to `LevelBoss`
- turn `LevelBoss` into the actual boss arena

Why this is easier than "boss spawns in the same scene after the waves":

- no need to interrupt or rewrite `RandomSpawnerEnemy` completion logic
- no need to hide the plant until after the boss
- no need to build a combined "waves + boss" phase manager yet

### 2. Remove normal wave logic from the boss scene

In `LevelBoss`:

- disable or remove the normal `EnemySpawner`
- keep the player, camera, pause menu, UI, plant, and basic arena
- set the plant's `nextSceneName` to the scene you want after the boss

Recommended post-boss routes:

- fastest demo route: `LevelBoss -> Creditos`
- fuller route: `LevelBoss -> Level02`

### 3. Create a dedicated boss controller

Create a new script such as:

- `Assets/_Scripts/BossFootController.cs`

This controller should own:

- boss state machine
- stomp timing
- stomp target point
- damage window
- vulnerability window
- boss death flow

For the first pass, do not build a generic "all future bosses" framework. A first-boss-specific controller is better for speed and lower risk.

### 4. Create the telegraph

Create one ground telegraph object:

- a quad, plane, or sprite-based object
- positioned on the arena floor
- normally hidden
- scaled up over time during the telegraph phase

For the first pass, the target should lock early:

- pick the player's position when the telegraph starts
- place the shadow there
- let it grow in place
- stomp that same locked point

This is important for fairness. If the shadow tracks the player until the final instant, the stomp becomes cheap instead of readable.

### 5. Make the stomp deal damage

At stomp impact:

- use a simple overlap check on the `Player` layer
- start with `Physics.OverlapSphere(...)`
- later, if needed, switch to `OverlapBox(...)` to better match the foot shape

Recommended first tuning:

- stomp damage: `3`
- player max health is currently `6`, so this feels threatening without making testing miserable

### 6. Make the boss hittable

This is one of the most important technical points in the whole plan.

Right now:

- `PlayerController3D.PerformDamageCheck()` only checks `c.GetComponent<HealthSystem>()`
- it does not use `GetComponentInParent<HealthSystem>()`

That means the easiest boss hurtbox setup is:

- put the boss hurt collider on the same object that has the boss `HealthSystem`
- put that collider on the `Enemy` layer
- enable it only during the vulnerable state

Alternative, slightly cleaner:

- update player hit detection so it can find `HealthSystem` on parent objects too

Either approach works. The first one is simpler for the MVP if the boss is just one foot object.

### 7. Make the boss react to damage and death

Because `HealthSystem` currently only knows player/enemy callbacks, the boss path needs one of these:

- bare-minimum route: teach `HealthSystem` how to notify `BossFootController`
- cleaner route: refactor those callbacks to an interface that player, enemies, and boss can all implement

For the MVP, the special-case boss callback is acceptable.

### 8. End the fight cleanly

When the boss dies:

- stop the attack loop
- hide or disable the boss
- optionally play a short delay
- call `PlantGrowthController.GrowPlant()`

That lets the project reuse the same victory structure it already has.

## Files Likely Involved In The Bare-Minimum Pass

Existing files likely to change:

- `Assets/_Scripts/PlayerController3D.cs`
- `Assets/_Scripts/HealthSystem.cs`
- `Assets/_Scripts/PlantGrowthController.cs` or only scene wiring, depending on final flow
- `Assets/_Scenes/Level01.unity`
- `Assets/_Scenes/LevelBoss.unity`
- `ProjectSettings/EditorBuildSettings.asset`

New files likely to be created:

- `Assets/_Scripts/BossFootController.cs`
- `Assets/_Scripts/BossShadowTelegraph.cs` or similar helper script

Files probably not worth touching in the MVP:

- `RandomSpawnerEnemy.cs`
- `EnemyBase.cs`
- `EnemyMelee.cs`
- `EnemyMeleeFireball.cs`

## What You Must Provide For The Bare Minimum

If you want the boss to look like a real foot instead of a programmer placeholder, I need at least:

- 1 foot image
- 1 shadow / warning image

### Foot asset

Recommended minimum:

- format: `.png`
- background: transparent
- resolution: `1024x1024` minimum, `2048x2048` preferred
- perspective: underside / sole view, or at least a view that makes sense when the foot lands from above
- consistent centered canvas with some transparent padding around it

If later you want simple animation:

- 4 to 8 frames is enough for a first stomp
- same canvas size for every frame
- either separate PNGs or one sprite sheet

### Shadow telegraph asset

Recommended minimum:

- format: `.png`
- background: transparent
- resolution: `512x512` minimum, `1024x1024` preferred
- shape: soft black or dark gray ellipse/circle
- edges: soft fade preferred, not a hard cartoon ring

If you do not have this yet, we can prototype with a simple placeholder circle/quad.

### Optional but useful even in MVP

- stomp sound effect: `.wav` or `.ogg`, `44.1 kHz` or `48 kHz`
- a simple dust impact sprite or particle texture

### Unity-side setup I will likely still need from you or from a Unity-editor pass

Because code alone is not the best tool for all Unity serialized wiring, I will likely need someone to do or confirm these editor steps:

- import the foot PNG
- import the shadow PNG
- create or place the boss GameObject in `LevelBoss`
- assign script references in the Inspector
- size the boss collider / stomp radius visually
- confirm the plant next-scene destination

If you want, we can later turn this into a precise step-by-step setup checklist.

## Bare-Minimum Suggestions And Doubts

- The first stomp should be slower than later stomps, even in MVP.
- The boss should not start stunned. It is better if the player learns the rule by dodging the first stomp.
- The shadow should lock before impact. "Tracking all the way down" is unfair.
- Start without a boss health bar. It is not required to validate the mechanic.
- Start without "only vulnerable on miss". That rule is better once the base loop already feels good.
- Start with a short vulnerable window, around `0.6s` to `0.9s`.
- Start with boss HP around `18` to `24` if you use the current raw player damage values.

## Easy To Implement And Worth Doing Soon After

This section is "above the minimum" but still relatively fast and realistic for this project.

## Easy Upgrade 0: Triple-stomp fixed loop

This is the strongest "still-minimal" upgrade from the ranked boss options:

- the boss performs 3 stomps in a row
- after the third stomp, the vulnerable window opens

Why it is worth considering early:

- it feels more like a real boss phase without adding new mechanics
- it keeps the same telegraph and punish rules
- it adds tension while staying readable

This can be added after the first working single-stomp MVP without changing any of the core architecture.

## Easy Upgrade 1: Boss intro using current UI

Use `StageAnnouncementUI.ShowAnnouncement(...)` to display something like:

- `BOSS`
- `GIANT FOOT`
- `Dodge the stomp and strike when it lands.`

This gives the encounter identity with almost no new system cost.

Likely files:

- `Assets/_Scripts/StageAnnouncementUI.cs` only if we want a convenience helper
- `Assets/_Scripts/BossFootController.cs`
- `Assets/_Scenes/LevelBoss.unity`

What you may need to provide:

- nothing required
- optional boss-specific text or localized wording

## Easy Upgrade 2: Boss becomes vulnerable only on miss

This is probably the best next rule if you want the fight to feel more "boss-like" without becoming too complex.

Rule:

- if the player is still inside the stomp zone at impact, player takes damage and boss is not exposed
- if the player successfully dodges, the foot gets stuck and becomes vulnerable

Why this is good:

- it rewards the dodge clearly
- it fits your original fantasy
- it makes the fight feel earned instead of automatic

Implementation note:

- define "miss" using the actual stomp hit zone, not screen position alone

## Easy Upgrade 3: Shadow tracks briefly, then locks

This is a strong improvement over a static telegraph, but still manageable.

Recommended behavior:

- shadow follows the player for `0.3s` to `0.5s`
- shadow then locks in place
- after `0.3s` to `0.5s` more, the stomp lands

This gives the player pressure and readability at the same time.

Do not let it track until the final frame.

## Easy Upgrade 4: Add a simple boss health bar

This is not mandatory for the first playable pass, but it is easy enough that you will probably want it soon.

Recommended approach:

- simple top-screen UI bar
- one background image
- one fill image
- optional boss name text

If no art exists yet, a plain colored bar is fine.

Likely files:

- new `BossHealthBarUI.cs`
- `LevelBoss.unity`

Assets you may want to provide:

- background bar PNG
- fill bar PNG
- optional decorative frame PNG

Recommended sizes:

- `1024x128` or similar wide horizontal image
- transparent background PNG

## Easy Upgrade 5: Attack speed ramps up

This is one of the best "cheap difficulty" tools for this boss.

Easy versions:

- telegraph time shortens as HP drops
- vulnerable time shortens as HP drops
- recovery time shortens as HP drops

Start simple:

- phase 1: slower telegraph
- phase 2: medium telegraph
- phase 3: fast telegraph

This is much easier than adding multiple new attacks.

## Easy Upgrade 6: Basic hit feedback on the boss

Even a very simple feedback layer helps a lot:

- flash color when hit
- slight squash
- short shake
- stomp dust on impact

This can make a static-foot boss feel much more alive without needing full animation.

## Files Likely Involved In The Easy-Upgrades Pass

- `Assets/_Scripts/BossFootController.cs`
- `Assets/_Scripts/BossShadowTelegraph.cs`
- `Assets/_Scripts/StageAnnouncementUI.cs` or direct reuse without changes
- new `Assets/_Scripts/BossHealthBarUI.cs`
- `Assets/_Scenes/LevelBoss.unity`

## What You May Want To Provide For The Easy-Upgrades Pass

- boss name text
- stomp SFX
- impact dust sprite / particle texture
- boss bar art
- optional "boss intro" audio cue

## Easy-Upgrade Suggestions And Doubts

- If we add only one improvement after MVP, I would choose "vulnerable only on miss".
- If we add two, I would choose "miss-only vulnerability" plus "boss intro UI".
- The health bar is useful, but less important than making the dodge rule clear.
- Speeding up the stomp is better than giving the boss huge damage too early.
- A first boss is usually better with one mechanic done well than with three shallow mechanics.

## Later / Complex / Harder Alternatives

These are good ideas, but they are clearly a bigger step than the first or second pass.

## Later Option 1: Boss appears in the same scene after the regular level clear

This is the most natural-feeling structure for the player, but it is harder in this project than using a separate scene.

Why it is harder here:

- `RandomSpawnerEnemy` currently auto-finishes the level by growing the plant
- we would need a new phase between "waves cleared" and "plant grows"
- the same scene would need to coordinate waves, boss state, and exit state

If we do this later, I recommend:

- do not cram boss logic directly into `RandomSpawnerEnemy`
- add a new scene-level director, something like `LevelPhaseDirector`
- let the spawner report completion to that director
- let the director decide whether to start the boss or grow the plant

This is the cleaner architecture if you later want more levels with mid-level bosses.

## Later Option 2: Visible foot at the top of the screen plus ground shadow

This is probably the best visual fantasy version of your idea, but it is more complex.

Why:

- the camera follows the player
- the foot must appear screen-anchored, not just world-anchored
- it must move in a readable way without becoming visually noisy

If we do this later, I recommend:

- keep the shadow as the true gameplay telegraph
- use the top-screen foot as flavor / anticipation
- lock the final stomp point before impact

This is a strong later upgrade, not a good MVP dependency.

## Later Option 3: Vulnerability rules like "after 3 attacks", "after 3 misses", "1 then 2 then 3"

These are valid design directions, but they need tuning and stronger feedback systems.

Why they are harder:

- the player needs clear feedback on the current count
- the fight pacing becomes harder to read
- the boss needs more visible state communication

If you want to try one of these later, the cleanest is probably:

- first phase: vulnerable after 1 miss
- second phase: vulnerable after 2 misses
- third phase: vulnerable after 3 misses

But I would not start there.

## Later Option 4: Boss is vulnerable but not really stunned

This is a good late-pass upgrade.

Examples:

- the foot stays vulnerable but slowly pulls away
- the foot wiggles while trying to recover
- the foot drags sideways and can still hurt the player on retreat
- the weak spot is only one part of the foot, not the whole sprite

This is much more expressive, but it requires better animation, collision, and tuning.

## Later Option 5: Multi-attack boss phases

Examples:

- double stomp
- fast fake stomp followed by real stomp
- side sweep after landing
- stomp plus shockwave ring

These are good second-boss or late-first-boss ideas, but they are not needed to prove the encounter.

## Later Option 7: Heavy stomp shockwave

This is a clean upgrade that still fits the stomp fantasy:

- a heavier stomp emits a visible shockwave ring
- the player must dash through or outrun it
- the punish window opens after the wave

It adds challenge without requiring new player verbs, but it does require clean VFX and tuning.

## Later Option 6: Better architecture refactor

If the boss implementation grows, the damage system will probably want cleanup.

Best-practice direction:

- replace direct type checks in `HealthSystem` with a small damage/death callback interface
- let player, enemy, and boss all implement that interface

That would be cleaner than adding one more special-case branch for the boss.

I would only do this after the first boss is working or if the boss implementation naturally forces it.

## What You Might Need To Provide For The Later Passes

- animated foot sprite sheet
- extra anticipation / recoil frames
- dedicated boss music
- extra SFX for intro, stomp, hurt, retreat, death
- boss UI art
- additional VFX textures
- optional floor crack / dust decals

## My Strongest Recommendations Across All Sections

- Use `LevelBoss` as a dedicated scene first.
- Reuse the existing plant exit after the boss dies.
- Start with shadow telegraph only.
- Make the stomp target lock before impact.
- Keep the first playable boss to one attack and one punish rule.
- Do not overbuild a generic boss system yet.
- Make sure the boss hurtbox is compatible with the current player attack system.
- Treat "miss-only vulnerability" as the first meaningful upgrade after the MVP works.

## Open Questions I Think We Should Answer Before Implementation

- Do you want the boss to currently end the demo, or should it lead into `Level02`?
- Do you want the boss to take raw damage with hidden HP first, or should we quickly add a visible health bar?
- Do you want the boss to be vulnerable after every stomp first, or do you want to start directly with "vulnerable only on miss"?
- Are you okay with placeholder art for the first implementation, or do you want the real foot/shadow assets first?
- Should the stomp be heavy damage but survivable, or should it be almost a one-hit kill?
- Do you want the first boss to feel more funny/cartoonish or more oppressive/scary? That affects art, timing, sound, and animation choices.
