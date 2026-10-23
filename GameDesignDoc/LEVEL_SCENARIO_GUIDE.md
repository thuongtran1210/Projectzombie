# TREBRAVE — MAP SÔNG QUÊ: LEVEL SCENARIO v6.2

> **Design APPROVED:** D01=A, D02=C, D06=A, D07=C, D08=B, D09=A, D10=C.  
> **Implementation:** Unity full stylized 3D **NOT IMPLEMENTED**; this is a design contract, not a tested Unity scene.  
> **OWNER-APPROVED P02/P03/P05/P08:** 600s overtime until Dạ Xoa killed or Base destroyed; Hero level-scaled respawn at original spawn; WoodDrop TTL initial 90s borrowed from VX **EXP** pickup; boss death Victory **immediately** and stop spawns. **P04 APPROVED**: 2 strategic state, boss đánh trong đấu trường → sau X giây kể từ BossSpawn chuyển sang đánh Nhà Chính. **K03 PENDING:** số X và thứ tự giải quyết death trong cùng tick.

## 1. Stage objectives

**Defense is the main mode.** Map 01 = Map Sông Quê, two river banks connected by three bridges (North/Mid/South). The Hero and towers defend Base on west bank against enemies originating from eastern territory.

**Win (D07=C):** **only when final boss Tướng Dạ Xoa (yaksha_boss) is defeated and BaseHP > 0**. Miniboss Ma Giáp Thống Lĩnh (magiap_boss) kill is not enough.

**Lose (D07=C):** **only when BaseHP reaches 0**. HeroHP=0 is not Lose. **Do not** derive Win or Lose from timer 600s. Boss kill vs Base destroyed in same tick needs order-atomic resolution tests.

**600s (D06=A, P02 APPROVED):** 10-minute designed **wave timeline**, not a hard match-end timeout. Boss can spawn at ~510s. **If still alive at 600s, continue OVERTIME** without launching new timeline events, until Dạ Xoa dies (WIN) or Base is destroyed (LOSE). **If Boss dies earlier, WIN immediately and stop all further spawns** (P08).

## 2. Full stylized 3D battlefield

- Unity 3D world plane = XZ, Y is height (not Unity x/y 2D).
- Bờ Tây = Base, Hero starting zone, tower pedestals; Bờ Đông = enemy spawns, portal, boss arena.
- Three lane bridges near z=-10.5, -0.5, +10.5 according to Vongxuyen reference, configurable offsets/scale.
- NavMesh/3D path graph should prevent ground enemies from crossing river except bridges; water and flying units have distinct movement policies.
- 3D camera/level lighting, colliders, movement/telegraph and touch raycasts are rebuild scope; no reuse of 2D Tilemap physics in the new Defense scene.
- Old Projectzombie 2D stage remains available in **Legacy Survival Mode**, with original 900s timeline/old enemies intact unless separately migrated.

## 3. Gameplay rhythm: five phases

| Phase | Time | Gameplay purpose |
| --- | --- | --- |
| 1 | 00:00–02:00 | Hero learns Wood pickup and one tower; Ma Giáp miniboss at 01:48 |
| 2 | 02:00–04:00 | Broader lane pressure, invest dropped wood in towers |
| 3 | 04:00–06:00 | Mix fast, ground and water threats with 3D bridge control |
| 4 | 06:00–08:00 | Tower upgrades, world pickup risk, larger lane waves |
| 5 | 08:00–10:00 | Boss threat, Dạ Xoa spawns ~08:30; wave end at 10:00 |

Pressure descriptions are **PROPOSED BALANCE**; individual event timings and spawn counts below are VX source baseline.

## 4. Stage reference events

Vongxuyen STAGE_01: **600s**, **34 scheduled wave events**, each at index×18s (index 0..33; final at 594s); phase=min(5,floor(t/120)+1). Normal enemy cycle:
1. magiap / eastBank
2. huyetdoi / northCanopy
3. thuyquai / canalWater
4. magiap / eastBank
5. magiap / southBank

At index 6 (t=108s) overrides with one **magiap_boss**. Normal count=min(8,3+phase+(index%2)). Active cap=32, pending cap=40, spawn budget/frame=4, pending lifetime=25s, minimum spawn distance=3.1.

