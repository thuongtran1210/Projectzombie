# PROJECTZOMBIE — MASTER GAME DESIGN DOCUMENT v6.0 (DRAFT)

> **Trạng thái:** REVIEW REQUIRED / CHƯA CHỐT / KHÔNG PHẢI BẢN PHÁT HÀNH.  
> **Ngày soạn:** 2026-10-23. **Mục tiêu:** hợp nhất gameplay Projectzombie với các trụ cột được xây dựng trong Vongxuyen / TREBRAVE.  
> **Repository triển khai:** Projectzombie, Unity 2022.3 LTS / C# / Android. **Repository thiết kế tham chiếu:** Vongxuyen, Three.js/WebGL.  
> **Nguồn khảo sát:** Projectzombie main 657c8215; Vongxuyen main 5f5d9b26.  
> **Quy tắc:** Các mục có ký hiệu **[Dxx]** là đề xuất chưa duyệt. Chỉ được chuyển trạng thái sang APPROVED sau khi chủ dự án chọn tại [Decision Checklist](DECISION_CHECKLIST_GDD_V6.md). Đây là bản **thiết kế tương lai**, không khẳng định tính năng đã được Unity triển khai.

## 0. Quy tắc nguồn sự thật

- GDD này là **đề xuất gameplay chuẩn cho Projectzombie**, không đồng nghĩa thay engine, đổi toàn bộ asset hoặc xóa chức năng Unity hiện có.
- Tài liệu hệ thống chuyên sâu: [Map & Defense](MAP_AND_DEFENSE_SYSTEM_SPEC.md), [Enemy Roster](ENEMY_ROSTER_VONGXUYEN.md), [Scenario](LEVEL_SCENARIO_GUIDE.md).
- [Document Refactor Ledger](DOCS_REFACTOR_LEDGER.md) chỉ rõ tài liệu cũ nào còn hiệu lực, đang xung đột hoặc cần chuyển thành phụ lục.
- Số liệu lấy từ code Vongxuyen được dán nhãn **BASELINE**, không phải giá trị cân bằng cuối cùng.
- Phân biệt **EXISTING-PZ**, **REFERENCE-VX**, **PROPOSED-PZ**, **APPROVED**, **DEPRECATED**, **BACKLOG**; không sử dụng “đã có” để mô tả đề xuất chưa implement.

## 1. Tầm nhìn và trụ cột trải nghiệm

**Pitch đề xuất:** Người chơi chọn **một nhân vật và một pháp bảo** cho một trận đấu, bảo vệ phần quê hương còn giữ được trước làn sóng yêu ma. Người chơi trực tiếp chiến đấu, thu thập EXP để nâng cấp nhân vật, nhận gỗ từ trận để xây/nâng tháp và tuyển đồng minh, điều quân giữ ba tuyến cầu bắc qua dòng sông. Thắng khi hoàn thành mục tiêu của màn và bảo toàn Nhà Chính theo luật được chốt.

**Các trụ cột bắt buộc trong bản hợp nhất:**
1. **Map Sông Quê:** hai bờ sông, ba cầu, ba lane Bắc/Giữa/Nam; Bờ Tây là căn cứ phòng thủ, Bờ Đông là lãnh thổ quê hương đang bị chiếm đóng.
2. **1 nhân vật trong run:** một player-controlled hero; số lượng nhân vật có thể chọn ở sảnh là vấn đề riêng **[D03]**.
3. **1 pháp bảo trong run:** chỉ một relic slot, không tự biến thành 6 hay 17 slot; danh mục và cách kích hoạt theo **[D04–D05]**.
4. **Hero + Tower + Allies:** sức mạnh đến từ kỹ năng tướng, công trình phòng thủ và lực lượng đồng minh; không biến Tower Defense thành một game chiến thuật thuần không có hero combat.
5. **Quái và Boss Vongxuyen:** dùng roster, silhouette, vai trò AI và cách tiến công theo lane từ mã nguồn Vongxuyen; cách xử lý quái Projectzombie cũ theo **[D09]**.
6. **Roguelite trong run:** EXP/gem, chọn 1/3 thẻ, build variation; tách khỏi kinh tế gỗ.

