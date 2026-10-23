# Stage01 & Enemy Contract — Vongxuyen to Unity (planning snapshot)

**Status:** DRAFT về thiết kế triển khai; **D-05 + D-11/A đã được OWNER APPROVED ngày 2026-10-23**. Đây là tài liệu dữ liệu, **không phải C# implementation/PlayMode PASS**.
**Source snapshot pinned:** Vongxuyen `src/config/stage01.js` blob `bb94a71df5ccddd8d93b0f8c961dac289667d9ea`; runtime `src/combat/WaveManager.js` blob `ec64826caf7de5f240a633de3f91d3b7ea46d4ab`; `src/entities/EnemyFactory.js`; `src/combat/SpawnPlanner.js`.
**Hướng dùng:** D-05 và D-11 là hợp đồng đã duyệt để lập `test fixtures`; D-06/A (bay 3D thật, Water→shore, 3 GhostCannon) đã được duyệt, nhưng Gate G0 còn mở. Không thay C# constants khi chỉ lập kế hoạch.

## 1. Stage01 source schedule — 34 events

Công thức từ JS: `at = index*18`; `phase = min(5,floor(at/120)+1)`; riêng `index===6` dùng `magiap_boss/eastBank/count=1`; các event còn lại luân phiên `magiap/eastBank, huyetdoi/northCanopy, thuyquai/canalWater, magiap/eastBank, magiap/southBank`; `count = min(8,3+phase+(index%2))`.

| # | Event ID | T(s) | Phase | Type từ config | Zone | Jobs |
|---:|---|---:|---:|---|---|---:|
| 1 | benque-01 | 0 | 1 | `magiap` | `eastBank` | 4 |
| 2 | benque-02 | 18 | 1 | `huyetdoi` | `northCanopy` | 5 |
| 3 | benque-03 | 36 | 1 | `thuyquai` | `canalWater` | 4 |
| 4 | benque-04 | 54 | 1 | `magiap` | `eastBank` | 5 |
| 5 | benque-05 | 72 | 1 | `magiap` | `southBank` | 4 |
| 6 | benque-06 | 90 | 1 | `magiap` | `eastBank` | 5 |
| 7 | benque-07 | 108 | 1 | `magiap_boss` | `eastBank` | 1 |
| 8 | benque-08 | 126 | 2 | `thuyquai` | `canalWater` | 6 |
| 9 | benque-09 | 144 | 2 | `magiap` | `eastBank` | 5 |
| 10 | benque-10 | 162 | 2 | `magiap` | `southBank` | 6 |
| 11 | benque-11 | 180 | 2 | `magiap` | `eastBank` | 5 |
| 12 | benque-12 | 198 | 2 | `huyetdoi` | `northCanopy` | 6 |
| 13 | benque-13 | 216 | 2 | `thuyquai` | `canalWater` | 5 |
| 14 | benque-14 | 234 | 2 | `magiap` | `eastBank` | 6 |
| 15 | benque-15 | 252 | 3 | `magiap` | `southBank` | 6 |
| 16 | benque-16 | 270 | 3 | `magiap` | `eastBank` | 7 |
| 17 | benque-17 | 288 | 3 | `huyetdoi` | `northCanopy` | 6 |
| 18 | benque-18 | 306 | 3 | `thuyquai` | `canalWater` | 7 |
| 19 | benque-19 | 324 | 3 | `magiap` | `eastBank` | 6 |
| 20 | benque-20 | 342 | 3 | `magiap` | `southBank` | 7 |
| 21 | benque-21 | 360 | 4 | `magiap` | `eastBank` | 7 |
| 22 | benque-22 | 378 | 4 | `huyetdoi` | `northCanopy` | 8 |
| 23 | benque-23 | 396 | 4 | `thuyquai` | `canalWater` | 7 |
| 24 | benque-24 | 414 | 4 | `magiap` | `eastBank` | 8 |
| 25 | benque-25 | 432 | 4 | `magiap` | `southBank` | 7 |
| 26 | benque-26 | 450 | 4 | `magiap` | `eastBank` | 8 |
| 27 | benque-27 | 468 | 4 | `huyetdoi` | `northCanopy` | 7 |
| 28 | benque-28 | 486 | 5 | `thuyquai` | `canalWater` | 8 |
| 29 | benque-29 | 504 | 5 | `magiap` | `eastBank` | 8 |
| 30 | benque-30 | 522 | 5 | `magiap` | `southBank` | 8 |
| 31 | benque-31 | 540 | 5 | `magiap` | `eastBank` | 8 |
| 32 | benque-32 | 558 | 5 | `huyetdoi` | `northCanopy` | 8 |
| 33 | benque-33 | 576 | 5 | `thuyquai` | `canalWater` | 8 |
| 34 | benque-34 | 594 | 5 | `magiap` | `eastBank` | 8 |

