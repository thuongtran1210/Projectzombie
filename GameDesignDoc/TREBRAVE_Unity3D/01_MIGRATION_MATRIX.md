# TREBRAVE — Feature Inventory & Vongxuyen → Unity Migration Matrix

**Status:** DRAFT / chờ xác minh Unity thực tế · 2026-10-23. **Không phải xác nhận đã chuyển đầy đủ**.
**D-01/A APPROVED:** **Engine đích chính là Unity Defense 3D** trong Projectzombie, nhánh `feat/trebrave-defense3d-first-scene`; **Vongxuyen/Three.js `main`** là nguồn tham chiếu hành vi, Enemy, asset. Giữ Survival 2D và Web độc lập; đây là mục tiêu triển khai, chưa phải QA đã đạt.
**Quy ước status:** `P`=đã có prototype Unity, `M`=một phần có/cần adapter, `X`=chưa có bản tương đương Unity được xác nhận, `L`=chỉ tồn tại local theo ảnh/log người dùng, `V`=cần QA Unity. **P không đồng nghĩa DONE-QA.**

## A. Migration Matrix theo subsystem

| ID | Nguồn Three.js (file thực tế) | Đích Unity hiện tại / evidence | Gap phải xử lý | Trạng thái |
|---|---|---|---|---|
| MAP-01 | `world/MapBenQue.js`, `config/mapConfig.js`, exporter PR #39 | local `Assets/Art/Environment/MapSongQue` + `MapSongQue3D_Prototype/Recovery.unity` theo người dùng; marker `DefenseSceneInfo3D` | version scene/GLB/meta còn ở local; test tọa độ, render và asset Git | L,V |
| MAP-02 | 3 lanes / Sông Quê | `DefenseEnemyBridgeWalker3D` NavMesh ground/3 bridges; `Defense_Navigation` local | path toàn tuyến, agent runtime, liên kết các loại air/water | P,V |
| MAP-03 | `DefenseRules.js` strategic tower slots | `DefenseSceneInfo3D` 10 sockets | đối chiếu ID/chỗ đặt và va chạm | P,V |
| STATE-01 | `core/GameStateManager.js`, `core/GameClock.js`, `core/GameEventBus.js` | `DefenseRun6003D.elapsed`; các script dùng `Time.time` | state machine authoritative, pause/level-up, event lifecycle, restart, deterministic clock | X |
| EN-01 | `entities/EnemyFactory.js` 6 type | `DefenseRun6003D.SpawnEnemy()` dựng capsule + `DefenseEnemyBridgeWalker3D` | cấu hình EnemyData, speed/HP/attack/size/loot theo type | M |
| EN-02 | `combat/EnemyDamageService.js`, `HitResolver.js` | Enemy `ReceiveDamage/Defeated` + WoodDrop | damage source, stun/slow/knockback, exact-once death/reward/EXP | M |
| EN-03 | `WaveManager.js` per-type behavior | Walker NavMesh chung, chỉ gây damage cho Base | D-06/A: Ground 3 cầu, Water→shore, Air 3D thật, anti-air, Landing; chưa triển khai | X |
| EN-04 | `EnemyFactory.js` procedural meshes | placeholder capsule Unity; Boss visual prefab hook | exporter per Enemy, prefab/pivot, rendering/animation, materials | X; art sau Core |
| WAVE-01 | `config/stage01.js`, `WaveScheduler.js`, Web `WaveManager.js` | `DefenseRun6003D` có 34 event/18s nhưng type vòng lặp và `enemiesPerEvent` cố định | **D-05 APPROVED:** #7 chỉ 1 miniboss; **D-11/A APPROVED:** mỗi event Ma Giáp 1 slot Phi Cương dẫn đầu, giữ số job và deterministic | M |
| WAVE-02 | `SpawnPlanner.js`, `WaveManager.js` queue | spawn east marker + fixed z ba lane; không thấy cap/pending | safe spawn 3.1m, 32 active, 40 pending, 4/frame, 25s TTL, no invalid NavMesh | X |
| WAVE-03 | `WaveManager.js` GhostCannon + `quaidu` | chưa có tương đương căn cứ/đạn pháo/thả dù | D-06/A: 3 Linh Pháo, projectile→parachute→QuaiDu West đã duyệt; chưa triển khai | X |
| BOSS-01 | `EnemyFactory.js` `magiap_boss` | Unity miniboss @108 HP350, một walker | type-specific HP/attack/role; **stage01 event #7 là miniboss thay event thường** | M |
| BOSS-02 | `combat/YakshaBoss.js`, `YakshaTelegraph.js` | Unity `DefenseYakshaArenaCombat3D` blast ring, `BeginBossAssault`, `DefenseRun6003D` | cleave, slam, crescent projectile, stun/pillar counter, target/AI/presence across river | M |
| BOSS-03 | `entities/YakshaBossMesh.js` | Unity optional `yakshaVisualPrefab` + editor assign | **chưa có xác nhận prefab Yaksha nguyên gốc đã import** | X; art sau Core |
| HERO-01 | `PlayerWanderer.js`, `combat/HeroRelicSystem.js` | `DefenseHeroMotor3D`, `DefenseHeroAutoAttack3D`, `DefenseHeroHealth3D` | dash, attack combos/targeting, status, character skill, relic 1 slot & touch input | M |
| HERO-02 | `HeroRelicSystem.js`, `SlipperProjectile.js` | chưa thấy Unity Defense3D implementation | một relic MVP: projectile/range/cooldown/ghost aim after approval | X |
| TOWER-01 | `defense/DefenseRules.js`, `DefenseManager.js` | `DefenseTowerBuildPrototype3D`, `DefenseTowerPrototype3D` | nâng tháp, HP/sửa/phá, targeting, cost & slot registry, save-in-run | M |
| ECON-01 | `DefenseRules.js` Wood | `DefenseWoodWallet3D`, `DefenseWoodDrop3D` | per-type reward, one-shot/expire/cap, cost balance, guard Pause/Result | M |
| EXP-01 | `pickups/PickupManager.js`, `progression/ExperienceManager.js`, `config/experience.js` | chưa thấy module Defense3D tương đương | EXP gem magnet/pickup, XP curve, queue level-up, reset | X |
| EXP-02 | `progression/UpgradeManager.js`, `config/upgrades.js`, `ui/UpgradeSelectionUI.js` | chưa thấy Defense3D 3-card modal | 3 non-duplicate options, stack cap, stat model authoritative | X |
| MINION-01 | `defense/MinionManager.js`, `entities/MinionFactory.js` | chưa thấy `Defense3D` tương đương | ít nhất một dân binh; roles Dân Binh/Phu Tạp Dịch/Thầy Lang backlog | X |
| RESULT-01 | Web `app/GameSession.js`, GameState, GameHUD | Unity `DefenseRun6003D.OnGUI` WIN ngay nếu hạ Boss, `DefenseHeroHealth3D` `Time.time` respawn | **D-03/A APPROVED:** WIN Base sống + đủ 600s + BossDead, OVERTIME nếu Boss còn; **D-04/A APPROVED:** Down 5 gameplay s rồi respawn; lifecycle/restart, input guards và terminal arbitration chưa code | M |
| QA-01 | Vongxuyen Node test & QA docs | Unity local screen/log + remote code review | Unity edit/play tests, full 600s, 3 restarts, regression old SampleScene, Android later | V |

