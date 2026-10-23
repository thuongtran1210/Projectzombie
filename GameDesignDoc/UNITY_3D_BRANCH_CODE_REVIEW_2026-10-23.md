# TREBRAVE — CODE RE-REVIEW: feature/migrate-to-3d (2026-10-23)

> **Scope:** đọc code GitHub ở `feature/migrate-to-3d@749c06de7835caa2b2d3af5b0994bb26e3a7ab65` so với `main@657c8215b047d2a25e19832c1882f32270fa94d1`. Đây là **static source review**, **không phải** Unity compile, PlayMode, Android build hay kiểm thử trên máy tác giả.  
> **Git compare:** nhánh feature **ahead main 3 commits, behind 0**, có **26 changed files** (+ các file phụ thuộc); last commit **2026-10-15**. `main` và Draft PR #1 (docs) chưa merge vào nhau.  
> **Correction of previous audit:** ngày 2026-10-23 audit cũ chỉ thấy main/docs; nay có `feature/migrate-to-3d` được push. Báo cáo này supersedes những khẳng định “không có nhánh code 3D trên remote” trước đó.  
> **Risk:** review prioritizes source lines clearly showing behavior; severity is based on likely impact; reproduction must still be verified in Editor/PlayMode.

## 1. Reality: what the 3D branch actually implements

**Implemented in source**
- `PlayerController` hybrid 2D/3D, detects `Rigidbody` in Awake and translates `Vector2` move input to XZ `Rigidbody.velocity`.
- `MovementPhysicsUtility3D` 3D obstacle spherecast/dash point planning, ground height raycast, NavMesh sampling and teleport grounding.
- `TargetingUtility3D` Physics3D overlap targeting enemy candidates.
- `AreaEffectCaster3D` AoE/knockback on XZ.
- `PlayerMagnetTrigger` now can generate `SphereCollider` for 3D.
- `Migration3DSetupTool` editor menu generating primitive-based sandbox, ground/obstacles/camera/dummy enemies/player.
- `Packages/manifest.json` now includes **`com.unity.ai.navigation: 1.1.5`**, `com.unity.probuilder: 5.2.3`, `com.unity.collections: 2.1.4`, `com.unity.mathematics: 1.2.6`. This corrects the old audit's NavMesh package observation.

**Not yet committed on this branch** (tree inspection of 6,238 entries; do not infer local status)
- No `Assets/Scenes/3D_Migration/Scene_3D_Playground.unity`: tool can generate it **locally**, but it is not stored in Git.
- No `MapSongQue3D.unity`, DefenseScene/DefenseMode bootstrapping, fixed Tower build pads, BaseHealth, WoodDropWallet, VX7 enemy 3D prefabs, Yaksha phase FSM; branch commits Phase 0/1 foundation and not GDD v6.2 full game.
- No migration-specific 3D PlayMode test files detected in Assets/Tests tree.

## 2. Findings — clear bugs and priority

### F01 — P0: 3D magnet triggers never dispatched to collectible handler

- **Source:** `Assets/Features/Player/PlayerMagnetTrigger.cs:68–76,148–166`.
- `PlayerMagnetTrigger.SetupMagnetCollider()` adds a `SphereCollider` and sets `isTrigger=true` for Rigidbody3D player. However `MagnetTriggerProxy` implements **only `OnTriggerEnter2D(Collider2D)`**, not `OnTriggerEnter(Collider)`.
- **Impact:** 3D-trigger contact has no callback and existing `ICollectible.StartMagnetEffect` is not invoked; likely broken magnet/pickup in 3D sandbox, even with valid 3D pickup collider/layer.
- **Repair:** add `private void OnTriggerEnter(Collider other)` in proxy; resolve `ICollectible` on target or parent as appropriate, call same `OnCollectibleDetected`; keep 2D callback. Validate trigger matrix, Rigidbody3D presence and single collectible claim. Note: existing `ExpGem` is sprite-based 2D, so 3D wood pickup needs its own actual 3D collectible/claim flow.
- **Test:** 2D exp pickup remains working; 3D collider enter fires exactly one pickup callback, no repeated grant.

### F02 — P0 when 3D network movement is active: unchecked Rigidbody2D access