**Tổng dữ liệu stage gốc:** `214` **spawn jobs theo khai báo**, gồm Ma Giáp `128`, Huyết Dơi `40`, Thủy Quái `45`, Miniboss `1`; phase job counts `[28,39,39,52,56]`.
**Sau D-11/A (đã duyệt):** 20 **event loại `magiap`**, mỗi event dành đúng 1 suất cho Phi Cương dẫn đầu → **20 `phicuong` + 108 `magiap` + 40 `huyetdoi` + 45 `thuyquai` + 1 `magiap_boss` = 214 suất theo kế hoạch**. Đây là **thay thế** chứ không sinh thêm 20 suất.
Đây **không phải 214 Enemy xuất hiện thành công**: `maxActiveEnemies=32`, `maxPendingSpawns=40`, `spawnBudgetPerFrame=4`, pending expiry 25s, safe spawn và giới hạn runtime có thể làm job chờ, hết hạn hoặc overflow. Cần telemetry `scheduled/queued/spawned/expired/rejected/alive`.

### Những event quan trọng
- `benque-07` @108s: **D-05 APPROVED** — 1 `magiap_boss`; không sinh batch thường rồi thêm boss; không áp dụng D-11 cho Miniboss.
- `benque-29` @504s: `magiap` x8 theo stage.
- **Dạ Xoa @510s**: sự kiện Boss độc lập `YakshaBoss.js`, không là phần của 34 stage rows.
- `benque-30` @522s: stage tiếp tục trong lúc Boss ở Arena; **D-03/A + D-02/A APPROVED**: game-time schedule chạy tới 600s dù Boss đã ra @510s, chỉ dừng timed-wave khi đã đến mốc 600s/terminal.
- `benque-34` @594s: last scheduled wave. Tới 600s không tạo thêm stage events.

## 2. Type/AI contracts — source vs đề nghị chuyển Unity

| Type ID | Source HP / speed / damage (unscaled) | Source spawn/behavior | Unity design (proposed) | Acceptance |
|---|---|---|---|---|
| `magiap` | 130 / 1.45 / 16 | East/South; jumping ground; armored | NavMeshAgent ground, target policy được config | 3 lanes, no water crossing outside bridge |
| `thuyquai` | 95 / 2.05 / 12 | canal water; undulating | water / amphibious path, not blindly Ground NavMesh | originates canal, enters valid combat path |
| `huyetdoi` | 50 / 3.1 / 8 | NorthCanopy, wing-flap flying | air mover / aerial targetability | no clipping/ground stuck |
| `quaidu` | 90 / 1.75 / 14 | GhostCannon paratrooper landing | landing + grounded AI, not regular Stage01 item | only spawn via intended special source |
| `phicuong` | 260 / 1.65 / 18 | elite substitution, claw windup, flies | elite air mover; telegraph before damage | source/phase semantics explicit |
| `magiap_boss` | 700 / 1.18 / 24 | miniboss leader, slam telegraph | separate miniboss profile, same shared health & wave reward | unique event at 108s |
| `yaksha_boss` | 3200 / 1.35 / 38 | independent Boss FSM, cleave/slam/crescent, arena/pillars | Boss authority owns skill/phase/HP, not duplicate regular Enemy scripts | early death/over-time cases |

