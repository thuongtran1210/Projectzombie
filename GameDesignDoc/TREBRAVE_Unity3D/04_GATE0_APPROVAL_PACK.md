# Gate G0 — Hồ sơ quyết định trước khi code Unity Defense3D

**Status: ALL PRODUCT DECISIONS APPROVED (D-01..D-11; D-07 = 8/8 principles) / GATE G0 NOT PASSED.** 2026-10-23 · Chỉ lập kế hoạch; local backup/Unity QA chưa xác minh, chưa code.
**Mục tiêu:** ghi biên bản phê duyệt đầy đủ **D-01/A, D-02/A, D-03/A, D-04/A, D-05, D-06/A 8/8, D-07/A 8/8, D-08/A, D-09/A, D-10/A, D-11/A**. Duyệt chính sách không đồng nghĩa QA, backup, khóa số balance hay quyền bắt đầu code.
Nguồn: [Decision Log](03_DECISIONS_QA.md), [Master Plan](00_MASTER_PLAN.md), [Vongxuyen master](https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md), [Unity DefenseRun6003D](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseRun6003D.cs).

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
| **D-10/A — APPROVED (POLICY)** | Giữ scene/GLB/NavMesh/.meta/GUID, Survival 2D, rollback; **bắt buộc backup và kiểm tra Unity Console trước code** | rebuild scene không được chọn | G0-S1..S6 vẫn NOT_VERIFIED/NOT_RUN |
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
| G0-S1 | `git status --short`, branch/head, ghi rõ file M/??/D | **NOT_VERIFIED_THIS_REVIEW** |
| G0-S2 | Backup offline scene `Assets/Scenes/Defense3D` gồm `.unity`/Meta, NavMesh data nếu nằm ngoài folder | **NOT_VERIFIED** |
| G0-S3 | Backup `Assets/Art/Environment` + `Assets/Features/Defense3D*.meta` + script `.meta` liên quan | **NOT_VERIFIED** |
| G0-S4 | Backup `Packages/manifest.json`, `packages-lock.json`, EditorBuildSettings, ProjectSettings/Packages nếu có | **NOT_VERIFIED** |
| G0-S5 | Unity mở scene local, Console 0 compile error, 0 Missing(Mono Script) | **NOT_RUN** |
| G0-S6 | Test mở lại Legacy `SampleScene` / menu cũ không lỗi | **NOT_RUN** |
| G0-S7 | Không làm `Build & Open Map Song Que Prototype`, `git reset --hard`, `git clean -fd`, `stash pop` toàn cục | **SAFETY POLICY** |

**Bằng chứng cần thu khi bắt đầu G0:** đường dẫn backup ngoài repo, thời điểm, commit và `git status`, Unity version/scene path, ảnh/log Console. Chưa có các bằng chứng này trong audit GitHub hiện tại.

## 6A. Remote Gate G0 audit đã làm / blocker còn lại

[10 — Remote Baseline Audit](10_G0_REMOTE_BASELINE_AUDIT.md) đã **chỉ đọc** nhánh GitHub ngày 2026-10-23: `Assets/Scenes/Defense3D` và `Assets/Art/Environment` **không có trên branch**, trong khi các C# script/meta, Editor installers, package AI Navigation và `SampleScene` có. BuildSettings remote chỉ chứa Legacy SampleScene. **G0-S1..S6 local đều NOT_VERIFIED/NOT_RUN**; không coi GitHub files thay cho backup. **Stop Core G1** đến khi có backup ngoài repo và bằng chứng Unity mở scene, Legacy, Console. Không dùng Scene Generator để tái tạo scene có chỉnh tay.

## 7. Tiêu chí ra khỏi Gate G0

