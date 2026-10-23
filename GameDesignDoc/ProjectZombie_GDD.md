# TREBRAVE — PROJECTZOMBIE MASTER GAME DESIGN DOCUMENT v6.2 (OWNER DESIGN APPROVED)

> **Trạng thái:** D01–D12 **OWNER-APPROVED** 2026-10-23; **RUNTIME IMPLEMENTATION NOT STARTED / DRAFT PR**.  
> **Ngày soạn:** 2026-10-23. **Mục tiêu:** hợp nhất gameplay Projectzombie với các trụ cột được xây dựng trong Vongxuyen / TREBRAVE.  
> **Repository triển khai:** Projectzombie, Unity 2022.3 LTS / C# / Android. **Repository thiết kế tham chiếu:** Vongxuyen, Three.js/WebGL.  
> **Nguồn khảo sát:** Projectzombie main 657c8215; Vongxuyen main 5f5d9b26.  
> **Quy tắc:** D01–D12 được phê duyệt trong [Decision Record](DECISION_CHECKLIST_GDD_V6.md). P01–P03, P05–P08 đã được **owner-approved**; P04 đã được **owner-approved (2 state)**; tham số K01–K06 (gồm X và tie-break của K03) vẫn **PENDING**. Không được tự suy diễn. Phê duyệt thiết kế không đồng nghĩa 3D/runtime đã được lập trình/QA.

## 0. Nguồn sự thật, trạng thái và phạm vi

- **Game thương hiệu:** TREBRAVE. Repository/namespace kỹ thuật vẫn **Projectzombie**; map đầu **Map Sông Quê**.
- **Nền tảng đích:** Unity **full stylized 3D** trên Android (D02=C), lấy art direction semi-chibi/heroic fantasy từ TREBRAVE, không giả vờ Unity repo hiện đã chuyển sang 3D. Unity 2022.3 LTS là hiện trạng, renderer và migration strategy còn P06.
- **Defense Mode là mode chính** (D01=A). **Survival giữ Legacy Mode**: bảo lưu asset, code, saved data; không đưa timeline/roster cũ vào Map 01 mới.
- **Thiết kế source:** Vongxuyen Three.js/WebGL có map, tower, enemy; Projectzombie Unity hiện có combat, meta, service và nhiều relic effects. “Đã có VX” ≠ “đã có PZ 3D”.
- Bất biến thiết kế **D01–D12 APPROVED**, P04 đã chốt Arena → Base sau X; K01–K06 còn cần chốt con số/chi tiết; mọi thông số “BASELINE” chỉ là số tham chiếu từ source VX chứ chưa là balance-locked.
- Tài liệu chuyên biệt: [Map & Defense](MAP_AND_DEFENSE_SYSTEM_SPEC.md), [Enemy Roster](ENEMY_ROSTER_VONGXUYEN.md), [Run Scenario](LEVEL_SCENARIO_GUIDE.md), [One Relic UX](WEAPON_VISUAL_MANIFESTATION_RULES.md), [3D Migration](UNITY_3D_MIGRATION_PLAN.md) và [Doc Ledger](DOCS_REFACTOR_LEDGER.md).

## 1. Tầm nhìn và gameplay đã phê duyệt

**Pitch chính thức:** TREBRAVE là game Unity 3D stylized hero action + tower defense + roguelite. Người chơi chọn **1 trong 4 Hero** ở sảnh và trang bị **đúng 1 trong 5 pháp bảo FUN** cho mỗi trận. Tại Map Sông Quê, Hero chiến đấu cùng các tháp xây bằng gỗ, bảo vệ Nhà Chính khỏi quái từ bờ Đông tấn công qua ba cầu. Enemy/Boss Map 01 dùng đúng 7 archetype từ Vongxuyen.

