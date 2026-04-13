# Descriptive Analysis

## What This Project Is

`Beans` is a small Unity action game centered on short combat stages, wave-based enemy pressure, and forward progression through a beanstalk motif. The current playable loop appears to be:

`Menu -> Cutscene -> Level01 -> Level02 -> Creditos`

There are also extra in-progress or side scenes such as `LevelBoss` and `LevelTesttiro player`, which look like experimental or future-playable branches of the same loop.

At a high level, the game currently reads as a stylized 2.5D arena action prototype:

- The world is rendered in 3D space with physics, camera, colliders, and URP lighting.
- Characters are visually presented through sprite-driven quad animation instead of full 3D character rigs.
- Combat is immediate and arcade-like: melee combos, dash/sprint, simple enemy AI, health pickups, wave announcements, pause/game over, and scene transitions.

## Core Fantasy And Identity

The strongest identity marker is the beanstalk progression. The project is not just a generic arena fighter; its structure ties combat completion to environmental growth. The player defeats waves, enables plant growth, then uses the grown plant to leave the stage. That gives the project a clear thematic spine:

- defend/progress through hostile encounters
- feed or protect the growth of the beanstalk
- climb/advance after clearing the arena

That theme is reinforced in UI text such as "Defeat all enemies to keep the beanstalk growing" and "Climb the beanstalk."

## What It Is Not

From the current repository state, this is not yet:

- a systems-heavy RPG
- a narrative-heavy adventure with branching logic
- a networked or multiplayer game
- a data-driven content framework
- a polished production pipeline with deep tooling, tests, or documentation

It is much closer to a vertical-slice / prototype-to-production transition stage: the core loop is visible, but many surrounding systems are still direct, manual, and scene-driven.

## Current State Of The Project

The project is already beyond a blank prototype. It has:

- a menu scene
- a cutscene scene
- at least two connected gameplay levels
- a credits/end scene
- player combat and damage logic
- enemy spawning, enemy death tracking, and level completion logic
- stage UI and pause flow
- multiple enemy prefabs
- reusable player, projectile, heart, and spawner prefabs

At the same time, it still has clear prototype residue:

- scene naming is inconsistent (`Level01`, `Level02`, `LevelBoss`, `LevelTesttiro player`, older serialized names like `Nivel_01`)
- tracked recovery/backup scenes existed in `Assets/_Recovery`
- there are unused or not-currently-wired prefabs (`N1/N2` enemy variants are present but not referenced by the main level scenes)
- some systems exist in duplicate or transitional forms (`EnemyController` and the newer `EnemyBase` hierarchy)
- naming quality varies a lot across code and assets

So the project feels actively under construction rather than stabilized.

## Game Design Patterns In Use

The current design trends are consistent with accessible arcade action:

- Short encounter-driven stages rather than long exploratory levels.
- Waves as the main pacing device.
- Clear stage-state transitions: intro -> combat -> final objective -> exit.
- Simple player kit with low input complexity but satisfying cadence: move, dash, combo attacks.
- Enemy readability prioritized over complexity.
- Health recovery through drops rather than long-term resource management.

The combat model is especially prototype-friendly:

- The player has a 3-hit combo with escalating damage and range.
- The third hit becomes the big payoff hit.
- Sprint/dash acts as the mobility tool and likely defensive repositioning tool.
- Hit-stop is used for feedback, which suggests the team is already thinking in feel, not just raw mechanics.

## Stylistic / Artistic Direction

The project has a hybrid presentation:

- 3D space and cameras
- sprite characters and effects
- billboarded or quad-driven visuals
- stylized UI screens and splash art

This creates a 2.5D look instead of a fully top-down pixel game or a fully 3D action game.

Some visible artistic tendencies:

- Expressive, hand-authored sprite sheets for player, enemies, and plant growth
- Distinct menu/pause/cutscene art assets rather than placeholder-only UI
- Strong reliance on silhouette and readable motion
- High contrast between gameplay sprites and stage space

The project currently feels more "indie action prototype with a stylized sprite presentation" than "clean minimalist systems prototype."

## Structural Design Choices

The design is scene-oriented rather than globally state-driven:

- The menu chooses the next scene directly.
- The cutscene scene loads the next gameplay scene directly.
- Each gameplay scene owns its own plant destination and enemy spawner configuration.
- The credits scene returns to the menu directly.

This is simple and practical for a small Unity project, but it also means scene wiring is carrying a lot of project logic.

## Overall Read

My current read is that this project is a compact, stylized, wave-based combat game with a beanstalk/climb progression theme, built as a small Unity production where the core play loop is already visible and fun-shaped, but the surrounding project structure still carries a lot of prototype leftovers, manual scene wiring, and naming inconsistency.

That is a good state for planning, because there is already enough here to map and stabilize, but it is still early enough to clean up architecture and workflow before the project gets too big.
