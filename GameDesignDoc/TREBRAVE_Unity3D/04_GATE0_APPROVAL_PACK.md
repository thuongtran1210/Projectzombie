# Gate G0 — Hồ sơ quyết định trước khi code Unity Defense3D

**Status: DRAFT / NEEDS OWNER APPROVAL.** 2026-10-23.
**Mục tiêu:** chuyển 11 quyết định (10 quyết định ban đầu và D-11 bổ sung) từ danh sách vấn đề thành câu hỏi thiết kế có lựa chọn, hệ quả và ca kiểm thử. **Không hàm ý người dùng đã chọn phương án**. Không sửa mã nguồn/scene, không tạo Issues tự động.
Nguồn: [Decision Log](03_DECISIONS_QA.md), [Master Plan](00_MASTER_PLAN.md), [Vongxuyen master](https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md), [Unity DefenseRun6003D](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseRun6003D.cs).

## 1. Tóm tắt quyết định còn mở (approval menu)

| ID | Chọn mặc định đề nghị (A) | Phương án thay thế (B) | Hệ quả nếu khác |
|---|---|---|---|
| D-01 | Unity Defense 3D là đích chuyển engine, Web Vongxuyen vẫn hoạt động | chỉ tiếp tục Web Three.js, Unity ở chế độ prototype | ảnh hưởng toàn roadmap |
| D-02 | 600 gameplay seconds, pause/level-up không tính giờ; Inspector Fast Test riêng | thay độ dài run | ảnh hưởng lịch wave/Boss |
| **D-03** | **WIN** khi Base còn sống **và** (đã đủ 600s **và** Yaksha đã chết). Boss chết sớm → tiếp tục phòng thủ; Boss sống sau 600s → overtime | WIN ngay khi giết Boss, dù chưa đủ 600s; hoặc WIN tự động ở mốc 600s | thay toàn bộ logic Result và kiểm thử |
| **D-04** | Hero Down → respawn sau 5 **gameplay seconds**, không LOSS trực tiếp; Base HP=0 → LOSS | Hero tử trận → LOSS; hoặc hệ mạng sống/hồi sinh giới hạn | lựa chọn ảnh hưởng cân bằng, clock và HUD |
| **D-05** | Event #7 tại 108s **chỉ là 1 `magiap_boss`**, không tạo thêm batch thường | thêm boss ngoài 34 sự kiện; ghi rõ thành 35+ event thực | khác danh mục stage và mật độ |
| D-06 | Ground/Water/Air/Landing có chính sách route/attack khác nhau; GhostCannon đánh dấu chưa quyết định scope | ép mọi Enemy dùng ground NavMesh, chấp nhận thay đổi thiết kế | ảnh hưởng authenticity và path tests |
| D-07 | giữ Web balance as **source reference** có version; tune Unity sau test, không auto-overwrite | nhập nguyên số Web vào Unity | ảnh hưởng DPS/HP/wood/test |
| D-08 | Core 1 Thư Sinh; thiết kế dữ liệu cho 4 Hero; roster chính thức chốt riêng | cố triển khai 4 Hero ngay trong Core | ảnh hưởng scope/stability |
| D-09 | 1 Relic slot, dùng Dép Tổ Ong MVP | Relic MVP khác trong 5 relic FUN | ảnh hưởng test input/combat |
| D-10 | giữ Scene/GLB/NavMesh/GUID local và Legacy riêng; backup trước tác động | rebuild scene từ generator | phương án B **không khuyến nghị** vì rủi ro mất dữ liệu |
| **D-11 (mới)** | dữ liệu `stage01` xác định **loại theo event**; phép chuyển `magiap` đầu frame thành `phicuong` là **SpawnModifier rõ cấu hình & test**, không cài ngầm | chạy đúng runtime Web mỗi frame như hiện tại | kết quả thay đổi theo FPS, cần quyết định deterministic policy |

**D-11** phát hiện khi đọc `src/combat/WaveManager.js`: `spawnType = (job.type === 'magiap' && spawnedThisFrame === 0) ? 'phicuong' : job.type`. Với nhiều job trong queue và spawn budget 4/frame, việc nâng cấp đầu frame thành Phi Cương phụ thuộc phân bổ frame, không chỉ ID event. Cần chốt **deterministic vs same Web runtime** trước khi lập snapshot expected type.

## 2. D-03 — Ma trận quyết định WIN/LOSS tại mốc 600s (phương án A)

