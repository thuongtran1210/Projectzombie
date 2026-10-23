# Gate G0 — Hồ sơ quyết định trước khi code Unity Defense3D

**Status: `GATE G0 = OWNER APPROVED / PASSED WITH RECORDED LIMITATIONS` — 2026-10-23 (Asia/Ho_Chi_Minh).** Owner chủ động phê duyệt **đóng G0** trên bằng chứng hiện có. **G1 = NOT AUTHORIZED / NOT STARTED.** Chỉ cập nhật tài liệu; không code, không pull/merge/reset/clean/stash pop, không sửa scene/NavMesh/GLB/GUID.
**Mục tiêu:** ghi biên bản phê duyệt đầy đủ **D-01/A, D-02/A, D-03/A, D-04/A, D-05, D-06/A 8/8, D-07/A 8/8, D-08/A, D-09/A, D-10/A, D-11/A**. Duyệt chính sách không đồng nghĩa QA, backup, khóa số balance hay quyền bắt đầu code.
Nguồn: [Decision Log](03_DECISIONS_QA.md), [Master Plan](00_MASTER_PLAN.md), [Vongxuyen master](https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md), [Unity DefenseRun6003D](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseRun6003D.cs).

## 0. BIÊN BẢN ĐÓNG GATE G0 — Quyết định chính thức của chủ dự án (2026-10-23)

**Decision:** **OWNER APPROVED / PASSED WITH RECORDED LIMITATIONS.** Chủ dự án đã xem hồ sơ Gate G0 và **đồng ý đóng G0 theo chứng cứ hiện có**, không yêu cầu thêm điều kiện mới để ghi nhận sign-off. Đây là **phê duyệt Gate kế hoạch và baseline an toàn**, KHÔNG phải code release, kiểm thử đầy đủ, nghiệm thu 27 ticket, hay chấp thuận chuyển sang G1.

**Evidence được chủ dự án chấp nhận để sign-off:**
1. **6/6 hash SHA-256 khớp** giữa local và backup: `MapSongQue3D_Prototype.unity` + `.meta`, `NavMesh-Defense_Navigation.asset` + `.meta`, `TREBRAVE_MapSongQue_ThreeJS.glb` + `.meta`; đều `SourceExists=True / BackupExists=True / HashMatches=True`.
2. **Git branch/HEAD/stash khớp snapshot:** `feat/trebrave-defense3d-first-scene`, local HEAD `43ee2a806c593afe8704730f9daf012b52f56b35`, hai stash Fast Test; **17 tracked modified + 8 untracked path entries** được giữ nguyên. Local HEAD **không được xem là trùng remote HEAD** vì remote có commit tài liệu mới.
3. **3/3 file cấu hình** `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/EditorBuildSettings.asset` backup có và SHA khớp.
4. **Unity/Legacy:** hai ảnh Console/Hierarchy (20 logs, 0 warning, 0 error tại lúc chụp), owner xác nhận Missing Scripts, GLB/3 cầu/10 socket/NavMesh và Legacy Survival 2D đã được kiểm tra.
5. **27/27 nhiệm vụ** đã rà soát từ xa (21 P0, 3 P1, 3 P2); các quyết định D-01..D-11 đã duyệt, D-07/A 8/8 nguyên tắc.

**GIỚI HẠN ĐƯỢC GHI NHẬN / ACCEPTED LIMITATIONS:**
- Chưa thử **khôi phục toàn bộ dự án từ backup**; 6 hash trọng yếu và 3 file cấu hình **không** chứng minh mọi material, importer, prefab hoặc file ngoài snapshot đều có bản sao.
- S5/S6 là **owner-reported verification** có ảnh hỗ trợ Console/Hierarchy, không phải bộ bằng chứng QA độc lập hoặc chứng minh 600s run.
- **28 QA chính**, **18 D06-QA**, **12 D07-QA**, **ML-01..10** và full-run Gameplay/G7 **chưa được nghiệm thu**; không tự chuyển bất kỳ case nào thành PASS.
- **Balance production chưa khóa**; **27 ticket chưa DONE_CODE / PASSED_PLAYMODE / ACCEPTED**, GitHub Issues chưa tạo.
- **Không được thay đổi** local scene, GLB, NavMesh, `.meta`, GUID, Legacy và 2 stash; **không pull/merge/reset/clean/stash pop hoặc chạy generator** trong lượt sign-off này.

