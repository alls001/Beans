# Technical Analysis

## Engine And Render Stack

- Unity version: `6000.2.6f2`
- Render pipeline: `Universal Render Pipeline 17.2.0`
- Graphics pipeline is configured to use a custom URP asset
- Text serialization is enabled (`EditorSettings.asset` uses Force Text / mode `2`)
- Active input handling is `2`, meaning both old and new input systems are enabled

This is a modern Unity 6 project using URP, but most runtime gameplay code still relies on classic `Input.*` calls.

## Packages In Use

From `Packages/manifest.json` and `packages-lock.json`, the notable packages are:

- `com.unity.render-pipelines.universal` `17.2.0`
- `com.unity.visualeffectgraph` `17.2.0`
- `com.unity.cinemachine` `3.1.4`
- `com.unity.inputsystem` `1.14.2`
- `com.unity.ai.navigation` `2.0.9`
- `com.unity.timeline` `1.8.9`
- `com.unity.ugui` `2.0.0`
- `com.unity.visualscripting` `1.9.7`
- `com.unity.test-framework` `1.6.0`
- `com.unity.collab-proxy` `2.10.0`
- `com.unity.ide.visualstudio` `2.0.23`
- `com.unity.ide.rider` `3.0.37`

## What The Project Is Actually Using

Based on the repository contents, the project is clearly using:

- URP
- uGUI / Canvas UI
- TextMesh Pro
- Rigidbody-based 3D movement and collisions
- legacy `Input` API in gameplay code
- VideoPlayer for cutscene/credits scenes
- custom sprite-on-quad animation system
- prefab-driven enemy and player setup
- scene-level serialized configuration

Packages that are present but not strongly reflected in current gameplay code:

- Input System package
- AI Navigation
- Visual Effect Graph
- Visual Scripting
- Timeline
- Multiplayer Center

For a small Unity project, that means the dependency list is broader than the currently visible runtime design.

## Architecture

The architecture is lightweight and MonoBehaviour-centric.

### Main runtime domains

- Player domain: `PlayerController3D`, `HealthSystem`, `QuadSpriteAnimator`
- Enemy domain: `EnemyBase`, `EnemyMelee`, `EnemyMeleeFireball`, `HealthSystem`
- Combat payloads: `Projectile`, melee overlap checks, hit-stop, knockback
- Stage flow: `RandomSpawnerEnemy`, `EnemyDeathListener`, `PlantGrowthController`
- UI flow: `StageAnnouncementUI`, `PlayerHealthUI`, `PauseMenuController`, `MenuPrincipalManager`
- Scene flow: `SimpleSceneLoader`, `CutsceneSceneManager`

### Style of architecture

This is not using:

- ScriptableObject-driven configuration architecture
- service locators / dependency injection
- event buses
- ECS / DOTS
- explicit state machines separated from MonoBehaviours

Instead it uses:

- direct component references
- inspector wiring
- `FindObjectOfType` / tag lookups as fallback
- coroutines for time-based flow
- prefab overrides for content variation

That is a very normal small-team Unity pattern, especially for a project still shaping its gameplay.

## Key Technical Patterns

### 1. Shared health with role-dependent reactions

`HealthSystem` is the cross-cutting damage/heal layer. It delegates reactions depending on what lives on the same GameObject:

- player -> `PlayerController3D.OnTakeDamage()` / `OnDeath()`
- enemy -> `EnemyBase.OnTakeDamage()` / `OnDeath()`
- generic fallback -> animator trigger / local death coroutine

This is simple and useful, but it also means actor identity is inferred dynamically from attached components.

### 2. Quad-based sprite animation in 3D space

`QuadSpriteAnimator` is one of the most defining technical choices in the project.

Instead of traditional 2D sprites alone or fully rigged 3D models, the project:

- renders characters with a `MeshRenderer`
- swaps sprite textures on a runtime material
- offsets the visual root based on sprite pivot
- flips orientation by rotating the transform

This is a practical way to get expressive sprite animation inside a 3D gameplay space.

### 3. Enemy inheritance split

There are two enemy approaches in the repository:

- `EnemyController`: older all-in-one state machine
- `EnemyBase` + `EnemyMelee` + `EnemyMeleeFireball`: newer inheritance-based split

Current active prefabs are using the newer `EnemyBase` family, which is the better direction. `EnemyController` looks like legacy/transitional code and can probably be retired later if nothing still depends on it.

### 4. Scene-owned progression

Each gameplay scene owns important configuration in serialized scene data:

- which plant scene to load next
- which pause menu target to return to
- which enemy spawner settings to use
- which UI objects exist for announcements and game over

This keeps logic easy to understand for a small project, but it also makes scene integrity critical.

## Standards And Quality Signals

### Strengths

- The project uses prefabs for the important reusable entities.
- The current gameplay code is understandable without heavy tooling.
- The newer enemy hierarchy is cleaner than the legacy enemy controller.
- The repo already has correct Unity fundamentals like text serialization and proper ignore rules for `Library`, `Temp`, `Logs`, and `UserSettings`.

### Weaknesses

- Naming consistency is weak across files, classes, scenes, and assets.
- There is no clear folder-level architecture standard beyond broad categories.
- Runtime tuning values are heavily embedded in prefabs and scenes instead of data assets.
- Scene serialization is carrying important truth that can drift out of sync.
- There is no visible automated testing usage despite the test framework package being present.
- There is no project documentation describing systems, conventions, or content ownership.

## Current Repo / Git Hygiene

The repo is broadly correct for a Unity project:

- tracked: `Assets`, `Packages`, `ProjectSettings`
- ignored: `Library`, `Temp`, `Logs`, `UserSettings`, generated solution/project files

Small fixes applied:

- `.vscode/` is now ignored
- `Assets/_Recovery/` is now ignored and should stay local-only

Why that matters:

- `.vscode/` is usually personal/editor-local
- `_Recovery` scenes are backup artifacts, not source-of-truth game content

## Things I Would Add Or Change

These are recommendations, not urgent requirements.

### 1. Add a `.gitattributes`

Recommended because Unity text assets benefit from consistent line endings and merge behavior.

Even a minimal file would help normalize:

- `*.cs`
- `*.shader`
- `*.asset`
- `*.prefab`
- `*.unity`
- `*.meta`

### 2. Reduce package surface

If unused, consider removing:

- `com.unity.visualscripting`
- `com.unity.multiplayer.center`
- `com.unity.collab-proxy`
- possibly `com.unity.ai.navigation` if navigation is not going to be used soon
- possibly `com.unity.visualeffectgraph` if effects stay sprite/simple-URP only

For a small project, fewer packages means less noise and fewer upgrade risks.

### 3. Move gameplay tuning into ScriptableObjects

Best candidates:

- enemy stats
- wave definitions
- player tuning
- stage progression metadata

That would make balancing easier and reduce prefab/scene override sprawl.

### 4. Standardize on one input approach

Right now the project includes the new Input System package but gameplay still uses classic `Input`.

Choose one of:

- keep old input for now and remove the package if unused
- migrate fully to the new Input System for cleaner rebinding and UI integration later

### 5. Add a small `Docs/` culture permanently

This repo was missing the kind of docs that become essential as soon as more features or collaborators arrive. Keeping these analysis files and updating them as the project changes will pay off quickly.
