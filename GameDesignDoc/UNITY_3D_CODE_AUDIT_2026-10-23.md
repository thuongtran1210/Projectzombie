# TREBRAVE — AUDIT MÃ NGUỒN UNITY & KẾ HOẠCH TRIỂN KHAI 3D (2026-10-23)

> **UPDATE / ERRATUM (2026-10-23):** Repo hiện **đã có** nhánh `feature/migrate-to-3d@749c06de` (ahead main 3 commits, 26 files changed), bổ sung hybrid PlayerController/Physics3D utilities/3D sandbox generator. Báo cáo bên dưới là **snapshot cũ lúc nhánh chưa hiện diện trong danh sách trả về** và chỉ đánh giá `main`. **Đọc bản mới [3D Branch Code Review](UNITY_3D_BRANCH_CODE_REVIEW_2026-10-23.md) trước khi quyết định triển khai**, tránh kết luận “chưa có code 3D”.

> **Trạng thái:** AUDIT OF COMMITTED CODE + PROPOSED NEXT IMPLEMENTATION PRS.  
> **Repo:** `thuongtran1210/Projectzombie` · **base:** `main@657c8215b047d2a25e19832c1882f32270fa94d1` (commit 2026-10-01).  
> **Nhánh đã quan sát tại thời điểm audit:** `main` và `docs/gdd-v6-vongxuyen-unification-draft`; **chưa thấy nhánh code 3D trên GitHub**.  
> **Thông tin từ chủ dự án:** đang chuyển Projectzombie từ Unity 2D sang Unity full stylized 3D. **Không thể xác minh thay đổi chưa push lên GitHub**, và audit này không kiểm thử Unity Editor hay Android.  
> **Design contract:** [Master GDD v6.2](ProjectZombie_GDD.md), [Decision Record](DECISION_CHECKLIST_GDD_V6.md), [3D Migration](UNITY_3D_MIGRATION_PLAN.md) và [Yaksha Boss P04](YAKSHA_BOSS_PHASE_SPEC.md).  
> **Kết luận:** **Không port/viết lại tất cả**; giữ domain/services và Legacy 2D, phát triển scene/runtime 3D riêng theo hợp đồng rõ ràng; tránh sửa hệ 2D đang chạy để “giả thành 3D”.

## 1. Inventory có căn cứ

**Repository public main:** cây Git gồm khoảng **6,223 entries** (bao gồm .meta/assets), **657 file .cs** trong Assets, chỉ có **hai .unity scene được commit** `Assets/Scenes/SampleScene.unity`, `Assets/Scenes/SteamSlipTest.unity`. Không thấy `Assets/Features/Defense`, `MapSongQue3D.unity`, `YakshaBoss3D`, `TowerBuildService`, `WoodDropConfigSO` trong tree main; đây là dấu hiệu **chưa có feature 3D Defense được commit** trong branch audit, không phải khẳng định các file không tồn tại trên máy tác giả.

**Project version:** `ProjectSettings/ProjectVersion.txt` = **Unity 2022.3.62f3**.

**Package sẵn có:** Universal Render Pipeline **14.0.12**, Cinemachine **2.10.7**, Input System **1.7.0**, Addressables **1.22.3**, Animation Rigging **1.2.1**, Unity Test Framework **1.1.33**, Physics3D module (`com.unity.modules.physics`), AI module (`com.unity.modules.ai`). **Chưa có dependency `com.unity.ai.navigation` trong manifest**; chưa khẳng định có/không NavMeshSurface qua external package hoặc local resources. Đánh giá NavMesh baking/runtime API khi dựng scene 3D.

**Repo status:** nhánh `docs/gdd-v6-vongxuyen-unification-draft` chỉ chứa cập nhật tài liệu; **Draft PR #1** là documentation contract, không thể tính là implementation sprint.

## 2. Ma trận hệ thống (KEEP / ADAPT / BUILD NEW / LEGACY)