**Core pillars**
1. **Thủ Nhà Chính:** **chỉ thua khi Base HP=0**; **thắng khi hạ Boss cuối Tướng Dạ Xoa** trong lúc Base chưa bị phá (D07=C). Hero HP=0 không phải trạng thái thua.
2. **Hành động tướng + một pháp bảo:** Basic Attack, Dash, Signature, **một ACTIVE relic skill với nút cast và ghost aim** (D03–D05).
3. **Map 3D, 3 lane:** hai bờ, dòng sông, ba cầu, 10 bệ tháp tham chiếu VX, 3D navigation/collision/telegraph.
4. **Wood pick-up:** kill enemy tạo **gỗ rơi**; trong MVP **Hero phải nhặt** mới có gỗ xây/nâng tháp (D08=B). Phu Tạp Dịch tự thu gom thuộc POST-MVP do D10=C.
5. **Địch VX:** 7 archetype cho Map 01; quái PZ cũ chỉ trong Legacy/Maps tương lai.
6. **Timeline 600 giây, 5 phase:** Nếu hết 600s mà Dạ Xoa còn sống, trận tiếp tục **overtime**; chỉ Boss chết mới WIN, Base bị phá mới LOSE (**P02 approved**). Boss chết sớm thì thắng ngay (**P08 approved**).

**MVP scope:** Hero, Base, Tower, Wood drops/pickup, three-lane enemy/boss, **chỉ 1 ACTIVE relic: Dép Tổ Ong Thần Sa**, EXP/card và UI. Bốn relic FUN khác **LOCKED/POST-MVP**. **Không bao gồm FriendlyArmy, Squad, Commander, Phu, Thầy Lang**, không buộc các tính năng multiplayer/gacha/ngũ hành v5 xuất hiện trong Defense MVP. Các hệ thống này còn code và roadmap nhưng không nằm release gate.

## 2. TREBRAVE fantasy, identity và narrative

- **Tên hiển thị:** TREBRAVE, tagline tham chiếu “Rooted in Courage.”; **tên repo vẫn Projectzombie**.
- **Map 01: Map Sông Quê**. Bờ Tây là phần quê hương người chơi còn bảo vệ; bờ Đông là lãnh thổ quê hương bị địch chiếm, không phải vùng đất của quốc gia khác.
- **Narrative loop dài hạn:** giữ đất → bảo vệ phòng tuyến → phản công qua sông → chiến thắng lực lượng chiếm đóng → giành lại khu vực bị mất. **Territory reclamation chưa thuộc MVP**, không tuyên bố có hệ thống chiếm cứ điểm.
- Cốt truyện Tứ Bất Tử, Vong Xuyên/Âm Ty trong tài liệu cũ là LEGACY/LORE BACKLOG; nếu sử dụng trong các map khác phải thiết kế lại để không phủ định nhận diện Map 01.
- **Mỹ thuật mục tiêu:** semi-chibi stylized heroic fantasy 3D, silhouette dễ đọc, chi tiết chất liệu Việt Nam, hero/relic/tower không cản boss telegraph. Đây là đích sản xuất asset, không phải đặc điểm runtime Unity hiện hành.

## 3. Gameplay loop chính — Defense (OWNER-APPROVED D01, D06–D08, P02–P03/P08)

**Sảnh:** chọn 1 Hero trong roster 4 bằng heroId (tên và kỹ năng từ SO); trang bị một relic khả dụng. **MVP chỉ mở Dép Tổ Ong Thần Sa**; Nồi/Điếu/Chiếu/Chổi có trong catalog nhưng ở trạng thái LOCKED.
**Spawn:** Hero tại **HeroInitialSpawnPoint** trên Bờ Tây (lưu WorldTransform của run để respawn cùng vị trí); Base full HP; 10 bệ tower; gỗ đầu trận baseline 100; shared clock 0–600 giây; không có friendly unit trong MVP.
**Combat:** enemy tiến qua 3 lane, Hero dùng Attack/Dash/Signature, **Dép ACTIVE** có ghost aim hợp lệ + cooldown, tower tự động bảo vệ; chưa áp dụng hiệu ứng FUN nâng cao.
**Reward:** Enemy chết -> tạo **WorldWoodDrop**, mặc định TTL **90 giây** theo *EXP gem reference* của Vongxuyen; Hero phải nhặt mới cộng WoodWallet. Đây không phải wood TTL đã có của VX. EXP giữ riêng; không auto-grant gỗ lúc kill.
**Spend:** Wood đã nhặt để build/upgrade tower. Không đòi Phu Tạp Dịch để hoàn thành MVP.

