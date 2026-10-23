# TREBRAVE Defense 3D (Unity) — Master Integration Plan

**Status (2026-10-23):** D-01..D-11 APPROVED về thiết kế (D-07/A 8/8). **Gate G0 OWNER APPROVED / PASSED WITH RECORDED LIMITATIONS** theo [hồ sơ owner sign-off](04_GATE0_APPROVAL_PACK.md); **G1 NOT AUTHORIZED, CHƯA CODE**. Full restore/full-run QA chưa nghiệm thu.
**Ngày lập:** 2026-10-23. **Đích:** `thuongtran1210/Projectzombie`, nhánh `feat/trebrave-defense3d-first-scene`.
**Nguồn tham chiếu:** `thuongtran1210/Vongxuyen` (Three.js), `docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md`, `docs/09-IMPLEMENTATION-PLAN-V04.md`, `src/config/stage01.js` và các module runtime (xem [Migration Matrix](01_MIGRATION_MATRIX.md)).
**Tình trạng kiểm thử:** audit mã nguồn GitHub + lịch sử thao tác Unity do người dùng cung cấp; **không thực hiện Unity PlayMode/Android test trong đợt viết tài liệu này**.

## 1. Kết luận audit và ranh giới

Đã có 2 bộ kế hoạch nhưng **chưa có bản triển khai chuyển engine thống nhất**:
- Vongxuyen: Three.js/WebGL; lộ trình Web P0–P7, S0–S4; có nhiều tính năng JS cần **viết lại hành vi trong C#**, không thể import JS vào Unity.
- Projectzombie: Unity 2022.3.62f3, dự án Survival 2D hiện hữu, GDD cũ thiên về 2D/20 phút. Nhánh Defense3D có prototype: scene Map Sông Quê tại máy người dùng, GLB môi trường, Ground collider/NavMesh ba cầu, Hero/Wood/Tower/Wave/Boss thử nghiệm.
- Code Unity **không tương đương chức năng Web**: Enemy dùng một walker chung; Run tự tính giờ/spawn; thiếu single-state, data-driven spawn, EXP/3 cards và Game Flow. Nhiều model chỉ capsule.
- Trạng thái local: scene/GLB/NavMesh/packages/`.meta` có thay đổi chưa được xác minh đã commit đầy đủ. Không viết script tái dựng scene, không reset hay thay GUID.
- **Kế hoạch này là đề xuất hợp nhất**, không thay thế quyết định đã được phê duyệt trong Vongxuyen, cũng không phê duyệt luật đang mâu thuẫn.

## 2. Vision / mục tiêu nghiệm thu

**Bản Core Vertical Slice:** một trận TREBRAVE Defense 3D độc lập, trên Map Sông Quê hai bờ / ba cầu / ba lane / 10 socket, 600 gameplay seconds; người chơi điều khiển một Hero đầy đủ vòng lặp cốt lõi, chống Enemy đa dạng, xây/nâng tháp bằng Gỗ, nhặt EXP và chọn thẻ, tương tác với Boss và nhận kết quả trận, restart sạch. Preserve Legacy Survival 2D.

**Mục tiêu chất lượng:** đúng luật, phản hồi rõ, ổn định trong 600s, **không đo thành công bằng số lượng capsule/script**. Không tuyên bố đã port toàn game hay đã đạt hiệu năng Android khi chưa test.

**Không thuộc Core Gate:** model/animation/VFX hoàn chỉnh, 4 Hero đủ kit, 5 relic FUN đầy đủ, Ngũ Hành sâu, gacha/meta monetization, multiplayer, phản công/giành lại lãnh thổ, Android store release. Ghi backlog riêng để không mất tầm nhìn.

## 3. Scope khóa đề xuất

