# TREBRAVE / PROJECTZOMBIE — DOCUMENT REFACTOR LEDGER v6.2

> **Status:** 12/12 product decisions OWNER-APPROVED (2026-10-23). **Implementation remains separate/in progress under owner's 2D→3D migration**; P01–P08 now owner-approved at design level (**P04 = 2 boss strategic states**); K01–K06 remain parameter/engineering-detail questions, see [Decision Record](DECISION_CHECKLIST_GDD_V6.md).  
> **Scope:** restructure documentation to make new TREBRAVE / full Unity 3D Defense first-class while protecting Projectzombie Unity 2D Legacy mode.  
> **Document-only Draft PR:** no Unity scene/script/asset refactor is claimed or committed.

## 1. Authority hierarchy / single source of truth

| Level | Files | Authority |
| --- | --- | --- |
| L0 Game Contract | [ProjectZombie_GDD.md](ProjectZombie_GDD.md), [DECISION_CHECKLIST_GDD_V6.md](DECISION_CHECKLIST_GDD_V6.md) | TREBRAVE, D01–D12, win/loss, 3D target, 1 Hero/Relic, core scope |
| L1 Feature Contract | [MAP_AND_DEFENSE_SYSTEM_SPEC.md](MAP_AND_DEFENSE_SYSTEM_SPEC.md), [ENEMY_ROSTER_VONGXUYEN.md](ENEMY_ROSTER_VONGXUYEN.md), [LEVEL_SCENARIO_GUIDE.md](LEVEL_SCENARIO_GUIDE.md), [WEAPON_VISUAL_MANIFESTATION_RULES.md](WEAPON_VISUAL_MANIFESTATION_RULES.md) | Map / wood drop / tower / 7 enemy / boss / active relic / stage |
| L1.5 Engineering Migration | [UNITY_3D_MIGRATION_PLAN.md](UNITY_3D_MIGRATION_PLAN.md) | Existing 2D → Unity full 3D, adapters and Legacy compatibility |
| [YAKSHA_BOSS_PHASE_SPEC.md](YAKSHA_BOSS_PHASE_SPEC.md) | **NEW — OWNER-APPROVED P04** | Two strategic states ARENA_COMBAT → after X seconds from spawn → ASSAULT_BASE, K03 numeric/tie pending |
| [UNITY_3D_CODE_AUDIT_2026-10-23.md](UNITY_3D_CODE_AUDIT_2026-10-23.md) | **NEW SOURCE AUDIT OF COMMITTED MAIN** | Classified existing Unity 2D dependencies and reuse potential; 3D branch not yet visible on GitHub; eight proposed implementation PRs |
| L2 Art, Technical | Art/VFX, physics layers, tilemap, UI, audio, profile and addressables | Must follow L0; existing 2D specs are **Legacy-specific** |
| L3 Optional | Multiplayer, cloud save, monetization, Ngũ Hành/Âm Dương | Code retained D12; may stay outside Defense MVP |
| L4 History | GDD v5.1 in Git history, old 20-minute survival/six-slot specs | Not authoritative for Map 01 TREBRAVE |

**Approved immutable rules:** main mode Defense, Legacy Survival retained; Unity target full stylized 3D; 4 selectable Heroes and 1/run; 5 FUN relic choices and 1 ACTIVE equipped/run; 600s timeline; **Base death ONLY Lose; Dạ Xoa final boss death Win**; wood drops collectible by Hero; 7 VX Map01 enemy; Hero/Tower only MVP; TREBRAVE brand, PZ repo technical; legacy optional code retained.

**P01–P03/P05–P08 approved:** four Hero skill/identity IDs are configured in ScriptableObjects; 600s triggers overtime if Boss still lives; Hero respawns at starting transform after level-scaled countdown; WoodDrop TTL default 90s (borrowed from VX **EXP gem** lifetime, not wood default); full-3D migration is actively being performed by owner, scope settings not yet independently verified; MVP only Slipper ACTIVE/basic FX, 4 other FUN relics LOCKED; Dạ Xoa death instantly triggers Victory and stops all further spawns.

**P04 OWNER-APPROVED:** Dạ Xoa uses ARENA_COMBAT after spawn → after **X gameplay seconds measured since BossSpawn** transitions to ASSAULT_BASE, navigates to attack Base. [P04 Boss Phase Spec](YAKSHA_BOSS_PHASE_SPEC.md). **K03 pending:** X exact value, same-tick Boss/Base death arbitration; K01–K06 also contain other numeric/engineering details.

## 2. File-by-file refactor status