**Nguồn raw stats chưa scale.** `EnemyFactory.js` có wave multiplier; Web runtime dùng `1+(phase-1)*0.18` cho HP khi spawn regular scheduled jobs. Unity hiện 100 HP quái thường, 350 miniboss, 1200 Boss; **chưa quyết định nên nhập số nào**.

## 3. D-11/A APPROVED — một Phi Cương dẫn đầu mỗi event Ma Giáp

**Hợp đồng Unity được chủ dự án duyệt ngày 2026-10-23:** 

1. **Đợt** trong D-11 = **một sự kiện** Stage01 có `type=magiap` (không phải phase 120s hoặc từng frame).
2. Gắn `eventId`, `slotIndex` cho mọi suất. Suất `slotIndex=0` của mỗi event `magiap` được định trước là `actualSpawnType=phicuong`; các suất còn lại giữ `actualSpawnType=magiap`. `declaredType` luôn là `magiap` cho cả event.
3. **Không sinh thêm số lượng**: event 4 suất → 1 Phi Cương + 3 Ma Giáp; event 5 suất → 1 Phi Cương + 4 Ma Giáp. Có **20 sự kiện** Ma Giáp, nên tổng kế hoạch có 20 Phi Cương, 108 Ma Giáp (không tính mất lượt do queue/cap).
4. Phải gán suất dẫn đầu tại thời điểm **lập job theo event**, không quyết định bằng `spawnedThisFrame`; job giữ nguyên loại khi chuyển frame, chờ slot hay restart. **Đặt thứ tự FIFO theo event và slot**.
5. Nếu job elite bị **overflow/hết hạn**, ghi lại job thất bại; không thay một job thường khác thành elite để bù. Khi tất cả suất event spawn thành công thì đúng một elite; số quái thực tế có thể thấp hơn khi bị cap/TTL.
6. **Ngoại lệ:** `magiap_boss` sự kiện #7, `yaksha_boss`, `quaidu`, các event `thuyquai/huyetdoi` không chuyển loại D-11. Các spawns đặc biệt GhostCannon/Boss được hạch toán riêng.
7. **Spawn zone:** khi chuyển `magiap→phicuong`, vẫn dùng vùng của *event Ma Giáp nguồn* (`eastBank` hoặc `southBank`), nhưng resolver Unity phải hỗ trợ locomotion bay/route phù hợp; Web `SpawnPlanner` hiện không whitelist `phicuong` cho các zone này. Đây là **ràng buộc thiết kế U3D-011/U3D-013**, không tự cập nhật scene.
8. Telemetry bắt buộc: `eventId`, `slotIndex`, `declaredType`, `actualSpawnType`, `zone`, `state` (scheduled/queued/spawned/expired/overflow) để QA đúng cả khi nhiều frame.

**Đối chiếu Web gốc:**

- Web runtime hiện trong `WaveManager.updateTimedSpawns()` thực thi:
```js
const spawnType = (job.type === 'magiap' && spawnedThisFrame === 0) ? 'phicuong' : job.type;
```
- `spawnedThisFrame` reset trong mỗi lần gọi hàm; do đó elite substitution hiện tại là **frame-sensitive**. Nếu frame đầu batch spawn một Ma Giáp, nó có thể thành Phi Cương; tỷ lệ không suy ra chỉ bằng bảng Stage01.
- **Khác biệt có chủ đích đã duyệt:** Unity dùng D-11/A theo **event**, không sao chép quy tắc Web theo **frame**. Phải có regression test với nhiều FPS, cap/TTL và frame skip.
- `quaidu` xuất hiện do `GhostCannon` thả lính, không phải một trong 34 Stage01 rows. Không đổi bảng để “ép có đủ 5 loại” mà không thiết kế trigger.

## 4. Spawn planning & space contract

