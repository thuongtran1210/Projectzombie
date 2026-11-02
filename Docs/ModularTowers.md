# Modular sockets, towers and projectiles

## Player flow

Select tower types in the Codex/loadout before entering a match. Existing saves retain `tower_cungtien` as their archer ID. Nỏ Thần, Băng Hà and Hỏa Long are available baseline choices. The build menu follows the selected loadout.

Approach an empty socket and use the context button or B. The menu shows only types allowed by both the loadout and socket, with level-one price and stats. Choosing a tower rechecks proximity, occupancy, gameplay state, hero life and wallet balance. Leaving the socket or pausing closes the menu. The village hut interaction retains priority in its recruitment zone.

An occupied socket offers upgrades according to that tower's actual maximum level. Defeat disables the tower and clears its socket. Restart clears every socket, recalls in-flight projectiles and closes the menu; it retains the original match loadout and prewarmed inventory.

## Ownership and extension points

| Component | Responsibility |
| --- | --- |
| DefenseTowerSocket3D | Stable authored socket ID, optional type restrictions, current tower |
| DefenseTowerCatalogSO | Authoritative tower definitions and actor prefabs for the map |
| SO_TowerData | Identity, targeting, tier stats, health/models, normal/special attack profiles |
| DefenseTowerBuildPrototype3D | Proximity and validated build/upgrade commands; existing class/GUID retained for compatibility |
| DefenseTowerInventory3D | Bounded inactive actor inventory, prepared before combat |
| DefenseTowerAttackProfileSO | Projectile definition, effects, shot count, damage multiplier, unlock level and special cooldown |
| DefenseTowerPrototype3D | Per-instance targeting/cooldowns/level; never writes runtime state into shared profiles |
| IDefenseProjectileService | Attack capability boundary; tower does not choose between concrete simulation implementations |
| DefenseProjectilePool3D / ProjectileCore3D | Main production projectile path, prefab reuse, trajectory, hit handling, session/lifetime validation |
| DefenseTowerBuildView / Presenter | Passive formatted view and event-driven choice presentation |

`TryBuild(socketId, towerId)` uses stable IDs; the index overload is a scene/UI adapter. The old one-preplaced-tower-per-socket array is removed. Scene markers no longer require exactly ten sockets. Sông Quê still has its ten authored positions.

`DefenseTowerSocketCore`, `DefenseProjectileSystem3D` and `DefenseTowerProjectileVfx3D` remain legacy/test utilities; the new production tower flow does not use them. The old LineRenderer simulation object is removed from the migrated map. Other legacy editor generators are not automatically run by the new installer.

## Loading and pooling

The scene composition applies the loadout before preparing towers. It awaits inventory preparation, adding at most one actor per yield. Only selected types receive actor inventory; each type can cover every authored socket, allowing any distribution of the selected types. Projectile types are prewarmed before the session starts, including a separate special-attack prefab if configured. Friendly targeting receives the inactive tower health endpoints before the spawn runtime is initialized.

The application loading cover remains visible until preparation and session initialization finish. Normal build/attack paths never instantiate towers or grow projectile pools. Default projectile budget is 32 per prepared prefab; a full pool rejects the shot, retains its attack cooldown opportunity, and never turns it into instant damage. Burst attacks check their whole slot requirement before launch.

Tower catalog prefab references remain map-owned asset dependencies. Preparing fewer instances is not the same as proving that unselected prefab assets are absent from memory. Asset-heavy future catalogs should be profiled and can migrate their heavyweight references to scoped Addressables leases; Android memory/FPS has not been measured here.

## Projectile rules

- A projectile freezes when its DefenseSession gate closes.
- Generation/session mismatches invalidate the shot.
- Homing shots retain the enemy's original actor lifetime, so a recycled instance cannot receive damage intended for its previous lease.
- Damage, speed and arc are captured at launch. Upgrading a tower does not retroactively strengthen in-flight shots.
- Each effect owns its trigger probability. There is no second probability roll in the projectile.
- Single-target and area-impact paths are available. Area hits use the session enemy index with a bounded snapshot, avoiding friendly colliders and repeated hits from multiple colliders. Saturation rejects the area query.
- Piercing requires a real swept collision/visited-target implementation. Catalog validation rejects the old placeholder piercing payload rather than presenting it as complete.

Special attacks execute automatically when a target is valid, the configured unlock level is reached and their own cooldown expires. Normal and special cooldowns belong to each tower instance. Nỏ Thần uses arrows; Băng Hà now uses instant laser pulses with slow. No additional special skill is enabled by default.

## Tower ordnance — 2026-11-02

| Tower | Projectile | Gameplay effect | Presentation |
| --- | --- | --- | --- |
| Nỏ Thần | Homing arrow | Single target, original tier damage/rate | Gold arrow, short trail, launch flash and hit sparks |
| Băng Hà | Instant ice laser | Level 1/2/3 fires 1/2/3 beams; distinct targets preferred, spare beams focus primary; 30% slow for 2 seconds once per target | White-hot core, cyan halo, travelling energy bands and camera-facing emitter/impact flares |
| Hỏa Long | Ballistic fireball | Captured aim point; 2m planar explosion; existing 1.25 attack multiplier; burn at 20% of impact damage per second for 3 seconds | Flame tail, blast flames/ring, attached flames while burning |

Burn uses one slot per enemy, strongest live DPS with duration refreshed rather than additive stacks. Damage is sampled every 0.5 session seconds, settling partial time on refresh and the final partial interval on expiry. Pause freezes its clock; a delayed update cannot deal more than the remaining duration's damage. Initialization, disable, death and restart clear burn and attached presentation. A damage callback that reinitializes an actor cannot attach a pending burn refresh to the new actor state. Ground shells do not track targets after launch; moving away can avoid the explosion.

