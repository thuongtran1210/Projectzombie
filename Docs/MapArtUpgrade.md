# Song Que river-village art pass

This first environment upgrade establishes the central bridge/tea-house/landing area as a visual reference, cleans the existing map geometry and adds an ancestral shrine to replace the main base's prototype cube. It is not a claim that the entire environment is production-ready.

## Installed content

- Central bridge: continuous wood planks, fascia, handrails, braces, capped posts and lanterns. The visual deck follows the existing 2.8m-wide gameplay floor at Z=-0.5, instead of the old stretched GLB bridge.
- Tea house: thick terracotta roof and tile courses, ridge, timber frame, plaster walls, counter, cups, ceramic storage and bench.
- River landing: bank-side boardwalk, moored sampan, oar, rope, reeds, silt, stones and static water glints. The landing remains on the bank; the boat is scenery in the water.
- Landscape: a stone-and-earth approach to the bridge, broadleaf trees, fenced vegetable garden and background tree groups.
- Base: an ancestral shrine with stone foundation, timber columns, double doors and bronze roof/plaque details. The original base actor, health and collider are retained; only its cube renderer is disabled.

The original GLB visual instance is retained inactive for comparison. Its replacement prefab contains cleaned static geometry plus the new models. Broken outline geometry, the old central bridge/tea-house, scattered tiny grass meshes and large flat water marks are excluded. Existing source colors are consolidated into shared URP Lit materials; static meshes are combined by material and 12m spatial cell.

The final generated visual has 268 chunk renderers and 56 active shared materials, compared with 1,641 renderers in the source instance. These are authoring counts, not measured draw calls or total loaded material memory. The retained inactive GLB still references its original resources. Removing that reference belongs to the final production cleanup once the replacement is accepted.

## Renderer correction

Gameplay previously inherited the project's URP 2D Renderer. A Universal Forward renderer is now appended to the existing pipeline asset and selected explicitly by the map camera. The pipeline's default renderer stays at index 0 for the menu/legacy 2D content. Directional lighting and trilight ambient colors provide 3D form and contact shadows for the environment.

No collider, collision layer, hero movement setting, gameplay marker, tower socket or baked NavMesh is changed by the art builder. New scenery contains no colliders. A later collision-authoring pass should be coordinated with any additional playable terrain or obstacle changes.

## Rebuild and review

```powershell
./Tools/Invoke-UnityBridge.ps1 -Command audit-map-art
./Tools/Invoke-UnityBridge.ps1 -Command build-map-art
./Tools/Invoke-UnityBridge.ps1 -Command preview-map-art
./Tools/Invoke-UnityBridge.ps1 -Command smoke-map-collision
./Tools/Invoke-UnityBridge.ps1 -Command validate-map-collision
./Tools/Invoke-UnityBridge.ps1 -Command validate-towers
./Tools/Invoke-UnityBridge.ps1 -Command smoke-thunder
```

Builders require Edit Mode and clean saved scenes, save only the target map and generated assets, and restore the previous scene setup. The first build may exceed the bridge's 45-second client timeout while importing assets; inspect the response/report before resubmitting.

Evidence: `scratch/MapArtBefore.png`, `MapArtAfter.png`, `MapArtOverview.png` and the real Play Mode capture `MapArtGameplay.png`. Build counts are in `scratch/MapArtBuild.txt`. The collision smoke checks both river banks, four map edges, all three bridge crossings, bridge sides, forced fall recovery, the gameplay Forward renderer and absence of scenery colliders. The lightning smoke checks signature gameplay and captures rendering after the renderer change.

## Remaining production work

The outer map, north/south bridges, palms, enemy territory and tower placement markers still include prototype art. The ground remains largely flat-color geometry, and the new water glints are static. Further work should establish richer ground blending, animated water, more organic bank silhouettes, coherent peripheral architecture and foliage variation. Mobile GPU time, shadow cost, overdraw and memory need measurement on target devices before setting final budgets.
