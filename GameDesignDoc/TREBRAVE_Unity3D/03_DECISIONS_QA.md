# TREBRAVE Unity Defense3D — Decision Log, Risk Register & QA Gates

**Status:** DRAFT / AWAITING OWNER APPROVAL · 2026-10-23.
**Mục đích:** chốt mâu thuẫn thiết kế **trước khi code** và có test thật sau mỗi gate. Chưa có QA mới trên Unity được chạy khi viết tài liệu.

## 1. Decision Log — 11 quyết định sản phẩm / kỹ thuật

| ID | Vấn đề và bằng chứng hai repo | Khuyến nghị **để duyệt**, không tự áp dụng | Status |
|---|---|---|---|
| **D-01** | Vongxuyen là Web/Three.js, Projectzombie có Unity Defense3D và Survival 2D cũ | **APPROVED/A — Unity Defense 3D là runtime đích chính**; Vongxuyen giữ làm nguồn tham chiếu thiết kế, Enemy, luật và tài nguyên, không xóa/ghi đè Web hay Survival 2D | **APPROVED — 2026-10-23** |
| **D-02** | Vongxuyen 600s game-time; Unity prototype dùng `Time.deltaTime` và sửa constant Fast Test thủ công | **APPROVED/A:** trận **600 gameplay seconds**; đồng hồ **dừng trong Pause, LevelUp, Win, Loss**, tiếp tục trong Overtime; Fast Test bằng cấu hình Inspector/test profile riêng, không sửa constants. Không coi đồng hồ Unity hiện tại đã tuân thủ | **APPROVED — 2026-10-23** |
| **D-03** | Web master yêu cầu thủ 600s; Unity prototype WIN ngay khi hạ Yaksha | **ĐÃ DUYỆT/A:** WIN chỉ khi **BaseHP>0 VÀ gameplayElapsed≥600s VÀ YakshaDead**; Boss chết trước 600s thì vẫn phòng thủ đến hết giờ; Boss còn sống @600s → OVERTIME, không thêm timed-wave jobs; BaseHP=0 → LOSS, ưu tiên nếu cùng gameplay tick với Boss chết | **APPROVED — 2026-10-23** |
| **D-04** | Web master nói Hero tử trận có thể LOSS; Unity prototype hồi sinh bằng `Time.time` | **ĐÃ DUYỆT/A:** Hero HP0 → Downed, **không LOSS trực tiếp**; khóa input Hero; hồi sinh tại HeroSpawn sau **5 gameplay seconds** (Pause/LevelUp không giảm countdown); Enemy vẫn đánh Base; BaseHP0 → LOSS; terminal WIN/LOSS hủy hồi sinh | **APPROVED — 2026-10-23** |
| **D-05** | `src/config/stage01.js`: event thứ 7 @108s thay bằng 1 `magiap_boss`; Unity `DefenseRun6003D` hiện spawn event thường + miniboss @108 riêng | **ĐÃ DUYỆT:** sự kiện #7 / `benque-07` tại 108s chỉ sinh **1 `magiap_boss` (Ma Giáp Thống Lĩnh)**; **không** có batch quái thường trùng mốc; Boss Dạ Xoa @510s vẫn độc lập | **APPROVED — 2026-10-23** |
| **D-06** | Web có Thủy Quái bơi, Huyết Dơi và Phi Cương bay bằng hình ảnh, Tiểu Quỷ từ Linh Pháo; Unity hiện mới dùng Ground Walker | **APPROVED/A (8/8)**: 4 domain Ground/Water/Air/Landing; NavMesh 3 cầu; nước lên bờ; bay 3D; anti-air; 3 Linh Pháo thả dù; aggro Hero/Support/Base; spawn an toàn. Chi tiết ở [08_D06_ENEMY_NAVIGATION_REVIEW.md](08_D06_ENEMY_NAVIGATION_REVIEW.md). Chưa code, 18 D06-QA chưa chạy | **APPROVED — 2026-10-23** |
| **D-07** | Web/Unity prototype có HP, Gỗ, Tower, Enemy/Boss và EXP khác nhau | **APPROVED/A đủ 8/8 mục D-07-01..08**: Web là nguồn benchmark; Unity Balance Profile riêng; baseline Gỗ đủ xây ≥1 Tower, 10 sockets/max Lv3, repair tốn Gỗ, Wood/EXP pickups riêng, Enemy/Boss theo profile, Fast Test/QA có version. **Phê duyệt NGUYÊN TẮC, KHÔNG KHÓA CON SỐ PRODUCTION**; 12 D07-QA NOT_RUN. [09 — Balance Review](09_D07_BALANCE_REVIEW.md) | **APPROVED (DESIGN) — 2026-10-23** |
| **D-08** | Vongxuyen có Thư Sinh/Thanh Đồng/Ẩn Sĩ/Đạo Sĩ; GDD Unity lịch sử có Võ Tăng | **APPROVED/A:** Core ưu tiên **1 Hero Thư Sinh hoàn chỉnh**, thiết kế API/data mở rộng đến 4 Hero; **không yêu cầu hoàn thành đủ 4 trong Core**; danh sách roster cuối cùng chưa tự suy thành đã khóa | **APPROVED — 2026-10-23** |
| **D-09** | Vongxuyen có 1 slot Pháp Bảo/run, 5 Relics FUN | **APPROVED/A:** **1 slot Pháp Bảo/Hero**, triển khai **Dép Tổ Ong đầu tiên** trong Core với ngắm/cast/cooldown/sát thương thật; các Relics khác để sau, không hiểu đây là đã có code | **APPROVED — 2026-10-23** |
| **D-10** | Scene/GLB/NavMesh/.meta/GUID Unity local và Legacy 2D có nguy cơ mất dữ liệu, chưa xác minh tracked/backup | **APPROVED/A (SAFETY POLICY):** giữ scene, GLB, NavMesh, GUID, Survival 2D; sao lưu **trước thay đổi code/scene**, kiểm tra Console, compatibility và rollback. **CHƯA THỰC HIỆN/CHƯA XÁC MINH BACKUP, Unity QA NOT_RUN** | **APPROVED (POLICY) — 2026-10-23** |
| **D-11** | `stage01.js` khai báo `magiap`; Web runtime có phép thay Ma Giáp đầu **mỗi frame** thành Phi Cương | **ĐÃ DUYỆT phương án A:** mỗi **sự kiện/đợt khai báo `type=magiap`** có **1 `phicuong` dẫn đầu, thay thế 1 suất `magiap`**; số suất của event giữ nguyên; không chuyển theo frame/FPS; không áp dụng cho `magiap_boss` | **APPROVED — 2026-10-23** |