**Đối chiếu code Unity phải tiến hành trên bản local sau khi user đã pull**, vì stash Fast Test `YakshaTime=10f` có thể làm script khác branch. Remote production default `510f`.

## B. Enemy Registry gốc (raw Vongxuyen EnemyFactory)

Các số này là **giá trị nguồn** trước wave multiplier, chưa duyệt cân bằng Unity:

| Enemy ID | Vai trò đề xuất trong Unity | HP source | Speed source | Damage source | Rủi ro chuyển |
|---|---|---:|---:|---:|---|
| `magiap` | armored ground / cầu | 130 | 1.45 | 16 | giáp vs HP; nhịp nhảy |
| `thuyquai` | canal/amphibious | 95 | 2.05 | 12 | spawn nước, không bị ép đi ground NavMesh |
| `huyetdoi` | fast aerial | 50 | 3.10 | 8 | bay và chọn mục tiêu hợp lệ |
| `quaidu` | parachute ra từ GhostCannon | 90 | 1.75 | 14 | spawn landing bờ Tây, không sinh đông bờ theo cách chung |
| `phicuong` | elite aerial / telegraphed claw | 260 | 1.65 | 18 | telegraph/stun, flying nav |
| `magiap_boss` | miniboss @108s | 700 | 1.18 | 24 | stage #7 dùng 1 con, không tạo thêm wave thường |
| `yaksha_boss` | Boss độc lập @510s | 3200 | 1.35 | 38 | boss `YakshaBoss.js` giữ logic riêng, chưa đồng nhất HP Unity |

**Lưu ý:** Unity hiện `maxHP=100` cho quái thông thường, HP350 Miniboss, HP1200 Yaksha; đây là số prototype khác Vongxuyen, không tự thay bằng source khi chưa duyệt balance. Một số đặc tính AI từ Web do `WaveManager.js` thực thi, không chỉ trong EnemyFactory.

## C. Stage 01 / Spawn diff (phải giải quyết trước code)