**NEXT AUTHORIZATION BOUNDARY:** G0 **đã đóng**; **G1 KHÔNG TỰ ĐỘNG BẮT ĐẦU**. Chỉ khi chủ dự án **phê duyệt code rõ ràng trong một lượt riêng**, mới triển khai G1; trước mọi tích hợp local phải có kế hoạch bảo toàn dirty/untracked data được duyệt. Không tạo commit gameplay, chỉnh scene, PR/merge hay mở issue trong lượt này.

**Nguồn bằng chứng:** [10 — Remote/Local Audit](10_G0_REMOTE_BASELINE_AUDIT.md), [11 — Readiness & Backlog Review](11_G0_READINESS_AND_BACKLOG_REVIEW.md), [07 — Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md).

---

## 1. Bảng quyết định đã duyệt về thiết kế (ngày 2026-10-23)

| ID | Chọn mặc định đề nghị (A) | Phương án thay thế (B) | Hệ quả nếu khác |
|---|---|---|---|
| **D-01/A — APPROVED** | **Unity Defense 3D là runtime đích chính**; Web Vongxuyen là nguồn gameplay/Enemy/asset tham chiếu, giữ Survival 2D Legacy | Web-only không được chọn | Duyệt 2026-10-23; chưa xác nhận QA tích hợp |
| **D-02/A — APPROVED** | 600 giây gameplay; Pause/LevelUp/Win/Loss không tăng clock; Fast Test là cấu hình riêng | thay thời lượng/run không được chọn | Chưa triển khai; phải test đồng hồ và D-03/A |
| **D-03 — APPROVED / A** | **ĐÃ CHỌN A:** WIN khi BaseHP>0 **và** đã đủ 600s gameplay **và** Yaksha chết; Boss chết trước 600 → tiếp tục trận; Boss còn sống @600 → OVERTIME, không timed-wave mới; Base HP0 → LOSS ưu tiên cùng tick | B (giết Boss thắng ngay) **không được chọn** | Được duyệt 2026-10-23; phải cập nhật Unity prototype sau Gate G0 |
| **D-04 — APPROVED / A** | **ĐÃ CHỌN A:** Hero Down khi HP0, không LOSS trực tiếp; hồi sinh tại HeroSpawn sau 5 **gameplay seconds**, countdown dừng khi Paused/LevelUp; Win/Loss dừng respawn | B (Hero chết là LOSS) **không được chọn** | Được duyệt 2026-10-23; HP hồi lúc respawn thuộc D-07 balancing |
| **D-05 — APPROVED** | **Đã chọn A:** event #7 tại 108s chỉ 1 `magiap_boss`, không có batch thường trùng mốc | Không áp dụng B | Duyệt 2026-10-23; nguồn [Stage01](05_STAGE_ENEMY_CONTRACT.md) |
| **D-06 — APPROVED / A (8/8)** | Ground/NavMesh 3 cầu; Thủy Quái bơi→lên bờ; Huyết Dơi/Phi Cương bay 3D; anti-air; 3 Linh Pháo thả dù; AI aggro; spawn safety | Ground-only/visual-only không được chọn | [08 D-06 Checklist](08_D06_ENEMY_NAVIGATION_REVIEW.md), 18 QA chưa chạy |
| **D-07/A — APPROVED 8/8 (PRINCIPLES)** | Benchmark Web, Unity Balance Profile riêng; Gỗ test đủ xây ≥1 Tower; 10 slots/Lv3; repair tốn Gỗ; Wood/EXP pickup; Enemy/Miniboss/Boss riêng; Fast Test và QA 600s | Chưa duyệt **mọi con số production** cụ thể | [09 — Balance](09_D07_BALANCE_REVIEW.md); 12 D07-QA NOT_RUN |
| **D-08/A — APPROVED** | Core hoàn thiện **Thư Sinh trước**, kiến trúc mở rộng tối đa 4 Hero; không làm đủ 4 Hero ngay | 4 Hero đồng thời chưa được chọn | Roster cuối cùng ngoài Hero MVP chưa khóa |
| **D-09/A — APPROVED** | Mỗi Hero **một slot Pháp Bảo**, Dép Tổ Ong MVP với ngắm/cast/cooldown/sát thương thật | chọn Relic khác làm đầu tiên không được chọn | 4 Relic còn lại sau Core |
| **D-10/A — APPROVED (POLICY)** | Giữ scene/GLB/NavMesh/.meta/GUID, Survival 2D, rollback; backup và kiểm tra Unity trước code | rebuild scene không được chọn | **G0 owner signed-off có giới hạn**: Git snapshot + 6 critical hashes; 3 config hashes; S5/S6 owner verified; full restore/QA chưa có |
| **D-11 — APPROVED / A** | **Đã chọn A:** 1 `phicuong` dẫn đầu **mỗi event Stage01 loại `magiap`**, thay đúng 1 suất Ma Giáp (không thêm quái), không phụ thuộc frame/FPS | B (thay theo frame Web) **không được chọn** | Duyệt 2026-10-23; phải bổ sung deterministic test |

