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

## 600s / 34 timed-event Defense run (opt-in prototype)

Install into your **existing saved scene** with imported Vongxuyen GLB and baked NavMesh via `TREBRAVE > Defense 3D > Install 600s Defense Run In Open Scene`. This adds a separate `DefenseRun6003D` controller and **disables** the earlier 3-wave and bridge-spawn test components; it never rebuilds or overwrites your GLB scene. If the menu is missing, attach `DefenseRun6003D` to a new active empty GameObject and manually disable the older test spawners.

- **34 timed spawn events** on game time `0,18,36,...,594` seconds. Default **3 placeholder capsule enemies per event**. The names cycle over `magiap`, `thuyquai`, `huyetdoi`, `quaidu`, `phicuong` but **do not yet implement the seven distinct archetypes' stats, artwork, or skills**.
- **Miniboss** `magiap_boss_MaGiap_ThongLinh` spawns at 108s with prototype 350 HP, otherwise using ordinary NavMesh bridge-walker behavior.
- **Yaksha** `yaksha_boss_ARENA_COMBAT` appears in the eastern arena at 510s with prototype 1200 HP. It stays in `ARENA_COMBAT` for inspector-configurable `yakshaArenaDuration` (default **45 gameplay seconds**), then permanently transitions to `ASSAULT_BASE` and navigates the **middle bridge** using NavMesh. This is a *temporary test duration*, NOT final GDD-approved X. Boss arena attacks/skills are not yet implemented; the boss is stationary while in its arena state.
- **WIN** as soon as Yaksha dies and Base HP is still above zero; later timed events stop immediately. **LOSS** only when Base HP becomes zero. At 600 seconds with Yaksha alive and Base alive, the match goes to **OVERTIME**, without adding new timed spawn events.
- Existing Hero auto-attack, WoodDrop pickups and Tower Builder use the same `DefenseEnemyBridgeWalker3D` enemy instances. When a test enemy dies, normal WoodDrop handling continues. The Base/Run debug logs and small IMGUI timer / HP / wave / state HUD help verify the loop.
- This is not a production-balanced release: Boss arena combat lacks proper attacks, the normal enemies are visual proxies, no actual 7-roster abilities, no on-screen victory overlay or completed run-result save flow. The 600s schedule and outcome logic are first-pass code requiring local Unity PlayMode QA.

Verification: Save scene → pull feature branch → reopen scene → install 600s run → save → Play. Check `600s run started`, `Timed event 1/34`, `Miniboss at 108 seconds`, `Yaksha entered ARENA_COMBAT`, `Yaksha entered ASSAULT_BASE` (45s after spawn), `WIN` or `LOSS`. The 34th event occurs at 594s. Do not choose `Build & Open Map Song Que Prototype` (would risk overwriting edited scene).

## Yaksha arena combat + Hero down / respawn (follow-up prototype)

- The 600-second run now adds a `DefenseYakshaArenaCombat3D` component to the Yaksha spawn at 510s. During `ARENA_COMBAT` it deals a timed 30-damage proximity blast to Hero within 5m every 3s. The danger radius is drawn as a red **Scene View Gizmo**, not yet a player-facing telegraph/VFX; arena-phase Boss does not chase the Hero.
- Hero has `DefenseHeroHealth3D` with 200 test HP, returns to the original Hero start position after a test **5-second** down timer. Hero down **never** causes LOSS, only BaseHP=0 does. This is a placeholder respawn delay, not the finalized level-scaled respawn formula.
- After configurable arena time (default 45s), Yaksha irreversibly enters `ASSAULT_BASE`, reuses the validated NavMesh middle bridge, and **preserves HP already lost in arena**, without teleporting to a bridge. Its radius blast stops in assault phase.
- HUD includes Yaksha HP and Hero HP. WIN/LOSS now shows a simple IMGUI centered result panel. After WIN, the Base rejects subsequent hits from surviving enemies. The future spawn schedule already stops on WIN/LOSS.
- **Important for existing scenes:** If `[TREBRAVE] 600s Defense Run` is already installed, attach **`DefenseHeroHealth3D`** manually to the `Hero_3D_Prototype` object before Play. Or run `TREBRAVE > Defense 3D > Install 600s Defense Run In Open Scene` again: this updated installer adds Hero Health even when the run controller is already present. Keep map GLB and baked NavMesh unchanged.

This implementation is an incremental combat prototype, **not** a production boss: capsule visual, no animation, no VFX attack telegraph, no final audio, no skill priority system, and no final results/save screen. Actual Unity EditMode/PlayMode tests have not been run from cloud.

## Yaksha visible blast + optional 3D prefab visual

The Yaksha arena blast now presents a **red warning ring in Game View** (LineRenderer, no Gizmos needed), then applies proximity damage after a **1.1-second dodge window**. The cycle repeats only during `ARENA_COMBAT`; warning stops on assault, victory, or defeat. The original 5m radius, 30 damage, Hero HP and 5s respawn prototype remain.

To replace the purple placeholder with **your own imported Yaksha visual**, select the existing `[TREBRAVE] 600s Defense Run` component and assign a compatible **GameObject prefab** to `Yaksha Visual Prefab`. The prefab is instantiated as a child of the gameplay object; its Colliders are disabled so it cannot alter your baked NavMesh. The capsule renderer becomes hidden, but its HP, bridge AI and hit detection continue to run. No model, skeletal animation or external asset is bundled with this commit; you must supply an imported character prefab yourself. Leave the field blank to keep the capsule prototype.

Quick test: pull branch, open saved scene, Play until 510s (or temporarily reduce run scheduling in your own test copy), walk Hero toward Yaksha, watch the red ring and move out before blast. Verify `Yaksha warning` and `Yaksha arena blast hit Hero` only if Hero remains inside the blast radius at detonation. Confirm transition at 555s removes warning and keeps Yaksha HP. The cloud edits have **not** been verified in Unity PlayMode.
