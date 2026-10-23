# TREBRAVE — MAP SÔNG QUÊ: LEVEL SCENARIO v6.1

> **Design APPROVED:** D01=A, D02=C, D06=A, D07=C, D08=B, D09=A, D10=C.  
> **Implementation:** Unity full stylized 3D **NOT IMPLEMENTED**; this is a design contract, not a tested Unity scene.  
> **PENDING:** P02 (600s timeout with final boss alive), P03 (Hero 0HP), P04/P08 (Boss raid/win timing), P05 (wood loot details).

## 1. Stage objectives

**Defense is the main mode.** Map 01 = Map Sông Quê, two river banks connected by three bridges (North/Mid/South). The Hero and towers defend Base on west bank against enemies originating from eastern territory.

**Win (D07=C):** **only when final boss Tướng Dạ Xoa (yaksha_boss) is defeated and BaseHP > 0**. Miniboss Ma Giáp Thống Lĩnh (magiap_boss) kill is not enough.

**Lose (D07=C):** **only when BaseHP reaches 0**. HeroHP=0 is not Lose. **Do not** derive Win or Lose from timer 600s. Boss kill vs Base destroyed in same tick needs order-atomic resolution tests.

**600s (D06=A):** run design clock/timeline horizon of 10 minutes. The boss can spawn around 510s and may die before 600s; GDD requires boss kill win. **P02/P08 PENDING:** if the Boss is still alive at 600s, or a player kills it early, whether overtime/win gating applies; until defined do not implement arbitrary time-based result.

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

- EnemyDeath -> RewardResolver -> spawn world **WoodDrop** with stable lootId, runId, amount, safe reachable floor position.
- **No instant wood credit on kill**; Hero must move into pickup range and claim loot (3D collision/interact).
- MVP has **no Phu** or allied soldiers. Phu pickup is post-MVP; it must use the same LootClaim transaction if implemented.
- Dropped loot amount baseline +10/kill, starting Wood 100, tower costs 50/40/80 — unapproved balance numbers.
- EXP gems and level-up cards remain separate from wood; reward events may create both EXP and WoodDrop without double grant.
- Tower building only at ten fixed pedestals; Hero can build and upgrade while defending Base.

## 6. Boss encounters

### 6.1 Ma Giáp Thống Lĩnh — 01:48

Miniboss at timeline event index 6, reference HP 700, armor/body recognition. May attack Hero/tower/Base based on 3D role policy; death awards drops/EXP but **cannot trigger Victory**.

### 6.2 Tướng Dạ Xoa — 08:30

**Final boss ID yaksha_boss** spawns in east-bank arena. VX source: ~3200 HP, cleave/slam/crescentWave, clear telegraph/windup. Hitting final boss by hero/tower must feed one authoritative death event; **Dạ Xoa death -> WIN if BaseHP>0**. Boss behavior that raids Base vs stays in arena is **P04 PENDING**, not assumed.

## 7. Run result decision table

| Event while run Active | Result | Status |
| --- | --- | --- |
| Base HP becomes 0 | **LOSE** (exactly once) | APPROVED D07 |
| Hero HP becomes 0, Base alive | **No Lose**; incapacitation/respawn TBD | D07 fixed, P03 pending |
| Ma Giáp miniboss killed | Continue run | APPROVED D07 |
| Dạ Xoa killed, Base alive | **WIN** (exactly once) | APPROVED D07 |
| Dạ Xoa killed, Base already destroyed | Remain Lose | APPROVED D07 |
| Reach 600s, Dạ Xoa alive, Base alive | **No arbitrary Win/Lose**; overtime/holding state TBD | P02 pending |
| Dạ Xoa killed before 600s, Base alive | Boss-kill WIN intended; timing gate needs explicit P08 resolution | APPROVED D07; P08 pending |
| Pause or LevelUp popup | Stop clock, waves, enemies, tower, loot pickup where appropriate | APPROVED consistency |
| Restart | Reset clock, waves, Base/Towers/Wood/loot/runId/boss state | Required engineering |

**RunResultResolver precedence:** BaseDestroyed after Win is not allowed to overwrite Win; FinalBossDefeated after Lose cannot overwrite Lose. For truly simultaneous signals, choose a deterministic arbitration rule in P04; the system must be idempotent.

## 8. Unity 3D implementation contracts

- New DefenseScenarioSO: stageId, mapId, targetDurationSeconds=600, mainBossType=yaksha_boss, lane/spawn refs, tower slot map, policy for HeroDowned and timeout (PENDING).
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
- [ ] Enemy death drops Wood; Hero pickup only; no auto credit/double claim.
- [ ] Wave scheduler maintains event IDs/times including miniboss and final Dạ Xoa.
- [ ] BaseDestroyed -> LOSE; HeroDeath != LOSE; MiniBossKilled != WIN; FinalBossKilled -> WIN.
- [ ] 600s unresolved behavior held as P02 until approved (no scripted timeout-loss).
- [ ] Run outcome/loot/AI reset correctly; Android 3D PlayMode/field QA measured.

## 10. Links

- [Master GDD](ProjectZombie_GDD.md)
- [Decision Record](DECISION_CHECKLIST_GDD_V6.md)
- [Enemy Roster](ENEMY_ROSTER_VONGXUYEN.md)
- [3D Migration Plan](UNITY_3D_MIGRATION_PLAN.md)
- [Defense Details](MAP_AND_DEFENSE_SYSTEM_SPEC.md)
