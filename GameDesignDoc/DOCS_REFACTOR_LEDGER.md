# TREBRAVE / PROJECTZOMBIE — DOCUMENT REFACTOR LEDGER v6.1

> **Status:** 12/12 product decisions OWNER-APPROVED (2026-10-23). **Implementation remains proposed**; P01–P08 are open sub-decisions, see [Decision Record](DECISION_CHECKLIST_GDD_V6.md).  
> **Scope:** restructure documentation to make new TREBRAVE / full Unity 3D Defense first-class while protecting Projectzombie Unity 2D Legacy mode.  
> **Document-only Draft PR:** no Unity scene/script/asset refactor is claimed or committed.

## 1. Authority hierarchy / single source of truth

| Level | Files | Authority |
| --- | --- | --- |
| L0 Game Contract | [ProjectZombie_GDD.md](ProjectZombie_GDD.md), [DECISION_CHECKLIST_GDD_V6.md](DECISION_CHECKLIST_GDD_V6.md) | TREBRAVE, D01–D12, win/loss, 3D target, 1 Hero/Relic, core scope |
| L1 Feature Contract | [MAP_AND_DEFENSE_SYSTEM_SPEC.md](MAP_AND_DEFENSE_SYSTEM_SPEC.md), [ENEMY_ROSTER_VONGXUYEN.md](ENEMY_ROSTER_VONGXUYEN.md), [LEVEL_SCENARIO_GUIDE.md](LEVEL_SCENARIO_GUIDE.md), [WEAPON_VISUAL_MANIFESTATION_RULES.md](WEAPON_VISUAL_MANIFESTATION_RULES.md) | Map / wood drop / tower / 7 enemy / boss / active relic / stage |
| L1.5 Engineering Migration | [UNITY_3D_MIGRATION_PLAN.md](UNITY_3D_MIGRATION_PLAN.md) | Existing 2D → Unity full 3D, adapters and Legacy compatibility |
| L2 Art, Technical | Art/VFX, physics layers, tilemap, UI, audio, profile and addressables | Must follow L0; existing 2D specs are **Legacy-specific** |
| L3 Optional | Multiplayer, cloud save, monetization, Ngũ Hành/Âm Dương | Code retained D12; may stay outside Defense MVP |
| L4 History | GDD v5.1 in Git history, old 20-minute survival/six-slot specs | Not authoritative for Map 01 TREBRAVE |

**Approved immutable rules:** main mode Defense, Legacy Survival retained; Unity target full stylized 3D; 4 selectable Heroes and 1/run; 5 FUN relic choices and 1 ACTIVE equipped/run; 600s timeline; **Base death ONLY Lose; Dạ Xoa final boss death Win**; wood drops collectible by Hero; 7 VX Map01 enemy; Hero/Tower only MVP; TREBRAVE brand, PZ repo technical; legacy optional code retained.

**Pending details:** P01 four heroes identity/kit mapping, P02 timeout 600s if boss alive, P03 Hero death/downed/revive, P04 boss raid and event priority, P05 loot lifespan & pickup, P06 3D camera/render/asset migration, P07 relic FX mapping, P08 whether early boss kill ends before 600s / victory gate.

## 2. File-by-file refactor status

