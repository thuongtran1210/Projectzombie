# TREBRAVE — Unity Defense3D Core Planning Hub

**Status bộ tài liệu:** **D-01..D-11 đã OWNER APPROVED về thiết kế ngày 2026-10-23 (D-07/A 8/8)**. **Gate G0 CHƯA PASS** do chưa xác minh backup ngoài repo, Console/scene/Legacy và QA; số balance production chưa khóa; **chưa code hoặc chỉnh scene**.

## Bộ tài liệu chuẩn bị để duyệt

| Tài liệu | Nội dung | Kết quả mong muốn |
|---|---|---|
| **[00 — Master Plan](00_MASTER_PLAN.md)** | Core vertical slice scope, 9 stage gates G0–G8, versioning, safety | biết làm gì trước/sau, chỗ nào chưa duyệt |
| **[01 — Migration Matrix](01_MIGRATION_MATRIX.md)** | 23 domain mappings, Enemy registry, khác biệt Stage01 vs Unity | xác minh thứ gì đã có code/thứ gì chưa |
| **[02 — Implementation Backlog](02_IMPLEMENTATION_BACKLOG.md)** | U3D-001..U3D-027, priority, dependencies, acceptance | mở issue theo gates sau khi duyệt |
| **[03 — Decision + QA](03_DECISIONS_QA.md)** | D-01..D-11 **đã APPROVED**, D-07/A đủ 8/8 nguyên tắc; 28 QA chính chưa chạy | Decision log + Gate evidence, không đồng nghĩa code/QA PASS |
| **[04 — Gate0 Approval Pack](04_GATE0_APPROVAL_PACK.md)** | Toàn bộ quyết định A đã duyệt, G0-S1..S6 và review backlog còn chưa xác minh | **Gate G0 vẫn mở** |
| **[05 — Stage01 & Enemy Contract](05_STAGE_ENEMY_CONTRACT.md)** | 34 event / 214 declared jobs; D-11/A → 20 Phi Cương + 108 Ma Giáp, giữ tổng suất | dữ liệu/test fixture cho Core Enemy/Wave |
| **[06 — Match Lifecycle Spec](06_MATCH_LIFECYCLE_SPEC.md)** | **D-03/A D-04/A đã chốt**, luật WIN sau 600s + Yaksha và Hero hồi sinh 5 gameplay s; ma trận edge cases | đặc tả hai luật đã duyệt để lập trình/kiểm thử sau khi G0 pass |
| **[07 — G0 Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md)** | Lệnh sao lưu copy-only ngoài repo, danh sách asset và form evidence Unity | bảo toàn scene/GLB/NavMesh/GUID và QA baseline |
| **[08 — D-06 Enemy AI Checklist](08_D06_ENEMY_NAVIGATION_REVIEW.md)** | 8/8 phương án A đã duyệt: Ground/Water/Air/Landing/3 GhostCannon/aggro/spawn; 18 QA chưa chạy | **APPROVED về yêu cầu**, chưa code/QA |
| **[09 — D-07 Balance Review](09_D07_BALANCE_REVIEW.md)** | **D-07-01..08/A APPROVED 8/8**, 12 D07-QA NOT_RUN, số production chưa khóa | **Phê duyệt nguyên tắc**, không chỉnh Inspector |
| **[10 — Gate G0 Remote Audit](10_G0_REMOTE_BASELINE_AUDIT.md)** | Đã kiểm kê GitHub: script/manifest/build settings có; **scene Defense3D & Environment không có trên branch** | remote inspected; local backup/QA chưa xác minh |
| **[11 — G0 Readiness & 27-Ticket Review](11_G0_READINESS_AND_BACKLOG_REVIEW.md)** | Rà soát 27 ticket/54 dependency references, checklist bản sao NavMesh & git HEAD, chuẩn bị sign-off | **REVIEWED (remote), G0 CHƯA PASS** |

## Kết luận audit ngắn