| Hệ thống / mã thực tế trong `main` | Phân loại | Bằng chứng / lý do | Hành động nhỏ nhất |
| --- | --- | --- | --- |
| `Features/Player/CharacterDataSO.cs` + `CharacterDatabaseSO.cs` | **KEEP + EXTEND** | Có `characterId`, `characterName`, `playerPrefab`, `basicAttackConfig`, `signatureSkillName`, `defaultRelic`; đã quản lý roster bằng SO | **Không tạo database Hero thứ hai không cần thiết**; bổ sung ID skill/ref nếu cần, hoặc tạo adapter mapping mới. Bảo lưu ID đã có |
| `Assets/_Data/CharacterS/Hero_{ThuSinh,DaoSi,ThanhDong,AnSi}.asset` | **KEEP AS DATA + NEW 3D PREFABS** | Bốn asset có `characterId=C001_ThuSinh,C002_DaoSi,C003_ThanhDong,C004_AnSi`; prefab đang trỏ asset cũ | Dùng alias/config trên SO cho `HERO_01–HERO_04`, không rewrite canonical IDs/Save keys; 3D prefab reference riêng |
| `Features/Player/PlayerController.cs` | **BUILD NEW 3D MOTOR** (2D stays Legacy) | `[RequireComponent(Rigidbody2D)]`, `Vector2`, `Physics2D.OverlapCircleNonAlloc`, hướng hình ảnh bằng localScale.x, `_rb.velocity` | `HeroMotor3D` + `HeroCombatInputAdapter3D` + `HeroAnimator3D`; share stats/health input where possible; tránh sửa PlayerController Legacy 2D |
| `Features/Shared/GameStateManager.cs` | **KEEP/EXTEND** | Có `GameState`, `IGamePausePolicy`, timeScale theo state | Share pause policy; hook Boss arena X, overtime, Hero respawn and wave tick to same gameplay clock, avoid independent Update timers |
| `Features/Shared/IDamageable.cs`, `DamageData.cs` | **KEEP CONTRACT / ADAPT** | IDamageable already exposes `Vector3 Position`, `TakeDamage(DamageData/DamageContext)`, `IsAlive` | Base/Tower implement IDamageable with 3D coordinates, add `IDefenseTarget` metadata (Faction/TargetKind) rather than breaking interface |
| `Features/MatchFlow/MatchFlowService.cs` | **KEEP CORE / ADAPT MODE ENTRY** | Loads SO and prefab Addressable `mapPrefabAddress` as GameObject, but fallback resolves `Stage_01_BambooForest`, enables static 2D Grid/Tilemap | Add `GameMode.Defense3D` + DefenseScenarioSO and 3D scene/Addressable path, leave legacy path unmodified; no accidental BambooForest fallback |
| `Features/Maps/StageDefinitionSO.cs` | **KEEP AS METADATA, EXTEND** | `stageId`, `mapPrefabAddress`, `timelineConfig`, `stageDurationSeconds`; descriptions/editor fields tailored Tilemap | Do not overload Legacy stage asset. Add independent `DefenseScenarioSO` linking 3D world config, 600s wave timeline, boss rule and tower slots |
| `Features/Spawners/Core/WaveScheduler.cs` | **REUSE LOGIC with TESTS** | Pure C# runner for `TimelineEvent`, separate from MonoBehaviour; `LevelDuration` uses `maxLevelDuration` or 900 fallback | New 600s config; **timeline end is NOT run end** (P02 overtime). Stop on Boss death; configure after-600 behavior. Consider avoiding `new List<TimelineEvent>` per Tick for Android hot path |
| `Features/Spawners/SpawnManager.cs` | **KEEP LEGACY / NEW 3D ADAPTER** | Uses `Collider2D`, `CameraAwareSpawnLocator`, and runtime strategies spawning around player/camera; consumes WaveScheduler | `DefenseSpawnDirector3D` receives scheduled events, spawn by named lane/zone NavMesh points; cap/priority/boss queue separate, no camera-based spawn for Defense |
| `Features/Spawners/Spatial/CameraAwareSpawnLocator.cs`, `ArenaBoundaryContext.cs` | **LEGACY ONLY** | Physics2D, Tilemap, Vector2 camera bounds and walkable map assumptions | `LaneSpawnLocator3D`, `MapSongQue3DNavGraph`/NavMesh and `BridgeNavLink`; keep old untouched |
| `Features/Enemies/FSM/Enemy.cs`, `ITargetSelector.cs` | **BUILD 3D ENEMY ADAPTER/FSM** | Enemy has `[RequireComponent(Rigidbody2D)]`, `Collider2D`, Vector2 distance; selector returns **PlayerContext** rather than Base/Tower | Separate `EnemyBrain3D`, `IDefenseTargetSelector` returning target metadata/`IDamageable`, `EnemyNav3D`. Avoid globally changing old `ITargetSelector` |
| `Features/Shared/TargetingUtility.cs` | **LEGACY PHYSICS2D** | Calls `Physics2D.OverlapCircleNonAlloc`, `Collider2D[]` | New `TargetingUtility3D` Physics3D overlap/raycast + faction filtering; share damage math |
| `Features/Weapons/WeaponManager.cs` | **KEEP SLOT/API / ADAPT 3D CAST** | `MAX_WEAPONS=1`, has `TriggerEquippedRelicSkill(AimResult)`; still has Collider2D hit hook | Keep 1 equipped; **Slipper only ENABLED** in Defense MVP, four relics locked. Adapter for 3D hit detection without changing old WeaponBase/2D Legacy code unnecessarily |
| `Features/Combat/Aiming/AimResult.cs` | **PARTIAL REUSE/ADAPT** | AimResult has `Vector2 Direction` but also `Vector3 TargetWorldPos`; existing `FromDirection` casts on XY | New `AimResult3D` or `AimProjection3D` on XZ with 3D valid hitpoint/range; never treat Vector2 direction as world XZ without explicit mapping |
| `Features/UI/HUD/RunHUDPresenter.cs` | **KEEP CONCEPT / NEW DEFENSE HUD VIEW** | Existing presenter integrates Player Health/EXP/Weapons and GameState | Add BaseHP, Wood, Tower slots/build, Boss bar, Hero respawn countdown, ACTIVE Slipper ghost. No squad dock in MVP |
| `Features/Spawners/LevelTimelineConfig.cs` + `Assets/_Data/Levels/*` | **NEW DEFENSE TIMELINE** | `maxLevelDuration` defaults 900s; StageDefinitionSO 600; Legacy data may conflict | Defense-only 600s/34 events SO with authoritative boss event at reference ~510s and overtime P02, no change Legacy assets |
| Multiplayer/Meta/Gacha/YinYang/Ngũ Hành | **PRESERVE OPTIONAL / LEGACY** | Existing features and services on main | Not dependencies of single-player Defense MVP (D12); no removal or new coupling |

