# TREBRAVE Unity 3D — Implementation Backlog (Planning only)

**Status:** DRAFT — scope & estimates relative, **chưa tạo Issues/chưa bắt đầu code**. Ngày 2026-10-23.
**Nguyên tắc:** ticket lập theo dependency và gate; `P0` chặn Core, `P1` cần trước vertical slice playable, `P2` sau Core. Kích thước `S/M/L` là độ phức tạp tương đối, **không phải số giờ/ngày**. Xem [Master Plan](00_MASTER_PLAN.md) và [Matrix](01_MIGRATION_MATRIX.md).

## 1. Tracking và quy tắc quản trị

- Ticket ID `U3D-xxx` để tránh trùng các `VX-xxx` thuộc Vongxuyen Web. **Bảng này chưa tạo GitHub Issues**; chỉ mở issue sau duyệt scope.
- State: `DRAFT` → `APPROVED` → `IN_PROGRESS` → `DONE_CODE` → `PASSED_PLAYMODE` → `ACCEPTED`; `BLOCKED` khi phụ thuộc, dữ liệu hoặc QA chưa đáp ứng.
- Mỗi ticket cần: GitHub issue link (sau duyệt), commit SHA, source version, scene impact (Yes/No), QA evidence (ảnh/log/test report), rollback steps, không có P0 lỗi mở.
- **Không sửa/regen scene local ở các ticket design**; mọi thao tác importer/Unity scene chỉ khi có backup và chủ máy xác nhận.
- **D-03/A, D-04/A, D-05, D-06/A và D-11/A đã APPROVED 2026-10-23**, chỉ cập nhật planning/acceptance; D-01/D-02/D-07..D-10 và backup/QA Gate0 vẫn chưa pass. Các ticket liên quan vẫn `DRAFT/BLOCKED` tới khi đủ phụ thuộc, **không bắt đầu code**.

## 2. Product baseline & kiến trúc (G0–G1)

| ID | Priority | Scope / deliverable | Depends | Size | Acceptance (trước khi đánh ACCEPTED) |
|---|---|---|---|---|---|
| U3D-001 | P0 | Chốt luật State, WIN/LOSS, Boss, Hero down, thời gian, Enemy lanes (D-01..D-11) | none | M | D-03/A + D-04/A + D-05 + D-11/A đã duyệt; D-01/D-02/D-07..D-10 và QA local vẫn chờ; không coi Gate G0 đã pass |
| U3D-002 | P0 | Backup & inventory local scene, GLB, NavMesh, packages, meta GUID; map asset register | none — thực hiện ngay, song song với D-xx | M | Scene mở lại không Missing Script; manifest/glTFast không vỡ; backup checksum/path |
| U3D-003 | P0 | Thiết kế authoritative `DefenseGameState` + allowed transitions + blocking contract | U3D-001 | M | Playing/Paused/LevelUp/Overtime/Win/Loss phản ứng đúng D-03/A; invalid transitions rejected; terminal LOSS ưu tiên khi BaseHP=0 cùng tick |
| U3D-004 | P0 | Thiết kế shared gameplay clock, event lifecycle, pause guard | U3D-003 | M | pause/level-up đóng băng AI/spawn/cooldown/damage/game-time, gồm **5s Hero respawn countdown D-04/A**; no double subscription |
| U3D-005 | P0 | Reset/Restart lifecycle spec và compatibility adapter cho existing `DefenseRun6003D` | U3D-003,U3D-004 | M | 3 lần restart không nhân GameObjects/listeners, clear Time/HP/slots/Gỗ/EXP, overtime flag và pending Hero respawn |

**Trình tự chuẩn bị G0-B (planning only):** **D-03/A + D-04/A đã được duyệt** trong [Match Lifecycle Spec](06_MATCH_LIFECYCLE_SPEC.md); tiếp tục thu baseline scene + backup theo [Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md). **U3D-001 và U3D-002 chưa được đánh DONE/PASS** cho tới khi owner + Unity evidence xác nhận.

**G1 prototype adapter:** trước tiên nối hệ thống mới vào script hiện hữu qua interface thay vì xóa/đổi tên `.cs`/`.meta`. Dễ rollback, an toàn scene.

## 3. Enemy foundation & combat shared contract (G2)