**Boss separate:** yaksha_boss is driven by distinct VX boss runtime around **t=510s**, not automatically included in the 34-event list. Unity DefenseScenario must explicitly compose timed boss spawn into one authoritative runtime; no duplicate boss or missed spawn due to overflow.

**Enemy selection D09=A:** exactly 7 VX archetypes are **eligible in Map 01 roster**. Normal 34 events contain only 3 normal enemy types + magiap_boss; **quaidu/phicuong/yaksha** need dedicated events/AI and should not be claimed as part of 34 base events.

## 5. Wood reward and build loop [D08=B]

- EnemyDeath -> RewardResolver -> spawn world **WoodDrop** with stable lootId, runId, amount, safe reachable floor position; **wood loot TTL = 90 seconds** via WoodDropConfigSO, a *new default borrowed from Vongxuyen EXP gemLifetimeSeconds=90*, not native VX wood loot expiry.
- **No instant wood credit on kill**; Hero must move into pickup range and claim loot (3D collision/interact).
- MVP has **no Phu** or allied soldiers. Phu pickup is post-MVP; it must use the same LootClaim transaction if implemented.
- Dropped loot amount baseline +10/kill, starting Wood 100, tower costs 50/40/80 — unapproved balance numbers.
- EXP gems and level-up cards remain separate from wood; reward events may create both EXP and WoodDrop without double grant.
- Tower building only at ten fixed pedestals; Hero can build and upgrade while defending Base.

## 6. Boss encounters

### 6.1 Ma Giáp Thống Lĩnh — 01:48

Miniboss at timeline event index 6, reference HP 700, armor/body recognition. May attack Hero/tower/Base based on 3D role policy; death awards drops/EXP but **cannot trigger Victory**.

### 6.2 Tướng Dạ Xoa — 08:30

**Final boss ID yaksha_boss** spawns in east-bank arena. VX source: ~3200 HP, cleave/slam/crescentWave, clear telegraph/windup. Hitting final boss by hero/tower must feed one authoritative death event; **Dạ Xoa death -> WIN IMMEDIATELY if BaseHP>0**; cancel wave scheduler/pending spawn jobs, stop AI and display result; **P04 APPROVED:** Boss bắt đầu ở **ARENA_COMBAT**; sau **X giây gameplay kể từ BossSpawn** chuyển sang **ASSAULT_BASE** và đi đánh Nhà Chính; X nằm trong SO, chưa có số. Chi tiết [Boss 2-State Spec](YAKSHA_BOSS_PHASE_SPEC.md).

### 6.3 State flow Boss (P04 owner-approved)

```text
t≈510s: YakshaSpawn  ->  ARENA_COMBAT
after X gameplay seconds since YakshaSpawn  ->  ASSAULT_BASE  ->  attacks Nhà Chính
BossDeath (either state)  ->  Victory + cancel spawns
BaseDestroyed             ->  Lose
```

**X là thời gian giữ Boss trong Arena tính từ lần spawn**, không phải t=510 + một mốc cứng hoặc thời gian wall-clock. `X` được quản lý trong `YakshaBossPhaseConfigSO.arenaHoldDurationSeconds`, **chưa có giá trị định lượng**. `Pause` dừng boss timer. Ở t=600, Boss vẫn giữ state và countdown X; overtime không tự ép sang Assault. Boss di chuyển qua cầu/3D nav link hợp lệ; không teleport sang Base. Xem [P04 Boss Phase Spec](YAKSHA_BOSS_PHASE_SPEC.md).

## 7. Run result decision table — approved P02/P03/P08

| Event while run Active | Result | Status |
| --- | --- | --- |
| Base HP becomes 0 | **LOSE** exactly once; stop spawns | D07 APPROVED |
| Hero HP becomes 0 | **NO LOSE**; start level-dependent respawn countdown, return to **HeroInitialSpawnPoint** | P03 APPROVED (numeric curve in HeroRespawnConfigSO, K01 pending) |
| Ma Giáp miniboss killed | Continue run; create wood drop if eligible | APPROVED |
| Dạ Xoa killed with Base alive | **WIN IMMEDIATELY**; StopWaveScheduler, clear pending, reject future enemy spawns, cleanup active gameplay | P08 APPROVED |
| Dạ Xoa killed after Base destroyed | Remain Lose | D07 APPROVED |
| 600s reached with Boss alive/Base alive | **OVERTIME**; không spawn event mới >594s. Boss giữ state Arena hoặc Assault tùy **elapsedBossTime ≥ X**; countdown X tiếp tục nếu còn Arena | P02/P04 APPROVED; X K03 TBD |
| 600s reached with Boss already dead | Not applicable: run already Won on Boss death | P08 APPROVED |
| Pause/LevelUp | Stop gameplay timers including respawn countdown, waves, towers and boss | Required policy |
| Restart/scene unload | Dispose pending spawns/drops/respawn and reset all state | Required policy |