**Kết quả trận / timeline:**
- **WIN:** final Boss **Tướng Dạ Xoa** chết + Nhà Chính còn HP => **Victory ngay lập tức**, kể cả trước 600 giây.
- **Sau Victory:** **ngừng spawn quái ngay**, dừng wave/boss spawner, clear pending jobs, ngừng combat AI, cleanup entities/VFX, ghi kết quả một lần và vào result screen.
- **LOSE:** **duy nhất BaseHP=0**; HeroHP=0 không Lose.
- **Hero HP=0:** vào trạng thái Downed và chạy đếm ngược hồi sinh theo Hero Level, level càng cao thời gian càng dài. Khi hết countdown, hồi sinh ở **chính HeroInitialSpawnPoint lưu lúc bắt đầu run**. Công thức/curve và giá trị số đưa vào **HeroRespawnConfigSO**; chưa chốt seconds cụ thể (K01). Chỉ dừng/resume đồng hồ theo shared GameState.
- **T=600s, Boss còn sống, Base còn HP:** tiếp tục **overtime cho đến khi Dạ Xoa chết hoặc Base bị phá**; không auto Win/Lose. Wave timeline 34 events kết thúc (tối đa 594s); không tự tạo vô hạn wave sau 600s. Hero/Tower/Boss còn hoạt động.
- **P04 APPROVED:** Dạ Xoa có 2 strategic state: **ARENA_COMBAT** (đánh trong đấu trường bờ Đông) → sau **X giây gameplay kể từ BossSpawn** → **ASSAULT_BASE** (bỏ đấu trường, đi qua cầu/nav 3D hợp lệ, đánh Nhà Chính). **X cấu hình qua YakshaBossPhaseConfigSO**; K03 chưa chốt số X và ưu tiên khi Boss/Base chết cùng tick. [Boss Phase Spec](YAKSHA_BOSS_PHASE_SPEC.md).
- **One authority:** RunResultResolver phát RunEnded đúng một lần, sau đó block mọi SpawnEvent/RewardDuplicate/Interaction.

## 4. Map 01 — Map Sông Quê trong Unity full 3D [D02=C]

- **Giữ identity map VX:** chiến trường tham chiếu 60×40m; ba cầu North z≈−10.5, Mid z≈−0.5, South z≈10.5; West/Base x≈−12,z≈0; East/spawn/boss arena.
- **Unity 3D plane = XZ, Y = cao độ**; dùng Collider, Rigidbody/NavMeshAgent hoặc movement controller 3D phù hợp; không dựa trên Physics2D/Tilemap 2D trong Defense scene. Tọa độ VX là **baseline world layout**; không copy mesh Three.js.
- **Terrain/props:** địa hình hai bờ, mặt sông, 3 cây cầu (traversability rõ), 10 vị trí xây tower, vùng enemy spawn và arena Boss. Ground enemy không đi xuyên sông; Thủy Quái/Flying có traversal policy riêng.
- **Camera:** stylized top-down/3-4 perspective 3D; camera angle, zoom và occlusion/visibility còn P06. Tương tác raycast xuống 3D collider/ground plane; joystick và target preview thích ứng mobile.
- **3D migration bắt buộc:** xem [UNITY_3D_MIGRATION_PLAN.md](UNITY_3D_MIGRATION_PLAN.md). Không gắn component Physics2D vào Defense 3D chỉ để tái sử dụng cho nhanh; pure combat domain phải độc lập adapter 2D/3D.
- **Legacy:** Survival scene 2D trước đây được giữ nguyên và load như mode Legacy, không áp geometry mới vào map lịch sử.

## 5. 4 Hero bằng mã SO — 1 Hero/run; relic MVP chỉ Dép Tổ Ong Thần Sa

### Hero selection — P01 APPROVED
**4 Hero**, 1 do người chơi điều khiển/run, key ổn định **HERO_01–HERO_04**. Không chốt tên, các tên/kit/skill đưa về **HeroDefinitionSO / SkillDefinitionSO** để điều chỉnh sau. Skill tham chiếu ID thay vì tên: **BASIC_01..04, DASH_01..04, SIGNATURE_01..04**, mỗi Hero map tới bộ config tương ứng. Không hardcode tên nhân vật/kỹ năng vào RunFlow/HUD/Save; muốn đổi tên không thay heroId hoặc Save schema.

