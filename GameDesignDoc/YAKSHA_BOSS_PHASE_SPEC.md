# TREBRAVE — TƯỚNG DẠ XOA: 2 TRẠNG THÁI CHIẾN ĐẤU (P04)

> **P04 — OWNER APPROVED (2026-10-23):** Final Boss Dạ Xoa có **hai trạng thái chiến thuật chính**: **ARENA_COMBAT → ASSAULT_BASE**. Sau khi ở đấu trường đủ **X giây** kể từ lúc Boss xuất hiện, Boss tiến đánh Nhà Chính. **X chưa chốt số; cấu hình ScriptableObject.**
>
> **Trạng thái triển khai:** Chỉ là tài liệu thiết kế; không khẳng định Unity 3D Boss FSM đã được code. **K03 PENDING**: ưu tiên khi BossKilled và BaseDestroyed xảy ra cùng tick, chi tiết AI/đòn đánh và thông số cân bằng.
>
> **Không thay đổi:** D07 BaseHP=0 mới LOSE, Yaksha Boss chết → WIN tức thì; P02 tiếp tục overtime nếu còn Boss sau mốc 600s; P08 Victory dừng mọi đợt spawn mới.

## 1. Hai trạng thái chiến thuật

| State | Điểm bắt đầu | Hành vi | Điều kiện chuyển |
| --- | --- | --- | --- |
| **ARENA_COMBAT** | Ngay sau khi `yaksha_boss` spawn tại đấu trường Bờ Đông (~510s của trận theo stage reference) | Chiến đấu với Hero hoặc các mục tiêu hợp lệ **trong phạm vi đấu trường**; dùng đòn Boss và telegraph, giữ boss ở arena, không tấn công Nhà Chính từ xa | Khi **elapsed active gameplay time since boss spawn ≥ `arenaHoldDurationSeconds` (X)** và Boss còn sống, trận chưa kết thúc → chuyển một lần sang ASSAULT_BASE |
| **ASSAULT_BASE** | Sau X giây từ BossSpawn | Boss rời đấu trường, di chuyển theo **đường 3D hợp lệ** từ Bờ Đông qua cầu/nav link tới Nhà Chính ở Bờ Tây; mục tiêu chiến lược là **Base**; có thể giao chiến với Hero/Tháp trên đường theo target policy có cấu hình nhưng không được quên mục tiêu Base | Giữ state này đến Boss chết → WIN, hoặc BaseHP=0 → LOSE; **không tự quay lại ARENA_COMBAT** trong MVP |

**Đây là 2 state ở cấp chiến lược.** Các tiểu trạng thái đòn đánh như `idle`, `chase`, `cleave_windup`, `slam_windup`, `stunned`, `die` từ Vongxuyen vẫn có thể là sub-FSM/animation trong mỗi state chiến lược, **không thêm state chiến lược thứ ba**.

## 2. Nguồn thời gian — X

- Đề xuất tên trường SO: `BossPhaseDefinitionSO.arenaHoldDurationSeconds` hoặc `YakshaBossPhaseConfigSO.arenaHoldDurationSeconds`. **X là khoảng thời gian từ lúc Boss thật sự được spawn và vào ARENA_COMBAT**, không phải thời gian kể từ khi bắt đầu trận.
- Nếu spawn Boss ở khoảng t=510s và `X=...`, chuyển state tại **BossSpawnGameplayTime + X**; `X` không có số cụ thể trong GDD.
- Dùng **gameplay clock**/accumulated unpaused delta. Pause, LevelUp pause, loading hoặc result screen **không tăng thời gian**.
- 600s là kết thúc timeline wave gốc, **không tự ép Boss ASSAULT_BASE** trừ khi X đã hết. Nếu t=600 Boss vẫn ở arena và X chưa hết thì overtime tiếp tục trong ARENA_COMBAT, rồi chuyển sang ASSAULT_BASE khi X đủ.
- SO phải validation `arenaHoldDurationSeconds >= 0`, hữu hạn, thuộc đúng scenario; có thể tune/balance lại mà không sửa code.
- Nếu Boss chết hoặc run kết thúc trước X, hủy transition; không teleport Boss hoặc phát state-change muộn từ pooled instance/run cũ.

## 3. Behavior 3D

**ARENA_COMBAT**
- Boss spawn 1 lần ở `BossArenaSpawnTransform`; boss healthbar/cảnh báo xuất hiện, Hero có thể đến đấu Boss.
- Target scan giới hạn vùng arena. Điều khiển vị trí Boss tránh rời vùng/húc vào water collider.
- Đòn cleave/slam/crescentWave vẫn có windup/telegraph và cooldown; tính năng cụ thể dựa trên boss 3D implementation, không mặc định WebGL mesh sẵn có.
- Có thể trúng sát thương Hero/Dép/Tower nếu đường đạn/tầm hợp lệ.