`SpawnPlanner.js` source:
- `gateWest`: x [-9.2,-8.5], z [-12.5,12.5] (`magiap`)
- `eastBank`: x [8.45,23.5], z [-12.5,12.5] (`magiap`, `thuyquai`, `magiap_boss`)
- `northCanopy`: x [-22,3.8], z [-13.5,-9.5] (`huyetdoi`)
- `canalWater`: x [5.25,7.35], z [-13.5,13.5] (`thuyquai`)
- `southBank`: x [-22.5,3.7], z [9.5,13.5] (`magiap`, `huyetdoi`)
- `minSpawnDistance=3.1` meters relative Hero; source `attempts=24`, reject invalid coordinates, protect Base/Hero zones.
- Map config: canal x [4.45,8.05]; bridge centerZ [-10.5,-0.5,10.5], arena center [25,0]. Unity coord validation required (not assumed equal by design).
- Suggested Unity resolver: **event.zone/type** → candidates → sampling movement domain (NavMesh/air/water/landing) → safety checks → allocate only if active cap allows. If no safe point, queue job until TTL then expire; do not teleport enemy or create invalid path.
- **Unresolved:** cap 32 counts Boss/GhostCannons or regular only; active object count vs AI active count; how quota reserves Boss spawn @510. Need acceptance before G3.

## 5. Deterministic tests after D-05/D-11 approved

| Test ID | Expected |
|---|---|
| ST-01 | 34 unique IDs, 0/18/.../594s, no missing or duplicate due to frame skips |
| ST-02 | 214 declared spawn jobs across 34 events (before cap, FIFO, expiry) |
| ST-03 | D-05 APPROVED: `benque-07` chỉ một `magiap_boss` @108s; không batch thường và không chuyển elite |
| ST-04 | Yaksha trigger independent once at 510s, not counted as event #29/#30 |
| ST-05 | Paused/LevelUp time freezes scheduler and queue TTL; no spawn or attack |
| ST-06 | 32 active / 40 pending / 4 spawned per Update / TTL25s; clear stats on restart |
| ST-07 | D-11/A: 20 event Ma Giáp → 20 suất Phi Cương + 108 suất Ma Giáp; tổng stage vẫn 214 suất, không thêm event/job |
| ST-08 | D-11/A: mỗi event Ma Giáp đúng một `slotIndex=0` Phi Cương, các slot còn lại là Ma Giáp, không lệ thuộc FPS/độ dài frame |
| ST-09 | Complete crossing / engagement all three ground lanes with existing local NavMesh |
| ST-10 | at 600s scheduled waves stop; active enemies/Boss continue per D-03 decision |
| ST-11 | Queue overflow/TTL: job elite không sinh → không nâng job thường khác lên elite; log failure; no double spawn |
| ST-12 | Phi Cương dùng đúng zone Ma Giáp khai báo và locomotion airborne hợp lệ; không bỏ qua whitelist/safety/spawn cap |

**Traceability:** Backlog [U3D-006, U3D-010..014](02_IMPLEMENTATION_BACKLOG.md); [QA-09..14](03_DECISIONS_QA.md); [Gate0 approval](04_GATE0_APPROVAL_PACK.md).

## 6. Stop conditions / blockers

- **D-05, D-06/A và D-11/A đã duyệt; G0 được chủ dự án đóng ngày 2026-10-23 theo `OWNER APPROVED / PASSED WITH RECORDED LIMITATIONS`.** Tuy nhiên **chưa có phê duyệt code G1/G3**, không tự bắt đầu Stage01/Enemy adapter; mọi ST tests vẫn NOT_RUN.
- D-06/A APPROVED → không ép Enemy nước/bay dùng chung Ground NavMeshAgent; chuẩn bị QA riêng, không triển khai trước G0.
- Scene NavMesh local chưa backup/verified → không kéo prefab/scene installer.
- Không đưa source Web `EnemyFactory.js` vào Unity runtime; export art là việc G8.
