# Defense3D standalone validation

Run from PowerShell:

```powershell
& Tools/Defense3DValidation/RunCoreChecks.ps1
```

The script invokes only Mono and the C# compiler bundled with the installed toolchain. It never launches Unity or references UnityEngine. Use `-EditorDirectory` if the toolchain is installed elsewhere. Output executables/response files go to `Temp`; evidence goes to `Logs/TraversalQA`.

Seventeen console harnesses (455 checks at the 2026-10-27 indicator milestone) cover lifecycle/spawn ownership, damage/target/stats, input/ability state, shared pointer gestures/XZ aim geometry, skill cast scope/commit/area snapshots, Vongxuyen immediate-preview/tap/drag math, actor lifetime/reward retirement, and Ground route/aggro policy. These checks cannot validate Unity lifecycle callbacks, input devices, scene rendering, physics or NavMesh.

`VerifyRefactorStep4.py [snapshot-directory]` compares the step-4 source boundaries and authored assets against its pre-change snapshot. It requires the snapshot manifest and logs produced with `RunCoreChecks.ps1 -LogPrefix defense-refactor-step4`. It does not execute or inspect Unity.

`VerifyRefactorStep5.py [snapshot-directory]` compares against the step-5 baseline, checks the shared gesture adapters and authored indicator YAML/material, and requires logs with prefix `defense-refactor-step5`.

`VerifyRefactorStep6.py [snapshot-directory]` checks the authored Dash/Dép control prefab and complete scene override graph, definition assets, composition participants, target snapshot boundary and logs with prefix `defense-refactor-step6`. It requires PyYAML. A successful static check does not confirm that Unity imported these assets or executed the controls/physics.


`VerifyMapHierarchy.py [snapshot-directory]` is the current hierarchy-consolidation check. It compares the scene to its pre-consolidation snapshot, proves unchanged poses through unchanged local TRS and identity ancestors, checks parent/child membership, preserves script/physics/NavMesh/prefab bindings and hashes protected files. It requires PyYAML and never starts Unity. The step4/5/6 checkers describe their historical scene revisions and are not interchangeable with this latest hierarchy baseline.


`VerifyVongxuyenControls.py [snapshot-directory]` is the current authored-control check. It verifies the thumb pads/circular cancel controls, range/arrow shapes, persistent foot prefab and 18-participant scene binding against the pre-port snapshot. It requires PyYAML and core logs with prefix `defense-vongxuyen-controls`. Rendering, touch and shader compilation remain outside these standalone checks.


`VerifyIndicatorRenderer.py [snapshot-directory]` checks the shader pass against the installed URP renderer source, not just material references. It reproduces rejection of the pre-fix UniversalForward-only pass by the configured 2D renderer and acceptance of SRPDefaultUnlit. Requires PyYAML and the renderer-fix snapshot; it does not compile shaders or verify pixels in Unity.


`VerifyP4FactoryLifecycle.py [snapshot-directory]` checks the initial P4 authored runtime roots, instance factory creation/release path and planner token guards against the pre-P4 snapshot. Requires PyYAML and core/compile logs with prefix `defense-p4-factory`. Native Unity callback ordering and pool reuse are not verified by these checks.


`VerifyP4GroundAggro.py [snapshot-directory]` checks the initial Ground/aggro slice, authored Ground prefabs/profile, dynamic Base route endpoint and preservation of scene/other locomotion assets. Requires PyYAML and core/compile logs with prefix `defense-p4-ground`. It does not verify NavMeshAgent motion, LOS or melee damage in Unity.


`DefenseSpawnQueueChecks` also reads the authored Stage YAML event times/counts directly for pure planner schedule simulations. It is not a Unity/geometry test. `VerifySpawnQueueFix.py [snapshot-directory]` verifies that only the scene Stage reference switches to Production, both source profiles/limits remain unchanged and queue expiry/telemetry regression logs pass. Requires PyYAML and logs with prefix `defense-spawn-queue`.


The additional harnesses cover spawn-queue pressure, Air targeting, Landing, multi-domain aggro, statuses, cone windups, shared-health Boss FSM, shared manual/auto Scholar attacks and cooldown refunds, and indicator screen projection/countdowns. `RunCoreChecks.ps1` runs these pure contracts only; it does not run the Unity test fixtures shipped under Assets/Tests.

`VerifyScholarCombatWiring.py [snapshot-directory]` describes the pre-indicator Scholar kit revision. `VerifyIndicatorWiring.py [snapshot-directory]` checks the latest indicator overlay, authored 32-slot warning pool, 21-participant scene wiring, read-only Stage observation and unchanged protected resources. Both need PyYAML and their matching local pre-change snapshot. Snapshots/logs stay under ignored Logs/Temp and are not shipped as portable fixtures. Without those snapshots, run the pure harnesses; do not treat a missing preservation baseline as Unity acceptance.