### Relic selection — D04/D05 + P07 APPROVED
Roster thiết kế **5 FUN relic** gồm: **Dép Tổ Ong Thần Sa (slipper — MVP ENABLED)**, **Nồi Cơm (pot — LOCKED)**, **Điếu Cày (pipe — LOCKED)**, **Chiếu Trải (mat — LOCKED)**, **Chổi Lông Gà (broom — LOCKED)**. Vì D04 đã khóa catalog có năm loại, nếu chỉ mở một thì có **bốn** món khóa. Câu nói “khóa 3 món kia” cần được xác nhận lại nếu có ý loại một món khỏi catalog, nhưng chưa tự xóa loại nào.

**Chỉ một ACTIVE relic slot**. MVP Dép: có nút Relic riêng, ghost aim 3D (đường bay/điểm trúng), cast hợp lệ, damage/cooldown và visual/âm thanh cơ bản. **Chưa yêu cầu boomerang combo, Quê Độ hoặc FUN effects đặc biệt**. Mọi hiệu ứng nâng cao và việc mở Nồi/Điếu/Chiếu/Chổi thuộc POST-MVP; code/asset Unity cũ được giữ, không tự kích hoạt trong mode mới. Thông số damage/cooldown/range nằm trong RelicDefinitionSO (K05).

**Mở rộng sau:** bật availability flag trong RelicDefinitionSO, port effect riêng theo đúng thiết kế và QA; không tăng số slot.

## 6. Nhà Chính, Tower Defense và Wood Pickup — MVP [D08=B, D10=C]

### Nhà Chính
BaseHP **baseline 500** (có cấu hình), công trình mục tiêu duy nhất xác định LOSE khi HP về 0. Base destruction là authoritative, exactly-once, có health bar và critical alert. Hero HP về 0 **không** dispatch RunEnded(LOSE).

### Wood drop & pickup
**Baseline**: 100 wood start, 10 wood per enemy kill (chưa balance-lock). EnemyKill → spawn WoodLoot entity tại vị trí tử trận/điểm an toàn có thể nhặt, có pickupId/runId/value/expiry/pool state; **expiry = 90 giây gameplay** là baseline áp dụng từ Vongxuyen EXP gem config (không phải default wood loot code ở VX), có thể chỉnh WoodDropConfigSO. Không grant gỗ trước pickup. **MVP Hero chủ động đến nhặt**. Phu chưa xuất hiện trong MVP. Khi có Phu, nó nhặt qua cùng một transaction/loot ownership. Chặn double-collect, nil spawn, drops nằm dưới nước/vùng inaccessible; nếu drop không thể nhặt, cần safe relocation/fallback trong phạm vi luật D08=B, không tự chuyển sang auto reward.

### Tower
**10 bệ cố định** (baseline VX). L1 xây 50 wood, HP 250, range 7; L2 nâng 40 wood, +100 maxHP và +1 range; L3 nâng 80 wood, +100 maxHP và +1 range. Tất cả số có thể cân bằng lại qua TowerDefinitionSO. Build/upgrade có validation/transaction và 3D ghost preview + raycast. Tower target, damage, destroyed, repair đều theo authority service; không chặn lối qua 3 cầu.

### Post-MVP (KHÔNG thuộc MVP launch)
**Dân Binh Giáo Tre, Phu Tạp Dịch, Thầy Lang Nam Dược, ba squad, stances defend/choke/follow, rally, commander & succession** chỉ là FUTURE PHASE. Tài liệu chi tiết được lưu trong [Map & Defense](MAP_AND_DEFENSE_SYSTEM_SPEC.md) dưới nhãn POST-MVP, không xây UI tuyển lính ở mốc P0–P6 MVP.

## 7. Enemy/Boss Map 01 [D09=A]

**7 loại source Vongxuyen:** magiap (Ma Giáp Cương Thi), thuyquai (Thủy Quái Rạch Đêm), huyetdoi (Huyết Dơi Đêm), magiap_boss (Ma Giáp Thống Lĩnh — **miniboss**), quaidu (Tiểu Quỷ Nón Lá Dù), phicuong (Phi Cương Tinh Anh) và yaksha_boss (Tướng Dạ Xoa — **final boss**).

