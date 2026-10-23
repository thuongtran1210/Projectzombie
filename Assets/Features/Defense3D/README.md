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
