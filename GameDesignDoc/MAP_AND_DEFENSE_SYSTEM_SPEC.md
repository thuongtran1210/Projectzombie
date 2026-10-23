# TREBRAVE — MAP SÔNG QUÊ + 3D TOWER DEFENSE SPEC (APPROVED DESIGN)

> **Trạng thái:** **D02=C, D06=A, D07=C, D08=B, D10=C đã APPROVED 2026-10-23**. chủ dự án **đang chuyển Unity 2D sang full 3D**, trạng thái tích hợp/QA từng feature chưa xác minh; friendly army chỉ POST-MVP.  
> **Phạm vi:** Chuyển chiến trường và gameplay Vongxuyen sang **Unity full stylized 3D**, không copy Three.js. Map/Tower/Wood là MVP; Friendly Army/Squad là BACKLOG, không làm release gate.

## 1. Chiến trường Unity full 3D và phương hướng [D02=C]

**Map 01: Map Sông Quê**, world geometry reference 60 × 40 m trên ground plane **XZ**, trục **Y = độ cao**.
- Bờ Tây (x âm): Nhà Chính, 10 bệ xây tháp, vị trí Hero spawn, phòng tuyến.
- Sông: vùng water impassable cho ground agents trừ qua 3 cầu; water traversal riêng cho Thủy Quái.
- Cầu Bắc z≈−10.5, Giữa z≈−0.5, Nam z≈10.5, tương ứng lane North/Mid/South.
- Bờ Đông (x dương): vùng spawn địch, Vong Môn, boss arena, toàn bộ là phần quê hương đang bị chiếm.
- Cấu trúc Unity: Terrain/MeshRenderer, MeshCollider/Collider 3D, NavMesh surface/links hoặc 3D path graph, World-space raycast target & tower pedestal sockets. **Không dùng TilemapCollider2D / Rigidbody2D / Physics2D cho scene Defense 3D.**
- Unity camera stylized 3D top-down/3-4 với ánh sáng, shadow LOD và line-of-sight đọc rõ enemies/towers/Boss; các giá trị góc camera/URP assets đang ở P06.
- **Bảo tồn Survival Legacy 2D** trên scene/pipeline/adapter riêng; dùng lại pure domain, pooling & services khi không phụ thuộc Unity 2D.

| Đối tượng | Navigation và vùng hợp lệ |
| --- | --- |
| Hero | Controller 3D, đi qua ba cầu; ground/water blockers theo MapSpec |
| Ma Giáp / ground enemy | 3D nav graph qua cầu, đến target tower/base theo policy |
| Huyết Dơi / Phi Cương | 3D flying navigation, giới hạn zone/camera clipping |
| Thủy Quái | Water zone và entry/exit shore points được validate |
| Tháp | Build socket cố định, range/line-of-sight theo Physics3D |
| Friendly Army | **POST-MVP**, path/rally spec giữ để tham chiếu nhưng không triển khai ở milestone đầu |

**Hợp đồng tọa độ:** Vongxuyen đã dùng XZ, nên có thể dùng layout tương ứng trên ground plane Unity XZ với scale/offset cấu hình; không đổi sang Unity x/y như bản kế hoạch 2D.

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
2. Người chơi không nằm trong modal/cast context (**rally chỉ POST-MVP**).
3. Bệ tồn tại trong map và chưa occupied.
4. Bệ không nằm trong vùng nước, không giao Base, không chắn nav.
5. Có đủ gỗ, đúng tower type và stage restriction.
6. TowerBuildService thực thi một transaction duy nhất; thành công mới emit TowerBuilt. Không đòi hỏi network server ở Defense single-player MVP.
7. Return failure enum cụ thể: InvalidState, OutOfZone, Occupied, NotEnoughWood, PlacementBlocked, MissingPrefab.

**Lưu ý:** Bản Vongxuyen snap theo **10 slot cố định**. Đây là baseline Map 01 đã chọn; muốn xây tự do phải có Change Request riêng, **D08 đã duyệt chỉ nói về cơ chế thu gom gỗ**, không liên quan giới hạn bệ xây.

## 3. Nhà Chính — nguồn thất bại DUY NHẤT [D07=C]

**BASELINE:** 500 MaxHP, vị trí tham chiếu VX x=−12,z=0. BaseHealth quản lý HP, DamageContext, faction, invulnerability, OnDamaged/OnDestroyed exactly once.

