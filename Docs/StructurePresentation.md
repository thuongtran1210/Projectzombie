# Tower and enemy launcher presentation

The current asset set includes three model tiers each for Nỏ Thần, Băng Hà and Hỏa Long, plus a siege mortar model for the three authored Ghost Cannon instances. Models use a URP two-tone structure shader with shared materials. Primitive construction is baked into meshes grouped by material and turret ownership; visual meshes have no additional colliders.

Nỏ Thần has a bronze ballista, jade banner and drawn bow strings. Băng Hà has a raised crystal and satellite shards. Hỏa Long has a hollow mortar barrel, bronze muzzle rim and dragon cheeks. Tier two adds guards; tier three adds master banners. The hostile launcher has a wheeled siege carriage and violet war banners. Each allied type has a distinct actor prefab; the pre-existing Hỏa Long catalog entry no longer shares the archer actor.

`DefenseTowerPrototype3D` preloads all tier models with the existing inventory flow. Only the chosen tier is active. It rotates that tier's `Turret` horizontally toward a valid shot target and captures the `Turret/Muzzle` world position at launch. The fallback model is disabled when a tier model is available. Upgrades do not instantiate model objects.

`DefenseStructureHealthPresenter3D` binds an `IDefenseDamageable3D`, the match context and an explicit scene camera. It reuses `DefenseEnemyHealthView3D` and the existing health-bar shader. Health events update the fill through a property block; pose updates billboard toward the camera and cull offscreen/behind-camera structures. Friendly bars are green; hostile launcher bars are red. Bars hide on pause, death, disable and cleanup, and recover when the actor is rebuilt or reset. The presenter retains its subscriptions while the tower is inactive in its inventory and removes them on rebinding/destruction.

The composition root binds bars after tower health endpoints and the cannon battery have initialized. Health bars sit outside the tier model groups so upgrading cannot disable them. Ghost Cannon's `LauncherModel` is a separate death group: destruction hides every body mesh and the hitbox, while the already-launched projectile/parachute remains outside that group and keeps its original flight behavior. Restart restores the model, health and target registration. Authored cannon IDs, landing anchors, shot schedules and landing enemy definitions remain scene-owned.

## Rebuild and review

- **Tools > ProjectZombie > Towers > Build Structure Models and Health** / bridge `build-structure-art`: updates the targeted prefab/data/material/mesh assets and renders `scratch/StructureArtPreview.png`. It does not save an open gameplay scene. Running it repeatedly preserves the asset paths/GUIDs and replaces the generated model groups.
- **Validate Modular Towers** / `validate-towers`: includes structure health damage/upgrade, pause, camera culling, death/inventory reuse, distinct actors/tiers, finite mesh bounds, valid shaders and model/projectile ownership checks.
- **Run Tower Smoke** / `smoke-towers`: builds all three towers, activates tier three, applies real HP damage, destroys a real enemy launcher and verifies restart restoration. It also retains the projectile/burn regressions.

Preview columns: Nỏ Thần, Băng Hà, Hỏa Long and enemy launcher. Rows show tiers one/two/three for allied towers and full/65%/25% health. The launcher has one model, repeated to show those health states. `scratch/StructureCombat.png` and `scratch/EnemyLauncherCombat.png` are captures of the running match through a temporarily reframed orthographic scene camera; the capture restores the original camera projection, transform, render target and size. They are separate from the isolated prefab preview.

The builder uses baked shared geometry, but renderer counts and shared materials do not establish a draw-call count or Android frame rate. Device profiling remains outstanding. The older enemy payload/parachute art is preserved; this update changes the launcher body and muzzle ownership rather than replacing the landing enemy or its flight simulation.