| Luật | Vongxuyen `stage01.js` / `SpawnPlanner.js` | Unity `DefenseRun6003D.cs` hiện tại | Kế hoạch |
|---|---|---|---|
| Thời lượng | 600 gameplay s | `elapsed += Time.deltaTime`, overtime sau 600 | shared clock + pause guard |
| Event | 34 events tại `index*18` = 0..594 | 34 events tại `index*18` | giữ ID + đảm bảo once |
| Phase | `min(5,floor(at/120)+1)` | không dùng phase config | stage data |
| Count | event #7 miniboss 1; thường `min(8,3+phase+(index%2))` | `enemiesPerEvent=3` và **miniboss riêng** | **D-05 APPROVED:** #7 chỉ 1 `magiap_boss`, không batch thường; 34 events / 214 planned jobs |
| Types | 20 events khai báo Ma Giáp (128 suất), 40 Huyết Dơi, 45 Thủy Quái, 1 Miniboss; runtime Web đổi quái đầu mỗi frame | xoay qua 5 tên (bao gồm quaiDu, phiCuong) | **D-11/A APPROVED:** mỗi event `magiap` thay suất 0 thành `phicuong` → 20 elite + 108 magiap, 214 suất tổng; không theo frame/FPS |
| Safe zone | EastBank/NorthCanopy/CanalWater/SouthBank/GateWest theo type; min 3.1m | vị trí east spawn + z cầu | zone→NavMesh/Air/Water adapter |
| Capacity | 32 active / 40 pending / 4 per frame / 25s pending lifetime | chưa thấy giới hạn tương đương | queue + fairness + drop expiry |
| Boss | `YakshaBoss.js` @510s | `YakshaTime=510f`, arena 45s, assault, overtime | cần chốt WIN/LOSS + boss AI |

**Ca kiểm thử đã có luật được duyệt:** @108s, event #7 có duy nhất 1 `magiap_boss`, không tạo event thường kèm theo; trong mỗi event `magiap`, job dẫn đầu được định danh `phicuong` ngay khi lập job, các job còn lại giữ Ma Giáp, không tùy FPS. **Lưu ý:** Web `SpawnPlanner.js` chưa whitelist `phicuong` trong vùng EastBank/SouthBank; resolver Unity phải được thiết kế (D-06/A đã duyệt) để elite bay xuất hiện hợp lệ. @510s, Boss Dạ Xoa là trigger độc lập với event lịch.

**Nguồn sự thật quyết định:** [Decision Log](03_DECISIONS_QA.md), [Gate G0 pack](04_GATE0_APPROVAL_PACK.md), [34 event / Enemy contract](05_STAGE_ENEMY_CONTRACT.md). **D-05/D-06/A/D-11 được duyệt về hành vi, không phải code Unity đã đồng bộ.**

**D-07 phê duyệt một phần:** [09_D07_BALANCE_REVIEW.md](09_D07_BALANCE_REVIEW.md): **D-07-01/A đã APPROVED** (dùng Web làm baseline tham chiếu, Unity Balance Profile riêng và điều chỉnh sau 600s PlayMode), **D-07-02..08 vẫn PENDING**. Không copy chỉ số Web sang Unity hay ghi đè scene/Inspector.

## D. Migration principles / asset handling

1. **Transfer data / behavior, not JavaScript code execution.** C# authority + ScriptableObject/JSON versioned; Unity NavMeshAgent hoặc controller phù hợp riêng mỗi locomotion; Three.js `MaterialManager` không hoạt động trong URP.
2. Enemy prototype visuals có thể giữ capsule cho Gate G2/G3, nhưng **hành vi không được dùng chung vô điều kiện**. Animation và GLB characters sau khi gameplay được nghiệm thu.
3. Không assume GLB bản đồ chứa model Enemy, vũ khí, Boss, code hoặc animation; nó là snapshot scene xuất riêng.
4. Với quái cần bay/nước: giữ nguyên spawn source semantics, thiết kế path/obstacle phù hợp; test không teleport xuyên sông/nhà.
5. Khi đưa asset Unity vào Git: theo checklist meta GUID, prefab refs, scene refs, glTFast manifest/lock, NavMesh; backup local trước khi stage/commit.

## E. Source references

- [EnemyFactory.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/entities/EnemyFactory.js)
- [WaveManager.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/WaveManager.js)
- [WaveScheduler.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/WaveScheduler.js)
- [SpawnPlanner.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/SpawnPlanner.js)
- [stage01.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/stage01.js)
- [YakshaBoss.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/YakshaBoss.js)
- [YakshaBossMesh.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/entities/YakshaBossMesh.js)
- [EnemyDamageService.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/EnemyDamageService.js)
- [Unity DefenseRun6003D.cs](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseRun6003D.cs)
- [Unity DefenseEnemyBridgeWalker3D.cs](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseEnemyBridgeWalker3D.cs)

**Mỗi lần triển khai:** đối chiếu lại file/commit, cập nhật `Status`, QA evidence và migration owner. Không coi bản Matrix này tự cập nhật theo code mới.
