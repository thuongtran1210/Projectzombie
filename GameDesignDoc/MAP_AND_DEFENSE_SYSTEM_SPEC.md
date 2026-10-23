# PROJECTZOMBIE — MAP SÔNG QUÊ, TOWER DEFENSE & FRIENDLY ARMY SPEC (DRAFT)

> **Trạng thái:** PROPOSED — chờ D02, D06, D07, D08, D10.  
> **Phạm vi:** Port mô hình chiến trường và gameplay từ Vongxuyen sang Unity; không copy Three.js hoặc giả định đã implement trong Projectzombie.

## 1. Chiến trường và phương hướng

**Map 01 = Map Sông Quê**:
- **Bờ Tây:** Nhà Chính (tọa độ source Vongxuyen x = −12, z = 0), vùng quân ta, điểm tuyển quân, bệ tháp và tuyến lui.
- **Bờ Đông:** vùng do quân địch kiểm soát, spawn enemy, cổng Vong Môn và đấu trường boss.
- **Sông:** khoảng x = 4.45 tới 8.05 trong hệ Three.js; là ranh giới cấm đi thường xuyên ngoài ba cầu.
- **Cầu Bắc:** z ≈ −10.5; **Cầu Giữa:** z ≈ −0.5; **Cầu Nam:** z ≈ 10.5. Map width×height tham chiếu 60×40.
- **Vùng boss:** giữ là zone riêng, tránh ngẫu nhiên spawn enemy thường chắn đường camera.
- **World-space contract:** Sử dụng MappingConfigSO từ (x,z) nguồn sang Unity Vector2(x,y) kèm scale/offset, origin, kiểm tra chính/tà; không hardcode chuyển nhầm z sang y ở nhiều nơi.

**Các lane:** North, Mid, South; waypoint phải tạo một path từ spawn trên bờ Đông tới mục tiêu. Lane không phải 3 bản đồ riêng; enemy chuyển mục tiêu trong lane khi có ally/tower hoặc áp lực quanh Base.

### Quy tắc đối tượng có thể di chuyển
| Faction | Qua cầu | Di chuyển dưới nước | Chốt/bệ tháp |
| --- | --- | --- | --- |
| Hero | Theo luật nav/map | Không, trừ skill hợp lệ | Tương tác xây |
| Ground Enemy | Theo path | Không | Có thể đánh tháp |
| Huyết Dơi | Flying traversal theo AI class | Không bị ràng buộc như ground, nhưng phải có anti-shortcut/LOS | Có thể tấn công mục tiêu |
| Thủy Quái | Spawn/cư trú vùng nước | Theo route chuyên biệt | Có chuyển pha lên bờ nếu AI định nghĩa |
| Dân Binh | Qua cầu trên nav hợp lệ | Không | Phòng thủ/rally |
| Tháp | Cố định | Không | Chỉ vị trí đã cho phép |

Đặc tính di chuyển là **đặc tả mục tiêu**; chưa được xem là Unity implementation hoàn tất.

## 2. Bệ xây dựng — dữ liệu tham chiếu chính xác

Vongxuyen DefenseRules có 10 slot (id không đổi trong mapping):
| ID | X | Z | Vai trò |
| --- | ---: | ---: | --- |
| mid_bridge_north | 2.8 | −2.4 | Bệ Mố Cầu Bắc tuyến giữa |
| mid_bridge_south | 2.8 | 1.4 | Bệ Mố Cầu Nam tuyến giữa |
| mid_court_front | −2.4 | 3.2 | Bệ Tiền Đồn Sân Gạch |
| mid_court_east | 0.5 | 2.4 | Bệ Vọng Gác Bến Nước |
| north_choke_east | 2.8 | −12.4 | Đầu Cầu Tre Bắc |
| north_choke_west | −14.5 | −6.5 | Tháp Canh Rào Bắc |
| south_choke_east | 2.8 | 12.4 | Đầu Cầu Ván Nam |
| south_choke_west | −14.5 | 6.5 | Tháp Canh Bãi Sậy |
| village_flank_north | −9.5 | −3.8 | Cổng Bắc |
| village_flank_south | −9.5 | 3.8 | Cổng Nam |

