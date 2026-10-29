# Defense3D — Enemy foundation

Updated: 2026-10-25 (Asia/Saigon).

## Responsibilities

| Component | Responsibility |
| --- | --- |
| `DefenseEnemyDataSO` | Authored identity, prefab, locomotion metadata, combat stats and rewards |
| `DefenseEnemyFactory3D` | Resolve a definition, instantiate its prefab, initialize a spawn snapshot and route |
| `DefenseEnemy3D` | Identity and health authority, backed by the existing pure C# `DefenseHealthCore` |
| `IDefenseEnemyTarget3D` | Enemy target contract, extending the existing damage contract |
| `DefenseEnemyTargets3D` | Active enemy index and shared nearest-live-target query; no per-shot scene scan |
| `DefenseGroundMovement3D` | NavMesh route following, complete-path validation, arrival and failure handling |
| `DefenseBridgeRoute3D` | Existing map's lane coordinates, separate from the navigation implementation |
| `DefenseEnemyAttack3D` | Receive a damageable target, check range and cooldown, deliver damage |
| `DefenseEnemyDrop3D` | Subscribe to death, create Wood once, destroy the defeated entity |
| `DefenseEnemyBridgeWalker3D` | Compatibility/composition adapter for existing scene, smoke-test and Yaksha callers |

The adapter retains existing serialized fields and GUIDs. It forwards health/events
and coordinates arrival → attack; it no longer implements path traversal, health
storage, attack timers or item creation. Arena → assault configures the route
without resetting HP or synthesizing damage to restore it.

Hero and Tower use `IDefenseEnemyTarget3D` and the shared target index. An Enemy
does not need `DefenseEnemyBridgeWalker3D` to receive their attacks. Base now
implements `IDefenseDamageable3D`, so Enemy attack does not depend on its concrete class.

## Authoring

- Fallback ordinary prefab: `Assets/Resources/Defense3D/DefenseEnemy3D.prefab`.
- Dedicated archetype prefabs & visuals:
  - `magiap`: `Assets/Resources/Defense3D/DefenseEnemy_Magiap.prefab` (Giáp đỏ sẫm, scale 1.0)
  - `thuyquai`: `Assets/Resources/Defense3D/DefenseEnemy_ThuyQuai.prefab` (Xanh lam ngọc bích, scale 0.9, 0.7, 1.3)
  - `huyetdoi`: `Assets/Resources/Defense3D/DefenseEnemy_HuyetDoi.prefab` (Đỏ huyết, scale 0.7, 0.65, 0.7)
  - `quaidu`: `Assets/Resources/Defense3D/DefenseEnemy_QuaiDu.prefab` (Xám rằn ri, scale 0.85, 1.0, 0.85)
  - `phicuong`: `Assets/Resources/Defense3D/DefenseEnemy_PhiCuong.prefab` (Tím hoàng kim, scale 1.2, 1.3, 1.2)
  - `magiap_boss`: `Assets/Resources/Defense3D/DefenseEnemy_MagiapBoss.prefab` (Đỏ rực nung nham, scale 1.7, 1.7, 1.7)
  - `yaksha_boss`: `Assets/Resources/Defense3D/DefenseYaksha3D.prefab` (Tím ma vương, scale 2.2, 2.2, 2.2)
- Authored definitions in `Assets/Resources/Defense3D/Enemies/*.asset` each link directly to their dedicated archetype prefab.
- The production timed-wave path resolves definitions through the factory and
  applies HP, speed, damage, interval, approach/attack range and Wood reward.
- `DefenseEnemyRegistry` remains benchmark/seed data for existing contracts and
  migration. Authored definitions are the normal timed-wave runtime source;
  the installer does not overwrite existing definition values.
- `Create Missing Enemy Prefabs` now creates all 7 distinct prefabs + materials, seeds missing definitions,
  wires definitions to their dedicated prefabs, and adds composition components (`DefenseEnemyBridgeWalker3D`,
  `DefenseEnemy3D`, `DefenseGroundMovement3D`, `DefenseEnemyAttack3D`, `DefenseEnemyDrop3D`).

To add a ground archetype, create `TREBRAVE/Defense3D/EnemyData`, give it a unique
ID and the composed prefab, and save it under `Resources/Defense3D/Enemies/<ID>`.
The factory can spawn that definition directly; connecting it to Stage01 also
requires its schedule/validation data. Hero/Tower require no archetype-specific changes.
Use separate prefab assets when an archetype needs its own authored visual.

Stats are copied per spawn. Taking damage does not modify the shared ScriptableObject.
The health authority is independent of drops: composing a health-only enemy does
not automatically create rewards, navigation or attack behaviors.

## Verification

Unity 2022.3.62f3 / Test Framework 1.1.33:

- `Logs/TraversalQA/enemy-foundation-stage0.xml`: **30 PASS, 1 FAIL** (31 EditMode cases).
- `Logs/TraversalQA/enemy-foundation-stage1.xml`: **1 PASS**, one PlayMode-session
  integration test containing five independently asserted scenarios.
- Combined: **31 PASS, 1 existing FAIL**. This is not a green full-suite claim.

The existing failure is `DefenseG1StateClockTests.StartPauseLevelUpResume_ObeysTransitions`
at line 10. `DefenseGameState.TryResume()` transitions Ready → Playing; the test
expects rejection. Both the implementation and test are unchanged from HEAD.
No state-machine fix is included in this Enemy refactor.

The integration output contains all completion markers:

1. `EnemyAttack`: data-driven damage/cooldown and stop-on-disable.
2. `WoodPickup`: exactly-once death/drop, proximity collection, delayed pickup,
   pause and late collector recovery.
3. `HeroAndTower`: damage an actor with no bridge-walker component.
4. `MaGiapFactory`: initialize from a definition, follow a complete synthetic
   NavMesh route, arrive once, then damage Base.
5. `YakshaArena`: warning before damage, range exclusion, stop on arena end.
6. `AllScenarios`: the test reached its final assertion.

The runner's repeated Enter/Exit PlayMode behavior in this framework version could
report success for test bodies that did not finish. Runtime scenarios are now run
inside one PlayMode session, and acceptance requires every completion marker.
Earlier combined-run XML without those markers is not accepted as runtime evidence.

## Remaining scope

- The route test uses synthetic NavMesh, not proof of all three authored bridges.
- Air/water/landing metadata is preserved; those waves still use the existing
  bridge prototype adapter. Flight and swimming strategies are not implemented here.
- EXP reward metadata is preserved; EXP pickup runtime remains separate work.
- Enemy aggro selection between Hero/Support/Base remains separate work; the
  current ground composition receives Base as its route/attack target.
- Boss-specific arena behavior remains separate. Full boss routing, 600-second
  acceptance, pooling, full reset/terminal pickup handling and device performance
  are not claimed complete.