| Domain | Core Gate yêu cầu | Sau Core Gate |
|---|---|---|
| Map | Giữ bản đồ GLB hiện có; collider/NavMesh 3 cầu; kiểm chứng 10 sockets và spawn | tối ưu đồ họa, thời tiết |
| State | một chủ quản MainMenu/Preparation/Playing/Paused/LevelUp/Win/Loss; một clock gameplay; reset | lưu trận/analytics |
| Hero | 1 Hero playable đủ move/attack/dash/HP; 1 pháp bảo trang bị & cast; chết theo luật đã duyệt | 4 Hero signature/passive |
| Enemy | 5 archetype từ Vongxuyen, miniboss Ma Giáp và Yaksha; data/AI/damage/drop | quái bổ sung/cơ chế hiếm |
| Wave | 34 event dữ liệu / 600s, cap active/pending, safe spawn, ba lane | các stage khác |
| Defense | Nhà Chính, 10 socket, xây/nâng/sửa tối thiểu 1 loại Tower, Gỗ | đa dạng tháp/vật liệu |
| Progression | EXP gem, level, 3 lựa chọn/thẻ, buff thực, pause modal | build nâng cao/Ngũ Hành |
| Boss | 1 Boss có telegraph né được, counterplay, HP/phase rõ; toàn bộ trận có thể WIN/LOSS | model/vũ khí/animation chi tiết |
| UX | HUD thông số tối thiểu, Pause, Result, Restart, vào scene từ flow chọn chế độ | mỹ thuật UI mobile hoàn chỉnh |

Đây là **phạm vi đề xuất**; không âm thầm nâng hạng các chức năng chưa duyệt thành bắt buộc.

## 4. Sơ đồ phụ thuộc / stage gates

| Mốc | Gói | Yêu cầu xuất | Exit gate |
|---|---|---|---|
| **G0 — Baseline** | Quyết định luật, inventory, bảo vệ scene và GUID | 6/6 critical Scene/NavMesh/GLB+meta SHA khớp backup, Git snapshot và 3 config khớp, Unity/Legacy owner verified; **full restore/QA giới hạn được ghi nhận** | **OWNER APPROVED / PASSED WITH RECORDED LIMITATIONS — 2026-10-23** |
| **G1 — State** | GameState/Clock/Event lifecycle, reset/test harness | 1 clock, pause không update gameplay, restart không nhân handler | State Gate |
| **G2 — Enemy Foundation** | Enemy definitions & damage/death/reward pipeline | HP/status/death/reward once; 5 type khác nhau trên dữ liệu | Enemy Data Gate |
| **G3 — AI & Waves** | AI NavMesh/air/water roles + 34 events, spawn planner/cap | 3 cầu hoàn chỉnh, event đúng một lần, no spawn vào Hero | Spawn Gate |
| **G4 — Defense & Hero** | Tower/Gỗ/Nhà Chính; Hero combat/Relic/dash, trúng đòn | Tài nguyên dùng đúng, combat có thể thắng/thua | Combat Gate |
| **G5 — Roguelite** | EXP, level, 3-card, actual stat modifiers | kill → drop → pickup → level → chọn → buff → resume | Roguelite Gate |
| **G6 — Boss & Results** | Yaksha AI/counterplay, arena→assault, results/restart/scene entry | đúng luật thắng/thua đã duyệt, ≥3 restart sạch | Full Match Gate |
| **G7 — QA & Stabilize** | test 600s, 3 lane, regression 2D, profile cơ bản | no P0 bug; biên bản QA có bằng chứng thực | Code/QA ready |
| **G8 — Art & Device** | GLB char/enemy/Boss, animation/VFX/UI, Android profiling | profile trên thiết bị định danh, giữ logic ổn định | Art/Device Gate (riêng) |

**Dependency:** G0 → G1 → G2 → G3; G4 có thể soạn thiết kế song song sau G1 nhưng tích hợp combat phải dựa G2; G5 sau G1/G2/G4; G6 sau G3/G4; G7 sau G1–G6. Không bắt buộc “xong model” mới nghiệm thu Core.

## 5. Quy tắc triển khai sau khi được duyệt

