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

## Dùng bản đồ gốc Vongxuyen Three.js trong Unity

[Draft PR Vongxuyen #39](https://github.com/thuongtran1210/Vongxuyen/pull/39) thêm nút **📦 Xuất Map .GLB** trên `map-overview.html`. Nguồn: `src/world/MapBenQue.js`, `ScenicDetails.js`, `src/config/mapConfig.js`.

1. Checkout nhánh `feat/export-song-que-map-glb-unity` của Vongxuyen, mở web qua local HTTP server (ví dụ VS Code Live Server), vào `map-overview.html`, bấm **Xuất Map .GLB**. Trình duyệt tải `TREBRAVE_MapSongQue_ThreeJS.glb`.
2. Unity 2022.3 của Projectzombie: cài glTF/GLB importer tương thích (ví dụ **glTFast**). Unity Editor không đảm bảo import GLB trực tiếp nếu thiếu package importer.
3. Import GLB vào `Assets/Art/Environment/MapSongQue/` rồi đưa model vào scene `MapSongQue3D_Prototype` thay **phần hiển thị** primitive. Giữ scene gameplay/marker/collider 3D để không phá luồng điều khiển.
4. Kiểm tra tỉ lệ (kỳ vọng 1 world unit = 1 m), hướng trục, tọa độ nhà x≈−12, đấu trường x≈25, rạch x≈4.45–8.05, ba cầu z≈−10.5,−0.5,10.5. Kiểm chứng bằng các marker thực tế.
5. Tạo/kiểm lại **3D Ground colliders**, bridge colliders, NavMeshSurface/links và layer `Ground`, bảo đảm không đi xuyên sông. Phần nước, sương, animation và shader Three.js cần chuyển sang vật liệu/FX URP riêng.
6. Khi được Unity kiểm chứng, commit file scene `.unity` và `.meta`, asset/model đã import cùng manifest package, rồi mới nối nút Vào Trận vào Defense3D.

**Quan trọng:** GLB là snapshot hình học/vật liệu Three.js, **không** tự chuyển gameplay scripts, vật lý, VFX động, wave, tháp hay Boss sang Unity. Chưa chạy thử exporter trong browser hoặc import trong Unity từ phía ChatGPT.

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

## NavMesh bridge smoke test (AI Navigation 1.1.5)

The smoke-test walker now uses Unity `NavMeshAgent` (not direct Transform movement). Existing `DefenseEnemyBridgeSmokeTest3D` and its Unity Editor installer continue to work; **do not rebuild the scene**, since the local GLB and NavMesh bake live in that scene.

1. In `MapSongQue3D_Prototype`, enable the prototype gameplay root and keep its Ground-layer Physics Colliders enabled (west/east banks and all three bridge colliders). Keep the imported Vongxuyen GLB as visual art.
2. Keep an active `NavMeshSurface` in the scene: `Use Geometry = Physics Colliders`, `Collect Objects = All Game Objects`, `Include Layers = Ground`; Bake and save NavMesh asset and scene.
3. Use `TREBRAVE > Defense 3D > Add Bridge Enemy Test To Open Scene` if not installed, then Play. Three red capsules spawn on the east side and travel through their own designated bridge via `NavMeshAgent`.
4. Each agent requires a complete navmesh path to *every* lane waypoint; it never crosses water via a Transform fallback. Console logs `NavMesh enemy reached Base` on arrival and subtracts 25 Base HP (75 total if all three arrive). If the path is not connected or waypoint is not baked, the enemy stops and logs `Enemy route stopped: ...` with its specific lane.

This is deliberately a navigation **smoke test**, not combat AI: no avoidance tuning, enemy attack animations, wave logic, or production Base damage arbitration. Unity PlayMode and Android have not yet been run by the author.

## Three-wave Defense mini vertical slice (opt-in)

Run after the 3-bridge NavMesh smoke test succeeds. **Keep the imported GLB and baked scene/NavMesh**; never rebuild the prototype scene to install this feature.

1. Save scene, close Unity, `git status`, then `git pull --ff-only origin feat/trebrave-defense3d-first-scene` only if safe against local tracked changes.
2. Reopen `MapSongQue3D_Prototype`, confirm `NavMeshSurface` is enabled and baked.
3. Choose `TREBRAVE > Defense 3D > Install 3-Wave Defense Test In Open Scene`. The installer disables the old bridge-only spawner component so the two tests do not spawn together. Save scene and Play.
4. Three quick test waves (3 enemies each, approximately 16-second wave interval) use NavMesh waypoint lanes. Enemies have 100 test HP, move to Base, and repeatedly hit it for 25 every 2 seconds until BaseHP reaches 0. Top-left IMGUI overlay shows Base HP and wave state. Base loss emits a signal and stops further timed wave spawns.
5. On finishing all 3 spawns, **no win is declared**. Boss is not yet implemented. Enemy damage intake API exists but Hero/towers cannot attack them yet.

Limits: This is NOT the approved 600-second 34-event wave schedule, Boss outcome arbitration, attack animations, Unity UI canvas/HUD production, or Android-ready enemy prefab system. No Unity PlayMode validation has been run from this repository-only change.

## Hero auto-attack smoke test (opt-in)

After syncing feature branch, open the existing imported-GLB scene and choose **TREBRAVE > Defense 3D > Install Hero Basic Attack In Open Scene**. This adds `DefenseHeroAutoAttack3D` to the existing Hero referenced by `DefenseSceneInfo3D` without regenerating the scene. Save and Play with the 3-wave test.

- Hero automatically selects the nearest active test enemy within 5 world units and deals 35 damage every 0.65 seconds. Test enemies have 100 HP and die on their third hit. The target selection queries live `DefenseEnemyBridgeWalker3D` instances, because the temporary enemy capsules do not have colliders.
- Approach enemy paths using WASD/arrows and observe Console `Hero hit` and `Enemy defeated`. Highlight Hero in Scene view to see the yellow range Gizmo.
- The attack intentionally uses no skill effects, aim animation, projectiles, hero HP, cooldown UI, tower or production DPS tuning. It is a smoke test to confirm enemies can be killed before reaching Base. Test in Unity Editor; not yet PlayMode QA-verified.

## Tower build prototype (opt-in)

From your **existing** scene with Vongxuyen GLB and baked NavMesh, run `TREBRAVE > Defense 3D > Install Tower Build Test In Open Scene`. Save scene and Play. Approach one of ten original tower socket markers (within 5m) and press **B**.

- Prototype starts with **100 test Wood** and consumes **25 test Wood** for each tower. This is a testing wallet, not physical WoodDrops. There is no automatic wood reward from enemy kills.
- Up to four towers can be placed with the default wallet; one tower per socket. Each simple cylinder tower automatically attacks a nearby living `DefenseEnemyBridgeWalker3D` within 8m for 22 damage every 0.9s.
- The existing Hero auto-attack, 3-wave test, Base HP, and NavMesh agents keep working; the installation menu does not rebuild/overwrite the GLB scene, collider setup, or NavMesh.
- Build visuals have no collider intentionally so they do not block baked navigation. Real tower prefabs, production visuals, physical wood gathering, mobile input and save/reload are not included.

If the menu is missing after Git pull, first check Unity Console for compile errors, then attach `DefenseTowerBuildPrototype3D` to a new active scene GameObject via Inspector Add Component.

## Physical WoodDrop economy (opt-in Hero wallet)

1. Save the GLB/NavMesh scene, pull this feature branch, and **do not rebuild** the map.
2. Open the existing Defense scene; run `TREBRAVE > Defense 3D > Install WoodDrop Economy In Open Scene`. This adds `DefenseWoodWallet3D` to the Hero marker (starting Wood = **0**). If Unity does not show the menu, Add Component > `DefenseWoodWallet3D` to `Hero_3D_Prototype`.
3. Keep `DefenseHeroAutoAttack3D`, `DefenseWaveSlice3D` and `DefenseTowerBuildPrototype3D` active. Kill a capsule enemy with Hero or tower; it creates a physical brown `WoodDrop_10` cube at the death position.
4. Move Hero within 1.4m of the pickup. Only this Hero's wallet increases (+10 Wood). Uncollected drops expire after 90 seconds. Kills **do not** grant Wood directly. Press **B** within 5m of an empty tower socket to spend 25 Wood; starting test Wood of 100 is removed.
5. Look for Console logs `Enemy defeated`, `Hero collected 10 Wood`, and `Tower built ... Wood: ...`. Three drops (30 Wood) can pay for one tower. A tower can still kill enemies, but drop pickup requires Hero proximity.

This is a prototype using physical visual pickup proximity checks rather than trigger Rigidbody colliders. The Wood wallet is per Play session, and save/restore, mobile HUD, magnet, pooling and production wood balancing are not implemented. Unity PlayMode has not been run from these GitHub changes.