**Build validation**:
1. Trận đang Playing và không bị lock input.
2. Người chơi không nằm trong modal/cast/rally context.
3. Bệ tồn tại trong map và chưa occupied.
4. Bệ không nằm trong vùng nước, không giao Base, không chắn nav.
5. Có đủ gỗ, đúng tower type và stage restriction.
6. BuildServer/Service execute một transaction duy nhất; thành công mới emit TowerBuilt.
7. Return failure enum cụ thể: InvalidState, OutOfZone, Occupied, NotEnoughWood, PlacementBlocked, MissingPrefab.

**Lưu ý:** Bản Vongxuyen snap theo 10 slot. Không chuyển thành xây tự do trên mọi tile nếu chưa được duyệt D08.

## 3. Nhà Chính

**BASELINE:** maxHP = 500, startingHP = 500.  
**API đề xuất:** BaseHealth : IDamageable / IReadOnlyStructureHealth; TakeDamage(DamageContext), Heal, OnHealthChanged, OnDestroyed.  
**Bất biến:** damage âm/NaN bị từ chối; HP clamp 0..maxHP; destroyed event đúng một lần; invulnerable sau end-run.

**Quy tắc targeting:** Không phải mọi enemy đều bắt đầu lao thẳng vào Base; theo RoleDefinition, target hợp lệ có thể là Ally, Tower, Hero và Base. Chính sách địch phải cân nhắc aggro, khoảng cách/choke point, ưu tiên chiến thuật, nav reachability, threat hysteresis để không nhảy mục tiêu từng frame.

## 4. Tài nguyên gỗ (Wood Economy)

| Thuộc tính | Vongxuyen baseline | Trạng thái |
| --- | ---: | --- |
| Starting Wood | 100 | Có trong DefenseRules |
| Kill reward mặc định | +10 | Có trong DefenseRules, routing có thể khác theo enemy |
| Tower build | 50 | Có |
| L1 → L2 | 40 | Có |
| L2 → L3 | 80 | Có |
| Dân Binh | 40 | Có trong MinionManager reference |
| Phu Tạp Dịch | 30 | Có |
| Thầy Lang | 35 | Có |

**Chưa chốt [D08]:** gỗ vào ví ngay khi enemy chết hay gỗ rơi vật phẩm để Phu thu gom, hoặc hỗn hợp. Nếu gỗ vào ví ngay, vai trò Phu tập trung chuyển/nhặt EXP hoặc vật phẩm vật lý khác; không tạo “thu gom gỗ” chỉ để trang trí.

**Service domain:**
- ReadOnlyBalance
- TrySpend(cost, reason, transactionId) -> Success/Failure
- Grant(amount, sourceKillId) -> Accepted/Duplicate/Invalid
- Reset(newRunId, scenario)
- OnWoodChanged(old, new, cause)

**Unit tests:** không âm tiền, thưởng một lần, tower build rollback, suspend/update race khi Pause, tắt prefab giữa transaction, save state không giữ số dư sau game-over.

## 5. Tháp cung thủ

**BASELINE:** Tower lvl1 maxHP 250, tầm 7; lvl2 maxHP 350, tầm 8; lvl3 maxHP 450, tầm 9; sửa theo service nếu cho phép.

**Chu kỳ:** Idle → AcquireTarget → WindUp → Attack/Projectile → Cooldown → Acquire, song song HealthDamage/Destroyed. Priority target cấu hình nearest/threat/lowestHP; không raycast/allocate tràn trong Update.

**Dữ liệu TowerDefinitionSO**: towerId, prefabAddress, maxLevel, buildCost, upgradeCosts, baseHP, hpPerLevel, rangeByLevel, attackDamageByLevel, attackCooldownByLevel, projectileType, damageElement, targetingPriority, canRepair/repairCost, constructionTime(optional), audio/VFX.