**Không mặc nhiên đồng nhất:** thiết kế gameplay với công nghệ 3D. **[D02]** quyết định dùng Unity 2D hiện tại hoặc lên kế hoạch chuyển toàn bộ sang 3D. Không tự động quảng cáo cơ chế chiếm lại lãnh thổ vì hiện chỉ là tầm nhìn.

## 2. Fantasy, bối cảnh và bản sắc [D01, D11]

- Vòng lặp narrative tham chiếu TREBRAVE: **Giữ đất → Chuẩn bị → Vượt sông → Đánh tan quân chiếm đóng → Giành lại vùng quê** (hai bước cuối là mục tiêu chiến dịch dài hạn, không phải tính năng MVP).
- **Map 01 = Map Sông Quê**; không còn sử dụng Bến Đò Vong Xuyên làm tên hiển thị của Map 01 nếu quyết định hợp nhất được phê duyệt.
- Quái dân gian, pháp bảo FUN, địa danh và mỹ thuật văn hóa Việt được giữ phù hợp; không buộc tất cả quái thuộc cùng một lore thần thoại/địa phủ.
- Tên thương hiệu sản phẩm (Projectzombie/Vong Xuyên/TREBRAVE) và việc giữ cốt truyện Tứ Bất Tử chưa được quyết định ở **[D11]**. Không đổi tên repository, package, namespace, Addressable keys trong PR sửa tài liệu.

## 3. Gameplay loop [D01, D06, D07]

**Sảnh:** chọn tướng → chọn một pháp bảo → chọn Map 01 / cấu hình trận → bắt đầu.

**Đầu trận:** spawn Hero ở Bờ Tây; Nhà Chính khởi tạo HP; nạp cấu hình bản đồ 3 lane / bệ tháp; khởi tạo Wood Economy và Timeline; giao diện cho phép xây tháp/tuyển lính theo giới hạn.

**Trong trận:**
1. Quái sinh ở spawn zone bờ Đông / rặng Bắc / sông / khu vực đặc biệt; theo navigation hợp lệ về các mục tiêu phòng thủ.
2. Hero di chuyển, Attack, Dash, Signature Skill và kích hoạt/tận dụng một pháp bảo theo quyết định cơ chế điều khiển.
3. Hạ quái → thưởng EXP và gỗ qua kênh sự kiện chống double reward.
4. EXP → lên level → chọn một trong ba thẻ nâng cấp.
5. Gỗ → xây tháp, nâng tháp, tuyển đồng minh. Không được trả bằng EXP/Coins.
6. Ra lệnh phòng tuyến/đặt cờ cho dân binh, tập trung ở các choke point khi cần.
7. Wave và Boss tăng áp lực có telegraph, UI báo lane/HP/tháp/boss.

**Kết trận:** run kết thúc duy nhất một lần; chiến thắng và thất bại được tính theo chính sách **[D07]**, lưu thống kê và phát thưởng meta nếu đã chốt. Tất cả controller ngừng cập nhật, hủy ghost preview, hoàn trả pooled objects.

**Thời lượng khởi điểm [D06]:** 600 giây theo STAGE_01 của Vongxuyen, nhưng mốc boss 510 giây và luật khi hết giờ phải thống nhất. Không giữ đồng thời luật 20:00 của GDD cũ và 10:00 của bản mới.

## 4. Map 01 — Map Sông Quê [D02]