**Status hợp lệ:** `PENDING` / `APPROVED` / `REJECTED` / `SUPERSEDED`. **TOÀN BỘ D-01..D-11 đã OWNER APPROVED về các phương án thiết kế A trong phạm vi đã ghi, ngày 2026-10-23.** D-07 duyệt 8/8 nguyên tắc, **con số production chưa khóa**. **Gate G0 chưa PASS:** backup scene/GLB/NavMesh/meta và Unity Console/Legacy/PlayMode chưa xác minh; backlog/QA chưa được nghiệm thu; **không cho phép code**.

### Biên bản quyết định đã được chủ dự án phê duyệt — 2026-10-23

- **Lượt phê duyệt mới 2026-10-23:** chủ dự án chọn **D-02/A, D-08/A, D-09/A, D-10/A và D-07-02..08/A**; không có ngoại lệ. D-07-01/A, D-01/A, D-03/A, D-04/A, D-05, D-06/A, D-11/A giữ nguyên.
- **D-02/A:** 600 giây game-time; Pause/LevelUp/Win/Loss đóng băng clock; Fast Test riêng; đúng điều kiện D-03/A 600s dù Boss test xuất hiện sớm.
- **D-08/A:** một Thư Sinh hoàn thiện Core, kiến trúc 4 Hero; chưa kết luận roster chính thức cả bốn.
- **D-09/A:** một Pháp Bảo trang bị/Hero, Dép Tổ Ong là MVP có ngắm, hồi chiêu và gây damage thật.
- **D-10/A:** bảo toàn scene/GLB/NavMesh/.meta/GUID và Survival 2D, backup/Console/rollback bắt buộc trước code. **Duyệt chính sách ≠ backup đã có/QA đã pass.**
- **D-07-02..08/A:** baseline test có đủ Gỗ xây ít nhất một Tower (chưa khóa số Gỗ cụ thể); 10 socket/tối đa Lv3; sửa tốn Gỗ, có cooldown/limit; Wood và EXP nhặt riêng; Enemy stats riêng và theo phase có cap, bảo đảm anti-air; Miniboss/Boss profile riêng; Balance/Fast Test có version và 600s QA trước khóa số. **D-07/A đủ 8/8 đã duyệt, production HP/cost/damage chưa chốt.**
- **D-01/A — APPROVED:** Unity Defense 3D thuộc Projectzombie là **runtime đích chính**. Vongxuyen/Three.js tiếp tục là nguồn tham chiếu thiết kế, hệ thống Enemy, gameplay và tài nguyên; giữ nguyên Web và Survival 2D hiện có. Đây là quyết định phạm vi dự án, không phải xác nhận đã đồng bộ thành công.
- **D-07-01/A — APPROVED (chỉ tiểu mục):** Chụp snapshot số liệu Web và Unity để đối chiếu; Web là **benchmark/nguồn vai trò**, Unity có Balance Profile riêng theo phiên bản và cần PlayMode 600s để cân chỉnh. **Không chốt HP, Gỗ, Tower, Enemy/Boss, EXP hoặc những lựa chọn D-07-02..08; D-07 tổng thể vẫn chờ.**
- **D-03/A — APPROVED:** Chủ dự án chọn “Theo phương án đề xuất” cho D-03/D-04. Điều kiện WIN = BaseHP>0 + game-time≥600s + Yaksha đã chết; Boss chết sớm thì tiếp tục thủ; Boss sống ở mốc 600s thì OVERTIME; Base HP0 thì LOSS ưu tiên trong cùng tick. OVERTIME không sinh thêm timed-wave; quái/Boss đã có vẫn hoạt động. Những chi tiết về giới hạn overtime và GhostCannon/summon vẫn là phạm vi chi tiết cần thiết kế.
- **D-04/A — APPROVED:** Hero HP0 → Downed; không thua trực tiếp; hồi sinh sau 5 **giây gameplay**, đồng hồ dừng khi Pause/LevelUp; Hero không hành động khi down, Enemy vẫn gây damage Base; Win/Loss chấm dứt hồi sinh. Lượng HP hồi sinh đề xuất mặc định 100% cho Core QA, còn thuộc balancing D-07.
- **D-06/A — APPROVED 2026-10-23:** Chủ dự án duyệt đủ 8 phương án A trong checklist: Ground 3 cầu, Thủy Quái bơi/lên bờ, Air 3D thật, anti-air, ba Linh Pháo thả Tiểu Quỷ, AI target và spawn policy; nội dung đã duyệt về thiết kế, chưa code.
- **D-05 — APPROVED:** “đúng 1 Ma Giáp Thống Lĩnh tại 108 giây, không có đợt thường trùng mốc.” Event `benque-07` giữ `count=1, type=magiap_boss, zone=eastBank`; **34 sự kiện** của stage không tăng số.
- **D-11 — APPROVED / A:** “1 Phi Cương dẫn đầu mỗi đợt Ma Giáp.” Định nghĩa nghiệm thu: **đợt = một event Stage01 khai báo `type=magiap`** (không phải phase 120s, không phải mỗi frame); **thay đúng một suất**, không thêm suất. Ví dụ event 4 suất Ma Giáp → 1 Phi Cương + 3 Ma Giáp; event 5 suất → 1 Phi Cương + 4 Ma Giáp. **Không chuyển miniboss `magiap_boss`**, không nhân đôi Boss.
- **Ảnh hưởng dữ liệu kế hoạch:** trong 34 event gốc có **20 sự kiện `magiap`**, tổng 128 suất Ma Giáp. Sau quy tắc D-11, **kế hoạch yêu cầu sinh 20 Phi Cương + 108 Ma Giáp**, cùng 40 Huyết Dơi, 45 Thủy Quái và 1 Miniboss = **214 suất** (trước queue/cap/TTL). Số spawn thực tế có thể ít hơn khi bị giới hạn; ghi `declaredType`, `actualSpawnType`, `eventId`, `slotIndex`.
- **Vấn đề kỹ thuật phải thiết kế:** vùng `eastBank`/`southBank` trong `SpawnPlanner.js` hiện chưa liệt kê `phicuong` là type hợp lệ; adapter Unity cần sử dụng vùng hợp lệ của đợt khai báo hoặc chính sách cho phép elite bay một cách tường minh. Không tự đổi map/zone.
- **Chưa được nghiệm thu (không phải quyết định đang PENDING):** các con số balance production chưa khóa; backup scene/GLB/NavMesh/meta, Unity Console, Legacy scene và các ca PlayMode đều chưa xác minh. Chưa có code mới.

