# Soul pickup presentation

`Assets/Resources/Defense3D/DefenseWoodDrop3D.prefab` now renders a small jade soul with a round head, two quiet eyes, a tapered tail and a soft halo. A camera-facing quad keeps the silhouette readable from the gameplay camera. It gently floats and breathes using the pickup's session clock, so pause freezes both the visual and magnet movement.

The resource remains Wood: value, pickup delay/radius, magnet, lifetime and minion gathering rules are unchanged. The existing `DefenseWoodDropPool` owns lifetime; no second VFX pool or despawn timer is added. `DefenseSoulPickupVisual3D` receives initialize/reset hooks from the pickup and clears its property block on return/disable. Its single renderer uses the existing shared material asset, an authored quad and a procedural alpha-blended URP shader. There are no particles or per-drop realtime lights, and the soft halo does not require Bloom.

The prefab and material GUIDs are retained. The billboard uses uniform scale 0.65 (35% smaller than the first version), with a brighter premultiplied body and additive jade halo in the same pass. Bobbing only moves rendered vertices; the pickup root remains controlled by the existing magnet.

```powershell
./Tools/Invoke-UnityBridge.ps1 -Command build-soul-pickup
./Tools/Invoke-UnityBridge.ps1 -Command validate-soul-pickup
./Tools/Invoke-UnityBridge.ps1 -Command smoke-soul-pickup
```

The native test subset covers pool lease/subscription cleanup, visible prefab spawning, reuse and clearing/reinitializing visual shader state. The Play check spawns through the production drop pool in a real match and verifies delay, exact collection value, magnet, pause, distant drops and restart cleanup.

Evidence: `scratch/SoulPickupPreview.png` (isolated prefab), `SoulPickupGameplay.png` (real map), `SoulPickupValidation.txt/.xml`, and `SoulPickupSmoke.txt`. Device performance has not been measured.