**Đã quyết định D-11/A:** source Web vẫn có `spawnType = (job.type === 'magiap' && spawnedThisFrame === 0) ? 'phicuong' : job.type`, nhưng **Unity không sao chép logic frame-sensitive này**. Thay bằng 1 job `phicuong` tại slot đầu tiên của mỗi event `magiap`, bất kể phân bổ spawn qua bao nhiêu frame. Source `stage01.js` giữ 34 event và 214 suất.

## 2. D-03/A APPROVED — Ma trận WIN/LOSS tại mốc 600s

| Base | Yaksha | Clock | Trạng thái đề nghị |
|---|---|---:|---|
| HP > 0 | sống, chưa ra | <510s | Playing |
| HP > 0 | sống | 510–<600s | Playing/Boss battle |
| HP > 0 | chết | <600s | Playing, tiếp tục wave đúng lịch |
| HP > 0 | chết | ≥600s | **WIN** chính xác 1 lần |
| HP > 0 | sống | ≥600s | **OVERTIME**, dừng **chỉ timed-wave schedule**, tiếp tục Boss combat và các mối nguy còn sống |
| HP = 0 | bất kỳ | bất kỳ thời điểm | **LOSS** chính xác 1 lần, ưu tiên hơn WIN trong cùng bước xử lý theo quy tắc được duyệt |

**Chưa kết luận:** OVERTIME có giới hạn thời gian không; những hệ thống đặc biệt (GhostCannon, summon, minions) còn spawn trong OVERTIME không? G0 cần duyệt. Đề xuất mặc định: không có giới hạn overtime cho Core Test; không tạo thêm *timed-wave* jobs; các quái đang tồn tại tiếp tục hoạt động; chỉ Boss có thể tiếp tục summon nếu mechanic được duyệt.

## 3. D-04/A APPROVED — Hero down & respawn

- Hero HP 0 → `Downed`; khóa Move/Attack/Relic/Tower interaction, không điều khiển camera như Hero đang sống.
- Respawn sau **5 giây gameplay**, không dựa trực tiếp `Time.time` khi trận Paused hoặc LevelUpSelection.
- Ở trạng thái WIN/LOSS: không tự hồi sinh nữa; cleanup một lần.
- Hero down **không ngừng cả trận**, Enemy tiếp tục tấn công Base. Cần HUD countdown và vị trí respawn an toàn (HeroSpawn).
- Nếu Hero down trong OVERTIME, vẫn respawn theo luật đã duyệt; Base vẫn có thể bị phá.
- **D-04/A đã được chốt:** Hero death không tự gây LOSS. Không giữ đồng thời quy tắc Hero chết là thua (B) trong gameplay Unity.

