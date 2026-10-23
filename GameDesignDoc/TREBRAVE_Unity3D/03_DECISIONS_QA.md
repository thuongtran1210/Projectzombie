# TREBRAVE Unity Defense3D — Decision Log, Risk Register & QA Gates

**Status:** DRAFT / AWAITING OWNER APPROVAL · 2026-10-23.
**Mục đích:** chốt mâu thuẫn thiết kế **trước khi code** và có test thật sau mỗi gate. Chưa có QA mới trên Unity được chạy khi viết tài liệu.

## 1. Decision Log — 11 quyết định sản phẩm / kỹ thuật

| ID | Vấn đề và bằng chứng hai repo | Khuyến nghị **để duyệt**, không tự áp dụng | Status |
|---|---|---|---|
| **D-01** | Vongxuyen master plan là Web/Three.js, Projectzombie đã có Unity 2D + Defense3D scene | **Unity Defense3D là runtime đích**, Vongxuyen tiếp tục là nguồn tham chiếu behavior/config/visual; không xóa game Web/Legacy | **PENDING** |
| **D-02** | Vongxuyen Web chốt 600 gameplay seconds; Unity `DefenseRun6003D` có 600s `Time.deltaTime` | **600s game time**, dừng hoàn toàn trong Pause/LevelUp/Win/Loss; fast test qua cấu hình riêng, không sửa constants | **PENDING** |
| **D-03** | Vongxuyen master loop “trụ vững 600 giây”; Unity WIN khi Boss chết ngay, overtime sau 600 nếu Boss còn sống | **Đề xuất:** WIN chỉ khi Base còn HP, thời gian ≥600s **và** Yaksha đã bị đánh bại; Boss còn sống @600 → OVERTIME (không spawn thêm); Base chết bất kỳ lúc nào → LOSS. Boss chết trước @600 thì tiếp tục phòng thủ tới 600. Cần owner quyết định trước code | **PENDING / CRITICAL** |
| **D-04** | Vongxuyen master nói Hero chết có thể LOSS; Unity `DefenseHeroHealth3D` respawn sau 5s, chỉ Base chết gây LOSS | **Đề xuất:** Hero down/respawn, không trực tiếp LOSS; cần owner duyệt, quy tắc respawn sau WIN/LOSS phải rõ | **PENDING / CRITICAL** |
| **D-05** | `src/config/stage01.js`: event thứ 7 @108s thay bằng 1 `magiap_boss`; Unity `DefenseRun6003D` hiện spawn event thường + miniboss @108 riêng | **ĐÃ DUYỆT:** sự kiện #7 / `benque-07` tại 108s chỉ sinh **1 `magiap_boss` (Ma Giáp Thống Lĩnh)**; **không** có batch quái thường trùng mốc; Boss Dạ Xoa @510s vẫn độc lập | **APPROVED — 2026-10-23** |
| **D-06** | Web spawn `thuyquai` vùng nước, `huyetdoi/phicuong` bay, `quaidu` đáp đất từ GhostCannon; Unity mọi quái đi ground NavMesh cầu | **Đề xuất:** Enemy chia locomotion Ground / Water / Air / Landing; khác đường đi nhưng path và target vẫn hợp lệ; đặc tả GhostCannon riêng | **PENDING** |
| **D-07** | Web `DefenseRules`: Base maxHP500, start100 Wood, towerCost50, tower upgrade levels; Unity Base1000, start0 Wood, towerCost25. Vongxuyen Yaksha3200 HP vs Unity1200 | **Đề xuất:** chụp config Web thành baseline data có version, **không overwrite số hiện Unity tự động**; khóa bảng cân bằng sau playtest | **PENDING** |
| **D-08** | Web data 4 Hero gồm Thư Sinh, Thanh Đồng, Ẩn Sĩ, Đạo Sĩ; GDD Unity lịch sử có Võ Tăng | **Đề xuất:** Core xác minh 1 Hero Thư Sinh, giữ API mở rộng; **chưa chốt** roster cả bốn hay thay Võ Tăng | **PENDING** |
| **D-09** | Vongxuyen đã chốt 1 Pháp Bảo trang bị/run, Web có 5 Pháp Bảo FUN | **Đề xuất:** chỉ một slot, dùng **Dép Tổ Ong** cho MVP combat đầu tiên vì Web có runtime tham chiếu | **PENDING** |
| **D-10** | Unity local GLB/scene/NavMesh/meta chưa xác nhận tracked; legacy SampleScene còn tồn tại; đã từng có Missing Mono Script | **Đề xuất:** giữ 2D Legacy riêng; trước bất kỳ code/merge nào, kiểm tra backup scene/GUID/importer + build settings; chỉ chỉnh có rollback | **PENDING / CRITICAL** |
| **D-11** | `stage01.js` khai báo `magiap`; Web runtime có phép thay Ma Giáp đầu **mỗi frame** thành Phi Cương | **ĐÃ DUYỆT phương án A:** mỗi **sự kiện/đợt khai báo `type=magiap`** có **1 `phicuong` dẫn đầu, thay thế 1 suất `magiap`**; số suất của event giữ nguyên; không chuyển theo frame/FPS; không áp dụng cho `magiap_boss` | **APPROVED — 2026-10-23** |

