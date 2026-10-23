# TREBRAVE — Unity Defense3D Core Planning Hub

**Status toàn bộ bộ tài liệu: DRAFT / GATE G0 CHƯA ĐÓNG.** **D-03/A, D-04/A, D-05 và D-11/A đã APPROVED ngày 2026-10-23; các quyết định còn lại vẫn chờ duyệt.** Ngày audit: **2026-10-23**. Tất cả nội dung ở đây là **planning/docs-only**, không phải code đã triển khai. Đích là Unity 2022.3.62f3 trong `thuongtran1210/Projectzombie` (feature branch `feat/trebrave-defense3d-first-scene`); nguồn tham chiếu là Vongxuyen/Three.js `main`. Không merge nhánh theo mặc định.

## Bộ tài liệu chuẩn bị để duyệt

| Tài liệu | Nội dung | Kết quả mong muốn |
|---|---|---|
| **[00 — Master Plan](00_MASTER_PLAN.md)** | Core vertical slice scope, 9 stage gates G0–G8, versioning, safety | biết làm gì trước/sau, chỗ nào chưa duyệt |
| **[01 — Migration Matrix](01_MIGRATION_MATRIX.md)** | 23 domain mappings, Enemy registry, khác biệt Stage01 vs Unity | xác minh thứ gì đã có code/thứ gì chưa |
| **[02 — Implementation Backlog](02_IMPLEMENTATION_BACKLOG.md)** | U3D-001..U3D-027, priority, dependencies, acceptance | mở issue theo gates sau khi duyệt |
| **[03 — Decision + QA](03_DECISIONS_QA.md)** | D-01..D-11 (**D-03/A, D-04/A, D-05, D-11/A approved**), risks R-01..R-10, **28 QA cases** | chốt luật và tiêu chí kiểm thử trước code |
| **[04 — Gate0 Approval Pack](04_GATE0_APPROVAL_PACK.md)** | D-03/A D-04/A D-05 D-11/A đã duyệt, các lựa chọn khác còn chờ, G0 safety | hồ sơ quyết định Gate G0 |
| **[05 — Stage01 & Enemy Contract](05_STAGE_ENEMY_CONTRACT.md)** | 34 event / 214 declared jobs; D-11/A → 20 Phi Cương + 108 Ma Giáp, giữ tổng suất | dữ liệu/test fixture cho Core Enemy/Wave |
| **[06 — Match Lifecycle Spec](06_MATCH_LIFECYCLE_SPEC.md)** | **D-03/A D-04/A đã chốt**, luật WIN sau 600s + Yaksha và Hero hồi sinh 5 gameplay s; ma trận edge cases | đặc tả hai luật đã duyệt để lập trình/kiểm thử sau khi G0 pass |
| **[07 — G0 Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md)** | Lệnh sao lưu copy-only ngoài repo, danh sách asset và form evidence Unity | bảo toàn scene/GLB/NavMesh/GUID và QA baseline |
| **[08 — D-06 Enemy AI Checklist](08_D06_ENEMY_NAVIGATION_REVIEW.md)** | 8 lựa chọn A/B về Ground/Water/Air/Landing/GhostCannon/Target/Spawn; 18 QA đề xuất | **REVIEW / PENDING**, không tự phê duyệt hay code |

## Kết luận audit ngắn

