# TREBRAVE — UNITY FULL 3D MIGRATION PLAN (v1 / DESIGN APPROVED)

> **Owner decision D02=C:** Full stylized 3D, hero/tower-defense Map Sông Quê as primary game.  
> **Reality:** Projectzombie Unity 2022.3 LTS is currently mostly **2D top-down with Physics2D, Tilemap and 2D hit/aim logic**. Vongxuyen reference is Three.js WebGL 3D, **not** Unity assets.  
> **Status:** Architecture proposal / engineering backlog, **no Unity 3D implementation completed in this documentation PR**.  
> **Dependencies:** D01 Defense primary; D03 4 Hero; D04 5 FUN relic; D05 active cast; D06 600s; D07 base-only loss, final boss kill win; D08 wood drop/pickup; D09 VX roster; D10 no allied army MVP.

## 1. Constraints and migration goals

1. Keep technical repository, existing Unity C# feature-based architecture, metadata/scene GUIDs, Addressables, save schema and **Legacy Survival 2D**.
2. Deliver playable **Unity 3D Defense scene**, stylized semi-chibi/heroic fantasy and mobile-friendly camera. **Do not repurpose 2D Tilemap/Collider/Rigidbody** for Defense physics.
3. Reuse **pure domain** logic (run lifecycle, combat math, stats, upgrade decisions, pooled abstractions) through interfaces; develop new **3D adapters** when existing code relies on Physics2D, SpriteRenderer, aim/collider/velocity in Vector2.
4. Keep exactly **one Hero controlled**/run, one equipped relic slot with **ACTIVE ghost aim**, Base + ten tower build sockets, dropped wood collected by Hero, 7 VX enemy types.
5. **No FriendlyArmy/Squad** in MVP. Phu collection and allied AI are future modules; do not block first 3D release.
6. Make functionality observable/testable through independent 3D test harness: FPS+RAM+allocation profiler on target Android device, input/physics/nav QA; avoid unproven '60 FPS zero GC' claims.

## 2. Current Unity 2D reuse audit

| Existing Projectzombie code | Decision | Migration guidance |
| --- | --- | --- |
| GameStateManager, GameState, pause policies | **REUSE service** | 3D Physics/gameplay clock obey IsPlaying, no second clock |
| MatchFlowService/Orchestrator | **ADAPT** | add DefenseScenarioSO/3D map addressable loading, leave old scene flow intact |
| WaveScheduler and timeline config | **REUSE core, NEW data** | 600s Defense timeline (34 events) and boss special spawn; don't reuse incompatible Legacy 900s |
| DamageData/DamageContext, IDamageable/IHealable | **REUSE domain** | 3D hitpoint, position, faction wrapper; different contact resolver |
| Enemy FSM, AttackStrategy, ITargetSelector<PlayerContext> | **ADAPT** | new 3D controllers/target adapter including Base and Tower; avoid global contract breakage |
| Physics2D TargetingUtility, CameraAwareSpawnLocator | **REPLACE for Defense 3D** | Physics.OverlapSphereNonAlloc or allocated-free physics queries as feasible, 3D lane spawn zones |
| Player 2D Rigidbody/Animator/SpriteRenderer | **NEW 3D adapter** | CharacterController/3D motor/Animator 3D with 4 Hero data models; keep legacy controller |
| WeaponManager MAX_WEAPONS=1 | **REUSE constraint; ADAPT runtime** | 5 FUN relic entries, manual activated 3D casts and ghost hitboxes; no auto relic firing |
| Existing upgrades/card systems | **REUSE domain** | new data-driven 3D defense pool; no extra relic slots; evaluate card effects |
| UI MVP/presentation | **ADAPT** | HUD for Base/Wood pickups/Tower/Boss and 3D raycast context |
| Photon Fusion, Meta Save, Ngũ Hành, Âm Dương, Gacha | **KEEP OPTIONAL** | no hard requirement for Defense MVP, preserve code/schema |
| Tilemap/2D art/Physics2D layer docs | **LEGACY-SPEC** | no authority for new 3D gameplay |

## 3. Scene, world scale and coordinate contract

**MapSongQue3D:** use Unity XZ plane, Y height. VX reference bounds ≈60m×40m, river x≈4.45..8.05, bridges z≈−10.5,−0.5,10.5; Base center x≈−12,z≈0; boss arena near x≈25,z≈0. Treat all as reference, not final locked meters; link to MapSongQueDefinitionSO transforms rather than scattering coordinates in code.

