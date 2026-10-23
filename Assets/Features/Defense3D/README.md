# TREBRAVE Defense 3D — First Scene Prototype

This module is intentionally **separate from Legacy Survival 2D** and from the old `SampleScene.unity` bootstrap.

## Open it locally in Unity 2022.3.62f3

1. Switch to `feat/trebrave-defense3d-first-scene` after the stacked PR #2 and #3 source changes are available.
2. Open the Unity project, wait for script import.
3. Select **TREBRAVE > Defense 3D > Build & Open Map Song Que Prototype**.
4. Unity generates `Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity` on your machine, adds it to Editor Build Settings **without removing SampleScene**, and opens it.
5. Hit **Play** in this scene; keyboard **WASD/arrows** or gamepad **left stick** moves the cyan capsule Hero on the XZ plane. Camera follows Hero.
6. Inspect two 3D ground banks, water strip, 3 bridge colliders, gold Base on west, enemy arena marker on east, and 10 gold tower sockets.

The generated `.unity` scene is NOT checked in as part of this PR because Unity Editor cannot be executed via the GitHub connector. **Commit the generated scene plus `.meta` on a Unity workstation only after import/test, and preserve these script GUIDs.** Do not claim a reproducible Unity scene asset exists on remote until this is done.

## Important limitations

- **Not wired to existing "Vào Trận" menu yet.** Existing menu boots Legacy Survival, so it will still show the old U Minh Giới Tilemap until a Defense3D mode selection/scene-routing PR is implemented. The Editor menu is the new entry point for this technical first scene.
- **Not the full TREBRAVE vertical slice:** no active enemies, AI navigation, waves, Tower build, WoodDrop, Slipper, Boss Yaksha, hero roster/selection, Overtime, Win/Lose UI yet.
- `DefenseBasePrototype3D` keeps placeholder HP only; do not treat it as the final authoritative result resolver.
- Hero uses a deliberately standalone `DefenseHeroMotor3D` rather than binding `PlayerController` and its old Survival UI/Co-op/2D dependencies.
- 3 bridges and 10 sockets are prototype markers/colliders, not completed meshes/art or NavMesh route data.
- Geometry references are roughly the Sông Quê prototype: Base west x≈−12, river x≈4.45–8.05, Boss arena east x≈25; verify scale and navigation against the approved GDD.
- To make an Android build open Defense rather than Legacy, the eventual mode router must explicitly select this scene; this PR only registers it in Build Settings after generation. **SampleScene remains first/default scene** for Legacy preservation.

## Acceptance tests after Unity import

- [ ] Unity C# compilation succeeds.
- [ ] Generator menu completes without exceptions.
- [ ] Scene saved and reopens without missing script GUIDs.
- [ ] Hero walks in four horizontal directions, cannot walk through river, can cross each bridge.
- [ ] Camera follows and scene hierarchy has exactly one player and one camera.
- [ ] Base West and three bridges/ten sockets/east spawn markers are populated.
- [ ] Legacy `SampleScene` still opens old Survival without regression.
- [ ] Unity-generated scene and `.meta` are checked into version control after Unity review.

No Unity Editor / PlayMode / Android test was executed remotely.