## 4. D-05 / D-11 — Quy tắc đã APPROVED, không còn chờ quyết định

- Event #7: `at=108`, `type=magiap_boss`, `zone=eastBank`, `count=1`. **Không sinh thêm batch thường cùng event #7.**
- `yaksha_boss` ở 510s là **trigger độc lập**, không nằm trong 34 sự kiện. Đây là **quy tắc nguồn Web**, Unity prototype đã có trigger riêng.
- `quaidu` và `phicuong` là loại có code trong Vongxuyen nhưng **không xuất hiện trực tiếp trong khai báo 34 event source**; có thể do GhostCannon, logic elite frame hoặc debug spawning. Không nhét các ID này vào stage table chỉ để đạt '5 types'.
- **D-11/A đã được duyệt:** `đợt` = **event Stage01 khai báo `type=magiap`**; quy tắc đổi `slotIndex=0` thành `phicuong`, các slot còn lại giữ `magiap`. Dữ liệu nguồn có **20 event Ma Giáp**, 128 suất ⇒ 20 suất Phi Cương + 108 suất Ma Giáp; tổng stage vẫn **214**, không nhân thêm 20. Không áp dụng cho `magiap_boss`.
- **Vùng sinh cần thiết kế:** event East/South có thể tạo Phi Cương nhưng Web `SpawnPlanner` không whitelist Phi Cương ở các zone Ma Giáp. Unity phải dùng nguồn zone của event, điều chỉnh resolver cho airborne và kiểm thử; **D-06/A đã duyệt**.
- **Hàng đợi:** gắn `actualSpawnType` theo event+slot trước khi enqueue; nếu job elite hết hạn/overflow, không biến job thường khác thành elite bù; cần log riêng, không phụ thuộc FPS.

## 5. D-06/A APPROVED (8/8) — Chiến lược Enemy locomotion / AI

| Type | Spawn semantics nguồn | Chính sách Unity đề xuất | Test gate |
|---|---|---|---|
| `magiap` | eastBank/southBank | ground NavMesh, 3 lanes, có thể ưu tiên base | G3 route |
| `thuyquai` | canalWater | nước/cạn theo quy tắc rõ, không sample NavMesh trong lòng nước vô điều kiện | G3 amphibious |
| `huyetdoi` | northCanopy | aerial controller (không bắt Ground NavMesh) | G3 air |
| `phicuong` | elite substitution | aerial + telegraph claw; xác định target/cooldown | G3/G4 |
| `quaidu` | GhostCannon dropped/landed | landing point west, sau đó ground agent hợp lệ | G3/G6 |
| `magiap_boss` | 108s stage event | miniboss ground NavMesh/telegraph | G3/G6 |
| `yaksha_boss` | East arena @510s | Boss state machine: arena combat → strategic assault (Unity proposal) | G6 |

**Lưu ý:** hành vi khác nhau là mục tiêu thiết kế; không tự xác nhận đã port đầy đủ AI Web chỉ vì Enemy render bằng prefab.

## 6. G0 Safety Gate — bằng chứng từ máy Unity bắt buộc

**Tuyệt đối không bắt đầu đồng bộ C# khi thiếu bản sao lưu kiểm chứng được.** Không có công cụ điều khiển máy Unity local trong phiên chat này.