- **Đúng 7 archetype** thuộc sản phẩm Map 01; chúng không bắt buộc cùng xuất hiện trong base 34-event timeline do source VX có special spawn pipeline.
- Các quái PZ cũ (Ngưu Đầu/Mã Diện/Diêm Vương/Ma Đòi Nợ) không nằm Map 01; giữ prefab/SO/code/save mapping trong Legacy/Maps tương lai, không xóa ngoài phạm vi PR GDD.
- Enemy phải hỗ trợ 3D nav/target types Player/Tower/Base, quyền truy cập ground/flying/water; có behavior khác nhau theo vai trò; thân Boss telegraph trước khi hit, có phase/cooldown, 3D hurtbox, VFX.
- **Dạ Xoa chết khi Base còn HP => WIN**. Ma Giáp Thống Lĩnh chết **không gây WIN**. Enemy Hero kill và Tower kill đều tạo wood loot drop và EXP theo cùng reward service.
- Những địch tấn công Base/tower phải kiểm tra range/reachability thật trên Physics3D, không lấy ProximityTargetSelector chỉ trả PlayerContext để cast sang Base.

Nguồn và thông số baseline đầy đủ: [ENEMY_ROSTER_VONGXUYEN.md](ENEMY_ROSTER_VONGXUYEN.md).

## 8. Wave, Boss hai state, overtime và kết trận [D06/D07, P02/P04/P08 APPROVED]

- **Base timeline:** 600s / 5 phase; 34 spawn events mỗi 18s (last event at 594s), active cap 32 và pending cap 40 từ VX; miniboss Ma Giáp Thống Lĩnh ở 108s, **Final Boss Dạ Xoa** ở 510s reference.
- **Dạ Xoa bị hạ và BaseHP>0:** phát **WIN ngay**, không đợi timer; StopWaveScheduler, CancelPendingSpawns, DisableBossSpawner, StopEnemyAI, dispose transient projectiles/drops, result exactly once. **Không spawn thêm quái khi boss chết**.
- **BaseHP=0:** duy nhất điều kiện **LOSE**. Hero downed không Lose; Ma Giáp miniboss chết không Win.
- **T=600s, Dạ Xoa còn sống:** clock chuyển trạng thái **OVERTIME**, không đổi điều kiện thắng/thua. 34 event không có sự kiện >594s nên không tự spawn waves mới; Boss/Enemy còn sống tiếp tục hoạt động, Hero respawn theo level (P03).
- **P04 APPROVED:** Dạ Xoa bắt đầu **ARENA_COMBAT** tại lúc spawn (~510s source reference); khi thời gian Boss đã sống trong Arena đạt **X unpaused seconds** (từ BossSpawn, SO config), chuyển **ASSAULT_BASE**, theo NavMesh/3D path đến đánh Nhà Chính. Nếu sau 600s X chưa hết, overtime vẫn đếm X; Boss không được dịch chuyển tức thời. K03: số X và cùng-tick BossKilled/BaseDestroyed chưa khóa. [2-State Boss Spec](YAKSHA_BOSS_PHASE_SPEC.md).
- **Timeline asset isolation:** Stage_01_BambooForest.asset ghi 600s nhưng Level1_Timeline.asset maxLevelDuration 900s, phải tạo Defense scenario/timeline mới, không sửa mù Legacy.
- **Data:** RunClock/PhaseTracker, WaveScheduler, EnemySpawnGate, BossState, ResultResolver và RunStats phải có runId; RunEnded chặn late callbacks/spawns/rewards.

## 9. Roguelite và các hệ thống legacy [D12=A]

**MVP Defense:** combat, EXP/gem, chọn 1 trong 3 upgrade cards, Hero stats/skills, **một relic ACTIVE duy nhất (Dép Tổ Ong; 4 món còn lại locked)**, tower upgrades bằng **wood nhặt**, HUD/Save thống kê. Không cho upgrade phát sinh thêm relic slot. Không auto-grant wood qua kill.

**Giữ CODE nhưng chưa bắt buộc bật trong Defense MVP:** Ngũ Hành, Âm Dương, Gacha, Meta Progression/Cloud Save, Photon Fusion Multiplayer, monetization và mọi content ngoài Map 01. Các hệ thống hiện tại vẫn phục vụ Legacy Mode và có thể được tích hợp dần sau với change request; không xóa asset/schema để tối ưu tiến độ.

**Performance mục tiêu:** benchmark trên Unity 3D Android thật; không gán claim 60FPS/Zero-GC cho 3D migration chưa đo. Giữ pooling, Event-driven domain, Addressables có migration plan.

## 10. UI/UX Android full 3D

