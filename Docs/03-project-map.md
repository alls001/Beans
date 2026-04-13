# Project Map

## High-Level Flow

The current intended player flow is:

1. `Menu`
2. `Cutscene`
3. `Level01`
4. `Level02`
5. `Creditos`
6. back to `Menu`

There are additional side scenes:

- `LevelBoss`
- `LevelTesttiro player`

Those look like variant or experimental gameplay scenes rather than part of the main shipped route.

## Main Runtime Systems

## 1. Scene / UX Layer

### `MenuPrincipalManager`

- Lives in `Menu.unity`
- Loads the configured gameplay-start scene, currently `Cutscene`
- Switches between initial menu and options panel
- Exits the application

### `CutsceneSceneManager`

- Lives in `Cutscene.unity` and `Creditos.unity`
- Watches a `VideoPlayer`
- optionally allows skip with button/key
- loads the configured next scene when the video ends or when skipped

### `SimpleSceneLoader`

- Present in gameplay scenes
- general-purpose button hook for loading scenes, reloading scenes, or quitting

### `PauseMenuController`

- Present in gameplay scenes
- toggles pause UI
- freezes time with `Time.timeScale = 0`
- can resume, quit, or return to `Menu`

## 2. Gameplay Loop Layer

### `RandomSpawnerEnemy`

This is the central stage controller.

Responsibilities:

- waits before starting
- introduces the level/waves through `StageAnnouncementUI`
- spawns waves from serialized scene/prefab data
- tracks active enemies
- waits until all enemies in all waves are dead
- triggers the final objective message
- tells the plant to grow

Important detail:

- the reusable prefab `Assets/_Prefabs/EnemySpawner.prefab` is only a base shell
- real content is defined through scene-level prefab overrides

### `EnemyDeathListener`

- added to spawned enemies at runtime
- reports enemy death back to the spawner on `OnDisable` if the enemy is actually dead

This is how the spawner knows when a wave has been cleared.

### `PlantGrowthController`

- attached to the beanstalk/plant objective in gameplay scenes
- waits for the spawner to call `GrowPlant()`
- plays the growth state
- enables a trigger after the growth delay
- when the player touches the grown plant, loads the next scene

This is the stage completion gate.

### `StageAnnouncementUI`

- shows level intro
- shows each wave intro
- shows final objective text after all waves are complete

Current note:

- the scenes have the visual references wired
- audio references for this system are currently unassigned in the level scenes

## 3. Player Layer

### `PlayerController3D`

Responsibilities:

- reads movement input
- applies Rigidbody movement
- handles sprint/dash
- handles 3-hit combo melee attacks
- plays quad animations based on movement and action state
- flips the character based on direction
- plays movement/attack/dash sounds
- shows game over panel on death

Combat model:

- Attack 1: `2` damage
- Attack 2: `4` damage
- Attack 3: `6` damage
- attack 3 can hit in `360`

Important implementation detail:

- attack hits use `Physics.OverlapSphere` from `attackPoint`
- forward-facing filtering is used for the first two attacks
- hit-stop is triggered if at least one enemy is damaged

### `HealthSystem`

This is the common health module for both player and enemies.

Responsibilities:

- track current/max HP
- receive damage
- apply optional knockback
- manage invulnerability window
- delegate death/hit reactions to the appropriate controller
- support healing

On the player prefab it currently provides:

- `maxHealth = 6`
- `invulnerabilityTime = 2`

### `PlayerHealthUI`

- reads `HealthSystem.CurrentHealth`
- swaps heart/health sprites based on remaining HP
- normally finds the player by tag if no direct reference is set

## 4. Enemy Layer

### Active runtime pattern

The active enemy design uses:

- `EnemyBase`
- `EnemyMelee`
- `EnemyMeleeFireball`

### `EnemyBase`

Shared enemy responsibilities:

- detect/find player
- move through Rigidbody velocity
- manage look direction and flip
- play idle/walk/hit/death quad animations
- receive damage and death callbacks
- optionally drop healing hearts on death

### `EnemyMelee`

- closes distance to the player
- attacks with overlap-sphere melee hit
- uses cooldown + windup

### `EnemyMeleeFireball`

- can melee at close range
- can shoot projectiles at mid range
- can optionally keep moving while preparing the fireball

### Current enemy content that is clearly in use

Main scenes currently reference:

- `Caracol.prefab`
- `lagartixa.prefab`

Enemy prefabs present but not currently referenced by the main scenes:

- `N1 Bite Dragon.prefab`
- `N1 Enemy Gary.prefab`
- `N2 Bite Dragon.prefab`
- `N2 Enemy Gary.prefab`

That suggests the enemy roster exists beyond what the current shipped route is actually using.

## 5. Combat Payload / Pickup Layer

### `Projectile`

- deals damage to objects with tag `Player`
- destroys itself on player hit or non-trigger collision

### `BillboardFlipByVelocity`

- keeps the projectile visual facing the camera
- flips horizontally based on projectile velocity

### `HealingHeart`

- heals the player on trigger enter
- self-destructs after a lifetime if not collected

## Data / Reference Flows

## A. Stage completion flow

1. `RandomSpawnerEnemy` starts the encounter.
2. It spawns enemies and tracks them in `activeEnemies`.
3. Spawned enemies get an `EnemyDeathListener`.
4. Enemy dies -> `HealthSystem` delegates to enemy controller.
5. Enemy is disabled after death.
6. `EnemyDeathListener.OnDisable()` reports back to the spawner.
7. When no waves remain and no active enemies remain, the spawner calls `plant.GrowPlant()`.
8. `PlantGrowthController` enables the plant trigger after the growth delay.
9. Player touches plant -> next scene loads.

## B. Player combat flow

1. Input enters `PlayerController3D`.
2. Controller triggers combo state and animation.
3. Animation events call `HitAttack1/2/3`.
4. `PerformDamageCheck()` runs overlap checks against `enemyLayer`.
5. Enemy `HealthSystem.TakeDamage()` is called.
6. Enemy controller plays hit/death response.
7. If at least one target was hit, player hit-stop briefly freezes time.

## C. Enemy damage flow

1. Enemy melee or projectile hits the player.
2. Player `HealthSystem.TakeDamage()` runs.
3. Knockback/invulnerability logic is applied.
4. `PlayerController3D.OnTakeDamage()` or `OnDeath()` is called.
5. Player state locks accordingly.

## D. Scene progression flow

1. `MenuPrincipalManager` loads `Cutscene`.
2. `CutsceneSceneManager` loads the next stage after the video/skip.
3. Gameplay scenes use `PlantGrowthController.nextSceneName` to move forward.
4. `Creditos` returns to `Menu`.

## Important Mapping Observations

- Scene wiring is the real backbone of the project.
- The spawner prefab is generic; the actual level content comes from scene overrides.
- The player and enemies share the same health abstraction.
- Enemy completion is tracked indirectly through disable/death reporting, not through a central combat registry.
- Current main-level enemy content is narrower than the prefab library suggests.
- There is a legacy enemy controller still in code, but the active prefab path is the newer base/inheritance path.