**Scene groups:** WorldTerrain, RiverWater, BridgesNorthMidSouth, BaseStructure, TowerPedestals10, SpawnZones, BossArena, HeroSpawn, PathGraph/NavMesh, Lighting, VFX, UI. Put colliders on physical blockers only; enemy/NavMesh agents must have traversal links at bridges. World-space UI labels/bars remain readable from camera.

**Camera:** full 3D stylized top-down/3-4, configurable perspective or orthographic projection depending on usability tests. Camera pitch/zoom/occlusion fade/follow/safe-area = **P06 PENDING**, not silently locked.

**Layers & faction:** WorldGround, Water, WorldObstacle, Player3D, Enemy3D, BaseStructure, TowerStructure, ProjectilePlayer, ProjectileEnemy, Loot, AimIndicator; validate 3D Physics Layer collision matrix. Legacy 2D layers remain independent.

## 4. Hero and one-active-relic contract

### Hero
- Four HeroDefinitionSO entries unlocked/selected in menu; one selected Hero per run. **P01** decides mapping of PZ kits and VX identities, without treating names as already resolved.
- 3D CharacterMotor, 3D Animator rig, combat hit shape, HeroStats and Health; Dash/Signature with telegraph, camera facing and target assist; hero death causes incapacitation, **not Lose**. **P03** must define rescue/respawn and anti-softlock.

### Relic
- Five FUN relic definitions (slipper, pot, pipe, mat, broom), **exactly one ACTIVE relic equipped**. Legacy auto/passive behavior must be mapped or isolated rather than accidentally co-triggered.
- Manual cast state IDLE → AIM_PREVIEW → VALIDATE → CAST → COOLDOWN → READY; CANCEL from preview never spends cooldown; charge/resource only when valid cast succeeds.
- 3D hit/aim shapes line, curved line, circle, cone, rectangle/projection, swept area. Ghost indicator must align to Physics3D collision and ground plane, not 2D shader coordinate.
- 3D VFX/FX conversions, projectile pooling, slow/fun status effects preservation flagged by **P07**.

## 5. Defense gameplay and wood pickups

- **Base:** HP baseline 500; Base destroyed triggers Lose exactly once. Do not use Hero death for game-over logic.
- **Tower:** ten fixed pedestal transforms, build level 1 baseline cost 50, upgrade levels 2/3 baseline 40/80, tower HP/range scale by data SO; tower projectile/line of sight uses 3D Physics.
- **Wood:** EnemyKilled spawns reachable 3D WorldWoodDrop with unique loot ID. **Hero must collect** via Collider trigger/Interaction; only claim updates wood count. Physics3D prevents pickups falling in river/obstacles, with post-spawn ground-safe resolution.
- **Phu:** not present in MVP; later uses same LootClaimService so both Hero and Phu cannot claim the same item.
- **No auto reward on kill** in wood economy. EXP can have its own pickup flow as per existing code.

## 6. Enemy + boss pipeline

**Roster:** magiap, huyetdoi, thuyquai, magiap_boss, quaidu, phicuong, yaksha_boss. Map 01 roster only. Unity legacy enemy prefabs remain.
**3D components:** EnemyDefinitionSO, EnemyController3D/FSM, 3D navigation agent, role-target policy, hit/collider/health, renderer+rig, animator, VFX/Audio, pooling hooks.
**TargetCatalog:** Base/Tower/Hero with IDamageable, faction and threat. ITargetSelector currently returns PlayerContext; don't globally change it without migration tests.
**Boss:** miniboss Ma Giáp Thống Lĩnh event t=108s; final Dạ Xoa special spawn ~510s with cleave/slam/crescentWave. **Final Boss death while Base alive => Win**; miniboss kill never triggers win.
**600s:** timeout handling while boss alive **P02 PENDING**; no false Win/Lose by duration.

## 7. Migration phases + milestones