- **Source:** `Assets/Features/Player/PlayerController.cs:425–442`.
- New hybrid controller caches `_rb=GetComponent<Rigidbody2D>()`, `_rb3D=GetComponent<Rigidbody>()`. In `ApplyNetworkMovement`, invalid game state/dead/coop-downed branches all execute `_rb.velocity=Vector2.zero` **without checking _rb for null**, whereas `ProcessMovement()` correctly handles both.
- **Impact:** Rigidbody3D-only Hero can throw `NullReferenceException` if `ApplyNetworkMovement` executes under any of these conditions. Multiplayer is optional for Defense MVP (D12), but hybrid utility must not introduce regression.
- **Repair:** share a `StopMovement()` helper for Rigidbody2D and Rigidbody3D paths; avoid raw `_rb` use when `Is3D`; test alive/downed/pause via both network and local input.
- **Test:** no exceptions in 3D network movement when pause/die/downed.

### F03 — P0 for 3D build: Camera3DFollower lives under Editor-only assembly

- **Source:** `Assets/Editor/Migration/Migration3DSetupTool.cs:207–221`; generator adds follower around lines 126–129.
- `Camera3DFollower : MonoBehaviour` is declared inside **`Assets/Editor`**, where scripts are compiled into Editor-only code by Unity. Generated scene serializes this component, but the corresponding script is not included in normal Android/player assemblies.
- **Impact:** camera follow is not guaranteed to exist in compiled player, and prefab/scene can contain missing editor-only script references after build. Prototype may appear to work in Unity Editor but not on Android.
- **Repair:** move `Camera3DFollower` into `Assets/Features/Player/Camera/Camera3DFollower.cs` or `Assets/Features/Camera3D`, in runtime assembly; keep `Migration3DSetupTool` Editor-only. Commit accompanying `.meta`; regenerate sandbox to reference runtime component.
- **Test:** development/player Android build; scene camera follows and has no missing scripts.

### F04 — P1: 3D AoE knockback direction loses world Z on interface conversion

- **Source:** `Assets/Features/Shared/AreaEffectCaster3D.cs:48–72`, `Assets/Features/Shared/Interfaces/IStatusReceiver.cs:37`.
- AoE constructs `knockbackDir : Vector3 = (targetPos-center)` and sets `knockbackDir.y=0`, correct XZ horizontal direction. Rigidbody3D path `AddForce(Vector3)` is fine **if RB is on collider GameObject**. But receiver fallback calls `receiver.ApplyKnockback(knockbackDir,...)`; interface expects `Vector2` and Unity converts Vector3→Vector2 using **(x,y)**, discarding Z (Y was zeroed).
- **Impact:** enemy without same-GameObject Rigidbody3D has wrong/zero knockback when attack direction runs along world Z.
- **Repair:** create explicit 3D status/knockback interface `I3DKnockbackReceiver.ApplyKnockback(Vector3,...)` or `new Vector2(knockbackDir.x,knockbackDir.z)` **only if receiver's 3D implementation maps Vector2 as XZ**; never depend on implicit Vector3→Vector2 projection. For child hit colliders, `GetComponentInParent<Rigidbody>()` may be needed.
- **Test:** AoE from four compass directions ±X/±Z with targets using 3D Rigidbody and status fallback.

### F05 — P1: 3D sandbox adds HealthSystem after PlayerController Awake

- **Source:** `Assets/Editor/Migration/Migration3DSetupTool.cs:89–102`; `PlayerController.cs:86–96`, `PlayerStats.cs:94–101`.
- Tool creates `PlayerStats`, `PlayerMagnetTrigger`, then `PlayerController`, and only **afterwards** `HealthSystem`. `PlayerController.Awake` caches `GetComponent<HealthSystem>()` while HealthSystem absent, leaving `_healthSystem=null`. PlayerStats.Awake already attempted to sync HP via GetComponent and missed it.
- **Impact:** Hero death check in `ProcessMovement` can be bypassed, and early stats-to-HP sync may fail in generated sandbox; the scene is not equivalent to production Hero prefab.
- **Repair:** create on inactive GameObject, add `HealthSystem` and all dependencies first, then `PlayerStats/Controller`, finally activate; or explicit post-add dependency injection/refresh; do not rely on Awake order for runtime-generated prefabs.
- **Test:** change Hero HP to zero after sandbox starts; movement should stop and source health component should be cached/valid.