## 3. Source-level blockers — thứ tự ưu tiên

### B01 — Chưa thấy Unity 3D Defense source trên remote (critical integration boundary)
- Only `main` + GDD docs branch visible; no committed MapSongQue3D scene / HeroMotor3D / 3D Boss.
- **Không kết luận 3D đang chưa được làm trên máy tác giả**; khi migration branch/commit được push, audit delta giữa branch đó và main trước khi tạo code PR, tránh conflict/duplicate công việc.
- **Gate:** biết base branch/version chính xác của nhánh đang triển khai Unity 3D.

### B02 — Legacy 2D components không thể gắn trực tiếp vào 3D prefab (P0)
- PlayerController và Enemy require Rigidbody2D; 2D colliders/Vector2 logic và camera spawn đã được dùng nhiều nơi.
- **Gate:** 3D hero and dummy enemy có thể di chuyển/tấn công trên ground XZ, không có 2D Collider dependency; Legacy vẫn hoạt động.

### B03 — Enemy chỉ target PlayerContext, không target Base (P0)
- `ITargetSelector.SelectTarget(Vector2, IPlayerRegistry)` returns `PlayerContext`. `Enemy.UpdateTarget` mang giả định player health/transform.
- **Gate:** new 3D enemy can select and damage a `BaseHealth : IDamageable` target; no cast/NullReference; 2D enemy remains unchanged.