1. **Một hệ thống một chủ sở hữu**: State/Clock, Spawn, Health/Damage, Economy/EXP, Result không bị các component song song điều khiển.
2. **Data-driven**: ID Enemy/Stage/Tower/Upgrade cố định, số liệu config có version; không thêm `if (name.Contains(...))` cho cơ chế cốt lõi.
3. **Compatibility migration**: giữ `DefenseEnemyBridgeWalker3D`, `DefenseRun6003D` và MonoBehaviour hiện có ở giai đoạn adapter; không xóa/đổi tên/đổi GUID khi scene local còn tham chiếu.
4. **Source ≠ Unity build**: Three.js behavior/telegraph/material phải chuyển thành logic C# + Unity asset; procedural geometry cần exporter prefab/GLB riêng. Không trộn 2 runtime.
5. **Local safety**: không chạy “Build & Open Map Song Que Prototype”, không `git reset --hard`, không tự restore `.meta`, không `git clean -fd`, không stash-all tự động. Trước pull/code: backup scene, GLB, packages, meta; chỉ chọn đường dẫn cần can thiệp.
6. **Trạng thái bằng chứng**: `NOT_STARTED`, `PLANNED`, `IN_PROGRESS`, `DONE_CODE`, `PASSED_PLAYMODE`, `PASSED_DEVICE`, `BLOCKED`. `DONE_CODE` không đồng nghĩa `PASSED_PLAYMODE`.
7. **Mỗi ticket**: nguồn + mục tiêu + acceptance + QA + rollback + tác động scene/meta; không merge PR khi thiếu kết quả kiểm thử.
8. **Fast Test**: game-time overrides qua Inspector/test config cho QA; mặc định production 600/108/510; không sửa `const YakshaTime` thủ công mỗi lần pull.

## 6. Toàn bộ quyết định sản phẩm D-01..D-11 đã duyệt về thiết kế

- **D-01 APPROVED/A 2026-10-23 — Runtime đích:** Unity Defense 3D (Projectzombie) là nền tảng chính; Vongxuyen Web là nguồn tham chiếu gameplay, Enemy và tài nguyên; giữ nguyên Web và Survival 2D Legacy.
- **D-02 APPROVED/A 2026-10-23 — Thời gian:** 600 giây gameplay, clock đóng băng trong Pause/LevelUp/Win/Loss, Fast Test qua cấu hình riêng thay vì sửa constants; Overtime tiếp tục xử lý theo D-03/A.
- **D-03 APPROVED/A 2026-10-23 — WIN/LOSS**: **WIN = BaseHP>0 + gameplayElapsed≥600s + YakshaDead**; Boss chết trước 600 → vẫn thủ đủ giờ; Boss sống @600 → OVERTIME, không lên lịch timed-wave mới; BaseHP0 → LOSS, ưu tiên nếu Boss chết cùng tick. Unity prototype còn luật WIN sớm, **chưa sửa code**.
- **D-04 APPROVED/A 2026-10-23 — Hero Down**: Hero HP0 → Downed, **không LOSS trực tiếp**, hồi sinh tại HeroSpawn sau **5s gameplay** (đóng băng khi Pause/LevelUp); Base vẫn bị Enemy đánh, terminal Win/Loss hủy hồi sinh. Unity prototype đang tính bằng `Time.time`, **chưa sửa code**.
- **D-05 APPROVED 2026-10-23 — Miniboss@108s**: sự kiện #7 `benque-07` chỉ có **1 `magiap_boss`**, không spawn batch thường trùng mốc. Unity hiện còn logic khác, **chưa sửa code**.
- **D-06 APPROVED/A 2026-10-23 — Enemy movement:** 8/8 phương án A: NavMesh 3 cầu cho Ground, Thủy Quái bơi→lên bờ, Air 3D thật, anti-air, 3 Linh Pháo thả dù, aggro Hero/Support/Base và SpawnPlanner an toàn. [Chi tiết](08_D06_ENEMY_NAVIGATION_REVIEW.md). Chưa triển khai/QA.
- **D-07 APPROVED/A (8/8) 2026-10-23 — Balance:** Web làm benchmark, Unity Balance Profile riêng; Gỗ khởi đầu ở baseline test đủ xây ≥1 Tower; 10 socket/Lv3; repair tốn Gỗ, có giới hạn; Wood/EXP là hai pickup riêng; stats Enemy và Boss riêng; Fast Test versioned + QA 600s. **Chưa khóa con số balance production**, chưa có Unity PlayMode evidence.
- **D-08 APPROVED/A 2026-10-23 — Hero:** Core hoàn thiện **Thư Sinh** trước, hệ thống data/skills mở cho 4 Hero; **chưa khóa toàn bộ roster cuối cùng** (Web/Unity GDD lịch sử khác nhau).
- **D-09 APPROVED/A 2026-10-23 — Pháp Bảo:** 1 slot/Hero, **Dép Tổ Ong** là MVP trước, phải có casting/aim/cooldown/damage thật, các Pháp Bảo khác sau Core.
- **D-10 APPROVED/A 2026-10-23 — An toàn Unity/Legacy:** bảo toàn scene, GLB, NavMesh, .meta/GUID và Survival 2D. Owner đã tạo snapshot copy-only, 6/6 file trọng yếu + 3 config hash khớp; Unity/Legacy owner verified. **Chưa có full restore/full 600s test**, phạm vi backup khác chưa chứng minh đầy đủ.
- **D-11 APPROVED/A 2026-10-23 — Elite dẫn đầu theo event**: trong mỗi event Stage01 loại `magiap`, **một suất đầu tiên** đổi thành `phicuong`; không cộng suất, không phụ thuộc FPS/frame; không đổi `magiap_boss`. 20 event Ma Giáp → **20 Phi Cương + 108 Ma Giáp**, tổng **214** spawn jobs của 34 event. Web hiện đổi theo frame, nên đây là khác biệt đã được chấp thuận cho Unity.