| ID | Priority | Scope / deliverable | Depends | Size | Acceptance |
|---|---|---|---|---|---|
| U3D-006 | P0 | Enemy registry / `EnemyDataSO`, giữ source IDs, stat defaults và version | U3D-001,U3D-003 | M | ID `magiap,thuyquai,huyetdoi,quaidu,phicuong,magiap_boss,yaksha_boss` duy nhất; type gốc vs type thực spawn phân biệt theo D-11/A; validate thiếu asset |
| U3D-007 | P0 | Unified Damage/Health/Death API + attacker source & status hooks | U3D-004,U3D-006 | L | damage >0, HP clamp 0, death exactly-once, trúng hitbox/target đúng, paused=no damage |
| U3D-008 | P0 | Event/Reward pipeline cho Enemy: kill, EXP, Wood, source tag | U3D-007 | M | 2 nguồn damage kết liễu cùng frame vẫn chỉ 1 death và 1 reward; không direct Wood nếu phải nhặt |
| U3D-009 | P0 | Enemy prefab/AI adapter, bảo toàn `DefenseEnemyBridgeWalker3D` scene refs | U3D-006,U3D-007 | M | các Enemy registry spawn được riêng, HP/speed/attack đúng; không cần art để test |

**G2 gate:** test thuần C# editor cho data/death/status, smoke một quái nhận hit Hero + Tower cùng frame, reward đúng một lần. Boss được cùng damage API nhưng có AI module độc lập.

## 4. Enemy locomotion / Wave / Spawn (G3)

| ID | Priority | Scope / deliverable | Depends | Size | Acceptance |
|---|---|---|---|---|---|
| U3D-010 | P0 | Ground AI NavMesh 3 cầu và target Hero/Support/Base theo D-06/A | U3D-002,U3D-009 | L | 3 bridge path complete; không xuyên nước/nhà; stuck detected & reported |
| U3D-011 | P0 | Amphibian/Flying policies cho `thuyquai`, `huyetdoi`, `phicuong`; xem [D-06 review](08_D06_ENEMY_NAVIGATION_REVIEW.md) | U3D-001,U3D-009,U3D-010 | L | **D-06/A APPROVED:** Water bơi rồi lên bờ; Air bay thật 3D, anti-air, hitbox/target; D06-QA chưa chạy |
| U3D-012 | P0 | `Stage01` schedule 34 event/214 suất nguồn; D-05: miniboss #7 một mình; D-11/A: đặt 1 Phi Cương dẫn đầu mỗi event `magiap` | U3D-004,U3D-006 | M | 34 unique events 0..594; event #7 đúng 1 `magiap_boss`, không batch thường; 20 event Ma Giáp ⇒ 20 suất Phi Cương + 108 Ma Giáp (thay, không thêm), 214 suất tổng; không phụ thuộc FPS/dt |
| U3D-013 | P0 | Safe Spawn Planner + bounded queue (32 active/40 pending/4 per frame/25s TTL, baseline), giữ `eventId/slotIndex/declaredType/actualSpawnType` | U3D-010,U3D-011,U3D-012 | L | không spawn trong 3.1m Hero, không spawn off-NavMesh; elite D-11/A duy nhất theo event, không đổi sau frame/queue; job elite hết hạn thì log, không nâng job khác bù; zone East/South hỗ trợ airborne hợp lệ |
| U3D-014 | P1 | `quaidu` landing source adapter & GhostCannon decision, [D-06-06](08_D06_ENEMY_NAVIGATION_REVIEW.md) | U3D-001,U3D-011,U3D-013 | M/L | **D-06/A APPROVED:** giữ 3 Linh Pháo East với projectile→dù→QuaiDu ground West, hoàn thành trước Full Core QA; không đưa QuaiDu vào 34 event Stage01 |

**G3 gate:** automated scheduler/spawn budget/safe radius; Unity scene test đủ 3 lane, 5 loại quái; **D-05 event #7 không double, D-11/A mỗi event Ma Giáp một elite slot dẫn đầu** và deterministic qua nhiều frame, không thay quái vượt cap; debug log chỉ có một spawner production đang hoạt động, old smoke tests phải tắt. QA thực hiện với bản đồ và NavMesh **local hiện có**. **Bổ sung QA-26/ST-07..12; không tạo ticket code mới.**

## 5. Defense/Tower/Economy & Hero (G4)

| ID | Priority | Scope / deliverable | Depends | Size | Acceptance |
|---|---|---|---|---|---|
| U3D-015 | P0 | Tower socket registry 10 slot, build/upgrade/repair/occupancy; [D-07 balance review](09_D07_BALANCE_REVIEW.md) | U3D-002,U3D-004,U3D-007 | L | xây đúng vị trí, phí 1 lần, không xây trùng, attack/HP qua API thống nhất |
| U3D-016 | P0 | Wood economy, pickup physical, costs & source reward rules; [D-07 review](09_D07_BALANCE_REVIEW.md) | U3D-008,U3D-015 | M | không nhận Gỗ trước nhặt, 1 pickup 1 lần, expiry/pause/reset |
| U3D-017 | P0 | Hero combat 1 character: move, dash, basic attack, damage/HP/target | U3D-004,U3D-007 | L | Hero có thể giết / bị giết, input bị chặn khi modal, không hit ghost target |
| U3D-018 | P1 | Một Relic FUN theo `1 slot`, cast/cooldown/hitbox/ghost aim | U3D-017,U3D-001 | L | chỉ một Relic equipped; cast confirm/cancel rõ; không tiêu cooldown khi cancel |