**Hợp đồng bất biến:** **Chỉ BaseHP ≤ 0 => LOSE**. Hero HP≤0 không kết thúc trận; **P03 APPROVED:** đếm ngược hồi sinh tăng theo Hero Level, hồi tại **HeroInitialSpawnPoint** đã lưu lúc bắt đầu run; công thức/giá trị cụ thể nằm HeroRespawnConfigSO, chưa khóa số. **Hạ final boss yaksha_boss => WIN** nếu Nhà Chính chưa chết; hạ miniboss không tạo Win. Đồng hồ 600s không tự quyết Win/Lose (P02).

Enemy attack range đến cấu trúc phải được đo bằng 3D collider/nav target, không cast TargetSelector<PlayerContext> sang Base. UI luôn hiển thị Base HP và cảnh báo khi sắp bị phá.

## 4. Tài nguyên gỗ — DROP/PICKUP ONLY [D08=B]

**Owner-approved:** EnemyKilled **không** ghi trực tiếp +wood vào Wallet. Enemy chết tạo **WorldWoodDrop** có lootInstanceId/runId/value/position/sourceEnemyId; 3D pickup collider/range, ground placement hợp lệ; **Hero phải nhặt** để grant số dư Wood.

**POST-MVP:** Phu Tạp Dịch được thêm vào sau (D10=C), sẽ tìm/vận chuyển pickup qua cùng LootClaim/Collect transaction. Trong MVP **không spawn Phu và không tự thu gom**.

| Tham số | Baseline từ VX | Trạng thái |
| --- | ---: | --- |
| Starting Wood | 100 | Reference, chưa balance-lock |
| Wood drop mỗi quái thường | 10 | Reference, không auto grant |
| Tower build | 50 | Reference |
| Tower L1→L2 | 40 | Reference |
| Tower L2→L3 | 80 | Reference |
| Dân Binh | 40 | POST-MVP |
| Phu | 30 | POST-MVP |
| Thầy Lang | 35 | POST-MVP |

**Luồng bắt buộc:**
1. EnemyDeath quyết định một killId; reward resolver xác định giá trị wood và tạo loot nếu chưa từng tạo cho killId.
2. Drop được đẩy lên ground nav/physics point hợp lệ có thể tiếp cận; không nằm trong sông sâu, dưới terrain hoặc trong boss prop.
3. Hero chạm pickup 3D và xác thực lootInstanceId/runId, trạng thái unclaimed, phạm vi, game Playing.
4. Atomic claim -> WoodWallet.Grant(value) -> Despawn loot. Nếu spawn/despawn pool fail, không grant trùng.
5. UI nhận WoodChanged; TowerBuildService dùng **wood đã nhặt** để TrySpend, transaction fail không trừ.
6. Cleanup khi reset/run-ended; loot từ run cũ không nhặt được trong run mới.

**P05 BASELINE APPROVED:** WoodDrop **lifetime = 90 giây gameplay**, đưa vào **WoodDropConfigSO**; 90 giây là giá trị có trong **Vongxuyen EXP_CONFIG.gemLifetimeSeconds**, **không phải gỗ rơi đang tồn tại trong VX**. PickupRadius/MagnetRadius/pooling/cap/stack giữ cấu hình hoặc TBD, không suy diễn các con số từ WoodSource. Khi thời gian hết drop despawn, không tự cộng gỗ. Không cho phép quay về auto wood grant.

**Acceptance:** kill enemy nhưng chưa nhặt thì số dư không đổi; nhặt một lần tăng đúng value; pickup lần 2 không tăng; wood drop tồn tại tối đa 90s gameplay (P05 default; không tự grant khi expire); nhiều enemy chết cùng vị trí vẫn giữ transaction identity; Hero nhặt được khi Phu chưa được triển khai.

## 5. Tháp cung thủ

**BASELINE:** Tower lvl1 maxHP 250, tầm 7; lvl2 maxHP 350, tầm 8; lvl3 maxHP 450, tầm 9; sửa theo service nếu cho phép. Tower acquisition/damage dùng Physics3D (LOS, OverlapSphere) trong Map 01, không Physics2D.

**Chu kỳ:** Idle → AcquireTarget → WindUp → Attack/Projectile → Cooldown → Acquire, song song HealthDamage/Destroyed. Priority target cấu hình nearest/threat/lowestHP; không raycast/allocate tràn trong Update.

**Dữ liệu TowerDefinitionSO**: towerId, prefabAddress, maxLevel, buildCost, upgradeCosts, baseHP, hpPerLevel, rangeByLevel, attackDamageByLevel, attackCooldownByLevel, projectileType, damageElement, targetingPriority, canRepair/repairCost, constructionTime(optional), audio/VFX.