- **Bố cục reference:** 60 x 40 world units; hai bờ, một đoạn kênh, ba cầu tại tuyến Bắc z≈−10.5; Giữa z≈−0.5; Nam z≈10.5 (hệ tọa độ Vongxuyen x/z chỉ là tham chiếu).
- **Bờ Tây:** Nhà Chính, 10 bệ công trình chiến lược, vùng spawn Hero/allies, kho tài nguyên/điểm tập kết và vùng phòng thủ.
- **Bờ Đông:** vùng spawn địch, lối tới đấu trường boss, cửa Vong Môn; chưa có chức năng giành quyền kiểm soát vùng đất.
- **Navigation:** waypoint graph được ánh xạ sang hệ tọa độ Unity 2D x/y; cấm địch đi xuyên mặt nước (trừ class thủy quái có route riêng), cấm tháp chặn toàn bộ lối đi.
- **Validation:** kiểm tra liên thông 3 cầu, mỗi spawn zone có đường đến mục tiêu; build sites không nằm trên nước; prefab/Tilemap có collider đúng Layer.
- **Không tự chuyển tọa độ 3D trực tiếp thành tọa độ Unity:** cần quy tắc scale, offset, projection và vị trí ground plane.

Chi tiết và sơ đồ phụ thuộc: [MAP_AND_DEFENSE_SYSTEM_SPEC.md](MAP_AND_DEFENSE_SYSTEM_SPEC.md).

## 5. Một nhân vật — một pháp bảo [D03, D04, D05]

**Hợp đồng bắt buộc:** mỗi run chỉ có 1 player-controlled Hero và tối đa 1 pháp bảo trang bị. Không cho phép thẻ nâng cấp phát sinh slot mới; chuyển pháp bảo giữa trận chỉ khi có quyết định sản phẩm mới.

**Hero sở hữu:** Basic Attack/Combo, Dash, Signature Skill, HP, EXP, thẻ nâng cấp, khả năng tương tác build/rally và phân biệt đồng minh/địch. **Số hero có thể unlock ở sảnh** và hero khởi đầu phụ thuộc [D03]. Projectzombie có bộ 4 hero được đặc tả, còn Vongxuyen tham chiếu Thư Sinh/Thanh Đồng/Ẩn Sĩ/Đạo Sĩ; cần mapping cụ thể trước khi viết character roster chuẩn.

**Pháp bảo:** nên giữ đúng 1 slot trong WeaponManager hiện tại (MAX_WEAPONS = 1). Năm pháp bảo FUN tham chiếu: Dép Tổ Ong, Nồi Cơm, Điếu Cày, Chiếu Trải và Chổi Lông Gà. Danh mục 12 cổ phong trong GDD cũ không mặc nhiên xóa khỏi code; phân loại sản phẩm qua [D04].

**Xung đột cần chốt [D05]:** GDD Projectzombie mô tả pháp bảo tự động/passive trong khi Vongxuyen dùng nút cast chủ động với ghost aim; cùng một vật phẩm có thể có hiệu ứng nâng cao khác nhau giữa 2 runtime. Định nghĩa rõ AUTO, ACTIVE, hay HYBRID theo relic trước khi chỉnh WeaponManager. Nếu ACTIVE, nút Relic nằm cạnh Attack/Dash/Signature, aim preview hợp lệ, hủy không tiêu cooldown.

## 6. Nhà Chính, tháp và tài nguyên gỗ [D08]

### 6.1 Nhà Chính
- **BASELINE:** HP 500, đặt ở khu vực phòng thủ phía Tây.
- Nhận damage từ enemy/ability đã qua kiểm tra hợp lệ; không bị trừ HP khi đã GameOver; phát event base-damaged/base-destroyed đúng 1 lần.
- UI: thanh HP cố định, cảnh báo dưới ngưỡng và hiệu ứng khi bị phá.
- **[D07]:** quyết định thua ngay khi Hero chết, Base chết, hay có revive/comeback.

### 6.2 Wood Economy
- **BASELINE:** gỗ đầu trận 100, mặc định +10 gỗ/kill, chi phí xây tháp 50 gỗ; nâng cấp level 1→2: 40, level 2→3: 80.
- Gỗ là **currency in-run**; không chuyển sang Cổ Tiền hay EXP. Chỉ service có thẩm quyền thay đổi số dư.
- Thưởng kill phải dùng identity của một enemy instance/run; chống double grant (kể cả chết bởi tower/ally/hero/DoT hoặc cleanup pool). Có thể chuyển sang “gỗ rơi/Phu thu gom” nếu chọn [D08].
- Build transaction phải atomic: kiểm tra slot, stage, tiền, preview; trừ tiền và spawn đồng thời. Nếu spawn thất bại phải hoàn tiền.