### B04 — Thời gian 600s và overtime khác nhau (P0)
- `WaveScheduler` marks isTimelineEnded when `_matchTime>=LevelDuration` and `Tick` returns newly allocated List; `SpawnManager.Update` calls `Tick(Time.deltaTime,out _)`, ignoring the end output and continuing to update spawn strategies.
- **Gate:** use `600s` to close scheduled timeline events; **no new waves after 600**, but remaining enemies, Hero, Tower and Yaksha Boss can fight until Boss dies/Base destroyed. On victory stop/cancel **every** spawn producer, not only scheduler.

### B05 — 4 Hero IDs in committed SO disagree with provisional GDD aliases (P0 save compatibility)
- **Current canonical character IDs:** `C001_ThuSinh`, `C002_DaoSi`, `C003_ThanhDong`, `C004_AnSi`. GDD proposes `HERO_01..04` for design-neutral identifiers.
- **Never overwrite IDs just to match GDD!** Preferred: `HeroIdentityAliasSO` or field `designHeroId` maps `HERO_01->C001_ThuSinh`, `HERO_02->C002_DaoSi`, `HERO_03->C003_ThanhDong`, `HERO_04->C004_AnSi`, retaining canonical serialized ID, prefab, save and unlock records.
- Use `SkillDefinitionSO` for signature/ability IDs/display name; existing `CharacterDataSO` has `signatureSkillName` string and `CharacterAttackConfig`, so add references rather than a duplicate database.
- **Gate:** four choices load existing data; title/name edits in SO do not change canonical selection/save ID.

### B06 — WeaponManager active capability already exists but aims in XY (P0)
- `MAX_WEAPONS=1` and `TriggerEquippedRelicSkill(AimResult)` are available, **do not rewrite the entire system**. But `AimResult.Direction` is Vector2 and `FromDirection` moves on XY plane.
- **Gate:** Dép Tổ Ong one active skill with XZ ghost aim and valid 3D projectile; no advanced FUN effect, all four other FUN relics locked in Defense; no double automatic trigger.

### B07 — Missing 3D tower/wood/defense domain on remote (P0)
- Cannot find 3D Defense-specific BaseHealth/TowerBuildService/WorldWoodDropConfigSO in committed main tree.
- Build the **small vertical slice first**: Enemy killed → creates WoodDrop (initial TTL 90s borrowed from VX EXP gem config, not pre-existing wood default) → Hero collects once → wallet balance increases → tower build consumes wood at one pedestal; Base destroyed leads defeat.
- **Gate:** no auto wood credit, no double grant, no FriendlyArmy/Phu recruitment dependency.

### B08 — Boss two strategic states and result arbitration (P1)
- **P04 approved**: Dạ Xoa spawn ~510s → `ARENA_COMBAT` for `arenaHoldDurationSeconds = X` since spawn (unpaused) → `ASSAULT_BASE` follows 3D bridge/nav and attacks Base.
- **K03 remains unresolved:** numerical X and same-tick BossKilled/BaseDestroyed priority (record and configure later, not guessed).
- **Gate:** Boss death in either state Win immediately, cancels all future/pending spawns; only Base destroyed Lose. Overtime does not force strategic state.

## 4. Architecture slices and ownership

```text
GameModeRouter
  ├── LegacySurvivalLoader -> existing 2D Player/Enemy/Spawn/Tilemap
  └── Defense3DLoader -> DefenseScenarioSO -> DefenseRunController
                             ├── Shared GameState / GameplayClock
                             ├── MapSongQue3D / 3 bridges / LaneGraph
                             ├── HeroRuntime3D (4 aliases on CharacterDataSO)
                             │      ├── HeroRespawnConfigSO
                             │      └── ACTIVE Slipper 3D aim adapter
                             ├── DefenseSpawnDirector3D -> 3D EnemyFSM -> TargetCatalog
                             ├── WoodDrop3D (TTL 90) -> LootClaimService -> WoodWallet
                             ├── BaseHealth + TowerPedestals + TowerBuildService
                             ├── Yaksha2State (ARENA_COMBAT → ASSAULT_BASE after SO X)
                             └── DefenseHUD / RunResultResolver
```