**Status hợp lệ:** `PENDING` / `APPROVED` / `REJECTED` / `SUPERSEDED`. **Người dùng đã phê duyệt rõ D-05 và D-11 ngày 2026-10-23; các mục khác vẫn PENDING.** D-03/D-04 vẫn chặn đặc tả Match Result; D-06, D-07 và backup local vẫn chặn G0/triển khai runtime. Phê duyệt D-05/D-11 **không đồng nghĩa duyệt toàn Gate G0 hoặc cho phép code**.

### Biên bản quyết định đã được chủ dự án phê duyệt — 2026-10-23

- **D-05 — APPROVED:** “đúng 1 Ma Giáp Thống Lĩnh tại 108 giây, không có đợt thường trùng mốc.” Event `benque-07` giữ `count=1, type=magiap_boss, zone=eastBank`; **34 sự kiện** của stage không tăng số.
- **D-11 — APPROVED / A:** “1 Phi Cương dẫn đầu mỗi đợt Ma Giáp.” Định nghĩa nghiệm thu: **đợt = một event Stage01 khai báo `type=magiap`** (không phải phase 120s, không phải mỗi frame); **thay đúng một suất**, không thêm suất. Ví dụ event 4 suất Ma Giáp → 1 Phi Cương + 3 Ma Giáp; event 5 suất → 1 Phi Cương + 4 Ma Giáp. **Không chuyển miniboss `magiap_boss`**, không nhân đôi Boss.
- **Ảnh hưởng dữ liệu kế hoạch:** trong 34 event gốc có **20 sự kiện `magiap`**, tổng 128 suất Ma Giáp. Sau quy tắc D-11, **kế hoạch yêu cầu sinh 20 Phi Cương + 108 Ma Giáp**, cùng 40 Huyết Dơi, 45 Thủy Quái và 1 Miniboss = **214 suất** (trước queue/cap/TTL). Số spawn thực tế có thể ít hơn khi bị giới hạn; ghi `declaredType`, `actualSpawnType`, `eventId`, `slotIndex`.
- **Vấn đề kỹ thuật phải thiết kế:** vùng `eastBank`/`southBank` trong `SpawnPlanner.js` hiện chưa liệt kê `phicuong` là type hợp lệ; adapter Unity cần sử dụng vùng hợp lệ của đợt khai báo hoặc chính sách cho phép elite bay một cách tường minh. Không tự đổi map/zone.
- **Chưa được duyệt:** luật WIN/LOSS D-03, Hero Down D-04, chi tiết đường đi D-06, cân bằng D-07, lựa chọn Hero D-08, Relic D-09, quyết định D-01/D-02/D-10. Chưa bắt đầu code.

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
| R-01 | Scene/GLB/NavMesh local chưa nằm đầy đủ trong Git branch, mất sau regen/clean | CRITICAL | backup thư mục `Assets/Scenes/Defense3D`, `Assets/Art/Environment`, NavMesh data, manifest, meta; chuẩn hóa owner asset | scene reopen, scene/asset references |
| R-02 | GUID thay đổi, duplicate `.cs`, `Missing (Mono Script)` | CRITICAL | không xóa/đổi `.meta`, không regen scene, kiểm tra duplicate class/compile | Console compile clean, count missing=0 |
| R-03 | Thêm manager mới nhưng old `DefenseRun6003D`, `WaveSlice3D`, `BridgeSmokeTest3D` còn active | HIGH | single spawner authority, opt-in adapter / old tests disabled | 1 event=1 spawn batch, hierarchy/log |
| R-04 | Pause/LevelUp vẫn tiến clock, AI/damage gây lỗi | HIGH | one clock + input/spawn/damage guards | QA-03/QA-04 |
| R-05 | Enemy bay/ở nước bị ép NavMesh ground → kẹt/teleport xuyên vật | HIGH | locomotion strategy per enemy, nav tests 3 lane | QA-10/QA-11 |
| R-06 | Boss kết thúc trận trước khi 600s hoặc overtime sai luật | HIGH | D-03 duyệt + deterministic boundary tests | QA-19..QA-22 |
| R-07 | Killed twice / Wood+EXP grant nhiều lần, gameplay economy sai | HIGH | exactly-once death/reward id, clear post-match | QA-06/QA-14 |
| R-08 | Import GLB bao trùm collider/map hoặc sai scale/pivot, camera không thấy Boss | MEDIUM | render-only prefab, colliders disabled, verified source export | QA-25 |
| R-09 | Tích hợp Unity 3D phá SampleScene/Legacy menu | HIGH | no automatic override default build scene, regression test | QA-24 |
| R-10 | Giả định mã GitHub chạy được Unity trên máy người dùng | HIGH | remote `DONE_CODE` ≠ `PASSED_PLAYMODE`; nhận log/screenshot/video local | QA gate mỗi stage |