**Feedback:** ghost green/red, cost rõ, preview range, HP bar khi hư, icon nâng tháp, cảnh báo tháp bị phá. Hủy preview/upgrade không trừ gỗ.

## 6. Friendly Army

### Dân Binh Giáo Tre
- Role = FrontlineTank; HP reference 260, recruit 40 gỗ.
- Ưu tiên chắn cầu, cản bước tiến, giữ aggro, chống quái tiếp cận tháp/nhà.
- Behavior FSM tham chiếu: march → hold → engage → combat → fallback, dùng hysteresis và target stickiness.
- Parry/block/counter được mô tả trong Vongxuyen Squad Design như **ý định thiết kế**; triển khai advanced chỉ khi scope D10 cho phép.

### Phu Tạp Dịch
- Role = LogisticsCollector; HP reference 115, recruit 30 gỗ.
- Tìm item/resource/EXP, pickup, mang về điểm giao theo route; flee khỏi threat, không tranh đường cầu với tank.
- Không tự nhân đôi EXP hoặc gỗ nếu Hero cũng auto-pickup. Mỗi pickup có owner/reservation ID.

### Thầy Lang Nam Dược
- Role = HealerSupport; HP reference 135, recruit 35 gỗ.
- Heal Hero/friendly/structure theo eligibility và range; có cooldown, ưu tiên target thấp HP%, không overheal; tránh đứng trước tank.
- Structure repair là hành động khác Heal, phải có design riêng nếu resource/cooldown khác.

### Chung
FriendlyUnitDefinitionSO gồm role, cost, hp, moveSpeed, heal/damage, range, cooldown, aggroRadius, leashRadius, allowedTerrain, formationSpacing, faction. Tất cả spawn qua service có pool và reset lifecycle.

## 7. Commanders, 3 Squads & Rally

**Mục tiêu reference:** 3 đội độc lập; tuyển ba Dân Binh đầu tiên thành Commander 1/2/3; các lính sau gia nhập đội ít người nhất. Stance cơ bản:
- **defend:** giữ bờ Tây, bảo vệ Base và tháp.
- **choke:** giữ cầu/choke-point gần cờ lệnh, đánh kẻ địch tại lối hẹp.
- **follow:** đi theo Hero/Commander ở leash range.
- **Rally:** cờ nằm trong zone hợp lệ, không cắm vào nước/ngoài map/bên dưới chướng ngại.

**Succession:** khi đội trưởng chết nhưng còn thành viên, tự bầu một thành viên còn sống; duy trì lệnh và cờ cũ. Nếu cả đội chết, slot commander được đánh dấu trống và tuyển mới theo quyết định D10.

**Invariant:** một unit chỉ thuộc một squad; mỗi squad có tối đa một commander sống; unit chết được hủy subscription; rally order mới phải override lệnh cũ, không tạo bộ đệm lệnh không giới hạn.

**MVP D10:** có thể giản lược thành 3 đội + stance + rally trước, hoãn commander promotion/block/counter thành phase sau.

## 8. Hợp đồng input & mobile UX

**Mode chuyển đổi độc quyền**: Normal, RelicAiming, TowerBuild, TowerUpgrade, SquadRally, Paused, Modal.
- Chỉ **một** mode điều khiển tương tác world-space ở một thời điểm.
- Joystick di chuyển mặc định không kích hoạt xây dựng.
- Trong TowerBuild, chạm vào slot hợp lệ → xác nhận; chạm nút cancel không tiêu gỗ.
- Trong SquadRally, chọn squad → hiển thị zone → chọn cờ → xác nhận.
- Mọi pending command bị hủy an toàn khi Pause/GameOver/SceneUnload.

## 9. Wave & lane navigation integration

Nguồn spawn được stage timeline gọi, nhưng locator trả vị trí hợp lệ theo zone/lane thay vì vị trí ngẫu nhiên quanh Player. Stage event phải có các trường stageId, eventId, atSeconds, enemyTypeId, count, laneOrZone, priority, isBoss.