Xem [Decision Log & QA](03_DECISIONS_QA.md): **D-01..D-11 được owner duyệt thiết kế**, **G0 đã được owner đóng có ghi giới hạn ngày 2026-10-23**. 27 backlog tickets remote-reviewed nhưng chưa code/ACCEPTED; thông số balance production chưa khóa. **Chỉ bắt đầu G1 sau khi owner cấp quyền code riêng**.

## 7. Danh sách tài liệu / audit trails

- [01 — Migration Matrix](01_MIGRATION_MATRIX.md): đối chiếu nguồn JS, code Unity và khoảng trống.
- [02 — Implementation Backlog](02_IMPLEMENTATION_BACKLOG.md): ticket IDs/ưu tiên/phụ thuộc/deliverable/acceptance.
- [03 — Decision Log & QA Gates](03_DECISIONS_QA.md): chờ duyệt, test case và QA evidence.
- [04 — Gate0 Approval Pack](04_GATE0_APPROVAL_PACK.md): các phương án quyết định A/B và bằng chứng Gate0.
- [05 — Stage01 & Enemy Contract](05_STAGE_ENEMY_CONTRACT.md): 34 mốc, 214 spawn jobs source, runtime enemy conversion và các ca QA.
- [06 — Match Lifecycle Spec](06_MATCH_LIFECYCLE_SPEC.md): lựa chọn D-03 / D-04, thời gian trận/Overtime, Hero down/respawn, kiểm thử biên.
- [07 — G0 Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md): backup copy-only scene GLB NavMesh meta, form bằng chứng Unity để đóng Gate G0.
- [08 — D-06 Enemy AI Review](08_D06_ENEMY_NAVIGATION_REVIEW.md): checklist 8 quyết định về Ground, Water, Air, Landing, GhostCannon, aggro, anti-air và safety; **8/8 phương án A đã duyệt, 18 D06-QA chưa chạy**.
- [09 — D-07 Balance Review](09_D07_BALANCE_REVIEW.md): **D-07/A 8/8 APPROVED về nguyên tắc**; số BaseHP/Gỗ/Tower/Enemy/Boss production chưa khóa; 12 D07-QA NOT_RUN.
- [Vongxuyen Master Plan](https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md) (kế hoạch Web).
- [Vongxuyen Web S0–S4](https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/09-IMPLEMENTATION-PLAN-V04.md) (kế hoạch Web).
- [Projectzombie GDD lịch sử](../ProjectZombie_GDD.md) (thiên về Unity Survival 2D).
- [README Defense3D prototype](../../Assets/Features/Defense3D/README.md) (không coi checklist cũ là QA đã đạt).

**Điểm dừng sau G0:** G0 **OWNER APPROVED / PASSED WITH RECORDED LIMITATIONS**; 6/6 critical hashes, snapshot Git và 3 config hashes được owner xác nhận; G0 giới hạn full restore/QA độc lập được chấp nhận. **G1 NOT AUTHORIZED** — không code, không sửa scene/GLB/NavMesh/GUID, không pull/merge/reset/clean/stash pop; 27 ticket vẫn planning.