## 3. QA Matrix — chưa được chạy trong đợt lập kế hoạch

Trạng thái mặc định của mọi case dưới đây: `NOT_RUN` (không tự động chấm Passed).

| QA ID | Gate | Ca test / đầu vào | Expected (sau duyệt tương ứng) | Evidence |
|---|---|---|---|---|
| QA-01 | G0 | Mở lại local Scene Defense3D | không Missing Script, giữ map/3 bridges/10 sockets | Unity screenshot + Console |
| QA-02 | G0 | Mở Legacy SampleScene | scene cũ không regress, settings giữ nguyên | screenshot + Console |
| QA-03 | G1 | Pause từ 30s trong 20s wall-clock | gameplay elapsed không tăng; no AI/spawn/damage/cooldown | timer/trace |
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
| QA-15 | G4 | Xây/nâng/sửa Tower tại 10 sockets | wallet trừ một lần, occupied đúng; không block 3 cầu | PlayMode/scene |
| QA-16 | G4 | Enemy damage Base/Tower, Hero tấn công | hit/damage/target/cooldown chính xác, đúng guard | combat log |
| QA-17 | G5 | EXP pickup đủ lên nhiều level | XP dư còn nguyên, mỗi level 1 lượt thẻ | automated+PlayMode |
| QA-18 | G5 | Chọn 3-card/Double-click/chọn lúc paused | một buff, cards hợp lệ, không duplicate | PlayMode |
| QA-19 | G6 | t=510 và pha ARENA_COMBAT | Boss spawn đúng một lần, telegraph visible trước damage | Hierarchy/Game View |
| QA-20 | G6 | Boss bị giảm HP rồi chuyển ASSAULT_BASE | HP giữ nguyên, movement/path sang Base hợp lệ | play log |
| QA-21 | G6 | BaseHP=0 trước 600/giữa Boss fight | LOSS ngay, dừng các nguồn damage/spawn hợp lệ | test report |
| QA-22 | G6 | Boss chết trước 600, sống sau 600, chết trong overtime | luật D-03 được thực hiện chính xác | boundary test |
| QA-23 | G7 | Run đủ 600s và restart x3 | no P0, no stuck, no ghosts; gameplay flow thông suốt | recording/log + commit |
| QA-24 | G7 | Vào Defense từ menu và quay lại 2D | correct scene routing, scene cũ nguyên vẹn | Android/editor smoke tùy môi trường |
| QA-25 | G8 | Export/import từng prefab Enemy/Yaksha | scale/URP/axis/pivot đúng, Collider và NavMesh không bị art phá | model inspector + screenshot |
| QA-26 | G3 | D-11: một event `magiap` có 4/5 suất, tách qua nhiều frame, queue đầy hoặc TTL hết | mỗi event **lên lịch 1** `phicuong` dẫn đầu thay một suất; còn lại `magiap`, không phụ thuộc FPS; không thêm slot, không áp dụng miniboss; nếu bị overflow/TTL thì đếm thất bại, không tự nhân elite | deterministic fixture + `eventId`, `slotIndex`, `declaredType/actualSpawnType` log |