Projectile definitions reference optional launch, impact and status `DefenseEffectProfileSO` assets. Composition binds the existing session effect service after the tower loadout/inventory is prepared. Single-target feedback requires an accepted hit; area feedback occurs once at detonation. Cosmetic rejection does not reject projectile damage. Burn presentation is sustained and stopped by the status owner, not by a separate timer/coroutine. Runtime geometry and materials are baked by the Editor builder, with property blocks for instance color.

Run **Tools > ProjectZombie > Towers > Build Tower Ordnance** (bridge `build-tower-ordnance`) to update only these assets and render `scratch/TowerOrdnancePreview.png`. It does not save an open gameplay scene. The preview columns are arrow, ice and fire; rows are projectile closeups, early impact and aftermath beside the actual enemy prefab. `scratch/TowerFireCombat.png` is the Play Mode integration capture.

The tower/projectile inventory remains bounded. Presentation uses the existing effect service: up to 32 active effects by default, two prewarmed instances per profile, with priority rejection/replacement and lazy pool expansion. This is not a claim of allocation-free VFX or mobile GPU performance. Audio clips are not added by this builder; legacy projectile SFX fields are not executed by the production core.

The current towers have nine authored low-poly tier models, baked by **Build Structure Models and Health**. Hỏa Long owns a separate actor prefab. Tier models are instantiated during preparation and only activated/deactivated during upgrades; the active turret aims at its target and provides its own muzzle position. Friendly towers and Ghost Cannons use explicitly bound billboard health views. See `Docs/StructurePresentation.md` for ownership, rebuild commands and validation. The older architecture installer only supplies fallback blockouts when a prefab has no renderer.

## Adding a tower

1. Create SO_TowerData with a unique stable ID and valid tier stats. Set its normal attack and optional special attack.
2. Create the attack profile and projectile definition. The definition's VisualPrefab is a full pooled projectile prefab containing ProjectileCore3D/ProjectileVisual3D. Its trajectory/speed/arc/lifetime configure movement; effects are separate assets.
3. Create an actor prefab with DefenseTowerPrototype3D, DefenseSupportTarget3D, collider, visual and FirePoint. Add data/prefab to TowerCatalog.
4. Run **Tools > ProjectZombie > Towers > Sync Loadout Metadata**. This generates a lightweight string-only projection used by the sảnh; new IDs do not require editing code constants. New content can be owned via `MetaProgressionSaveData.unlockedTowerIds`; baseline types remain available.
5. If the number of choices changed, run **Install Modular Towers** to resize the generated map menu and update scene wiring. It preserves additional catalog entries. Review the generated UI if using a custom layout.
6. Select the new type in the loadout and validate its build, upgrade, projectile and reset behavior. Socket restrictions can narrow allowed IDs without choosing a fixed prefab.

Adding another existing trajectory/effect combination is data work. Laser pulses have a separate tested attack strategy. New behavior such as chain lightning, swept piercing or persistent hazards needs a new execution strategy; it is not supplied merely by adding an enum value.

## Validation

### Runtime hardening — 2026-11-02

The current production path supports Nỏ Thần and Băng Hà through map-owned data, bounded inventories and socket commands. The legacy socket core is not the production authority. Existing normal/special profiles already provide extension points; adding a new combat mechanic still requires an execution strategy.

The controller now preserves an injected projectile service across gameplay binding, including services that are not MonoBehaviours. Upgrade prices use the authored value exactly, including zero; negative prices reject the action. A per-instance guard prevents wallet change callbacks from recursively buying additional levels during the same upgrade. Normal and special launches share checks for living towers and living, in-range targets, and safely reject destroyed Unity actors retained through an interface. Restart resets the special cooldown with the new run's clock.

Regression coverage includes zero/paid upgrade prices, callback reentry, destroyed targets, injected services across rebinding, and special cooldown recovery after restart. The Play Mode smoke additionally exercises the authored build UI, both tower types, destruction/rebuild, projectile pause/slow, pool exhaustion, restart and return to hub.

Remaining content/release work: replace the documented blockout models, author any desired additional tower/special profiles, and measure representative Android builds. These are not established by the Editor regression suite.

- **Install Modular Towers** snapshots the loaded map under `scratch/TowerArchitectureBackup`, preserving current scene edits before migration.
- **Validate Modular Towers** writes `scratch/TowerValidation.txt/.xml`. Tests cover data/ID mapping, shared-profile independent cooldowns, pause, enemy lease reuse, probability, bounded pools and loadout compatibility. A legacy test that regenerates production preparation UI is excluded from this suite; preference tests save/restore the user's migration keys.
- **Run Tower Smoke** starts a temporary Play session with all three tower types, exercises the actual build menu, build/upgrade/destruction/rebuild, slow hit, fire explosion/burn on an actual enemy, pause, projectile exhaustion, restart/status/VFX cleanup and return to hub. It restores the previous Editor Play start-scene setting. Reports: `scratch/TowerSmoke.txt`; Game View captures: `scratch/TowerBuildMenu.png`, `scratch/TowerFireCombat.png`.
- Bridge equivalents: `install-towers`, `validate-towers`, `smoke-towers` through `Tools/Invoke-UnityBridge.ps1`.

These checks establish Editor behavior and bounded object counts, not an Android performance guarantee. Validate the player Addressables build and profile representative tower/projectile loads before release.