- [x] **D-02/A APPROVED 2026-10-23** — clock gameplay 600 giây, pause/resume và Fast Test riêng.
- [x] **D-08/A APPROVED 2026-10-23** — Thư Sinh Core đầu tiên, hệ thống mở cho 4 Hero.
- [x] **D-09/A APPROVED 2026-10-23** — một Pháp Bảo, Dép Tổ Ong MVP.
- [x] **D-10/A APPROVED 2026-10-23** — chính sách bảo vệ local scene/GUID; **chưa backup/QA**.
- [x] **D-07-02..08/A APPROVED 2026-10-23** — cấu trúc cân bằng và thử nghiệm; **không khóa số**.

- [x] **D-01/A APPROVED 2026-10-23** — Unity Defense3D là runtime đích, Web/Legacy được giữ nguyên.
- [x] **D-07-01/A APPROVED 2026-10-23** — thống nhất cách đối chiếu/chọn nguồn balance; **7 tiểu mục còn lại PENDING**.
- [x] **D-03/A APPROVED 2026-10-23** — WIN sau 600s + hạ Dạ Xoa khi Base còn; OVERTIME nếu Boss sống.
- [x] **D-06/A APPROVED 2026-10-23** — 8/8 lựa chọn A: Ground/Water/Air/Landing, 3 Linh Pháo, anti-air, Enemy aggro/spawn; 18 QA chưa chạy.
- [x] **D-05 APPROVED** — event #7 tại 108s chỉ một Ma Giáp Thống Lĩnh.
- [x] **D-11/A APPROVED** — một Phi Cương dẫn đầu mỗi **event** Ma Giáp, không thêm suất.
- [x] **D-07/A 8/8, D-08/A Hero, D-09/A Relic đã APPROVED** về nguyên tắc. Các con số balance production chưa khóa.
- [ ] G0-S1..G0-S6 đủ evidence, đặc biệt scene local/Meta.
- [ ] Mapping và 27 backlog tickets review xong, chỉ các ticket G1 đủ điều kiện chuyển `APPROVED`.
- [ ] Gate G0 được chủ dự án đồng ý đóng. **Không tự đánh dấu pass khi chỉ có tài liệu.**

### Việc cần làm sau khi phê duyệt thiết kế

Không cần duyệt lại D-01..D-11. Cần có bằng chứng **backup ngoài repo, trạng thái Git local, scene/Console, Legacy regression** và phê duyệt stage G0; BalanceProfile production chỉ khóa sau số liệu thử nghiệm. Các QA test vẫn NOT_RUN, code chưa bắt đầu.

## 8. Tài liệu G0 bước tiếp theo

- [06 — Match Lifecycle Spec](06_MATCH_LIFECYCLE_SPEC.md): hai phương án D-03/D-04, các ca biên khi Boss chết và Hero Down.
- [07 — Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md): backup copy-only bên ngoài repo, bảo vệ scene/GLB/NavMesh/meta và form QA evidence.
- [08 — D-06 Enemy Navigation & AI](08_D06_ENEMY_NAVIGATION_REVIEW.md): đã APPROVED toàn bộ 8 phương án A, có 18 QA nghiệm thu NOT_RUN.

**Trạng thái:** tất cả D-01..D-11 đã phê duyệt **yêu cầu**. Điều kiện G0-S1..S6, thực thi backup, PlayMode và review ticket vẫn chưa có bằng chứng. **Không đánh dấu G0 PASS.**

- [09 — D-07 Balance Review](09_D07_BALANCE_REVIEW.md): **8/8 phương án A APPROVED về cấu trúc/phương pháp**, con số production chưa khóa; 12 D07-QA NOT_RUN.

## 9. Pinned source revision cho audit

- [Vongxuyen stage01.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/stage01.js) blob `bb94a71df5ccddd8d93b0f8c961dac289667d9ea`
- [Vongxuyen WaveManager.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/WaveManager.js) blob `ec64826caf7de5f240a633de3f91d3b7ea46d4ab`
- [Vongxuyen mapConfig.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/mapConfig.js) blob `81fb83c5f90653117bf5d87214849280b8e06223`
- [Projectzombie DefenseRun6003D.cs](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseRun6003D.cs) blob `cf38737d150d524770375abe76cfd7384b44cd76`