### Form chữ ký quyết định (copy vào issue/review)

```text
Decision ID:
Option approved (nguyên văn):
Rationale:
Impacted tickets/assets/scenes:
Owner approval date:
New test / migration required:
```

## 2. Risk Register

| Risk ID | Mô tả / dấu hiệu | Severity | Biện pháp trước code | QA evidence |
|---|---|---|---|---|
| R-01 | **REMOTE CONFIRMED 2026-10-23:** branch không có `Assets/Scenes/Defense3D` và `Assets/Art/Environment`; scene/GLB/NavMesh có thể chỉ ở local, dễ mất sau regen/clean | CRITICAL | [Remote audit 10](10_G0_REMOTE_BASELINE_AUDIT.md) + backup copy-only `.unity`/GLB/NavMesh/.meta/package ngoài repo trước pull/rebuild; không khởi chạy scene generator | local backup hashes, scene reopen, GUID refs, Console/Legacy evidence **NOT_RUN** |
| R-02 | GUID thay đổi, duplicate `.cs`, `Missing (Mono Script)` | CRITICAL | không xóa/đổi `.meta`, không regen scene, kiểm tra duplicate class/compile | Console compile clean, count missing=0 |
| R-03 | Thêm manager mới nhưng old `DefenseRun6003D`, `WaveSlice3D`, `BridgeSmokeTest3D` còn active | HIGH | single spawner authority, opt-in adapter / old tests disabled | 1 event=1 spawn batch, hierarchy/log |
| R-04 | Pause/LevelUp vẫn tiến clock, AI/damage gây lỗi | HIGH | one clock + input/spawn/damage guards | QA-03/QA-04 |
| R-05 | Enemy bay/ở nước bị ép NavMesh ground → kẹt/teleport xuyên vật | HIGH | locomotion strategy per enemy, nav tests 3 lane | QA-10/QA-11 |
| R-06 | Boss kết thúc trận trước khi 600s hoặc overtime sai luật | HIGH | D-03 duyệt + deterministic boundary tests | QA-19..QA-22 |
| R-07 | Killed twice / Wood+EXP grant nhiều lần, gameplay economy sai | HIGH | exactly-once death/reward id, clear post-match | QA-06/QA-14 |
| R-08 | Import GLB bao trùm collider/map hoặc sai scale/pivot, camera không thấy Boss | MEDIUM | render-only prefab, colliders disabled, verified source export | QA-25 |
| R-09 | Tích hợp Unity 3D phá SampleScene/Legacy menu | HIGH | no automatic override default build scene, regression test | QA-24 |
| R-10 | Giả định mã GitHub chạy được Unity trên máy người dùng | HIGH | remote `DONE_CODE` ≠ `PASSED_PLAYMODE`; nhận log/screenshot/video local | QA gate mỗi stage |