1. **Bản đồ:** GLB Map Sông Quê, local scene, NavMesh 3 bridges và 10 sockets đã được người dùng dựng/kiểm thử cơ bản; chưa xác minh chúng đã được commit đầy đủ hay portable sang máy khác.
2. **Enemy:** source Vongxuyen có `magiap`, `thuyquai`, `huyetdoi`, `quaidu`, `phicuong`, `magiap_boss`, `yaksha_boss` (6 từ EnemyFactory + Yaksha riêng). Unity prototype vẫn dùng cùng một `DefenseEnemyBridgeWalker3D` và capsule, chưa có AI theo role thực tế.
3. **Waves:** 34 event @18s của Vongxuyen có phase/type/count/zone và giới hạn 32 active/40 pending/4 per frame/25s TTL; Unity có mốc giờ nhưng chưa đồng bộ quy tắc. **Khác biệt @108s:** source event #7 *thay bằng* 1 miniboss; Unity *bổ sung* 1 miniboss sau event thường.
4. **Loop:** Unity còn thiếu authoritative GameState, shared clock, EXP/3-card, combat/reward unified, Tower nâng/sửa, minion và full menu/result/restart lifecycle.
5. **Boss:** có arena ring / optional visual importer ở Unity, nhưng Boss Vongxuyen `YakshaBoss.js` có nhiều skill và interaction với cột đá chưa được port. Model gốc chưa xác nhận đã import.
6. **Luật thắng/thua:** **D-03/A đã chốt**: Base sống + đủ 600s + hạ Yaksha mới WIN; **D-04/A đã chốt**: Hero Down hồi sinh sau 5s gameplay. Unity prototype hiện vẫn khác luật mới, **chưa sửa code**.

## Quy trình đề nghị sử dụng tài liệu

**G0 REMOTE AUDIT ĐÃ THỰC HIỆN (2026-10-23):** [Báo cáo 10](10_G0_REMOTE_BASELINE_AUDIT.md) xác minh trên remote **không có** `Assets/Scenes/Defense3D` và `Assets/Art/Environment`; Editor BuildSettings remote mới có Legacy SampleScene. Đây là **rủi ro local-only**, **không được suy ra scene đã bị xóa khỏi PC**. GitHub có 14 Defense3D gameplay scripts, 8 Editor scripts và `.meta` tương ứng. **Không kéo/rebuild/clean/reset** trước khi local backup.

**G0 bước tiếp theo:** [11 — Readiness & Backlog Review](11_G0_READINESS_AND_BACKLOG_REVIEW.md) đã hoàn thành **rà soát từ xa 27/27 ticket** (không đổi sang ACCEPTED). Cần owner xác minh branch/HEAD/stash, hash cụ thể NavMesh + `.meta` / GLB / scene trong bản sao, sau đó **chủ dự án quyết định đóng Gate G0** nếu evidence đủ. G1 không được bắt đầu tự động; các số production chỉ khóa sau Balance QA.

**Vòng A — Design approval: HOÀN THÀNH về yêu cầu** — tất cả D-01..D-11 đã được chốt phương án A (D-05 không thêm miniboss; D-11/A một elite/event; D-07/A 8/8). Không coi đây là release-ready hoặc chứng nhận đủ 600s Unity.

**Vòng B — Gate G0 readiness: CHƯA HOÀN THÀNH** — owner đã tạo backup ngoài repo, báo cáo hash subset của scene/GLB/NavMesh và 3 file Packages/Build Settings; ảnh Console/Hierarchy 0 Warning/0 Error; **owner xác nhận đã kiểm tra Missing Scripts, 3 cầu/10 sockets/NavMesh và Legacy Survival 2D**. Các mục đó được ghi **OWNER_REPORTED_VERIFIED** (không phải QA độc lập). Vẫn chờ branch/HEAD/stash, hash NavMesh+.meta đích danh, review 27 backlog và quyết định đóng Gate. **Không code, không đánh dấu G0 PASS.**

**Vòng C — Code (CHƯA BẮT ĐẦU):** mã nguồn chỉ thay đổi sau khi G0 pass, bắt đầu G1 State/Clock; không rebuild scene, không rewrite mọi prototype cùng lúc. Mỗi ticket phải có PlayMode evidence, không đánh dấu DONE-QA chỉ từ GitHub diff.

## Hướng dẫn cho máy Unity local (chưa thực hiện tự động)

- **Trước khi pull bất kỳ thay đổi code nào:** kiểm tra `git status --short`; backup `Assets/Scenes/Defense3D`, `Assets/Art/Environment`, NavMesh assets, `Packages/manifest.json`, `Packages/packages-lock.json`, toàn bộ `.meta` liên quan và bản `DefenseRun6003D.cs` local.
- Không chạy `TREBRAVE > Defense 3D > Build & Open Map Song Que Prototype` trên scene đã chỉnh; không `git reset --hard`, `git clean -fd`, thay GUID hoặc `git stash pop` tự động.
- Tài liệu này được ghi trên remote branch. Nếu local còn thay đổi, **không cần pull chỉ để đọc kế hoạch**: mở GitHub web là đủ.
- Các file Web `docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md`, `docs/09-IMPLEMENTATION-PLAN-V04.md` và Unity GDD lịch sử vẫn tồn tại như nguồn; không xóa hay ghi đè.

**Nguyên tắc chốt:** ưu tiên hệ thống Core có thể kiểm thử toàn trận, sau đó mới model 3D/animation/VFX và QA thiết bị Android. Mọi con số balance gốc là **đầu vào để duyệt**, không auto-import.
