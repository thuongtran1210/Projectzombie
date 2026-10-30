# Bootstrap, stage selection and match lifetime

Implemented for Unity 2022.3 / Addressables 1.22.3. This document describes project decisions, not a Unity-mandated architecture.

## Ownership

`Bootstrap.unity` is the only built-in player entry scene. Its linked `AppBootstrap.prefab` owns application composition, domain services, an EventSystem, audio and a loading/error cover. Bootstrap stays loaded; content scenes are loaded additively and explicitly unloaded. MainMenu and Sông Quê are local Addressable scenes. No content scene owns duplicate save/meta/GameState managers or an EventSystem.

CoreBootstrapper registers save/progression services once under the application root. Legacy PersistentSingleton implementations and AudioManager recognize Bootstrap ownership and do not move its objects to DontDestroyOnLoad. Other legacy lazily created persistent systems outside this feature still exist; this change is not a project-wide removal of all singleton usage.

MainMenu owns its own camera, canvases, navigation adapter and screen presenters. A map owns camera, lighting, navigation, actors, pools, HUD, VFX and DefenseSession. Persistent navigation never retains a MainMenu CanvasGroup or presenter.

## Data and flow

`WorldStageDatabaseSO` remains the stage catalog. A `MapDefinitionSO` contains map identity and an Addressables scene reference. A `StageDefinitionSO` selects a map, a rules reference, duration, rewards and optional prerequisite completed-stage IDs. Several stages can share one map.

Production Sông Quê rules retain their original GUID and spawn schedule. They are moved from Resources to `Assets/_Data/Levels/Rules/Stage01_Production.asset`. The editor fast-spawn fixture remains separate. Existing blueprint authoring is supported as an alternate rule source; a stage should use one rule source.

MainMenu Start opens StageSelect. StageSelect checks the actual map scene key and prerequisite completion. Selecting an available stage opens DefensePreparation. Preparation captures the selected stage and sends an immutable MatchLaunchRequest with a loadout snapshot and a new Run ID. Missing routes are rejected; there is no implicit Sông Quê fallback.

AppFlow serializes all enter/return transitions. Under the application loading cover it unloads the previous content, releases its rules, loads selected rules and map, sets the map active, and explicitly initializes DefenseSceneCompositionRoot. Composition binds dependencies and initializes HUD before starting DefenseSession. Gameplay does not depend on scene activation completing before or after Start callbacks.

The loader owns the exact scene handle. Rules loader owns the rules handle. Assets are released after scene cleanup, so actors cannot reference released rules. Scenes/rules in `App_Scenes_Local` use Pack Separately; the catalog does not directly retain the production rules asset graph.

On load/initialization failure, AppFlow unloads partial content, releases rules and attempts to reload MainMenu. A dismissible application error cover explains the failure. If hub recovery also fails, a blocking error remains. Transitions are serialized rather than advertised as cancellable: Unity/Addressables operations already started are always awaited and accounted for.

## Session and restart

DefenseSession is authoritative for Running/Paused/LevelUp/Win/Loss. GameStateManager receives a compatibility projection without independently changing the Defense clock or Time.timeScale. AppFlow tracks Boot/Menu/Loading/Match/Returning/Error.

Restart uses the existing cleanup/reset/pooling path. It retains scene and loadout, advances run generation and creates a new Run ID. Results use the actual stage ID and configured victory coins, not a fixed Sông Quê identity. The existing result service guard is supplemented by a save-backed history of the most recent 128 rewarded Run IDs, preventing duplicate grants across result service instances in that window. This is not an indefinite replay ledger or a saved-match-resume system. Experience rewards and broader economy design remain in the existing progression system.

## Editor workflow

- Install/migrate: `Tools > ProjectZombie > Architecture > Install Bootstrap and Multi Map Flow`.
- Validate: `Tools > ProjectZombie > Architecture > Validate App Flow`.
- Navigation smoke: `Tools > ProjectZombie > Architecture > Run Navigation Smoke`.
- Direct Play from a content scene uses an Editor-only Bootstrap adapter. The map's explicit direct-play stage determines its test rules. Player builds enter through Bootstrap.

The Editor adapter initializes missing application services before content-scene Awake/OnEnable, and skips creation when the scene already contains AppCompositionRoot. MainHub independently waits for AppBootGate, resolves IMetaCurrencyService once and unsubscribes on disable/destroy. It displays a loading placeholder until services are ready instead of treating an unavailable balance as zero. Navigation smoke checks also record early MetaCurrencyManager warnings.

Installer snapshots loaded scenes, including current unsaved edits, under `scratch/ArchitectureBackup/<timestamp>/` before preserving/migrating them. It retains production asset GUIDs and refuses Play Mode installation. It is an explicit migration command, not an automatic domain-reload rewrite. UIRegistry generation no longer rewrites assets on every assembly reload/Play entry.

Bridge commands are available through `Tools/Invoke-UnityBridge.ps1`: `status`, `hierarchy`, `refresh`, `install-app-architecture`, `validate-app-architecture`, `smoke-app-architecture`, `smoke-bootstrap`, `smoke-direct-map`. Validation and smoke reports live under `scratch`. The smoke test enters Sông Quê twice and returns twice; unit tests use distinct route fixtures to exercise multiple map routing. Bootstrap entry and direct Editor map entry are also exercised. There is currently one authored production map, not a newly created second environment.

## Adding a map or stage

1. Create the map scene with exactly one configured DefenseSceneCompositionRoot, runtime roots, scene camera and authored actor/UI bindings. Reuse shared prefabs; keep application services and EventSystem out of the map.
2. Add its scene to an Addressables group with the intended local/remote policy; create a MapDefinition with that scene reference.
3. Create stage rules and make them Addressable. Create a StageDefinition referencing the map/rules; configure duration, rewards and prerequisite IDs.
4. Add the StageDefinition to WorldStageDatabase. For direct Editor Play, assign the explicit stage on composition.
5. Validate rules, build Addressables content and test the actual player build. No changes to Start button code, navigation constants or scene build index are required.

## Verification limits

Editor validation covers stage routing, transition locking, failed composition recovery, prerequisites, attempt identities, reward idempotency and authored scene ownership, plus existing session/pool/VFX/UI regressions. The Play Mode smoke exercises the actual Start button, StageSelect, two Sông Quê entries, restart, returns and owner counts.

Passing Editor tests does not prove remote bundle availability, Android memory budgets, 60 FPS or complete compatibility of legacy multiplayer. Addressables Editor AssetDatabase mode can hide bundle/dependency problems. A local Addressables content build and device profiling remain required before release. Several pre-existing gameplay systems still load other assets through Resources; their migration and shared bundle dependency analysis are separate work, not covered by the new map/rules loader.

## Official references used for this design

- [Unity 2022.3 execution order](https://docs.unity3d.com/2022.3/Documentation/Manual/ExecutionOrder.html): sceneLoaded precedes Start; explicit readiness avoids ordering assumptions.
- [Unity 2022.3 DontDestroyOnLoad](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Object.DontDestroyOnLoad.html): persistence follows hierarchy ownership.
- [Addressables 1.22.3 scene loading](https://docs.unity3d.com/Packages/com.unity.addressables@1.22/manual/LoadingScenes.html): scene handles, activation behavior and Editor/player differences.
- [Addressables 1.22.3 memory management](https://docs.unity3d.com/Packages/com.unity.addressables@1.22/manual/MemoryManagement.html): paired load/release and bundle reference counts.
- [SetActiveScene](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SceneManagement.SceneManager.SetActiveScene.html): active scene controls lighting and default creation destination.