**Cần thêm:** Unity Editor tests cho các contract (Clock/State/Scheduler/Reward), PlayMode smoke per gate và performance profile trên thiết bị thật; chỉ ghi `PASSED_DEVICE` khi có model device, FPS/frametime/RAM, phiên bản/commit và evidence. Không dùng log Editor để khẳng định Android QA pass.

## 4. Exit gates với bằng chứng cụ thể

| Gate | Exit condition | Ai xác nhận |
|---|---|---|
| G0 | D-01..D-10 chốt tối thiểu các mục liên quan stage; scene backed up; import/compile clean | owner + tester Unity |
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

- [ ] Owner phê duyệt **D-03** luật WIN/LOSS liên hệ 600s/Yaksha.
- [ ] Owner phê duyệt **D-04** Hero death/respawn.
- [x] Owner **APPROVED D-05 ngày 2026-10-23**: đúng 1 Miniboss tại 108s, thay event thường.
- [x] Owner **APPROVED D-11/A ngày 2026-10-23**: 1 Phi Cương **mỗi event Ma Giáp**, thay 1 suất, không chuyển theo frame/FPS.
- [ ] Owner duyệt scope một Hero, 5 enemy archetype, 1 Relic, 10 sockets, EXP/3 cards.
- [ ] Chốt chính sách Ground/Water/Air và GhostCannon.
- [ ] Ghi nhận được backup scene/GLB/NavMesh/meta/package local, không tái tạo scene.
- [ ] Unity scene mở lại, Console sạch lỗi compile/missing script; version/commit được ghi.
- [ ] Trạng thái backlog chuyển `DRAFT→APPROVED` cho các ticket Gate G1; các Gate sau chưa được bật.

**D-05 và D-11 đã được chốt bằng lời rõ ràng; các quyết định khác vẫn PENDING.** Không suy từ việc duyệt hai mục rằng G0 đã xong, hoặc mặc định được phép code.


## 6. Review pack chi tiết (tài liệu lập kế hoạch)

- [04 — Gate G0 Approval Pack](04_GATE0_APPROVAL_PACK.md): lựa chọn A/B, luật kết quả theo 600s, khóa G0-safety, D-11.
- [05 — Stage01 & Enemy Contract](05_STAGE_ENEMY_CONTRACT.md): bảng 34 event / 214 spawn jobs theo config nguồn, runtime elite transform, spawn restrictions, test fixtures.
- [06 — Match Lifecycle Spec](06_MATCH_LIFECYCLE_SPEC.md): **D-03 và D-04 PENDING**, A/B rõ trường hợp Boss chết sớm, Overtime, Base HP 0, Hero Down/Pause; ML-01..ML-10 **NOT_RUN**.
- [07 — G0 Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md): chỉ dẫn sao lưu có thể đối chứng trước khi code; **backup local chưa xác minh**.