**P04 OWNER-APPROVED (two strategic states):** `ARENA_COMBAT` từ lúc Dạ Xoa spawn → khi elapsed gameplay time kể từ BossSpawn đạt `arenaHoldDurationSeconds` (X; `YakshaBossPhaseConfigSO`) → `ASSAULT_BASE` di chuyển qua đường 3D hợp lệ tới Nhà Chính và tấn công Base. **K03 pending** chỉ còn giá trị X, tactical target weights và deterministic priority khi BossKilled/BaseDestroyed cùng tick. Kết trận vẫn exactly once.

**Hero respawn config:** use `HeroRespawnConfigSO` with a level→duration function (curve or table), monotonic **non-decreasing** by hero level, stored initial spawn Transform (not current death location). No unapproved numeric seconds hardcoded in document.

**Relic MVP:** only `slipper` Dép Tổ Ong Thần Sa available; other four FUN items LOCKED. Simple 3D ghost aim/cast/damage/cooldown; FUN mechanics POST-MVP.

## 8. Unity 3D implementation contracts

- New DefenseScenarioSO: stageId, mapId, targetDurationSeconds=600, mainBossType=yaksha_boss, **YakshaBossPhaseConfigSO (arenaHoldDurationSeconds = X)**, lane/spawn refs, tower slot map, policy for level-scaled HeroRespawn and Overtime/BossInstantVictory (P02/P03/P04/P08 approved).
- New MapSongQue3D scene with ground/water/crossing colliders, EnemyPrefab3D, tower socket transforms, camera.
- MatchFlowService/GameStateManager still control state, but adapt 3D scene and combat interaction; WaveScheduler consumes gameplay time, never wall-clock.
- Pool or limits for enemy, wood drops, projectiles, VFX; reset all for new run. Analytics include baseDamage, towerBuilds, woodDropped/collected, bossOutcome, duration.
- No FriendlyArmy dependencies in MVP scene; no requirement for squad/rally UI.
- **Do not overwrite** Projectzombie's Stage_01_BambooForest.asset (600s) or Level1_Timeline.asset (900s) as a shortcut. Link a **new Defense timeline** to Map 01 and add validation to block mismatches.

## 9. Acceptance criteria

- [ ] Mode selection loads Map Sông Quê full 3D, Legacy Survival remains playable.
- [ ] Three ground routes cross three bridges without water shortcut.
- [ ] Attack & one ACTIVE relic work using 3D targeting and aim shapes.
- [ ] Tower placed in valid slot; enemy damage reaches Base; HP/UI correct.
- [ ] Enemy death drops Wood (initial 90s expiry), Hero pickup only; no auto credit/double claim.
- [ ] Wave scheduler maintains event IDs/times including miniboss and Dạ Xoa. Boss spawn → ARENA_COMBAT → sau X seconds active time → ASSAULT_BASE; X được load từ SO, không hardcode.
- [ ] BaseDestroyed -> LOSE; HeroDowned -> level-scaled respawn at run start; MiniBossKilled != WIN; FinalBossKilled -> instant WIN and stop spawns.
- [ ] At 600s with Boss alive, game enters OVERTIME and waits Boss death or Base destruction; no scripted timeout-loss.
- [ ] Run outcome/loot/AI reset correctly; Android 3D PlayMode/field QA measured.

## 10. Links

- [Master GDD](ProjectZombie_GDD.md)
- [Decision Record](DECISION_CHECKLIST_GDD_V6.md)
- [Enemy Roster](ENEMY_ROSTER_VONGXUYEN.md)
- [3D Migration Plan](UNITY_3D_MIGRATION_PLAN.md)
- [Defense Details](MAP_AND_DEFENSE_SYSTEM_SPEC.md)