**G4 gate:** một phiên chạy với Hero bắn/đánh, Enemy chịu damage, Nhà Chính chịu damage, xây/nâng/sửa tháp bằng Gỗ và reset toàn bộ. **Giá tháp hiện Unity 25 Wood không đồng nghĩa nguồn Web đã duyệt.**

## 6. EXP/Roguelite (G5)

| ID | Priority | Scope / deliverable | Depends | Size | Acceptance |
|---|---|---|---|---|---|
| U3D-019 | P0 | EXP gem drop/magnet/collect + per-run XP curve/pending levels | U3D-004,U3D-008 | L | EXP và Gỗ tách biệt; 1 gem collected 1 lần; dư XP giữ, multi-level queued |
| U3D-020 | P0 | Upgrade pool 3 cards/level, stack rules, fallback, apply stat modifiers | U3D-019,U3D-017 | L | 3 lựa chọn hợp lệ không trùng; chọn đúng 1; damage/speed/cooldown đổi thực; game frozen |

**G5 gate:** `kill→EXP drop→collect→level-up pause→select 1/3→apply→resume` và sau 3 restart không còn gem/card cũ.

## 7. Boss, Match Result, End-to-End (G6–G7)

| ID | Priority | Scope / deliverable | Depends | Size | Acceptance |
|---|---|---|---|---|---|
| U3D-021 | P0 | Yaksha Behavior FSM: telegraphed cleave/slam/crescent, arena→assault, stun/counterplay | U3D-007,U3D-011,U3D-012,U3D-017 | L | warning trước damage; có phương án né/counter; Boss HP không reset khi đổi phase |
| U3D-022 | P0 | Match Result & overtime đúng D-03/D-04, cleanup spawners | U3D-001,U3D-005,U3D-021 | M | **D-03/A:** chỉ WIN sau 600s + BossDead + BaseHP>0; Boss chết sớm vẫn phòng thủ, Boss còn sống @600 → OVERTIME không thêm timed-wave; LOSS BaseHP0 (ưu tiên cùng tick). **D-04/A:** Hero Down respawn sau 5 gameplay s (Pause/LevelUp đóng băng), terminal state hủy respawn; no spawn/damage sau kết thúc |
| U3D-023 | P1 | Menu/scene route + HUD trạng thái, 2D mode preserved | U3D-005,U3D-022 | M | vào Defense rõ ràng, về menu/Restart; Legacy `SampleScene` vẫn hoạt động |
| U3D-024 | P0 | Unity editmode/playmode/full 600s QA; benchmark baseline | U3D-010..U3D-023 | L | QA matrix `03_DECISIONS_QA.md`, ≥3 restart, 0 blocker P0, scene reopen GUID valid |

**G6/G7 gate:** kết luận `ACCEPTED` chỉ từ PlayMode trên máy người dùng, ghi Unity version, scene/commit, log, video/screenshot. Review GitHub code không thay thế QA.

## 8. Sau Core — Art/Performance (G8, không chặn G1–G7)

| ID | Priority | Deliverable | Depends | Acceptance |
|---|---|---|---|---|
| U3D-025 | P2 | Export Enemy/Yaksha procedural visuals từ Three.js thành asset riêng + Unity prefab | U3D-009,U3D-021,U3D-024 | từng Enemy có asset, pivot/scale correct, prefab no gameplay colliders |
| U3D-026 | P2 | Animation/VFX/SFX/URP & HUD mobile art | U3D-025 | animation event đồng bộ damage, vật liệu đúng và telegraph rõ |
| U3D-027 | P2 | Android native device profiling / input / performance | U3D-024,U3D-026 | báo cáo model device, FPS/frametime/RAM, regression, không suy từ desktop |

## 9. Chuẩn ticket khi biến backlog thành GitHub Issue

```text
ID / title:
Stage / priority / relative size:
Input: versioned Vongxuyen file + Unity adapter references:
Decision dependencies (D-xx):
Scope in / out:
No scene/meta change? If Yes: explicit local backup and diff plan.
API + data contract:
Acceptance (Given/When/Then):
Automated + Unity PlayMode test:
Evidence (Unity version, scene, branch/commit, screenshot/log):
Rollback / regression check:
Status: DRAFT | APPROVED | IN_PROGRESS | DONE_CODE | PASSED_PLAYMODE | ACCEPTED | BLOCKED
```

**Không mở issue/code khi chưa duyệt.** Khi bắt đầu triển khai, chỉ chọn 1–2 ticket trong cùng gate để giảm xung đột local/Git; cập nhật Matrix, QA và Decision Log cùng PR.