**Interface boundaries (draft only):**
- `IGameplayClock`: `ElapsedSeconds`, `IsAdvancing`; time for waves/Boss X/loot TTL/hero respawn.
- `IDefenseTarget`: targetId, TargetKind, Transform/Position, IDamageable, Faction, Alive.
- `IDefenseSpawnService`: spawn by enemyId/zone/lane, `StopAll()`, `ClearPending()`.
- `IWoodLootClaimService`: spawn(EnemyKillId), tryClaim(lootId, actorId, runId), despawn/expire.
- `IDefenseRunResultResolver`: TryFinish(Win/Lose, cause, runId) exactly once.
- Existing `IPlayerRegistry`, `ITargetSelector`, `CameraAwareSpawnLocator` remain Legacy adapters.

**Caveat:** These interfaces/classes are **proposals**, not code already present in GitHub.

## 5. Implementation PR order — after syncing 3D work to GitHub

| PR | Deliverable | Exit test / definition of done | Depends on |
| --- | --- | --- | --- |
| **3D-01** | Mode routing + `DefenseScenarioSO` + 3D bootstrap scene shell + shared pause/clock | Defense loads 3D scene; Legacy loads existing 2D; no scene/asset GUID break; 600s config is Defense-only | Branch with current 3D work |
| **3D-02** | Hero3D motor + camera + ID mapping to 4 existing CharacterDataSO assets | One 3D Hero moves in XZ; menu can resolve all 4 by IDs; pause + no 2D physics; old save IDs stable | 3D-01 |
| **3D-03** | Map Sông Quê three bridges + BaseHealth + 3D nav graph | One 3D dummy enemy can cross at bridge and hurt Base; BaseHP=0 Loss once; test all bridge links | 3D-02 |
| **3D-04** | Defense 3D Enemy FSM/target/catalog + 3D wave spawn zones | One enemy type targets Hero/Tower/Base without PlayerContext cast; no water shortcut, no camera spawning | 3D-03 |
| **3D-05** | WoodDrop3D (TTL90) + Hero pickup + WoodWallet + one tower pedestal/tower | Enemy death drop; no wallet grant until claim; claim exactly once; spend builds and tower attacks | 3D-04 |
| **3D-06** | ACTIVE Dép Tổ Ong basic 3D cast/aim + relic availability gating | Slipper 3D ghost + projectile/damage/CD, four other relics locked, no FUN effects; 1 slot | 3D-02/3D-04 |
| **3D-07** | Stage 600s, final Dạ Xoa 2-state, Hero respawn and overtime/results | Config X transitions Arena→Base on gameplay clock; level-dependent respawn; at 600 overtime; Boss death immediate Win/stop spawns | 3D-04/05/06 |
| **3D-08** | Defense HUD, full 3 lane content (VX7), test/balance/mobile integration | Base/Wood/Tower/Boss/Respawn shown; legacy regression; Android profiling, no release-blocking bugs | 3D-01..07 |

**PR-03 to PR-05 can be sliced smaller** if scope/merge conflict grows. First vertical slice can replace advanced 3D art with primitives; no wait for custom models.

## 6. First playable vertical slice (smallest real game loop)

**Prefab/scene minimum**: `MapSongQue3D_Prototype.unity`, `Hero3D_Prototype`, `BaseStructure`, `TowerSocket_01`, `ArrowTower_Prototype`, `Enemy_Magiap_Prototype`, `WoodDrop_Prototype`, `DefenseHUD_Prototype`. Use primitive art and temporary geometry if art not ready.