**Feedback:** ghost green/red, cost rõ, preview range, HP bar khi hư, icon nâng tháp, cảnh báo tháp bị phá. Hủy preview/upgrade không trừ gỗ.

## 6. Friendly Army (BAO GỒM PHU) — CHỈ POST-MVP [D10=C]

**Không thực hiện trong Defense MVP:** không tuyển Dân Binh, không spawn Phu Tạp Dịch, không tuyển Thầy Lang, không hệ Commander/Squad/Rally. Các class, HUD và kinh tế tuyển quân **không** là tiêu chí nghiệm thu Map 01 MVP.

**Thiết kế tham chiếu VX để phục vụ future phase:**
- Dân Binh Giáo Tre: HP 260, cost 40, tank/chặn cầu/đỡ đòn.
- Phu Tạp Dịch: HP 115, cost 30, vận chuyển/thu gom world pickups; sẽ dùng **cùng LootClaim service** với Hero; không phát tiền lần hai.
- Thầy Lang Nam Dược: HP 135, cost 35, healer/bảo dưỡng phòng tuyến có cooldown.
- Squad: tối đa 3 đội, stance defend/choke/follow, rally flags, commander succession; phần block/counter theo spec VX sâu hơn được giữ trong backlog.
- Tất cả thông số trên là REFERENCE-VX, không phải MVP build assets.

**Điều kiện bắt đầu future phase:** MVP Hero+Base+Tower+WoodDrop+Boss ổn định; có 3D NavMesh solution, target/faction APIs; xác định scope performance trước khi thêm NPC đông.

## 7. Post-MVP Squad Commander / Rally — BACKLOG

**D10=C đã duyệt:** hệ đội quân/tướng chỉ huy **không nằm trong MVP**. Không tạo bắt buộc SquadDock, Commanders, flags, zone overlays, revive commander trên giao diện đầu tiên.

Khi được làm về sau: 3 đội dân binh độc lập, orders defend/choke/follow, 3D world-space rally point nằm trong zone hợp lệ, 1 commander sống/squad, succession khi commander chết, followers giữ formation với target hysteresis; không đặt cờ trong nước/ngoài map hoặc nơi không có NavMesh.

UI cần ưu tiên thao tác TowerBuild/ACTIVE Relic trước. Mọi class FriendlyArmy là proposed future modules, **không khẳng định đã port**.

## 8. Hợp đồng thao tác màn hình chạm MVP

**Các input mode:** NORMAL, RELIC_AIM, TOWER_BUILD, TOWER_UPGRADE, PAUSED, LEVEL_UP, GAME_OVER. **SQUAD_RALLY chỉ POST-MVP**, không cần implement MVP.
- Joystick điều khiển Hero 3D; raycast Physics3D chọn pedestal hợp lệ; HUD hiển thị wood hiện có/pickups.
- Relic Active ghost 3D và Tower Build ghost **không được active đồng thời**; Cancel cast không tiêu cooldown; Cancel build không tiêu Wood.
- Hero incapacitation (P03) phải khóa thao tác thích hợp nhưng tuyệt đối không tạo RunEnded(LOSE).
- Pause/LevelUp/GameOver hủy ghost, khóa toàn bộ runtime hành động, không xuất hiện một tháp vì UI click xuyên.
- Context build/upgrade rõ cost, show range & valid socket; world-space HP của Base và Tháp; 3 lane threat + Dạ Xoa boss HP.
- Không có Recruit/Command/Rally trên MVP HUD.

## 9. Wave & 3D lane integration — Map 01

3D Stage timeline reference: 600 giây/5 phase, 34 events mỗi 18 giây, mini Ma Giáp Boss ~108s, final Dạ Xoa ~510s. SpawnLocator dùng 3D Nav/zone fixed theo lane; không random offscreen quanh Hero kiểu survival 2D.

Các trạng thái bắt buộc: SpawnScheduled → SpawnEligible → Spawned / Deferred → Alive → Killed → LootCreated → LootClaimed. Enemy dead chỉ tạo loot một lần. Queue cap/backpressure và priority boss phải bảo đảm không mất final boss khi active count đạt 32.