**Remote G0 review đã thực hiện:** [10 — Remote Audit](10_G0_REMOTE_BASELINE_AUDIT.md) ghi nhận scene và environment **không có trên feature branch**; đây là **remote inventory**, không phải G0-S1..S6 PASS hay backup local. Mọi QA bên dưới vẫn NOT_RUN.

## 3. QA Matrix — chưa được chạy trong đợt lập kế hoạch

Trạng thái mặc định của mọi case dưới đây: `NOT_RUN` (không tự động chấm Passed).

| QA ID | Gate | Ca test / đầu vào | Expected (sau duyệt tương ứng) | Evidence |
|---|---|---|---|---|
| QA-01 | G0 | D-10/A: Unity mở lại local scene Defense3D sau backup đối chiếu path/.meta | no Missing Mono Script, map GLB/3 cầu/NavMesh/10 sockets đúng, không mất GUID | git status + hashes + Unity screenshot/Console; **NOT_RUN** |
| QA-02 | G0 | D-10/A: mở Legacy Survival 2D / SampleScene trước và sau khi phát triển | không hồi quy menu, scene cũ nguyên và không Missing Script | Unity screenshot + Console; **NOT_RUN** |
| QA-03 | G1 | D-02/A: 600s gameplay clock, Pause 30s trong 20s wall-clock, Fast Test Boss @10s | gameplay clock/AI/spawn/cooldown đóng băng khi Pause; Fast Test **không rút ngắn điều kiện thắng 600s**; Overtime đúng D-03/A | EditMode + Unity PlayMode; **NOT_RUN** |
| QA-04 | G1/G5 | LevelUpSelection và GameOver | chặn input chiến đấu/spawn; không duplicate event | integration log |
| QA-05 | G1 | Restart 3 lần | no duplicated manager/event/listener; reset state | hierarchy/log 3 cycles |
| QA-06 | G2 | Hero và Tower cùng kết liễu quái | một death, một WoodDrop, một EXP reward | deterministic damage test |
| QA-07 | G2 | Damage cao hơn HP / damage bất hợp lệ | HP không âm, target chết 1 lần, reject invalid damage | editmode test |
| QA-08 | G2 | Apply stun/slow/status, rồi reset | đúng thời lượng game-time, paused không tick status | edit/play log |
| QA-09 | G3 | 34 event, bước dt vượt 2–3 mốc | mỗi event đúng một lần, đúng thứ tự/time | scheduler test |
| QA-10 | G3 | 3 Ground lanes/bridges đến Base | complete NavMesh path, không xuyên sông/địa hình | nav debug + PlayMode |
| QA-11 | G3 | Thủy Quái, Huyết Dơi, Phi Cương | air/water không hỏng route; target reachable | PlayMode + video |
| QA-12 | G3 | Hero đứng gần vùng spawn | không spawn đè/within 3.1m; zone fallback hợp lệ | seeded test |
| QA-13 | G3 | Wave dồn, frame dài | active≤32, pending≤40, spawn≤4/frame, TTL25s | trace counter |
| QA-14 | G3 | t=108s stage event #7, kể cả frame skip/queue/backpressure | `benque-07` tạo 1 suất `magiap_boss` duy nhất, không có batch thường cùng event; không kích hoạt D-11 cho miniboss | event log + queued-job assertions |
| QA-15 | G4 | D-07-02/03/04/A: đủ Gỗ baseline test xây 1 tháp, 10 sockets, xây/nâng tối đa Lv3/sửa | Gỗ trừ một lần, không xây trùng, repair tốn Gỗ/cooldown hoặc limit, HP clamp; giá production chưa khóa | PlayMode + versioned test BalanceProfile; **NOT_RUN** |
| QA-16 | G4 | D-07-06/A: Enemy per-type/phase, Hero Thư Sinh và Tower anti-air; Base nhận damage | targeting/damage/cooldown đúng role, Hero down/hitbox 3D đúng, phase scaling không tạo quái vô lý | Combat + PlayMode log; **NOT_RUN** |
| QA-17 | G5 | D-07-05/A: Wood và EXP pickup độc lập, EXP đủ vượt nhiều level | reward đúng 1 lần, XP dư nguyên, 1 modal level-up/level, không phát Gỗ/EXP trực tiếp khi đã dùng pickup | EditMode + PlayMode; **NOT_RUN** |
| QA-18 | G5 | Chọn 3-card/Double-click/chọn lúc paused | một buff, cards hợp lệ, không duplicate | PlayMode |
| QA-19 | G6 | t=510 và pha ARENA_COMBAT | Boss spawn đúng một lần, telegraph visible trước damage | Hierarchy/Game View |
| QA-20 | G6 | Boss bị giảm HP rồi chuyển ASSAULT_BASE | HP giữ nguyên, movement/path sang Base hợp lệ | play log |
| QA-21 | G6 | BaseHP=0, bao gồm khi Boss chết cùng gameplay tick | **D-03/A:** LOSS một lần, ưu tiên kết quả khi cùng tick; dừng đúng nguồn damage/spawn | MatchResult test + PlayMode |
| QA-22 | G6 | Boss chết @540s, sống @600s, chết trong OVERTIME | **D-03/A:** Boss chết sớm → tiếp tục tới 600; sống @600 → OVERTIME (không timed-wave mới); chết trong Overtime, Base sống → WIN; không auto-WIN do hết giờ | deterministic boundary test |
| QA-23 | G7 | D-07-08/A + D-02/A: full run 600 gameplay s, FastTest profile riêng, restart 3 lần | không P0/softlock/duplicate listeners, profile có version, telemetry Wood/EXP/Base/Enemy/Boss, không làm khác D-03/A | 600s recording/log + commit; **NOT_RUN** |
| QA-24 | G7 | D-08/A+D-09/A: vào Defense chọn Thư Sinh, trang bị 1 Dép Tổ Ong; về Legacy 2D | Thư Sinh playable; Relic aim/cast/cancel/cooldown/damage thật, chỉ một slot; scene routing và Legacy không hồi quy | Unity Editor/PlayMode smoke; **NOT_RUN** |
| QA-25 | G8 | Export/import từng prefab Enemy/Yaksha | scale/URP/axis/pivot đúng, Collider và NavMesh không bị art phá | model inspector + screenshot |
| QA-26 | G3 | D-11: một event `magiap` có 4/5 suất, tách qua nhiều frame, queue đầy hoặc TTL hết | mỗi event **lên lịch 1** `phicuong` dẫn đầu thay một suất; còn lại `magiap`, không phụ thuộc FPS; không thêm slot, không áp dụng miniboss; nếu bị overflow/TTL thì đếm thất bại, không tự nhân elite | deterministic fixture + `eventId`, `slotIndex`, `declaredType/actualSpawnType` log |
| QA-27 | G1/G6 | Hero down @100s, Pause @102s trong 20s wall-clock, Base sống | **D-04/A:** còn 3 gameplay seconds tới hồi sinh sau Resume, không gây LOSS khi Hero down; terminal state hủy hồi sinh | Unity game-clock trace |
| QA-28 | G6 | Boss chết sớm, Boss/Base chết cùng tick, Base còn sống @600s | **D-03/A:** WIN chỉ khi đủ 600s + hạ Boss + Base sống; LOSS ưu tiên nếu BaseHP=0 cùng tick | MatchResult deterministic tests |