### 6.3 Tower System
- **BASELINE:** 10 bệ cố định; tháp HP 250 level 1; maximum level 3, tăng 100 MaxHP mỗi level; range level 1 = 7, +1 per level.
- Tháp tự tìm địch trong tầm, tấn công, nhận damage và bị phá. Cơ chế sửa/tháp khác loại sẽ cần quyết định riêng.
- Không xây trên nước, lên bệ đang có tháp, trong bán kính cấm của Base, ngoài vùng hợp lệ, hoặc khi UI chặn input.
- Ưu tiên tái sử dụng Physics2D NonAlloc, damage pipeline, projectile pooling và TargetingUtility.

### 6.4 Friendly Army
- Ba vai trò tham chiếu: **Dân Binh Giáo Tre** (40 gỗ, tank/chặn cầu), **Phu Tạp Dịch** (30 gỗ, thu gom/tiếp vận), **Thầy Lang Nam Dược** (35 gỗ, hồi máu/hậu cần). Đây là BASELINE, chưa chốt phạm vi launch [D10].
- Đồng minh không được nhận XP như Hero hoặc tính là enemy; có faction/targeting rõ ràng.
- Đòn tấn công lính đi qua cùng damage resolution; kill reward không trùng.

### 6.5 Squad / Commander
- Tham chiếu Vongxuyen: tối đa 3 tiểu đội dân binh, stance **defend / follow / choke** (vị trí rally tùy thế trận).
- Có đội trưởng/đội viên, bổ nhiệm người kế nhiệm khi chết, rally flag, vùng hoạt động hợp lệ, ưu tiên mục tiêu và trở về đội hình.
- Cơ chế commander kế nhiệm chi tiết là reference design; mức ưu tiên triển khai trong [D10].
- Mobile: click/chạm đội trưởng hoặc chọn đội ở Dock, hiển thị zone/flag; không để lệnh điều quân va chạm với build, cast và joystick.

## 7. Kẻ địch và Boss — roster Vongxuyen [D09]

**Danh sách tồn tại ở enemy factory:** Ma Giáp Cương Thi (magiap), Thủy Quái Rạch Đêm (thuyquai), Huyết Dơi Đêm (huyetdoi), Ma Giáp Thống Lĩnh (magiap_boss), Tiểu Quỷ Nón Lá Dù (quaidu), Phi Cương Tinh Anh (phicuong). Boss riêng Tướng Dạ Xoa (yaksha_boss).

**Tính trạng quan trọng:** STAGE_01 chạy lịch cho magiap/huyetdoi/thuyquai và ma giáp boss; quái dù, phi cương và Dạ Xoa có nguồn runtime/sự kiện khác, **không tuyên bố tất cả cùng xuất hiện trong timeline cơ bản**.

**Tổ chức AI:**
- Thường/giáp: đi trên lane, ưu tiên quân phòng thủ và công trình hợp lệ; quái có đặc tính khác nhau.
- Bay: có quy tắc route riêng và tấn công đột kích.
- Thủy quái: spawn nước/ven sông hợp lệ, không tùy ý xuyên đất hoặc công trình.
- Elite/Boss: telegraph trước khi gây damage, giai đoạn tấn công riêng, không spawn nhầm khi timer pause.
- Lựa chọn mục tiêu: Player, tower, ally, Base, và lựa chọn theo vai trò / threat / phạm vi; không áp nguyên xi ITargetSelector hiện trả PlayerContext để target Base.
- Spawn phải có cap, backpressure, đường đi hợp lệ, pooling và hủy sau run.

