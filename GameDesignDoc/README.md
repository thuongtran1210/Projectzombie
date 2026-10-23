# TREBRAVE / PROJECTZOMBIE — GAME DESIGN DOCUMENTATION HUB (v6.1)

> **OWNER DESIGN APPROVED:** 12/12 decisions D01–D12 on **2026-10-23**.  
> **Important:** This is a **design approval**, not an assertion Unity Projectzombie already runs full 3D Defense. The existing codebase primarily uses Unity 2D; migration is a separate implementation roadmap. **Draft PR #1 remains for documentation review; main branch not updated.**

## Design single source of truth

1. **[Master GDD v6.1 — approved product rules](ProjectZombie_GDD.md):** TREBRAVE branding, Defense as main mode, Map Sông Quê, one controlled Hero and one ACTIVE FUN relic, win by final Boss defeat, lose only by Base destroyed.
2. **[Decision Record — 12 approved choices + P01–P08 pending details](DECISION_CHECKLIST_GDD_V6.md):** source of product approvals; unresolved implementation choices explicitly marked.
3. **[Unity Full 3D Migration Plan](UNITY_3D_MIGRATION_PLAN.md):** target is full stylized 3D; what is new vs reusable; maintain Survival 2D Legacy.
4. **[Map Sông Quê & Tower Defense](MAP_AND_DEFENSE_SYSTEM_SPEC.md):** 3 bridges, 3 lanes, 10 build sockets, Base, towers, **physical wood drops and Hero pickup**, friendly army **post-MVP**.
5. **[Vongxuyen Enemy & Boss Roster](ENEMY_ROSTER_VONGXUYEN.md):** 7 VX types only for Map 01. Final Dạ Xoa, miniboss Ma Giáp; old PZ roster Legacy.
6. **[Level Scenario](LEVEL_SCENARIO_GUIDE.md):** 600s wave baseline, 108s miniboss / 510s final boss, **Base-only loss/Boss-only win** and timeout P02.
7. **[One Hero / One ACTIVE Relic — 3D UI](WEAPON_VISUAL_MANIFESTATION_RULES.md):** 4 selectable Heroes (1 per run), 5 FUN relics (1 ACTIVE equipped), ghost aim, no auto relic behavior in Map 01.
8. **[Doc Refactor Ledger](DOCS_REFACTOR_LEDGER.md):** decisions conflict registry and audit of older files.

## Approved rules — compact

| Topic | Rule |
| --- | --- |
| Product / repository | TREBRAVE brand, technical repo **Projectzombie** retained |
| Main gameplay | Hero Tower Defense, Survival retained as **Legacy mode** |
| Technical target | **Unity full stylized 3D** for Defense; old runtime Unity 2D preserved in Legacy |
| Map | **Map Sông Quê**, west/east banks, three bridges/lanes |
| Heroes | Four choices in menu; **one controlled/run** |
| Relics | **Five FUN relic choices; exactly one ACTIVE relic equipped/run** |
| Stage | **600-second** timeline; 5 phases |
| Win / Lose | **Win: kill final boss Dạ Xoa while Base alive. Lose: only Base HP = 0. Hero 0 HP is NOT loss.** |
| Wood | Enemy death creates dropped wood; **Hero must pick it up** in MVP; do not instantly credit on kill |
| Enemy | Map 01 uses 7 Vongxuyen types, old PZ enemies Legacy/Future Maps |
| MVP | **Hero + Base + Tower + Wood pickup + Enemies/Boss + Relic**; **no Phu, soldiers, healers, squad or rally yet** |
| Optional systems | Keep old Ngũ Hành/Âm Dương/Gacha/Multiplayer/Meta code, no mandatory Defense MVP support |

**Pending:** P01 exact four Hero names/kits, P02 what happens if boss alive at 600s, P03 Hero downed/revive, P04 Boss attack/early win priority, P05 wood-loot parameters, P06 full 3D render/nav/camera implementation detail, P07 FUN effects adaptation, P08 10-minute pacing versus immediate boss-win. Those do not reverse already-approved choices.

## Legacy / supporting docs (not Map 01 primary rules)

| Document | Classification | Notes |
| --- | --- | --- |
| [FUN Combat & Enemy Mechanics](FUN_COMBAT_AND_ENEMY_MECHANICS_DOC.md) | **LEGACY** | Reuse useful FUN effects, but old enemy roster and auto casting are superseded by GDD v6 |
| [Tilemap Design](TILEMAP_DESIGN_SPEC.md) | **LEGACY 2D ONLY** | PPU/Tilemap guidance for surviving Unity 2D scenes, **not full 3D Map 01** |
| [Physics Layer Design](PHYSICS_LAYER_DESIGN_SPEC.md) | **LEGACY 2D** | Need independent 3D Physics/Navigation spec as P06 work |
| [Art / VFX Style](ART_VFX_STYLE_GUIDE.md) | **LEGACY 2D** | TREBRAVE needs new semi-chibi 3D art bible |
| [Game UI Style](GAME_UI_STYLE_GUIDE.md) | **REFACTOR TODO** | Hero, Base HP, Wood loot, Tower Build, Boss telegraphs; no squad HUD MVP |
| [UI Prompt / Art](UI_PROMPT_AND_ART_GUIDE.md) | **REVIEW TODO** | Not authority for 3D rigs/assets |
| [Audio](AUDIO_SYSTEM_DESIGN_SPEC.md) | **KEEP/EXTEND** | Add 3D spatial feedback and Defense events |
| [Cloud Save](CLOUD_SAVE_SYSTEM_SPEC.md) | **KEEP/EXTEND** | Never persist run-only wood balance into meta save |
| [Multiplayer](MULTIPLAYER_ONLINE_SYSTEM_SPEC.md) | **OPTIONAL D12** | Photon Fusion code preserved; Co-op Defense outside MVP |
| [AdMob](ADMOB_MONETIZATION_SPEC.md) | **OPTIONAL D12** | Revenue flow should not redefine core gameplay |
| [Performance Report](PERFORMANCE_OPTIMIZATION_REPORT.md) | **HISTORICAL** | Old performance figures are not verified for 3D release |
| Uprades.doc | **LEGACY / AUDIT PENDING** | Original binary document preserved |

**Do not delete any legacy scripts/scenes/asset GUIDs/save keys in a documentation-only PR.** The earlier Projectzombie 20-minute survival scenario and multi-slot relic visual descriptions remain accessible in Git history, but cannot override new Map 01.

## Engineering and references

- [Projectzombie Developer Guide](../DEVELOPMENT_GUIDE.md) — modular C#, Unity services, tests, code reviews.
- [Repository README](../README.md) — describes current Unity/2D project setup, not a guarantee 3D target already implemented.
- [Vongxuyen Product Vision](https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/PRODUCT-VISION.md)
- [Vongxuyen Master GDD](https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md)
- [Vongxuyen Defense Rules](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/defense/DefenseRules.js)
- [Vongxuyen Enemy Factory](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/entities/EnemyFactory.js)