**Happy path**
1. Start Defense3D, spawn one Hero at `HeroInitialSpawnPoint`.
2. One Ma Giáp prototype spawns at East lane, walks over 3D bridge.
3. Hero hits enemy; EnemyKilled spawns 1 WoodDrop (90s).
4. Hero walks to loot; wallet credit only after successful pickup.
5. Hero builds one Tower at fixed 3D pedestal; tower targets another enemy.
6. Enemy reaches Base and deals damage; BaseHP=0 => Loss. Separate test calls FinalBossDefeated => Victory, stop spawns (full Yaksha 2-state later).

**Out-of-scope:** all five relics, special FUN FX, friendly army/squad, Photon coop, reclaim system, extra maps, advanced 3D art polish.

## 7. Verification plan

**Static/source**: confirm no accidental changes to main and no renaming of canonical hero IDs; Unity version/package lock; asset GUID references; Defense/Legacy mode isolation.

**EditMode**:
- Hero alias ID mapping; exactly one relic enabled, one equipped.
- GameResult once; BaseDestroyed Lose, BossKilled Win, HeroDowned not Lose.
- WoodDrop claim one-time and 90s expiry on gameplay clock, pause frozen.
- Timeline 600s ends wave events without ending run; Boss X pause/resume/overtime transition.

**PlayMode 3D**:
- 3 navigable bridges, ground enemies cannot cross river without bridge.
- Hero movement/input/aim works on XZ; 3D collider damage and Base target.
- Tower placement/cost; Hero pickup 90s, no double claim/pooling stale state.
- Yaksha Arena -> after SO X -> valid bridge assault Base; Boss died mid-transition cancel.
- Hero respawn delay monotonically nondecreasing by level at initial Transform.

**Android / regression**: install/test build on physical target device, measure frame times/GC/render load, 10+ minute overtime stress and wave caps; re-run 2D Legacy scene and old save/load to ensure backward compatibility. **Not run in this audit**.

## 8. Decisions and next code gate

**No product question must block a 3D-01 mode/scene shell**, but coding against `main` alone risks overwriting owner's unpublished 2D→3D work.

1. Owner 3D work should be available in a **GitHub branch/commit**, or otherwise its changed-file inventory must be supplied for diff-based audit. No speculative status claims about local files.
2. Use this audit and `UNITY_3D_MIGRATION_PLAN.md` as implementation boundaries, base first code PR on actual 3D branch once known.
3. Keep Draft PR #1 docs-only; do not merge runtime code into GDD PR.
4. **K03** (X seconds and same-tick Boss/Base result priority) can remain SO/test TODO for early vertical slice but must be resolved before release of 3D-07.

## 9. Direct evidence (at reviewed main commit)

- [ProjectVersion.txt](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/ProjectSettings/ProjectVersion.txt), [manifest.json](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Packages/manifest.json)
- [CharacterDataSO.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Player/CharacterDataSO.cs), [CharacterDatabaseSO.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Player/CharacterDatabaseSO.cs), [4 Hero Assets](https://github.com/thuongtran1210/Projectzombie/tree/657c8215/Assets/_Data/CharacterS)
- [PlayerController.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Player/PlayerController.cs), [Enemy.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Enemies/FSM/Enemy.cs), [ITargetSelector.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Enemies/ITargetSelector.cs)
- [MatchFlowService.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/MatchFlow/MatchFlowService.cs), [StageDefinitionSO.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Maps/StageDefinitionSO.cs)
- [WaveScheduler.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Spawners/Core/WaveScheduler.cs), [SpawnManager.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Spawners/SpawnManager.cs), [CameraAwareSpawnLocator.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Spawners/Spatial/CameraAwareSpawnLocator.cs)
- [IDamageable.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Shared/IDamageable.cs), [TargetingUtility.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Shared/TargetingUtility.cs)
- [WeaponManager.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Weapons/WeaponManager.cs), [AimResult.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Combat/Aiming/AimResult.cs)
- [GameStateManager.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/Shared/GameStateManager.cs), [RunHUDPresenter.cs](https://github.com/thuongtran1210/Projectzombie/blob/657c8215/Assets/Features/UI/HUD/RunHUDPresenter.cs)