**D-07/A (8/8) đã được duyệt về nguyên tắc:** 12 ca D07-QA-01..12 [trong checklist D-07](09_D07_BALANCE_REVIEW.md) và 18 ca D06-QA-01..18 [D-06](08_D06_ENEMY_NAVIGATION_REVIEW.md) vẫn **NOT_RUN**, theo dõi riêng. 28 QA chính ở bảng này cũng **NOT_RUN**. Duyệt yêu cầu **không** đồng nghĩa QA đã PASS.

**Cần thêm:** Unity Editor tests cho các contract (Clock/State/Scheduler/Reward), PlayMode smoke per gate và performance profile trên thiết bị thật; chỉ ghi `PASSED_DEVICE` khi có model device, FPS/frametime/RAM, phiên bản/commit và evidence. Không dùng log Editor để khẳng định Android QA pass.

## 4. Exit gates với bằng chứng cụ thể

| Gate | Exit condition | Ai xác nhận |
|---|---|---|
| G0 | **D-01..D-11 APPROVED về thiết kế**; G0-S1..S6 phải có backup ngoài repo đã đối chứng, Console sạch, scene/Legacy được kiểm tra; review 27-ticket plan và owner đóng Gate. **Hiện CHƯA PASS** | owner + tester Unity |
| G1 | QA-03..05 pass; one clock and no listener duplication | reviewer + tester |
| G2 | QA-06..08 pass; HP/death/reward exactly once; registry verified | reviewer + tester |
| G3 | QA-09..14 pass; wave/zone/3 bridges verified, special air/water tests | tester Unity |
| G4 | QA-15/16 + Hero/relic input guard pass | tester Unity |
| G5 | QA-17/18 pass; actual buff applied | tester Unity |
| G6 | QA-19..22 pass under approved win/lose spec | owner + tester |
| G7 | QA-01/02 + QA-23/24 full pass; no P0; changelog and branch/commit recorded | owner |
| G8 | QA-25 + performance/visual QA separate | owner + tester device |