| Base | Yaksha | Clock | Trạng thái đề nghị |
|---|---|---:|---|
| HP > 0 | sống, chưa ra | <510s | Playing |
| HP > 0 | sống | 510–<600s | Playing/Boss battle |
| HP > 0 | chết | <600s | Playing, tiếp tục wave đúng lịch |
| HP > 0 | chết | ≥600s | **WIN** chính xác 1 lần |
| HP > 0 | sống | ≥600s | **OVERTIME**, dừng **chỉ timed-wave schedule**, tiếp tục Boss combat và các mối nguy còn sống |
| HP = 0 | bất kỳ | bất kỳ thời điểm | **LOSS** chính xác 1 lần, ưu tiên hơn WIN trong cùng bước xử lý theo quy tắc được duyệt |

**Chưa kết luận:** OVERTIME có giới hạn thời gian không; những hệ thống đặc biệt (GhostCannon, summon, minions) còn spawn trong OVERTIME không? G0 cần duyệt. Đề xuất mặc định: không có giới hạn overtime cho Core Test; không tạo thêm *timed-wave* jobs; các quái đang tồn tại tiếp tục hoạt động; chỉ Boss có thể tiếp tục summon nếu mechanic được duyệt.

## 3. D-04 — Hero down & respawn (phương án A)

- Hero HP 0 → `Downed`; khóa Move/Attack/Relic/Tower interaction, không điều khiển camera như Hero đang sống.
- Respawn sau **5 giây gameplay**, không dựa trực tiếp `Time.time` khi trận Paused hoặc LevelUpSelection.
- Ở trạng thái WIN/LOSS: không tự hồi sinh nữa; cleanup một lần.
- Hero down **không ngừng cả trận**, Enemy tiếp tục tấn công Base. Cần HUD countdown và vị trí respawn an toàn (HeroSpawn).
- Nếu Hero down trong OVERTIME, vẫn respawn theo luật đã duyệt; Base vẫn có thể bị phá.
- Nếu owner chọn Hero dead = LOSS (B), sửa toàn bộ expected của QA-21/22 và cập nhật nguồn thiết kế; không giữ đồng thời hai luật.

## 4. D-05 — Miniboss và D-11 — Enemy phase rules

- Event #7: `at=108`, `type=magiap_boss`, `zone=eastBank`, `count=1`. **Không sinh thêm batch thường cùng event #7.**
- `yaksha_boss` ở 510s là **trigger độc lập**, không nằm trong 34 sự kiện. Đây là **quy tắc nguồn Web**, Unity prototype đã có trigger riêng.
- `quaidu` và `phicuong` là loại có code trong Vongxuyen nhưng **không xuất hiện trực tiếp trong khai báo 34 event source**; có thể do GhostCannon, logic elite frame hoặc debug spawning. Không nhét các ID này vào stage table chỉ để đạt '5 types'.
- Nếu dùng policy Phi Cương đầu frame, phải chỉ rõ semantics *frame* hoặc đổi thành *event/phase* deterministic; phương án thứ hai cần sản phẩm duyệt vì thay gameplay Web.

## 5. D-06 — Chiến lược locomotion / vai trò enemy (cần mô tả trước code)

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

## 7. Tiêu chí ra khỏi Gate G0

- [ ] Chủ dự án xác nhận các phương án **D-03, D-04, D-05** (và D-11), ghi decision `APPROVED` vào [Decision Log](03_DECISIONS_QA.md).
- [ ] Chốt policy Enemy ground/air/water/landing, phạm vi GhostCannon, Hero/Relic MVP, số liệu balance baseline (D-06..09).
- [ ] G0-S1..G0-S6 đủ evidence, đặc biệt scene local/Meta.
- [ ] Mapping và 27 backlog tickets review xong, chỉ các ticket G1 đủ điều kiện chuyển `APPROVED`.
- [ ] Gate G0 được chủ dự án đồng ý đóng. **Không tự đánh dấu pass khi chỉ có tài liệu.**

### Cách trả lời cho chủ dự án (không cần form)

Có thể phê duyệt theo cụm bằng lời, ví dụ: **“D-03 A, D-04 A, D-05 A; D-11 chờ thảo luận.”** Sau đó cập nhật Decision Log; không tự suy từ `Tiếp tục` rằng mọi quyết định đã được duyệt.

## 8. Pinned source revision cho audit

- [Vongxuyen stage01.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/stage01.js) blob `bb94a71df5ccddd8d93b0f8c961dac289667d9ea`
- [Vongxuyen WaveManager.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/WaveManager.js) blob `ec64826caf7de5f240a633de3f91d3b7ea46d4ab`
- [Vongxuyen mapConfig.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/mapConfig.js) blob `81fb83c5f90653117bf5d87214849280b8e06223`
- [Projectzombie DefenseRun6003D.cs](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseRun6003D.cs) blob `cf38737d150d524770375abe76cfd7384b44cd76`