| Phase | Work | Exit gates |
| --- | --- | --- |
| M0 — 3D feasibility | 3D scene, camera/mobile controls, URP profile, stylized placeholder meshes, hero move, terrain collision | Runs on target Android, Legacy scene still opens |
| M1 — Map + Base | Two banks, water/bridges, BaseHP, new 3D nav + physics, BaseDestroyed Lose | Enemy crosses bridge, attacks Base; Hero death no Lose |
| M2 — 1 enemy + WoodLoot + Tower | Spawn->Death->Loot->HeroPickup->Build->Tower attack loop | One 3D lane fully playable; no auto wood grant |
| M3 — Full 3 lanes + enemies | All seven VX archetypes, zone spawn, boss arena, target policy, NavMeshLinks | No bridge/ground/water shortcuts, boss telegraphs aligned |
| M4 — 4 Hero + 5 ACTIVE relics | 3D anim/skills, 5 ghost shapes/effects, upgrade/card UI, 1 slot | 4 selectable heroes, one Hero/Relic per run; effects test |
| M5 — Timeline + results | 600s clock, 5 phases, 34 events, miniboss, Dạ Xoa final, Base lose | Boss win once, Base lose once, P02/P03 resolved before QA gate |
| M6 — Content & mobile optimization | 3D environment art, animation, VFX, input/HUD, LOD/shadows, Addressables | Mobile visual readability and measured performance budget |
| M7 — Legacy regression & release candidate | Save/Addressables tests, existing 2D Legacy support, PlayMode, real-device CI/manual QA | No P0/P1 bugs, rollback/migration documented |

**NO Friendly Army/Commander/Squad in M0–M7.** Add only in post-MVP roadmap approved separately.

## 8. Required test suites

**EditMode:** Data validation (one relic slot, 4 Hero roster, 7 enemies, build slot 10, stage duration 600, unique eventIDs), reward idempotency, Base end-only, Boss end-only, stage timeline/pause, save separation.

**PlayMode 3D:** NavMesh paths by 3 bridges, 3D collider/no water route, tower placement raycast/cost, wood dropped/picked only once, boss triggers and telegraphs, Hero downed not Lose, Boss killed Win, Base destroyed Lose, mouse/touch interactions, 3D camera occlusion.

**Legacy regression:** old Unity 2D scene, Physics2D attacks, Addressables maps, weapons and Save still load in Legacy mode.

**Android QA:** 16:9 and tall portrait/landscape as supported; performance 10-minute stress run, memory/GC/frame-time, battery heat, GPU overdraw/light/shadow, enemy caps and drop caps; actual metrics to set after prototype.

## 9. Risk register

| Risk | Mitigation |
| --- | --- |
| Physics2D source calls embedded in gameplay | Separate 3D adapters, use interfaces, regression tests |
| Legacy 2D and Defense 3D share scene references | Separate Addressables labels/scene IDs, no in-place prefab conversion |
| Enemy targeting only PlayerContext | Bridge target abstraction for Base/Tower; keep Legacy target selector |
| Wood drops unreachable under terrain/water | Nav-snap spawn position, cap lifetime/stack, pickup QA |
| FPS drops from 3D assets and high enemy count | Pool, GPU instancing/LOD, baked lighting tests, progressive content |
| Hero 0HP leaves player unable to defeat final Boss | Resolve P03 with revive/respawn; no forced loss |
| Run reaches 600s before Boss dies | Resolve P02 before final runtime acceptance; avoid implicit timeout result |
| 4 Hero/5 relic effects require full 3D models/animations | Placeholder vertical slice first, art contract P01/P06/P07 |

## 10. Data and implementation repository rules

- Don't rename project/repository/package/namespace without separate migration approval; brand **TREBRAVE** is presentation metadata.
- Don't delete old Enemy/Relic/Survival assets or rewrite their 2D scripts in documentation PR.
- Every 3D asset has independent prefab/definition, versioned Addressable key and appropriate meta GUID. Shared models/scripts only when safe.
- New runtime code needs dedicated implementation branches/PRs; this draft PR edits **only Markdown** and annotations.
- **Source:** Vongxuyen MapBenQue/DefenseRules/EnemyFactory/YakshaBoss/stage01 and Projectzombie Developer Guide & game systems; treat source visuals as artistic references, not ready-to-import Unity assets.

## 11. Dependencies
- [Master GDD](ProjectZombie_GDD.md)
- [Owner Decision Record](DECISION_CHECKLIST_GDD_V6.md)
- [Map & Tower Spec](MAP_AND_DEFENSE_SYSTEM_SPEC.md)
- [Enemy Roster](ENEMY_ROSTER_VONGXUYEN.md)
- [Relic Input Spec](WEAPON_VISUAL_MANIFESTATION_RULES.md)
- [Scenario](LEVEL_SCENARIO_GUIDE.md)