**Những enemy Projectzombie cũ** (Ma Đòi Nợ, Ngưu Đầu/Mã Diện, Diêm Vương, v.v.) chưa được công nhận thuộc Map 01; cách archive/move to future map phụ thuộc [D09]. Xem [ENEMY_ROSTER_VONGXUYEN.md](ENEMY_ROSTER_VONGXUYEN.md).

## 8. Wave, Boss và luật thắng/thua [D06, D07]

**BASELINE tham chiếu:** STAGE_01 có duration 600s, 34 event cách nhau 18s, chia 5 phase, cap active 32, pending 40, max spawn/frame 4, pending lifetime 25s. Event index 6 có Ma Giáp Thống Lĩnh. Tướng Dạ Xoa có spawn/AI riêng ở mốc khoảng 510s.

**Đề xuất biểu diễn dữ liệu:** StageDefinitionSO -> DefenseScenarioSO -> WaveTimelineSO -> SpawnEvent(type, lane, zone, count, timestamp, objective). Sử dụng WaveScheduler hiện có; không viết clock thứ hai.

**Rủi ro cụ thể:** Stage_01_BambooForest.asset có stageDurationSeconds 600 nhưng Level1_Timeline.asset maxLevelDuration 900 (boss 900). Defense Mode bắt buộc có asset timeline tách riêng và validation; không chỉnh thời lượng mode cũ một cách âm thầm.

**Luật kết thúc đề xuất tạm thời:** 600s là mục tiêu sống sót; BaseHP = 0 thì thua; Hero HP = 0 cũng thua. Boss là áp lực ở cuối run, **[D07]** quyết định có phải giết boss để thắng hay không. Khi kill boss sớm, cần xác nhận có end early, hay kết thúc đủ time.

## 9. In-run progression và các hệ thống giữ lại [D12]

- Giữ combat/weapon, EXP/gem, chọn 1 trong 3 thẻ, cooldown, dash, signature, damage/resistance, pool và game-state policy từ Unity hiện có.
- Card pool phải kiểm soát cấp nhân vật, chỉ số pháp bảo duy nhất và một số buff phòng tuyến nếu được duyệt. Không để thẻ “thêm relic slot” lách luật.
- Âm Dương, Ngũ Hành, gacha, meta progression và các dịch vụ live ops đã có trong Projectzombie có thể tiếp tục ở cấp code, nhưng **mức độ xuất hiện trong GDD MVP Defense** chờ [D12].
- Phải tránh quảng cáo 60 FPS/0-GC như một kết quả đo đã xác nhận. KPI performance là mục tiêu test, không bảo đảm.

## 10. UI/UX cho mobile

Màn hình phải thể hiện rõ Hero HP, Base HP, Wood, EXP/Level, Timer/Phase, cảnh báo 3 lane, xây tháp và nâng tháp, tuyển lính, chọn squad/stance/rally, thông tin boss và game result.

**Ưu tiên tác vụ:** thao tác di chuyển và chiến đấu phải hoạt động liên tục; build menu, rally/cast aim được quản lý bởi một Input Interaction Coordinator để không một chạm tạo cả tháp và cast relic. Modal Pause/LevelUp/GameOver chặn mọi hành động gameplay. Không phơi bày quá nhiều nút cố định làm che màn hình nhỏ; dùng context menu cho build/squad.

## 11. Hợp đồng kỹ thuật Unity