**ASSAULT_BASE**
- Boss phát cảnh báo `BossAssaultStarted` để UI báo sắp đánh Nhà Chính.
- 3D NavMesh/waypoints dẫn từ arena qua **một trong 3 cầu hợp lệ**; **không xuyên nước/terrain, không teleport**.
- Nếu Base còn sống và có đường hợp lệ, Boss tiến tới phạm vi đánh Base và dùng bộ đòn được phép gây damage Base.
- Hero có thể truy đuổi, bảo vệ đường hoặc dùng tháp đánh Boss, nhưng Base là mục tiêu chiến lược. Nếu NavMesh route fail, fallback/repath có giới hạn và cảnh báo debug; không tự kết thúc trận.
- Khi Boss chết lúc đang di chuyển/tấn công, mọi pending hit/projectile phải bị vô hiệu hóa theo RunEnded để không phá Base sau Victory.

## 4. State transition và game result

```text
Run Active (t ≈ 510s)
    |
    v
YakshaBoss Spawn
    |
    v
ARENA_COMBAT
    |       \-- BossKilled --> WIN immediately -> stop waves and pending spawns
    |
    | elapsedBossTime >= X (config)
    v
ASSAULT_BASE
    |       \-- BossKilled --> WIN immediately -> stop waves and pending spawns
    |
    \-- BaseHP = 0 --> LOSE (from either state if Base damaged)

600s with boss alive: OVERTIME; maintain current Boss state/timer.
```

**K03 PENDING:** Nếu `BaseDestroyed` và `FinalBossKilled` phát ra trong cùng một authoritative simulation tick, owner chưa quy định event nào ưu tiên; phải làm deterministic/event arbitration trước runtime release. Luật Base-only-loss và Boss-only-win không đổi.

## 5. ScriptableObject contract (đề xuất tên)

```csharp
// Data schema proposal, NOT existing Unity code
YakshaBossPhaseConfigSO
{
    string bossTypeId = "yaksha_boss";
    float arenaHoldDurationSeconds;  // X, >= 0, value TBD
    TransformReference arenaSpawn;   // assign via scene binding, not hardcoded
    ZoneReference arenaCombatBounds;
    PathReference baseAssaultPath;    // reachable bridge / 3D nav path
    TargetPolicyReference arenaTargetPolicy;
    TargetPolicyReference assaultTargetPolicy;
    AttackSetReference arenaAttackSet;
    AttackSetReference assaultAttackSet;
    bool allowRetreatToArena = false; // fixed MVP behavior
}
```

**Run state**: `runId`, `bossSpawnGameplayTime`, `elapsedArenaSeconds`, `currentBossStrategicState`, `hasTransitionedToAssault`, `baseTargetId`, `isBossAlive`. Reset all on new run or pool checkout.

Event contract: `BossSpawned`, `BossArenaEntered`, `BossAssaultStarted`, `BossDefeated`, `RunEnded`. Each carries runId/bossInstanceId and ensures exactly-once transition.

## 6. PlayMode / QA acceptance

- [ ] Yaksha spawn at configured stage boss event and starts in **ARENA_COMBAT**.
- [ ] After **X seconds of active boss-time** (not real-world time), transition exactly once to **ASSAULT_BASE**.
- [ ] Pause/LevelUp pauses elapsed arena duration; at 600s and X not elapsed, Boss remains in arena during overtime.
- [ ] Boss crosses a navigable 3D bridge; no water shortcut, stuck target or fake long-distance Base damage.
- [ ] Boss chooses Base as strategic target in ASSAULT_BASE and can damage it at real 3D attack range.
- [ ] Kill Boss in either state while Base alive => WIN immediately, wave scheduler canceled, pending spawns cleared, no new enemies.
- [ ] BaseHP=0 => LOSE; Hero downed only triggers respawn, never LOSE.
- [ ] Boss death before X cancels transition, no delayed attack after victory.
- [ ] New run resets timers, state and subscriptions; no duplicate phase transition due to pooling.
- [ ] K03 same-tick BossKilled/BaseDestroyed ambiguity has a deterministic reviewed solution before PlayMode signoff.

## 7. Related specifications

- [Master GDD](ProjectZombie_GDD.md)
- [Owner Decision Record](DECISION_CHECKLIST_GDD_V6.md)
- [Level Scenario](LEVEL_SCENARIO_GUIDE.md)
- [Enemy Roster](ENEMY_ROSTER_VONGXUYEN.md)
- [3D Migration Plan](UNITY_3D_MIGRATION_PLAN.md)