**HUD MVP:** Hero HP/Downed status, Base HP (prominent), Wood Wallet và floating Wood Loot, EXP/level, Timer 0–600 và thông tin Boss, cảnh báo ba lane, Tower Build/Upgrade, Dép ACTIVE + cooldown/ghost aim, Attack/Dash/Signature và hồi sinh đếm ngược khi Hero downed. **Không có recruit/squad/rally dock trong MVP** (D10=C).

**Input:** joystick, 3D camera, raycast ground/slot, hero movement, active cast ghost (line/circle/cone/rect), contextual build panel. Cần input interaction owner duy nhất: ActiveRelicAim và TowerBuild không thể cùng chiếm cú chạm. Khi pause/levelup/gameover, 3D placement, projectile, tower, AI, loot pickup cùng bị khóa theo time/state.

**UX ưu tiên:** Hero/Tháp/Map đủ đọc ở mobile, Boss telegraph không bị VFX FUN và props 3D che, pickup wood phải dễ nhìn/nhặt, Base death feedback rõ. Camera/render style lock ở P06.

## 11. Kiến trúc Unity Full 3D — migration chứ không port code Three.js

**Hiện trạng:** Projectzombie Unity 2022.3 LTS thiên về **2D**; GameStateManager, MatchFlowService, WaveScheduler, WeaponManager, DamageData/IDamageable, Event contracts, pooling, upgrades, services có thể sử dụng lại có chọn lọc.

**Mục tiêu:** Unity 3D stylized: camera perspective/3-4, Physics (Collider/Rigidbody3D/Physics.Raycast), 3D pathing (NavMesh/waypoint graph hoặc custom solver), animation rig/avatar, projectile/hitbox 3D, 3D character controller. **Không gọi Physics2D.OverlapCircleNonAlloc hay TilemapCollider2D trong runtime Defense scene 3D**.

**Module đề xuất:**
- Defense/Core: DefenseRunController, BaseHealth, WoodLootDropService, WoodWallet, TowerBuildService, TowerRuntime.
- World3D/MapSongQue: river bridges/nav links, spawn zones, build sockets, boss arena.
- AI/DefenseTarget: EnemyTargetCatalog, NavPolicy, tower/base damage adapters để tránh phá ITargetSelector<PlayerContext> của Legacy.
- Relics/Active: **Slipper MVP**, one-slot RelicAbilityController, 3D ghost indicator, valid cast và basic damage/cooldown; FUN effects / 4 relic khác để POST-MVP.
- UI/Defense: Base HUD, LaneThreat, Wood pickup, Tower panel, Boss HUD. **FriendlyArmy module để backlog.**

**Legacy 2D mode:** giữ ở scenes/assemblies/adapters riêng; không phá asset GUID và Save Keys. Khối domain logic có thể dùng chung nếu thực sự independent của physics/rendering. Cần migration PR theo phases, not an instant Unity 3D flip.

Xem: [UNITY_3D_MIGRATION_PLAN.md](UNITY_3D_MIGRATION_PLAN.md).

## 12. Lộ trình phát triển theo bản đã duyệt

| P | Scope | Gate nghiệm thu |
| --- | --- | --- |
| P0 — Design & Engine Spike | GDD v6 approved, 3D architecture, 3D player/map small slice, policy P02/P03 | 3D app scene riêng, Survival Legacy vẫn mở được |
| P1 — Core Base + Result | 3D Base, HP/damage, RunResult, Hero incapacitation policy | **Base=0 Lose**, Hero HP=0 **hồi sinh theo cấp**, boss death Win |
| P2 — 3D Map/Lanes + Enemies | Hai bờ, 3 cầu, navigation/collisions, 7 enemy type definitions | 3D traversability và 3 lane hoạt động |
| P3 — Wood Drop & Tower | Wood pickup cho Hero, economy, 10 build slots, Tower L1–L3 | Không cộng Wood trực tiếp khi kill, không double pickup |
| P4 — Hero + Slipper MVP | 4 hero IDs/SkillDefinitionSO, 1 hero/run, 1 Dép ACTIVE, 3D ghost; 4 relic LOCKED | Không kích hoạt passive/FUN sai; đúng 1 slot |
| P5 — 600s Waves/Boss | 34 events baseline, miniboss 108s, final Dạ Xoa 510s; **2 state Arena → đánh Base sau X (SO)**, overtime và stop spawns on Victory | Boss đánh trong arena trước, sau X đi đánh Base; chết Win ngay; Base chết Lose |
| P6 — UI/Balance/3D Content | HUD, intro, progression, stylized 3D assets, Android | Run hoàn chỉnh, safe-area, perf được đo |
| P7 — Regression/QA | Legacy survival, build pipeline, profiler and mobile devices | No critical regression, release candidate |