| File | Status on draft PR | Next task |
| --- | --- | --- |
| [ProjectZombie_GDD.md](ProjectZombie_GDD.md) | **UPDATED v6.1 / OWNER DESIGN APPROVED** | Fill Pxx after owner follow-up; not claim Unity runtime is 3D yet |
| [DECISION_CHECKLIST_GDD_V6.md](DECISION_CHECKLIST_GDD_V6.md) | **UPDATED 12/12 APPROVED** | Track P01–P08 |
| [MAP_AND_DEFENSE_SYSTEM_SPEC.md](MAP_AND_DEFENSE_SYSTEM_SPEC.md) | **UPDATED** 3D, wood pickup, tower-only MVP | Balance and 3D assets, QA |
| [ENEMY_ROSTER_VONGXUYEN.md](ENEMY_ROSTER_VONGXUYEN.md) | **UPDATED** VX7, final boss, 3D prefab targets | Prefab creation/AI design/tests |
| [LEVEL_SCENARIO_GUIDE.md](LEVEL_SCENARIO_GUIDE.md) | **UPDATED** 600s, boss kill win, Base only loss, legacy timeline isolated | P02/P03/P04 event resolution |
| [WEAPON_VISUAL_MANIFESTATION_RULES.md](WEAPON_VISUAL_MANIFESTATION_RULES.md) | **UPDATED** 5 active FUN, one slot, 3D aim ghost | P07 effect mapping |
| [UNITY_3D_MIGRATION_PLAN.md](UNITY_3D_MIGRATION_PLAN.md) | **NEW** 3D scene/pipeline, Legacy 2D and migration phases | P06 architecture/asset spike |
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
| C04 | Hero dies Lose / 20m Diêm Vương boss vs Dạ Xoa | **RESOLVED D07=C:** only Base destroyed Lose, final Dạ Xoa killed Win; P02/P03/P04 still pending |
| C05 | Six relics visual rule vs WeaponManager=1 | **RESOLVED D03/D04/D05:** 1 ACTIVE relic equipped/run |
| C06 | 17 historic relics vs VX FUN 5 | **RESOLVED D04=A:** VX FUN five Map 01; legacy set preserved |
| C07 | Old PZ enemies vs VX seven | **RESOLVED D09=A:** VX seven Map 01, old PZ enemies Legacy/Future |
| C08 | 2D Tilemap/PPU vs VX 3D art | **RESOLVED D02=C TARGET:** full stylized Unity 3D, **MIGRATION NOT DONE**; P06 |
| C09 | Four hero identities/kit naming | **PARTIAL D03=A:** four selectable, one/run; **P01 pending** identity mapping |
| C10 | Multiplayer, Gacha, Meta in first launch | **RESOLVED D12=A:** code preserved, non-core out of MVP |
| C11 | Wood credited on kill vs physical pickup | **RESOLVED D08=B:** wood **drops**, Hero pickup in MVP; Phu post-MVP |
| C12 | Commander/squad launch scope | **RESOLVED D10=C:** NO friendly army/squad in MVP |

## 4. Deliverables and future documentation PRs

**Documentation PR #1:** decisions and authoritative GDD/spec contracts recorded + compatibility notices. Keep it in draft while implementation-details Pxx and review are open.

**Next doc set after P06:**
1. Unity full 3D Art Bible (concept ratios, rigs, meshes, shaders, FX), scene and camera setup, performance LOD.
2. 3D Physics/AI Navigation/Enemy Target Layer Spec with raycasts, NavMesh topology, water and flying.
3. Defense-only HUD/UX spec, active five relic VFX and wood pickup input.
4. Hero roster mapping P01 and 3D Enemy/Boss art sheets/skills.
5. Wood loot, boss after-timeout and respawn flow P02–P05.
6. Level data SO contracts, timeline migration, 3D scene/prefab Addressables naming map.
7. Post-MVP Dân Binh/Phu/Thầy Lang/Squad specification in separate backlog, not MVP.

**Source snapshots:** PZ main@657c8215 and VX main@5f5d9b26. Git history preserves old GDD v5.1; never delete assets/keys when refactoring docs.

## 5. Authoritative product vocabulary

- Brand: **TREBRAVE**; technical repo/namespace: **Projectzombie**.
- Mode: **Defense (primary)**; **Survival Legacy** retained.
- Map 01: **Map Sông Quê**; two banks, three bridges/lanes.
- Hero: **one controlled per run, from roster of 4**; exact kit names P01.
- Relic: **one equipped ACTIVE per run, from five FUN**.
- Base: **only Lose trigger**. Final Boss Dạ Xoa: **Win trigger**.
- Timer: **600 gameplay seconds**; P02 overtime pending.
- Wood: **world drop, then Hero pickup**; never auto-grant on kill.
- Tower: 10 fixed pedestals baseline, L1–L3; friendly army **post-MVP**.
- Status: **OWNER DESIGN APPROVED** / **PENDING DETAIL** / **UNITY IMPLEMENTATION TODO** / **LEGACY** / **HISTORICAL**.