1. **Bản đồ:** GLB Map Sông Quê, local scene, NavMesh 3 bridges và 10 sockets đã được người dùng dựng/kiểm thử cơ bản; chưa xác minh chúng đã được commit đầy đủ hay portable sang máy khác.
2. **Enemy:** source Vongxuyen có `magiap`, `thuyquai`, `huyetdoi`, `quaidu`, `phicuong`, `magiap_boss`, `yaksha_boss` (6 từ EnemyFactory + Yaksha riêng). Unity prototype vẫn dùng cùng một `DefenseEnemyBridgeWalker3D` và capsule, chưa có AI theo role thực tế.
3. **Waves:** 34 event @18s của Vongxuyen có phase/type/count/zone và giới hạn 32 active/40 pending/4 per frame/25s TTL; Unity có mốc giờ nhưng chưa đồng bộ quy tắc. **Khác biệt @108s:** source event #7 *thay bằng* 1 miniboss; Unity *bổ sung* 1 miniboss sau event thường.
4. **Loop:** Unity còn thiếu authoritative GameState, shared clock, EXP/3-card, combat/reward unified, Tower nâng/sửa, minion và full menu/result/restart lifecycle.
5. **Boss:** có arena ring / optional visual importer ở Unity, nhưng Boss Vongxuyen `YakshaBoss.js` có nhiều skill và interaction với cột đá chưa được port. Model gốc chưa xác nhận đã import.
6. **Luật thắng/thua:** **D-03/A đã chốt**: Base sống + đủ 600s + hạ Yaksha mới WIN; **D-04/A đã chốt**: Hero Down hồi sinh sau 5s gameplay. Unity prototype hiện vẫn khác luật mới, **chưa sửa code**.

## Quy trình đề nghị sử dụng tài liệu

**G0 bước kế tiếp:** Chủ dự án **review [checklist D-06 Enemy AI](08_D06_ENEMY_NAVIGATION_REVIEW.md)**, chọn A/B cho 8 nhóm (đặc biệt thực bay 3D, Thủy Quái bơi/đổ bộ, 3 Linh Pháo và anti-air). Sau đó tiếp tục D-01/D-02/D-07..D-10 và baseline an toàn theo [Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md). **D-06 vẫn PENDING.**

**Vòng A — Design approval:** **D-05 APPROVED** — tại 108s đúng 1 Ma Giáp Thống Lĩnh; **D-11/A APPROVED** — đúng 1 Phi Cương thay suất Ma Giáp dẫn đầu **mỗi event loại `magiap`**, không theo frame/FPS. Biên bản đã lưu ở [03](03_DECISIONS_QA.md) và [04](04_GATE0_APPROVAL_PACK.md). **D-03/A (WIN/LOSS) và D-04/A (Hero down) cũng đã APPROVED; vẫn chờ D-01/D-02/D-06..D-10 cùng kiểm tra Gate G0 local.**

**Vòng B — Planning freeze:** duyệt Core scope trong [00](00_MASTER_PLAN.md), từng dòng Migration Matrix ở [01](01_MIGRATION_MATRIX.md), dữ liệu Stage01 tại [05](05_STAGE_ENEMY_CONTRACT.md) và ticket/gate trong [02](02_IMPLEMENTATION_BACKLOG.md). Đổi DRAFT → APPROVED chỉ cho gói thực sự đã duyệt.

**Vòng C — Code (CHƯA BẮT ĐẦU):** mã nguồn chỉ thay đổi sau khi G0 pass, bắt đầu G1 State/Clock; không rebuild scene, không rewrite mọi prototype cùng lúc. Mỗi ticket phải có PlayMode evidence, không đánh dấu DONE-QA chỉ từ GitHub diff.

## Hướng dẫn cho máy Unity local (chưa thực hiện tự động)

- **Trước khi pull bất kỳ thay đổi code nào:** kiểm tra `git status --short`; backup `Assets/Scenes/Defense3D`, `Assets/Art/Environment`, NavMesh assets, `Packages/manifest.json`, `Packages/packages-lock.json`, toàn bộ `.meta` liên quan và bản `DefenseRun6003D.cs` local.
- Không chạy `TREBRAVE > Defense 3D > Build & Open Map Song Que Prototype` trên scene đã chỉnh; không `git reset --hard`, `git clean -fd`, thay GUID hoặc `git stash pop` tự động.
- Tài liệu này được ghi trên remote branch. Nếu local còn thay đổi, **không cần pull chỉ để đọc kế hoạch**: mở GitHub web là đủ.
- Các file Web `docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md`, `docs/09-IMPLEMENTATION-PLAN-V04.md` và Unity GDD lịch sử vẫn tồn tại như nguồn; không xóa hay ghi đè.

**Nguyên tắc chốt:** ưu tiên hệ thống Core có thể kiểm thử toàn trận, sau đó mới model 3D/animation/VFX và QA thiết bị Android. Mọi con số balance gốc là **đầu vào để duyệt**, không auto-import.