| File | Status on draft PR | Next task |
| --- | --- | --- |
| [ProjectZombie_GDD.md](ProjectZombie_GDD.md) | **UPDATED v6.2 / OWNER DESIGN APPROVED** | Fill Pxx after owner follow-up; not claim Unity runtime is 3D yet |
| [DECISION_CHECKLIST_GDD_V6.md](DECISION_CHECKLIST_GDD_V6.md) | **UPDATED D01–D12 + P01–P03/P05–P08 APPROVED** | Track K03 X and simultaneous event priority, K01–K06 params |
| [MAP_AND_DEFENSE_SYSTEM_SPEC.md](MAP_AND_DEFENSE_SYSTEM_SPEC.md) | **UPDATED** 3D, wood pickup 90s, tower-only MVP | Balance and 3D assets, QA |
| [ENEMY_ROSTER_VONGXUYEN.md](ENEMY_ROSTER_VONGXUYEN.md) | **UPDATED** VX7, final boss, 3D prefab targets | Prefab creation/AI design/tests |
| [LEVEL_SCENARIO_GUIDE.md](LEVEL_SCENARIO_GUIDE.md) | **UPDATED** 600s overtime, Boss instant win + stop spawns, Base only loss, legacy timeline isolated | P02/P03/P04 event resolution |
| [WEAPON_VISUAL_MANIFESTATION_RULES.md](WEAPON_VISUAL_MANIFESTATION_RULES.md) | **UPDATED** Slipper-only MVP, 4 FUN items locked, one slot, 3D aim ghost, no advanced FUN effects | P07 effect mapping |
| [UNITY_3D_MIGRATION_PLAN.md](UNITY_3D_MIGRATION_PLAN.md) | **NEW** 3D scene/pipeline migration in progress per owner, Legacy 2D preserved and migration phases | P06 architecture/asset spike |
| [README.md](README.md) | **UPDATED** design hub / approved vs pending | Audit links and optional specs |
| [FUN_COMBAT_AND_ENEMY_MECHANICS_DOC.md](FUN_COMBAT_AND_ENEMY_MECHANICS_DOC.md) | **LEGACY** combat effects reference, enemy list replaced | Later split reusable FUN effects into 3D active specs |
| [TILEMAP_DESIGN_SPEC.md](TILEMAP_DESIGN_SPEC.md) | **LEGACY ONLY** 2D/PPU 64 | No longer defines Map 01 TREBRAVE |
| [PHYSICS_LAYER_DESIGN_SPEC.md](PHYSICS_LAYER_DESIGN_SPEC.md) | **LEGACY 2D**; 3D needs dedicated layer matrix | Add 3D-only layer/collision/raycast guide after P06 |
| [ART_VFX_STYLE_GUIDE.md](ART_VFX_STYLE_GUIDE.md) | **LEGACY 2D**; 3D target from TREBRAVE art vision | New TREBRAVE 3D art bible, rigs, silhouette/LOD |
| [GAME_UI_STYLE_GUIDE.md](GAME_UI_STYLE_GUIDE.md) | **NEEDS 3D DEFENSE HUD REFACTOR** | Base, wood pickup, tower, boss, 5 relics active; NO Squad dock MVP |
| [UI_PROMPT_AND_ART_GUIDE.md](UI_PROMPT_AND_ART_GUIDE.md) | **LEGACY / REVIEW** | New 3D camera and UI style after P06 |
| [AUDIO_SYSTEM_DESIGN_SPEC.md](AUDIO_SYSTEM_DESIGN_SPEC.md) | **KEEP** services; extend 3D spatial audio | Add wood pickup/base/tower/boss alerts |
| [CLOUD_SAVE_SYSTEM_SPEC.md](CLOUD_SAVE_SYSTEM_SPEC.md) | **KEEP**; migration required | Separate legacy vs Defense and wood run-only |
| [MULTIPLAYER_ONLINE_SYSTEM_SPEC.md](MULTIPLAYER_ONLINE_SYSTEM_SPEC.md) | **KEEP OPTIONAL** D12=A | Co-op Defense out of MVP |
| [ADMOB_MONETIZATION_SPEC.md](ADMOB_MONETIZATION_SPEC.md) | **KEEP OPTIONAL** D12=A | Monetization not required for first Defense slice |
| [PERFORMANCE_OPTIMIZATION_REPORT.md](PERFORMANCE_OPTIMIZATION_REPORT.md) | **HISTORICAL BASELINE** | Measure true Android Unity 3D performance |
| Uprades.doc | **LEGACY PENDING AUDIT** | Inspect original binary before archive/migration |
| ../README.md | **OLD UNITY 2D README / Legacy-current-runtime** | Display accurate present code vs approved future TREBRAVE target |
| ../DEVELOPMENT_GUIDE.md | **KEEP** feature-based architecture | Add 3D implementation modules only when code exists |

**Note:** Some untouched old docs still describe legacy 2D/survival scope; these do not override GDD v6.1. UI, Art, Physics and Audio deep refactors are separate post-P06 documentation work, not silently claimed completed by this PR.

## 3. Conflict registry — resolved vs pending

