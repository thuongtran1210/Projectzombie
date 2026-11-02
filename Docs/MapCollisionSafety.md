# Map collision safety

The Song Que scene currently uses separate box floors for the two banks and three bridges. The visual GLB is not the collision authority. Walking previously assigned Rigidbody velocity without testing ledges, while dash already checked floor and NavMesh boundaries.

`DefenseHeroGroundSafety3D` checks the world-scaled capsule footprint against non-trigger Ground colliders. Normal movement samples its proposed path in steps of at most 0.1m, blocks unsupported movement, and permits a supported sliding axis along an edge. Water and decorative meshes do not count as ground. The motor enables continuous dynamic collision detection for solid obstacles.

The motor records supported positions and restores the most recent one after 0.25 seconds without support, clearing velocity and cancelling dash. Reset clears the old recovery anchor. No safe position is invented before the first supported sample. This is a safety net for displacement/falls; it does not replace authoring collision meshes that match future terrain and bridge models.

Bridge traversal stays on the existing continuous gameplay floor, independent of decorative plank gaps. The map visual/collider discrepancy, curved bridge deck heights and decorative prop collision still require a separate art/collision authoring pass when those models are upgraded. No GLB or NavMesh was rebuilt by this change.

Validation commands:

```powershell
./Tools/Invoke-UnityBridge.ps1 -Command validate-map-collision
./Tools/Invoke-UnityBridge.ps1 -Command smoke-map-collision
```

The native Edit Mode suite covers footprint edges, sliding, bridge connections and sides, high-speed gap crossing, forced fall recovery, reset and trigger rejection, alongside existing traversal/dash regressions. Reports are `scratch/MapCollisionValidation.txt/.xml`.

The Play Mode smoke launches the real map through AppFlow, drives the real hero motor into both river banks and four outer edges, crosses all three bridges, checks bridge sides and forced fall recovery, and returns to the hub. Its report is `scratch/MapCollisionSmoke.txt`. The smoke preserves the previous Play start scene and refuses dirty scenes.