### F06 — P1: Generated 3D scene is not source-controlled; missing new script meta GUIDs

- **Source:** Git tree of `feature/migrate-to-3d`: no `Assets/Scenes/3D_Migration/Scene_3D_Playground.unity`; no `.meta` files for `Assets/Editor/Migration/Migration3DSetupTool.cs`, `Features/Shared/{MovementPhysicsUtility3D,TargetingUtility3D,AreaEffectCaster3D}.cs`.
- **Impact:** Unity generates .meta locally upon import, GUIDs can differ between machines; committing scene/prefab references later from different machine without consistent meta can cause missing script references.
- **Repair:** commit `.meta` for all scripts; if a reproducible sandbox is intended, commit generated scene + directory .meta and its dependencies, or document it as deliberately generated-only and keep tests programmatic.
- **Test:** clean clone, open Unity, no missing references after regenerating/opening sandbox.

### F07 — P1: TargetingUtility3D can return duplicate/wrong targets for multi-collider enemies

- **Source:** `Assets/Features/Shared/TargetingUtility3D.cs:42–128,145–160`.
- `FindTarget`/`FindTargetsNonAlloc` use `Collider.transform` (may be hitbox child), call `hit.TryGetComponent<HealthSystem>` only on collider GameObject (can fail if HealthSystem is on parent), and do not deduplicate multiple colliders of the same enemy.
- **Impact:** priority LowestHealth/HighestHealth can treat child collider with missing health as HP=0; multi-target AoE can hit same enemy twice; aim can lock onto hitbox child instead of enemy root.
- **Repair:** resolve target/HealthSystem from parent/root and dedup by stable enemy instance ID; check `IsAlive`, enemy faction, radius, layer, max target overflow. Ensure helper does not assume Enemy MonoBehaviour with mandatory Rigidbody2D for 3D.
- **Test:** one enemy with 3 colliders, one dead enemy, tower/base non-enemy colliders, 120+ simultaneous colliders.

### F08 — P2: Dash safety utility is separate but not used by actual PlayerController 3D dash

- **Source:** `MovementPhysicsUtility3D.cs:120–174` performs spherecast and floor stepping; `PlayerController.cs:475–480` simply sets Rigidbody3D velocity for dash. No call from actual 3D dash path to `CalculateDashDestination`.
- **Impact:** anti-clipping/edge snap promises in the new utility do not presently govern the Hero dash behavior. Regular Rigidbody contact may stop walls but not guarantee intended swept collision/ledge policy, particularly at fast dash speed.
- **Repair:** choose dash-motion owner; use continuous collision detection or validated sweep/traversal in Rigidbody3D motion loop (not `transform.position` teleports), and real 3D slopes/bridges.
- **Test:** high dash velocity into thin wall, bridge edge, water, slopes and obstacles.

### F09 — P2: Physics3D utilities need stronger walkability checks

- **Source:** `MovementPhysicsUtility3D.cs:33–35,95–108,179–214`.
- `GroundMask` includes `Default`; prefab props with Default collider may be treated as floor. `ClampToWalkableGround` returns nearest NavMesh sample **without applying the same obstacle/ground validation**, and fallback origin is not verified walkable. `NavMesh.SamplePosition` may choose nearby surface through topology unless path reachability is separately checked.
- **Impact:** teleport could land on unwanted surface/behind wall/in water, especially Map Sông Quê with a river and bridges.
- **Repair:** ground-only mask and walkable-area policy; `NavMesh.CalculatePath` reachability where required; post-sample 3D obstacle/height check, validate fallback.
- **Test:** invalid water-side teleports, under bridge, near obstruction, between two banks.

## 3. GDD-v6.2 coverage on 3D branch