| Check | Bằng chứng cần có | Status |
|---|---|---|
| G0-S1 | `git status --short`, branch/head, ghi rõ file M/??/D | **OWNER_OUTPUT_VERIFIED** — branch/HEAD (43ee2a8...) và 2 stashes khớp các .txt trong snapshot; 17 M / 8 ?? local vẫn giữ, không pull |
| G0-S2 | Backup offline scene `Assets/Scenes/Defense3D` gồm `.unity`/Meta, NavMesh data nếu nằm ngoài folder | **CRITICAL_ASSET_HASH_VERIFIED** — `.unity` + `.unity.meta` + `NavMesh-Defense_Navigation.asset` + `.asset.meta`: 4/4 Source/Backup/Hash=True; full restore chưa thử |
| G0-S3 | Backup `Assets/Art/Environment` + `Assets/Features/Defense3D*.meta` + script `.meta` liên quan | **CRITICAL_ASSET_HASH_VERIFIED** — `TREBRAVE_MapSongQue_ThreeJS.glb` + `.glb.meta`: 2/2 Source/Backup/Hash=True; không chứng minh mọi material/importer khác |
| G0-S4 | Backup `Packages/manifest.json`, `packages-lock.json`, EditorBuildSettings, ProjectSettings/Packages nếu có | **3 FILES VERIFIED SHA-256 / PARTIAL** — `manifest.json`, `packages-lock.json`, `EditorBuildSettings.asset` đều `BackedUp=True`, `HashMatches=True`; chưa verify toàn bộ folders/restore |
| G0-S5 | Unity mở scene local, Console 0 compile error, 0 Missing(Mono Script) | **OWNER_REPORTED_VERIFIED + ẢNH CONSOLE/HIERARCHY**: owner xác nhận đã kiểm tra Missing Scripts, GLB, 3 cầu, 10 socket, NavMesh; 2 ảnh trước có Console 0 Warning/0 Error. Chưa có ảnh Inspector/test mới để QA độc lập |
| G0-S6 | Test mở lại Legacy `SampleScene` / menu cũ không lỗi | **OWNER_REPORTED_VERIFIED** — owner xác nhận đã kiểm tra checklist Legacy 2D; chưa có ảnh/log regression độc lập |
| G0-S7 | Không làm `Build & Open Map Song Que Prototype`, `git reset --hard`, `git clean -fd`, `stash pop` toàn cục | **SAFETY POLICY** |

**LỊCH SỬ — Bằng chứng local do owner báo cáo trước sign-off (2026-10-23 21:16 +07):** backup tại `C:\Users\thuon\Unity\TREBRAVE_G0_Backup_20261023_211614`, lệnh copy-only hoàn tất không báo lỗi; SHA256 **8 file** trong hai cây Scene Defense3D / Art Environment khớp, **0 missing/mismatches**. Nội dung branch/HEAD/stash đã ghi vào backup nhưng chưa gửi, NavMeshData và backup Packages/ProjectSettings chưa xác minh đầy đủ; Unity Console/Legacy **NOT_RUN**. Chi tiết ở [10 — Local Backup Evidence](10_G0_REMOTE_BASELINE_AUDIT.md).

**LỊCH SỬ — Bằng chứng bổ sung tại thời điểm đó:** chủ dự án gửi bảng SHA-256, xác nhận 3 file cốt lõi `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/EditorBuildSettings.asset` đã backup và hash khớp. Chỉ nâng **G0-S4 lên PARTIAL VERIFIED**, chưa có QA Unity/restore, NavMesh chưa xác minh; **Gate G0 NOT PASSED**.

**LỊCH SỬ — Đối chiếu danh mục backup trước hash chi tiết:** owner đã liệt kê `MapSongQue3D_Prototype.unity` (135896 bytes), `TREBRAVE_MapSongQue_ThreeJS.glb` (6507936 bytes), và `NavMesh-Defense_Navigation.asset` dưới scene folder (`InBackup=True`). Dòng `.meta` NavMesh bị cắt cột nhưng có chữ `Tru`; bản sao 8 file Scene/Environment đã có 0 SHA mismatch trong lần kiểm tra trước, **không có danh sách từng hash để xác định trực tiếp file nào**. **G0-S2/S3 vẫn PARTIAL / Unity NOT_RUN**, không tự đóng Gate.

**LỊCH SỬ — Bằng chứng Unity Editor và owner bổ sung (2026-10-23):** hai ảnh có Console 20 Logs/0 Warnings/0 Errors và Hierarchy Map Sông Quê, Hero, GLB, Navigation, Defense Run. Sau đó chủ dự án xác nhận **đã kiểm tra Missing Scripts, 3 cầu/10 socket/NavMesh và Legacy Survival 2D**. Ghi trạng thái **G0-S5/S6 OWNER_REPORTED_VERIFIED** (không có ảnh Inspector/Legacy riêng để kiểm chứng độc lập). **Không coi đây là PlayMode 600s PASS hoặc Gate G0 PASS**. Chi tiết ở [10 — Evidence Audit](10_G0_REMOTE_BASELINE_AUDIT.md).