- **Giữ:** Feature-based Architecture, C# services, ScriptableObjects, URP, DamageData / DamageContext, IDamageable, IHealable, WeaponManager, GameStateManager, WaveScheduler, MatchFlowService, RunHUD, pooling, Addressables và Unity Test Framework.
- **Thêm module** Defense (BaseHealth, Economy, Structure, Tower, Placement, RunController), FriendlyArmy (Units/Orders/Commander), Maps/Lanes, DefenseUI và adapter target.
- **Đường di chuyển:** Vongxuyen x/z được chuyển thông qua explicit projection mapping sang Unity x/y, không copy code Three.js.
- **IDamageable:** có thể dùng cho Base/tower nếu không tạo phụ thuộc sai; giải quyết faction, nhận damage, death pipeline.
- **Enemy target:** interface ITargetSelector hiện trả PlayerContext và nên giữ backward-compatible trong Survival. Thêm adapter hoặc interface chiến lược mục tiêu chung; không thay mọi enemy hiện tại trước khi có test.
- **Multiplayer:** giới hạn MVP Defense single-player nếu [D12] đồng ý, nhưng không hardcode phụ thuộc network trong domain; khả năng co-op sau bằng adapter authority.
- **State & Reset:** 1 owner của state và time, cleanup event subscriptions/coroutines/pooled instances sau mỗi run.
- **Save:** tách meta save với runtime Wood; không tự lưu Wood giữa run. Tránh đổi key đã phát hành khi chưa có migration.

## 12. Sản phẩm tối thiểu và lộ trình

| Mốc | Nội dung | Nghiệm thu tối thiểu |
| --- | --- | --- |
| P0 | GDD approved, DefenseScenario, stage timer, entry point | Cả Survival cũ và Defense đều start/stop ổn |
| P1 | Nhà Chính, damage, điều kiện thua | Base bị đánh, mất HP, end đúng một lần |
| P2 | Map 2 bờ, 3 bridge, lanes, enemy navigation | Quái theo đúng lane, không xuyên nước |
| P3 | Wood, 10 build slots, tower 1–3 | Build/upgrade/kill reward atomic |
| P4 | 3 Friendly Unit role | AI tank, collect, healer theo faction |
| P5 | 3 squads, stance, rally, commander | Lệnh độc lập, kế nhiệm đúng |
| P6 | 34 wave event, elite, Boss, UI, result | Chơi trọn 600s trên device |
| P7 | Regression, tuning, perf, Addressables, QA | No critical bugs, mobile budget đo thật |

**Vertical slice đầu tiên:** 1 lane, 1 Base, 1 Tower, 1 enemy type, wood income, loss/win và HUD. Phải chơi được trước khi viết commander phức tạp.

## 13. Definition of Done cho tài liệu và code

- Mỗi hệ thống có owner, config, events, input, output, failure modes và tests.
- Mọi ID được dùng nhất quán giữa GDD, enemy roster, timeline, balance, prefab và Addressables.
- Không có hai rule khác nhau về thời lượng trận, mục tiêu chiến thắng, số slot pháp bảo, loại quái Map 01.
- Phân biệt **đã code trên Vongxuyen** với **đã port và kiểm thử trên Unity**.
- Tài liệu được chủ dự án duyệt checklist trước khi PR merge vào main.
- Code được xác nhận bằng EditMode, PlayMode, kiểm thử Android và profiling phù hợp scope.

## 14. Open decisions

Xem [DECISION_CHECKLIST_GDD_V6.md](DECISION_CHECKLIST_GDD_V6.md) — **D01–D12 đều ở trạng thái PENDING**, kể cả khi GDD có phương án đề xuất. Sau khi nhận quyết định, cập nhật các phụ lục cùng một PR và đổi trạng thái các mục tương ứng thành APPROVED.

## 15. Tài liệu nguồn

- Projectzombie GDD v5.1: https://github.com/thuongtran1210/Projectzombie/blob/main/GameDesignDoc/ProjectZombie_GDD.md
- Projectzombie Developer Guide: https://github.com/thuongtran1210/Projectzombie/blob/main/DEVELOPMENT_GUIDE.md
- Vongxuyen Product Vision: https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/PRODUCT-VISION.md
- Vongxuyen Master GDD: https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md
- Vongxuyen Stage 01: https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/stage01.js
- Vongxuyen DefenseRules: https://github.com/thuongtran1210/Vongxuyen/blob/main/src/defense/DefenseRules.js
- Vongxuyen EnemyFactory: https://github.com/thuongtran1210/Vongxuyen/blob/main/src/entities/EnemyFactory.js