### QA report template

```text
QA date/time + timezone:
Tester:
Projectzombie branch & commit SHA:
Unity version / scene path / build target:
Local modifications affecting test (yes/no; files):
Gate and QA IDs:
Steps performed:
Expected / observed:
Result: NOT_RUN | PASS | FAIL | BLOCKED
Evidence: log/screenshot/video/test-output path:
Bug issue + repro if failed:
Notes / next decision:
```

## 5. Approval checklist trước khi mở code

- [x] Owner **APPROVED D-02/A ngày 2026-10-23**: 600 giây game-time, Pause/LevelUp/Result đóng băng, Fast Test cấu hình riêng.
- [x] Owner **APPROVED D-07/A (8/8) ngày 2026-10-23**: 8 nguyên tắc cân bằng được chốt; chỉ số production chưa khóa; 12 D07-QA NOT_RUN.
- [x] Owner **APPROVED D-08/A ngày 2026-10-23**: Thư Sinh trước, mở rộng 4 Hero sau Core.
- [x] Owner **APPROVED D-09/A ngày 2026-10-23**: 1 Pháp Bảo, Dép Tổ Ong MVP.
- [x] Owner **APPROVED D-10/A ngày 2026-10-23**: bảo vệ Unity local/Legacy; **backup chưa thực hiện**.
- [x] Owner **APPROVED D-01/A ngày 2026-10-23**: Unity Defense 3D là nền tảng đích, giữ Vongxuyen và Survival 2D độc lập.
- [x] Owner **APPROVED D-07-01/A ngày 2026-10-23** (trước đó) và **D-07-02..08/A lần này**; D-07 đã được duyệt **8/8 về thiết kế**, không phải sản phẩm đã cân bằng.
- [x] Owner **APPROVED D-03/A ngày 2026-10-23**: WIN Base còn HP + 600s gameplay + hạ Dạ Xoa, overtime nếu chưa hạ Boss.
- [x] Owner **APPROVED D-04/A ngày 2026-10-23**: Hero down hồi sinh sau 5s gameplay, không thua trực tiếp.
- [x] Owner **APPROVED D-05 ngày 2026-10-23**: đúng 1 Miniboss tại 108s, thay event thường.
- [x] Owner **APPROVED D-06/A ngày 2026-10-23** — đủ 8/8 mục; 18 ca D06-QA vẫn NOT_RUN.
- [x] Owner **APPROVED D-11/A ngày 2026-10-23**: 1 Phi Cương **mỗi event Ma Giáp**, thay 1 suất, không chuyển theo frame/FPS.
- [x] Owner duyệt **phạm vi Core theo D-06/A, D-07/A, D-08/A, D-09/A**: một Thư Sinh, Enemy roles, một Dép Tổ Ong, 10 socket, EXP/3 cards trong backlog; chưa qua QA.
- [x] **D-06/A đã duyệt chính sách Ground/Water/Air/Landing và ba GhostCannon;** vẫn cần nghiệm thu D06-QA thực tế.
- [ ] Ghi nhận được backup scene/GLB/NavMesh/meta/package local, không tái tạo scene.
- [ ] Unity scene mở lại, Console sạch lỗi compile/missing script; version/commit được ghi.
- [ ] Trạng thái backlog chuyển `DRAFT→APPROVED` cho các ticket Gate G1; các Gate sau chưa được bật.

