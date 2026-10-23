# TREBRAVE / PROJECTZOMBIE — GAME DESIGN DOCUMENTATION HUB (v6.2)

> **OWNER DESIGN APPROVED:** D01–D12 và P01–P03/P05–P08 ngày **2026-10-23**. **P04 đã duyệt 2 state** (ARENA_COMBAT → sau X giây từ BossSpawn → ASSAULT_BASE đánh Nhà Chính); **K01–K06 còn các tham số/chi tiết** (đặc biệt K03 = X và cùng-tick arbitration). Owner xác nhận đang chuyển Unity 2D sang 3D, chưa xác minh build/QA.  
> **Important:** This is a **design approval**, not an assertion Unity Projectzombie already runs full 3D Defense. The existing codebase primarily uses Unity 2D; migration is a separate implementation roadmap. **Draft PR #1 remains for documentation review; main branch not updated.**

## Design single source of truth

1. **[Master GDD v6.2 — approved product rules](ProjectZombie_GDD.md):** TREBRAVE branding, Defense as main mode, Map Sông Quê, one controlled Hero and one ACTIVE FUN relic, win by final Boss defeat, lose only by Base destroyed.
2. **[Decision Record — D01–D12 và P01–P08 approved; Kxx params pending](DECISION_CHECKLIST_GDD_V6.md):** source of product approvals; unresolved implementation choices explicitly marked.
3. **[Unity Full 3D Migration Plan](UNITY_3D_MIGRATION_PLAN.md):** target is full stylized 3D; what is new vs reusable; maintain Survival 2D Legacy.
   - **[Unity 3D Source Code Audit — 2026-10-23](UNITY_3D_CODE_AUDIT_2026-10-23.md):** reviewed committed `main@657c8215` (not unpublished local 3D work); KEEP/ADAPT/NEW matrix, 8 engineering blockers, Hero canonical ID mapping and **3D-01..3D-08 implementation PR order**.
4. **[Map Sông Quê & Tower Defense](MAP_AND_DEFENSE_SYSTEM_SPEC.md):** 3 bridges, 3 lanes, 10 build sockets, Base, towers, **physical wood drops and Hero pickup**, friendly army **post-MVP**.
5. **[Vongxuyen Enemy & Boss Roster](ENEMY_ROSTER_VONGXUYEN.md):** 7 VX types only for Map 01. Final Dạ Xoa, miniboss Ma Giáp; old PZ roster Legacy.
6. **[Level Scenario](LEVEL_SCENARIO_GUIDE.md):** 600s baseline (108s miniboss/510s final), **OVERTIME until boss kill**, **instant Victory + stop spawns**, Base-only loss and level-scaled Hero respawn.
   - **[Yaksha Boss 2-State Spec](YAKSHA_BOSS_PHASE_SPEC.md):** P04 approved Arena → sau X giây (ScriptableObject, chưa chốt số) → Assault Base; tie-break cùng tick K03 pending.
7. **[One Hero / Slipper-only ACTIVE MVP — 3D UI](WEAPON_VISUAL_MANIFESTATION_RULES.md):** 4 Heroes with IDs and skills in SO; 5 FUN catalog with **only Dép Tổ Ong enabled**, 4 LOCKED; no advanced FUN effects.
8. **[Doc Refactor Ledger](DOCS_REFACTOR_LEDGER.md):** decisions conflict registry and audit of older files.

## Approved rules — compact

| Topic | Rule |
| --- | --- |
| Product / repository | TREBRAVE brand, technical repo **Projectzombie** retained |
| Main gameplay | Hero Tower Defense, Survival retained as **Legacy mode** |
| Technical target | **Unity full stylized 3D** for Defense; old runtime Unity 2D preserved in Legacy |
| Map | **Map Sông Quê**, west/east banks, three bridges/lanes |
| Heroes | Four HERO_01–HERO_04 choices; one/run; skill and display names configured by SO |
| Relics | **Five FUN catalog items**, **MVP only ACTIVE Slipper enabled**, four other items LOCKED |
| Stage | **600-second wave timeline**; after 600s Boss still alive -> OVERTIME, giữ timer state; P04 Boss ARENA_COMBAT → ASSAULT_BASE sau X giây từ spawn |
| Win / Lose | **Final Dạ Xoa death -> instant Victory + stop spawns**; only Base HP = 0 triggers Lose; Hero 0 HP -> level-based respawn at starting point |
| Wood | Drop + Hero pickup; **default lifetime 90s** based on VX EXP pickup lifetime, not VX wood loot; no auto grant |
| Enemy | Map 01 uses 7 Vongxuyen types, old PZ enemies Legacy/Future Maps |
| MVP | **Hero + Base + Tower + Wood pickup + Enemies/Boss + Relic**; **no Phu, soldiers, healers, squad or rally yet** |
| Optional systems | Keep old Ngũ Hành/Âm Dương/Gacha/Multiplayer/Meta code, no mandatory Defense MVP support |

**Approved additional scope:** P01 Hero/Skill IDs in SO; P02 overtime until Boss death; P03 respawn countdown increases with level at original Hero spawn; P05 default wood drop TTL 90s from VX EXP baseline; P06 owner actively migrating Projectzombie from Unity 2D to full stylized 3D; P07 only Slipper active (basic cast/damage/ghost, no FUN effects), 4 FUN relics locked; P08 immediate Victory/stop waves upon final Boss death. **P04 approved:** Boss đánh trong arena rồi sau X giây boss-time chuyển sang đánh Nhà Chính qua cầu 3D. **Still pending:** giá trị X và same-tick outcome K03, K01–K06 numeric/technical config.

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