**Victory trigger:** final yaksha_boss killed + BaseHP>0 => **WIN NGAY; hủy scheduler + pending spawns, chặn spawn mới** (P08 APPROVED). **Loss trigger:** BaseHP=0 only. **T=600s:** Nếu Boss còn sống thì **OVERTIME** cho đến khi Boss chết hoặc Base bị phá; scheduled waves kết thúc ở t=594, không tự phát thêm event (P02 APPROVED). Hero Downed -> respawn theo cấp tại spawn đầu (P03). Boss Mini killed at 108s không end run.

## 10. Cấu trúc code đề xuất (Unity full 3D)

**MVP modules**
- Assets/Features/Defense/Core: DefenseRunController, BaseHealth, WoodWallet, LootClaimService, WorldWoodDropManager, WoodDropConfigSO (initial lifetime 90s), RunResultResolver, HeroRespawnConfigSO.
- Assets/Features/Defense/Towers: TowerDefinitionSO, TowerBuildService, TowerPlacementController (3D raycast), TowerCombatController.
- Assets/Features/Maps/World3D: MapSongQueDefinitionSO, LaneDefinitionSO, 3D LanePathGraph, NavMeshLinks, WaterAccessPolicy.
- Assets/Features/Spawners/Spatial: LaneSpawnLocator3D.
- Assets/Features/Enemies/Defense3D: DefensiveTargetPolicy, IDamageableTargetAdapter, 3D EnemyMovement.
- Assets/Features/Weapons/ActiveRelics3D: one-slot ActiveRelicController, 3D GhostAim, CastValidator. **MVP chỉ Slipper**; 4 relic khác LOCKED.
- Assets/Features/UI/Defense: BaseHP, Wood/loot feedback, LaneThreat, TowerPanel, BossHUD, HeroRespawnCountdown; after Victory no spawn UI.

**POST-MVP only:** Assets/Features/FriendlyArmy, SquadManager, Commander, RallyPointController, RecruitHUD.

Các tên class là đề xuất. Không cần port/destroy code 2D Legacy, chọn pure gameplay layer dùng chung và 3D adapter mới.

## 11. Event/Authority Contracts

- RunStarted(runId), GameplayTimerUpdated(time), PhaseStarted(phase), RunEnded(result,reason) exactly once.
- BaseDamaged, BaseDestroyed → **Lose**; FinalBossDefeated → **Win** khi Base alive. HeroDowned **không** phát GameOver.
- EnemyKilled(killId,enemyId,pos), WoodLootSpawned(lootId,killId,value), WoodLootClaimed(lootId,actorId), WoodChanged(balance); **EnemyKilled không trực tiếp WoodChanged**.
- TowerBuilt, TowerUpgraded, TowerDamaged, TowerDestroyed.
- All events carry runId; reward/pickup only once. Scenarios ignore old run and wrong mode; when Legacy loaded, Defense events disabled.
- **No FriendlyUnit/Squad events required for MVP** (future once phase approved).

## 12. Acceptance / Tests

**Full 3D Map:** 3 bridges passable for ground enemies/hero; water obstacles use Physics3D; base/tower colliders receive legit damage; flying/water enemy nav modes; 3D camera handles occlusion and mobile UI.

**Tower/Economy:** start baseline 100 wood; build 1 tower cost 50, second cost 50; kill one enemy but do not collect -> wood unchanged; collect one drop -> +value; double collect -> no extra; pickup cannot be inaccessible or use an expired runId; upgrade costs level 2/3 respect balance data.

**Rules:** Base HP reaches 0 -> one LOSE; Hero HP reaches 0 while Base alive -> **đếm ngược và hồi sinh tại spawn đầu, không LOSE**; Ma Giáp miniboss killed -> no WIN; Dạ Xoa killed while Base alive -> **WIN tức thì + không spawn quái nữa**; 600s Boss alive -> **OVERTIME**, không auto result; run result atomic.

**MVP scope:** user can finish Defense without Phu, Dân Binh, Thầy Lang, squad/rally; chỉ **một ACTIVE Slipper** với basic damage/ghost/cooldown, 4 FUN relic khác LOCKED; no recruit dock as release dependency.

**Legacy regression:** existing Survival Unity 2D scene, old enemy prefabs, code/Save work in Legacy Mode; no Force 3D migration of legacy maps.

## 13. Sources

- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/world/MapBenQue.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/mapConfig.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/defense/DefenseRules.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/defense/MinionManager.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/PLAN-DANBINH-SQUAD-RALLY-SYSTEM.md
- https://github.com/thuongtran1210/Projectzombie/blob/main/Assets/Features/Spawners/Spatial/CameraAwareSpawnLocator.cs