**Tất cả D-01..D-11 đã được duyệt về thiết kế (D-07/A 8/8 nguyên tắc).** **Gate G0 CHƯA PASS** vì chưa có bằng chứng backup/Console/Legacy QA, chưa nghiệm thu backlog; không tự động code.


## 6. Review pack chi tiết (tài liệu lập kế hoạch)

- [09 — D-07 Balance Review](09_D07_BALANCE_REVIEW.md): **D-07-01..08/A APPROVED 8/8 về nguyên tắc**, thông số production chưa khóa; 12 D07-QA **NOT_RUN**, 28 QA chính vẫn chưa được thực thi.

- [04 — Gate G0 Approval Pack](04_GATE0_APPROVAL_PACK.md): lựa chọn A/B, luật kết quả theo 600s, khóa G0-safety, D-11.
- [05 — Stage01 & Enemy Contract](05_STAGE_ENEMY_CONTRACT.md): bảng 34 event / 214 spawn jobs theo config nguồn, runtime elite transform, spawn restrictions, test fixtures.
- [06 — Match Lifecycle Spec](06_MATCH_LIFECYCLE_SPEC.md): **D-03/A và D-04/A APPROVED**, luật Boss chết sớm/Overtime/Hero Down/Pause, ML-01..ML-10 vẫn **NOT_RUN**.
- [07 — G0 Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md): chỉ dẫn sao lưu có thể đối chứng trước khi code; **backup local chưa xác minh**.
- [08 — D-06 Enemy AI Checklist](08_D06_ENEMY_NAVIGATION_REVIEW.md): **8/8 phương án A APPROVED**, 18 ca nghiệm thu chi tiết **NOT_RUN** trong checklist riêng (chưa cộng vào 28 QA chính).