**LỊCH SỬ — Bằng chứng PowerShell trước owner sign-off (2026-10-23):** nhánh local `feat/trebrave-defense3d-first-scene`, HEAD `43ee2a806c593afe8704730f9daf012b52f56b35`, hai stash Fast Test **khớp y hệt file snapshot**; 17 M / 8 ?? cần giữ nguyên. **SHA-256 6/6** file Scene, NavMesh và GLB kèm `.meta` đều có SourceExists/BackupExists/HashMatches=True. Xem [11 — Readiness Review](11_G0_READINESS_AND_BACKLOG_REVIEW.md). **Tại thời điểm ghi log này: đủ để trình owner, lúc đó chưa signed-off; xem MỤC 0 cho trạng thái đã đóng. G1 vẫn CHƯA ĐƯỢC CHO PHÉP.** G0-S4 chỉ có 3 key file hashes và chưa thử restore toàn bộ; G0-S5/S6 là user reported verified (Console/Hierarchy có ảnh).

## 6A. Remote Gate G0 audit đã làm / blocker còn lại

[10 — Remote Baseline Audit](10_G0_REMOTE_BASELINE_AUDIT.md) đã **chỉ đọc** nhánh GitHub ngày 2026-10-23: `Assets/Scenes/Defense3D` và `Assets/Art/Environment` **không có trên branch**, trong khi các C# script/meta, Editor installers, package AI Navigation và `SampleScene` có. BuildSettings remote chỉ chứa Legacy SampleScene. **G0-S1/S2/S3 đã được đối chiếu tại thời điểm G0 sign-off; G0-S4 partial/3 configs matched, G0-S5/S6 owner verified**; không coi GitHub files thay cho backup. **Stop Core G1** đến khi có backup ngoài repo và bằng chứng Unity mở scene, Legacy, Console. Không dùng Scene Generator để tái tạo scene có chỉnh tay.

## 7. Tiêu chí ra khỏi Gate G0

- [x] **D-02/A APPROVED 2026-10-23** — clock gameplay 600 giây, pause/resume và Fast Test riêng.
- [x] **D-08/A APPROVED 2026-10-23** — Thư Sinh Core đầu tiên, hệ thống mở cho 4 Hero.
- [x] **D-09/A APPROVED 2026-10-23** — một Pháp Bảo, Dép Tổ Ong MVP.
- [x] **D-10/A APPROVED 2026-10-23** — backup được chủ dự án thực hiện, 6 file trọng yếu + 3 config hash đã đối chiếu; owner xác nhận Unity/Legacy; G0 được đóng **có ghi giới hạn full-restore/QA**, không cấp phép code.
- [x] **D-07-02..08/A APPROVED 2026-10-23** — cấu trúc cân bằng và thử nghiệm; **không khóa số**.