**MVP excludes:** Friendly Army, Squad, Commander, Phu/Thầy Lang AI, co-op, gacha, reclaim territory. Khi làm sau, tách Epic rõ và cập nhật spec post-MVP.

**Vertical slice đầu:** Hero 3D + một lane + Nhà Chính + một enemy tạo world wood drop + Hero pickup + một Tower + final boss trigger and run result.

## 13. Definition of Done và tài liệu kèm

- **12 owner decisions D01–D12 đã chốt**; các chi tiết P01–P03 và P05–P08 đã chốt nguyên tắc; P04 2 state đã chốt; K01–K06 là thông số/chi tiết còn mở. Tài liệu phải phân biệt “approved design” với “implemented Unity runtime”.
- Document checks: liên kết đúng, không active doc nào nói 20 phút / 6 relic slots / Diêm Vương final / Hero chết thua / wood auto-grant / friendly army launch MVP / Projectzombie runtime 2D là target cuối.
- Code gates: Unity compile, EditMode/PlayMode, Android smoke/profiling, source asset migration, 3D physics/nav QA, old Survival Legacy regression.
- D07 event tests: base death only loss; final boss death win; miniboss death not win; Hero death not loss; timeout 600 no arbitrary result; exactly-once completion.
- D08 pickup tests: item spawned after kill, collect once, not exploitable via pooled stale ID, reachable from floor/edge/water cases.
- D10 scope tests: no mandatory squad recruitment or friendly AI in MVP; absence of friendly army does not stop release.
- Tất cả specs SSoT link [Decision Record](DECISION_CHECKLIST_GDD_V6.md) và [Refactor Ledger](DOCS_REFACTOR_LEDGER.md).

## 14. Quyết định đã duyệt và các điểm đang mở

**OWNER-APPROVED 2026-10-23:** D01=A, D02=C, D03=A, D04=A, D05=A, D06=A, D07=C, D08=B, D09=A, D10=C, D11=A, D12=A. Đây là hợp đồng thiết kế, không phải xác nhận Unity build sẵn sàng.

**P01–P03/P05–P08 OWNER-APPROVED:** Hero and skill IDs configured in SO; overtime until Boss killed; level-scaled respawn at initial Hero spawn; WoodDrop 90s baseline adapted from VX EXP gems; Unity full 3D migration is underway; only Slipper ACTIVE/basic MVP with 4 other FUN relics locked; Boss death -> instant Win and stop spawns.

**P04 OWNER-APPROVED:** Dạ Xoa có 2 strategic state (ARENA_COMBAT → sau X giây Boss-time → ASSAULT_BASE đánh Nhà Chính). **K01–K06 PENDING PARAMS:** K03 gồm giá trị X, ưu tiên same-tick BaseDeath/BossDeath, target attack set; các K khác gồm respawn curve, pickup, Hero SO, Slipper balance, camera/3D settings.

Chi tiết phê duyệt/checklist: [DECISION_CHECKLIST_GDD_V6.md](DECISION_CHECKLIST_GDD_V6.md).

## 15. Tài liệu nguồn

- Projectzombie GDD v5.1: https://github.com/thuongtran1210/Projectzombie/blob/main/GameDesignDoc/ProjectZombie_GDD.md
- Projectzombie Developer Guide: https://github.com/thuongtran1210/Projectzombie/blob/main/DEVELOPMENT_GUIDE.md
- Vongxuyen Product Vision: https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/PRODUCT-VISION.md
- Vongxuyen Master GDD: https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md
- Vongxuyen Stage 01: https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/stage01.js
- Vongxuyen DefenseRules: https://github.com/thuongtran1210/Vongxuyen/blob/main/src/defense/DefenseRules.js
- Vongxuyen EnemyFactory: https://github.com/thuongtran1210/Vongxuyen/blob/main/src/entities/EnemyFactory.js
