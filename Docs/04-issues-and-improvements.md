# Issues, Risks, And Improvements

## Highest-Value Issues

## 0. Boss scene serialization and code drifted apart

Observed issue:

- `LevelBoss.unity` was still serializing an older `BossFootController` field layout after the script had already changed.
- That made the repo copy of the scene less trustworthy than the live Unity editor state.
- Several boss docs also kept describing older ground and motion logic after the implementation changed.

Status:

- partially fixed in this pass

Why it matters:

- future sessions can misdiagnose bugs because they are reading stale YAML, not the scene the user is actually testing
- editor preview, sorting, and serialized defaults can look inconsistent even when runtime code changed
- docs drift makes it easy to regress to already-rejected behavior

Improvement:

- whenever serialized boss fields change, re-save the affected scene or patch the YAML immediately
- keep `05`, `10`, `11`, `12`, and `13` aligned so one older doc does not quietly reintroduce bad assumptions

## 1. Scene progression was partially broken

Observed issues:

- `Cutscene.unity` pointed to `Nivel_01`, but the real scene on disk is `Level01.unity`
- `EditorBuildSettings.asset` still contained stale/missing scene paths
- `CutsceneSceneManager` in `Cutscene.unity` and `Creditos.unity` had no `VideoPlayer` reference assigned

Status:

- fixed in this pass

Why it mattered:

- the main flow could fail between cutscene and gameplay
- the video-end automatic transition logic could not fire

## 2. Tracked recovery scenes were polluting the repo

Observed issue:

- `Assets/_Recovery` contained backup/recovery scenes that do not belong in shared source control for a small Unity repo

Status:

- moved into the "should stay local-only" category in this pass

Why it matters:

- backup noise makes the repo harder to trust
- collaborators can mistake recovery files for real content
- repo history becomes messier for no gameplay value

## 3. Scene/state truth is too distributed

Symptoms:

- scene names, build settings, cutscene target names, and prefab overrides can drift apart
- progression logic depends heavily on inspector wiring

Risk:

- small content changes can silently break stage progression
- issues are hard to spot until runtime

Improvement:

- centralize stage metadata in a small ScriptableObject or config asset
- or at minimum keep a single scene-flow checklist doc updated whenever scenes change

## 4. The spawner prefab serialization looks stale

Observed issue:

- `EnemySpawner.prefab` only serializes a subset of fields from `RandomSpawnerEnemy`
- fields like `announcementUI`, `levelNumber`, and `plant` are not present in the prefab source data
- scene overrides still reference some of those properties

Risk:

- serialization drift
- confusing prefab override behavior
- level numbering likely defaults to `1` everywhere unless fixed elsewhere

Improvement:

- open/save/reserialize the spawner prefab in Unity after confirming the intended field set
- explicitly set level numbers per scene

## 5. Level identity is under-specified

Observed issue:

- `RandomSpawnerEnemy.levelNumber` is not visibly serialized anywhere in the repo
- level scenes may all announce themselves as `LEVEL 1`

Improvement:

- serialize and set `levelNumber` in each level scene
- alternatively remove that concept until real per-level numbering is needed

## 6. Legacy and new enemy architectures coexist

Observed issue:

- `EnemyController` still exists
- active prefabs are using `EnemyBase`-derived scripts instead

Risk:

- future work may accidentally patch the wrong enemy path
- maintenance cost increases

Improvement:

- verify whether `EnemyController` is unused
- if unused, archive or delete it
- if still needed, document exactly where and why

## Gameplay / Balance Improvements

## 7. Player attack ranges look very large

Observed on the player prefab:

- attack 1 range `9.08`
- attack 2 range `9.29`
- attack 3 range `22.4`

Given the rest of the project scale, these look extremely generous for melee unless intentionally compensating for camera/sprite presentation.

Improvement:

- re-check combat scale in playtests
- if these values are intentional, document why
- if not, rebalance with visualized gizmos during tuning

## 8. Wave composition is repetitive in current main scenes

Observed:

- `Level01` only references `Caracol`
- `Level02`, `LevelBoss`, and `LevelTesttiro player` appear to mix mainly `lagartixa` and `Caracol`
- the N1/N2 variants currently do not participate in the main level scenes

Improvement:

- either bring the unused enemy variants into the encounter design
- or cut/archive them until they are actually part of the roadmap

## 9. Stage announcement audio exists in code but is not wired

Observed in gameplay scenes:

- `StageAnnouncementUI.audioSource` is null
- `StageAnnouncementUI.announcementSfx` is null

Improvement:

- either wire those references to finish the feature
- or remove the audio fields until they are needed

## 10. Footstep audio triggers during boss stomp phase

Observed:

- stepping sounds play during the boss loop even when the foot visuals are not clearly stomping

Improvement:

- confirm which AudioSource is firing during the boss loop
- gate or replace it with a dedicated boss stomp SFX

## Technical / Workflow Improvements

## 11. Input strategy is mixed

Observed:

- project has the new Input System package
- gameplay scripts use the old `Input` API
- project input handling is set to both systems enabled

Improvement:

- choose one path and document it

## 12. Repo hygiene can still improve

Recommended next steps:

- keep `.vscode/` out unless you intentionally decide to share workspace config
- keep `_Recovery` local-only
- add a `.gitattributes`
- consider an LFS decision for large binary assets if art/video volume grows

## 13. Naming consistency should be cleaned up before the project grows more

Examples:

- `RnadomSawnerEnemy.cs`
- `EnemyMeele.cs`
- `cutscene maneger.cs`
- mixed Portuguese/English scene and asset naming
- experimental scene names like `LevelTesttiro player`

Improvement:

- define naming conventions for:
  - scripts
  - scenes
  - prefabs
  - folders
  - temporary/test content

## 14. Docs were missing

Status:

- addressed in this pass with the new `Docs/` files

Why it matters:

- this project is now large enough that undocumented scene wiring and content assumptions will slow future work

## Suggested Priority Order

### Near-term

- verify scene progression in-editor after the fixes
- resave the spawner prefab and confirm scene overrides behave as expected
- decide whether `_Recovery` files can be removed from remote history permanently
- confirm level numbering and scene naming standards

### Next stabilization pass

- remove dead/legacy enemy code
- clean scene and prefab names
- move balance/content data into ScriptableObjects
- reduce unused packages

### Later

- add basic automated checks
- formalize docs and task planning around the project map