| ID | Conflict before owner decision | Resolution |
| --- | --- | --- |
| C01 | Survival vs Defense core | **RESOLVED D01=A:** Defense primary, Survival Legacy |
| C02 | Old Bến Đò Vong Xuyên Map 01 | **RESOLVED D11=A:** Map Sông Quê, TREBRAVE |
| C03 | 20m old vs 600s VX vs 900s timeline asset | **RESOLVED DESIGN D06=A:** 600s Defense; **engineering pending** new stage asset |
| C04 | Hero dies Lose / 20m Diêm Vương boss vs Dạ Xoa | **RESOLVED D07=C/P02/P03/P08:** only Base destroyed Lose, Dạ Xoa dies instant Win and stop spawns, Hero level-scaled respawns, 600s overtime; P04 strategic 2-state approved; K03 X/tie pending |
| C05 | Six relics visual rule vs WeaponManager=1 | **RESOLVED D03/D04/D05:** 1 ACTIVE relic equipped/run |
| C06 | 17 historic relics vs VX FUN 5 | **RESOLVED D04=A/P07:** VX FUN 5 are catalog; **MVP Slipper enabled, 4 others locked**, no advanced FUN FX; legacy preserved |
| C07 | Old PZ enemies vs VX seven | **RESOLVED D09=A:** VX seven Map 01, old PZ enemies Legacy/Future |
| C08 | 2D Tilemap/PPU vs VX 3D art | **RESOLVED D02=C TARGET:** full stylized Unity 3D, **MIGRATION NOT DONE**; P06 |
| C09 | Four hero identities/kit naming | **RESOLVED P01:** 4 Hero IDs and skill IDs defined in SO; display names and kit contents configurable later, no fixed naming gate |
| C10 | Multiplayer, Gacha, Meta in first launch | **RESOLVED D12=A:** code preserved, non-core out of MVP |
| C11 | Wood credited on kill vs physical pickup | **RESOLVED D08=B:** wood **drops**, Hero pickup in MVP; Phu post-MVP |
| C12 | Commander/squad launch scope | **RESOLVED D10=C:** NO friendly army/squad in MVP |

## 4. Deliverables and future documentation PRs

**Documentation PR #1:** decisions and authoritative GDD/spec contracts recorded + compatibility notices. Keep it in draft while implementation-details Pxx and review are open.

**Next doc set during P06 3D migration:**
1. Unity full 3D Art Bible (concept ratios, rigs, meshes, shaders, FX), scene and camera setup, performance LOD.
2. 3D Physics/AI Navigation/Enemy Target Layer Spec with raycasts, NavMesh topology, water and flying.
3. Defense-only HUD/UX spec, **Slipper-only ACTIVE MVP** and 90s WoodDrop pickup input; other relics post-MVP.
4. HeroDefinitionSO/SkillDefinitionSO key contract + 3D Enemy/Boss art sheets/skills; names can be configured later.
5. WoodLoot 90s/edge pooling, 600s overtime and level-scaled respawn test spec; **P04 Boss 2-state approved; K03 X/tie pending**.
6. Level data SO contracts, timeline migration, 3D scene/prefab Addressables naming map.
7. Post-MVP Dân Binh/Phu/Thầy Lang/Squad specification in separate backlog, not MVP.

**Source snapshots:** PZ main@657c8215 and VX main@5f5d9b26. Git history preserves old GDD v5.1; never delete assets/keys when refactoring docs.

## 5. Authoritative product vocabulary

- Brand: **TREBRAVE**; technical repo/namespace: **Projectzombie**.
- Mode: **Defense (primary)**; **Survival Legacy** retained.
- Map 01: **Map Sông Quê**; two banks, three bridges/lanes.
- Hero: **one controlled per run, from roster of 4**, stable HERO_01–HERO_04 and SkillDefinitionSO IDs; display names configurable.
- Relic: **five FUN catalog items, only Slipper ACTIVE enabled in MVP**, four locked; one slot/run.
- Base: **only Lose trigger**. Final Boss Dạ Xoa: **Win trigger**.
- Timer: **600-second wave timeline**, if Boss alive then **OVERTIME**; Dạ Xoa kill = WIN instantly and no further spawns.
- Boss strategic FSM **P04**: `ARENA_COMBAT` after BossSpawn → `ASSAULT_BASE` after `X=arenaHoldDurationSeconds` unpaused Boss-time; attack Base across valid 3D bridge; **X number and same-tick arbitration pending K03**.
- Wood: **world drop lifetime baseline 90s** adopted from VX **EXP gem** config, then Hero pickup; never auto-grant on kill.
- Tower: 10 fixed pedestals baseline, L1–L3; friendly army **post-MVP**.
- Status: **OWNER DESIGN APPROVED** / **P04 2-STATE APPROVED** / **K03 X/TIE PENDING** / **K01–K06 CONFIG TBD** / **UNITY 3D MIGRATION IN PROGRESS (owner report)** / **LEGACY** / **HISTORICAL**.