Nếu cap active bị đầy, queue có lifetime; policy drop/defer phải không tạo boss bị biến mất vô thông báo. Enemy có lanePath state, target memory và tránh lặp waypoint; reset on pool checkout.

**Boss arena:** Dạ Xoa tại bờ Đông; raid/engage rule rõ ràng theo thời điểm; không đưa hành vi boss sang bờ Tây nếu không có path/cờ raid.

## 10. Cấu trúc code Projectzombie — đề xuất

- Assets/Features/Defense/Core: DefenseRunController, BaseHealth, DefenseEconomy
- Assets/Features/Defense/Towers: TowerDefinitionSO, TowerBuildService, TowerPlacementController, TowerCombatController
- Assets/Features/FriendlyArmy: FriendlyUnitDefinitionSO, FriendlyUnitBrain, SquadManager, RallyPointController
- Assets/Features/Maps: LaneDefinitionSO, LanePathGraph, MapDefenseZones, WorldProjectionConfigSO
- Assets/Features/Spawners/Spatial: LaneSpawnLocator
- Assets/Features/UI/Defense: DefenseHUDPresenter/View, TowerBuildPanel, SquadCommandDock
- Assets/_Data/Defense: ScriptableObject balance assets, stage01_defense, tower/friendly configs

Các tên class ở đây chưa tồn tại trong Projectzombie trừ khi được đánh dấu trong GDD, nên phải tra trước khi thực thi.

## 11. Event & authority contracts

- RunStarted(runId), RunTimeChanged(t), DefensePhaseStarted(phase), RunEnded(result)
- EnemySpawned(spawnId, typeId, lane), EnemyKilled(killId, killerFaction, typeId)
- WoodGranted, WoodSpent, TowerBuilt, TowerUpgraded, TowerDestroyed
- BaseDamaged, BaseDestroyed, AllyRecruited, SquadOrderChanged, CommanderPromoted
- Cùng một runId cho tất cả event; consumer bỏ qua event từ run cũ.
- Quy định chống reward duplicate, hủy subscription lúc reset và không có nhiều service cùng authoritative về BaseHP/Wood/RunTime.

## 12. Test cases / Acceptance Criteria

**Map**
- Mỗi lane có đường qua đúng cầu; enemy ground không xuyên sông; ưu tiên nav recalculation khi bệ tháp bị phá; test spawn boss hợp lệ.
- Đổi aspect ratio/camera không làm spawn lệch; 2D collider đúng.

**Defense Economy & Tower**
- Start 100 gỗ → xây 1 tháp còn 50; xây tháp thứ 2 còn 0; build fail khi không đủ gỗ; upgrade 1→2 đúng 40; upgrade 2→3 đúng 80.
- Tower cấp 3 không thể nâng; tháp 0 HP bị hủy khỏi slot; sửa tháp không vượt maxHP.
- Base 500 HP → mất hết → GameOver một lần.

**Friendly & Squad**
- Recruit theo cost, không spawn đè; lính không đánh nhau; healer chỉ heal target hợp lệ; collector không nhân đôi pickup.
- 3 đội nhận lệnh riêng, follower không vượt zone, khi commander chết người kế nhiệm giữ đúng lệnh (nếu D10 duyệt).
- Pause ngắt clock/AI, restart không giữ Wood/units/towers từ run trước.

**Mobile**
- Có HP Base, Wood, Timer, 3 cảnh báo lane trong HUD; thao tác build/rally không làm cast relic; aspect portrait/landscape.

## 13. Sources

- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/world/MapBenQue.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/mapConfig.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/defense/DefenseRules.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/defense/MinionManager.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/PLAN-DANBINH-SQUAD-RALLY-SYSTEM.md
- https://github.com/thuongtran1210/Projectzombie/blob/main/Assets/Features/Spawners/Spatial/CameraAwareSpawnLocator.cs