| Required design feature | Static review of branch | Conclusion |
| --- | --- | --- |
| Unity full stylized 3D | **Partial**: hybrid player 3D controller, physics helpers, sandbox generator + packages | Foundation/prototype, not game mode implemented |
| Map Sông Quê river/2 banks/3 bridges/10 tower sockets | No checked-in 3D Map Sông Quê scene or Tower pad system | **NOT COMMITTED** |
| 4 selectable Hero SO IDs | Existing `CharacterDataSO` assets have canonical `C001_ThuSinh,C002_DaoSi,C003_ThanhDong,C004_AnSi`; no new 3D-selection override in delta | **LEGACY DATA reusable**, alias mapping TODO |
| 1 Slipper ACTIVE relic, 4 locked | WeaponManager already allows 1 and active API in Legacy; no 3D Slipper runtime/lock policy in delta | **PARTIAL / TODO** |
| 7 Vongxuyen enemies / new 3D nav | No VX7 3D EnemyFSM/prefabs in delta | **NOT COMMITTED** |
| WoodDrop3D TTL90, Hero pickup → WoodWallet → tower | Magnet trigger hybrid exists but F01; no world Wood economy/Tower runtime in delta | **NOT COMMITTED** |
| BaseHP death only Lose / Boss death instant Win | No Defense-specific result resolver in delta | **NOT COMMITTED** |
| Hero level-scaled respawn at initial spawn | No new Defense respawn implementation in delta | **NOT COMMITTED** |
| 600s waves/overtime | Existing Legacy `WaveScheduler`; no 3D Defense timeline override in delta | **NOT COMMITTED** |
| Dạ Xoa 2 state Arena→X sec→Assault Base | No YakshaBoss3D FSM/config in delta | **NOT COMMITTED** |

**Do not merge this code branch blindly into main** until critical F01–F05 are reviewed and Unity tests pass; these are code-review findings and not automatic proof of build failure.

## 4. Recommended repair order, separate from GDD Draft PR #1

1. **P0 correctness patch**: F01 (3D trigger), F02 (network move guard), F03 (move Camera3DFollower out of Editor). Save `.meta` for new scripts F06.
2. **Physics/AI foundation patch**: F04 (XZ knockback), F05 (sandbox Health initialization), F07 (multi-collider target dedup). Add EditMode/PlayMode tests.
3. **Movement validation patch**: F08/F09 (actual dash and teleport/walkable safety).
4. **First Defense vertical slice after fixes**: MapSongQue3D primitive scene, one Hero, one Enemy, Base HP, 3D nav on one bridge, dropped wood pickup, one tower; then scale to VX7 and Yaksha P04.

**User-facing status:** 3D migration has started on `feature/migrate-to-3d`, **not yet a playable TREBRAVE Defense Map 01 according to committed source**. This report does not modify any .cs files. Runtime code corrections should go into a code PR based on `feature/migrate-to-3d`, not into this docs-only PR.

## 5. Source URLs

- [Compare main...feature/migrate-to-3d](https://github.com/thuongtran1210/Projectzombie/compare/main...feature/migrate-to-3d)
- [PlayerMagnetTrigger](https://github.com/thuongtran1210/Projectzombie/blob/feature/migrate-to-3d/Assets/Features/Player/PlayerMagnetTrigger.cs)
- [PlayerController](https://github.com/thuongtran1210/Projectzombie/blob/feature/migrate-to-3d/Assets/Features/Player/PlayerController.cs)
- [AreaEffectCaster3D](https://github.com/thuongtran1210/Projectzombie/blob/feature/migrate-to-3d/Assets/Features/Shared/AreaEffectCaster3D.cs)
- [IStatusReceiver](https://github.com/thuongtran1210/Projectzombie/blob/feature/migrate-to-3d/Assets/Features/Shared/Interfaces/IStatusReceiver.cs)
- [Migration3DSetupTool](https://github.com/thuongtran1210/Projectzombie/blob/feature/migrate-to-3d/Assets/Editor/Migration/Migration3DSetupTool.cs)
- [TargetingUtility3D](https://github.com/thuongtran1210/Projectzombie/blob/feature/migrate-to-3d/Assets/Features/Shared/TargetingUtility3D.cs)
- [MovementPhysicsUtility3D](https://github.com/thuongtran1210/Projectzombie/blob/feature/migrate-to-3d/Assets/Features/Shared/MovementPhysicsUtility3D.cs)
- [Packages manifest](https://github.com/thuongtran1210/Projectzombie/blob/feature/migrate-to-3d/Packages/manifest.json)