- [x] **D-01/A APPROVED 2026-10-23** — Unity Defense3D là runtime đích, Web/Legacy được giữ nguyên.
- [x] **D-07-01/A APPROVED 2026-10-23** (lượt trước), **D-07-02..08/A cũng đã APPROVED**, đủ 8/8 về nguyên tắc; chỉ số production vẫn chưa khóa.
- [x] **D-03/A APPROVED 2026-10-23** — WIN sau 600s + hạ Dạ Xoa khi Base còn; OVERTIME nếu Boss sống.
- [x] **D-06/A APPROVED 2026-10-23** — 8/8 lựa chọn A: Ground/Water/Air/Landing, 3 Linh Pháo, anti-air, Enemy aggro/spawn; 18 QA chưa chạy.
- [x] **D-05 APPROVED** — event #7 tại 108s chỉ một Ma Giáp Thống Lĩnh.
- [x] **D-11/A APPROVED** — một Phi Cương dẫn đầu mỗi **event** Ma Giáp, không thêm suất.
- [x] **D-07/A 8/8, D-08/A Hero, D-09/A Relic đã APPROVED** về nguyên tắc. Các con số balance production chưa khóa.
- [x] **G0-S1** branch/HEAD/stashes khớp snapshot; **G0-S2/S3** 6/6 file critical Scene/NavMesh/GLB và meta khớp SHA-256.
- [x] **Owner chấp nhận giới hạn G0-S4/S5/S6 tại Gate G0**: 3 file settings khớp, Unity/Legacy owner-verified, **full restore và independent/full-run QA vẫn chưa làm**, ghi nhận ở mục 0.
- [x] **Remote review** Mapping và **27/27 backlog IDs** đã rà soát schema/dependency (21 P0/3 P1/3 P2; chưa tạo GitHub Issues). Chi tiết [11 — G0 Ticket Review](11_G0_READINESS_AND_BACKLOG_REVIEW.md).
- [x] **Owner đã xem và chấp nhận review kế hoạch 27 nhiệm vụ làm cơ sở đóng G0.** Không coi đây là `ACCEPTED` của từng ticket hay quyền bắt đầu G1.
- [x] **CHỦ DỰ ÁN ĐÃ SIGN-OFF ĐÓNG GATE G0 ngày 2026-10-23** — **OWNER APPROVED / PASSED WITH RECORDED LIMITATIONS**; **G1 NOT AUTHORIZED**.

### Việc cần làm sau khi phê duyệt thiết kế

Không cần duyệt lại D-01..D-11. Cần có bằng chứng **backup ngoài repo, trạng thái Git local, scene/Console, Legacy regression** và phê duyệt stage G0; BalanceProfile production chỉ khóa sau số liệu thử nghiệm. Các QA test vẫn NOT_RUN, code chưa bắt đầu.

## 8. Tài liệu G0 bước tiếp theo

- [11 — G0 Readiness & Backlog Review](11_G0_READINESS_AND_BACKLOG_REVIEW.md): 27 tickets đã remote-audit, lệnh read-only đối chiếu branch/HEAD/stash/NavMesh hash; các điều kiện còn thiếu trước sign-off.

- [06 — Match Lifecycle Spec](06_MATCH_LIFECYCLE_SPEC.md): hai phương án D-03/D-04, các ca biên khi Boss chết và Hero Down.
- [07 — Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md): backup copy-only bên ngoài repo, bảo vệ scene/GLB/NavMesh/meta và form QA evidence.
- [08 — D-06 Enemy Navigation & AI](08_D06_ENEMY_NAVIGATION_REVIEW.md): đã APPROVED toàn bộ 8 phương án A, có 18 QA nghiệm thu NOT_RUN.

**Trạng thái chính thức (2026-10-23):** **G0 OWNER APPROVED / PASSED WITH RECORDED LIMITATIONS.** 27/27 ticket đã được owner chấp nhận ở mức **kế hoạch review**, **chưa ACCEPTED về triển khai**. 6/6 asset hash và 3 config hash khớp, S5/S6 owner verified. Full restore/QA độc lập/full-run còn mở như rủi ro sau G0; **G1 chưa được cấp quyền, không code**. Các đoạn phía trên mục 6 ghi theo thời điểm từng bằng chứng và không thay thế sign-off tại mục 0.

- [09 — D-07 Balance Review](09_D07_BALANCE_REVIEW.md): **8/8 phương án A APPROVED về cấu trúc/phương pháp**, con số production chưa khóa; 12 D07-QA NOT_RUN.

## 9. Pinned source revision cho audit

- [Vongxuyen stage01.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/stage01.js) blob `bb94a71df5ccddd8d93b0f8c961dac289667d9ea`
- [Vongxuyen WaveManager.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/WaveManager.js) blob `ec64826caf7de5f240a633de3f91d3b7ea46d4ab`
- [Vongxuyen mapConfig.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/mapConfig.js) blob `81fb83c5f90653117bf5d87214849280b8e06223`
- [Projectzombie DefenseRun6003D.cs](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseRun6003D.cs) blob `cf38737d150d524770375abe76cfd7384b44cd76`
