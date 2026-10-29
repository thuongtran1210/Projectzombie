# Defense3D — Stage spawn configuration

Updated: 2026-10-25 (Asia/Saigon).

## Edit and select profiles

Profiles are editable ScriptableObjects:

- `Assets/Resources/Defense3D/Stages/Stage01_Production.asset`
- `Assets/Resources/Defense3D/Stages/Stage01_FastSpawn.asset`

Open either asset in the Inspector. Edit `Events`, `Zones`, arena-boss settings
and queue limits, then click **Validate Spawn Config**. Validation reports
duplicate/missing IDs, missing references, undefined zones, invalid times/stats
and invalid queue settings before a run starts.

On the scene's `DefenseRun6003D` component, assign the desired asset to
**Stage Spawn Config**. An unassigned field loads `Stage01_Production` by default.
Exit Play Mode before changing profiles. A run compiles its event/zone snapshot
at Start; Restart resets that snapshot's cursor and queue, rather than mutating
or re-reading the authoring asset.

New profiles: `Create > TREBRAVE > Defense3D > Stage Spawn Config`.
To restore missing default assets: `TREBRAVE > Defense 3D > Create Missing Stage Spawn Configs`.
The installer preserves existing authored profiles.

## Profile semantics

| Setting | Production | FastSpawn |
| --- | --- | --- |
| Version | stage01-production-v1 | stage01-fast-spawn-v1 |
| Ordinary events | 34, 0–594 gameplay seconds | Same 34, times multiplied by 0.05; last at 29.7 seconds |
| Ordinary slots | 214 | 214 |
| Miniboss | benque-07 at 108 seconds; one magiap_boss | Same event at 5.4 seconds |
| Yaksha | Independent boss event at 510 seconds | Independent boss event at 10 seconds |
| Arena duration | 45 seconds after actual spawn | 5 seconds after actual spawn |
| Match duration | 600 gameplay seconds | 600 gameplay seconds |

FastSpawn previews spawn/arena behavior earlier. It does not shorten the win
condition or change Unity time scale. Both profiles include one independent
arena boss in addition to the 34 ordinary events: 35 scheduled events / 215 slots.
Seeded boss HP remains the previous prototype override (1200), explicitly editable;
these are test values, not approved production balance numbers.

Each ordinary event has a unique ID, gameplay time, Enemy Definition, Zone ID,
count and optional slot-0 replacement definition. The replacement consumes one
of the existing slots. Production's 20 magiap events replace their first slot
with phicuong, preserving 20 phicuong + 108 magiap, not adding enemies.
Changing a profile does not require editing the scheduler or factory.

## Spawn zones

Each zone has a unique ID, an origin and points. A point has an offset and bridge
lane (0–2); a job chooses a point by its slot index modulo the zone's point count.
If `Relative To East Marker` is enabled, the origin is relative to
`DefenseSceneInfo3D.EastEnemySpawn`. Otherwise it is a world origin.

The default assets explicitly define `eastBank`, `northCanopy`, `southBank`,
`canalWater` and `arenaEast`. Runtime resolves the declared zone; it no longer
silently sends every job to the East marker's three lanes. Ordinary spawns
must project onto NavMesh within 2 meters and have a complete path to their
bridge entry. The Hero safe radius is checked against the resolved position.

These coordinates are starter values for the current map. All authored zones
still need scene acceptance. Air/water enemies retain the existing ground
prototype adapter; these profiles do not implement flight or swimming. Unsafe
or unreachable points remain queued and expire with a zone/event/slot warning.

## Responsibility boundaries

- `StageSpawnConfig`: Inspector authoring and validation/compilation.
- `DefenseStageSpawnPlan`: copied event/zone values and Enemy Definition lookup.
- `DefenseStageSpawnScheduler`: pure C# due-event cursor; emits jobs through a
  supplied callback, without scene, Resources or prefab access.
- `DefenseSpawnPlanner`: bounded pending queue, active cap, budget, TTL; accepts
  a safety predicate and does not decide geometry or instantiate prefabs.
- Zone snapshot resolver: converts zone + slot + scene anchor to position/lane.
- `DefenseRun6003D`: composition root; connects gameplay clock, NavMesh checks,
  scheduler, queue, factory, boss arena and entity lifetime.
- `DefenseEnemyFactory3D`: creates an Enemy using its definition.

`DefenseStage01Schedule` remains benchmark/seed data and regression evidence;
runtime no longer calls it. Boss spawn timing is data, not a 10-second constant.

Blocked jobs rotate through a bounded inspection pass so an unsafe zone cannot
starve safe jobs in another zone. The slot's declared/actual enemy identity is
preserved through deferral and expiry. Queue settings come from the profile.
Enemy destruction releases one active slot. Restart increments the run ownership
generation before deferred destruction, preventing old callbacks from decrementing
the new run's active count. Boss arena timing begins at actual spawn, including
when cap/safety rules delay its scheduled event.

## Verification

- `Logs/TraversalQA/stage-spawn-stage0.xml`: 40 PASS, 1 existing G1 FAIL.
- `Logs/TraversalQA/stage-spawn-stage1.xml`: one PlayMode integration PASS,
  containing the foundation scenarios plus config-driven scheduling/cap/restart.
- Combined: **41 PASS, 1 existing FAIL**, not a full-suite green claim.

New tests cover baseline counts, elite/miniboss/boss timing, frame catch-up,
duplicate prevention, scheduler reset, separate FastSpawn times, validation,
snapshot isolation, distinct zone resolution and custom queue limits/fairness.
The PlayMode scenario uses synthetic NavMesh and a cloned two-event profile:
one active enemy holds the second job; death releases the slot; restart spawns
the new run's first job without releasing extra slots from old callbacks.
Acceptance requires `RUNTIME_COMPLETED: StageSpawnConfig` and all other runtime
completion markers, rather than trusting runner status alone.

The pre-existing G1 failure remains `StartPauseLevelUpResume_ObeysTransitions`:
`TryResume()` permits Ready → Playing. No state-machine change is included here.
Full map-zone NavMesh checks, full 600-second acceptance and Android performance
remain outstanding.
