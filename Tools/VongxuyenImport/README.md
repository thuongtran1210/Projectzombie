# Vongxuyen Scholar import

The committed `Assets/Art/Characters/ThuSinh/ThuSinh_Vongxuyen.glb` contains the visible Scholar hierarchy, colors, baked outlines and six transform animations from Vongxuyen. Source hashes and export dimensions are recorded next to it in `.source.json`. The source repository is read-only during export.

In Unity, exit Play Mode and run **TREBRAVE > Defense 3D > Import Vongxuyen Scholar**. This creates materials, six animation assets, an Animator Controller and a reusable visual prefab, then attaches it to the Defense3D scene. The capsule renderer is disabled; physics, damage, controls and cooldowns remain on the existing hero. Save any scene changes when prompted.

The exporter needs Three.js 0.160.0 `three.mjs`, `GLTFExporter.mjs` and `TextureUtils.mjs` in a temporary dependency directory:

```powershell
node Tools/VongxuyenImport/ExportScholar.mjs G:/ben-que-survival Temp/ScholarExport Assets/Art/Characters/ThuSinh/ThuSinh_Vongxuyen.glb
```

Three.js `onBeforeCompile` lighting cannot travel through glTF. Outline extrusion is baked into geometry; the generated Unity materials use the included four-tone shader, with an `SRPDefaultUnlit` pass compatible with the project's URP 2D renderer. Web-only combat VFX are excluded so the existing Defense indicators retain ownership. This is a mesh hierarchy with transform animation, not a Humanoid skeleton.

Rendering, animation feel and device performance have not been accepted through manual QA.

## Enemy visuals

`ExportEnemies.mjs` takes the same source/dependency directories and an output directory. It exports `magiap`, `thuyquai`, `huyetdoi`, `quaidu`, `phicuong` and `magiap_boss` from the original `EnemyFactory`, excluding web health bars, transparent auras and lights. Each GLB has source hashes and bounds in its manifest.

Run **TREBRAVE > Defense 3D > Import Vongxuyen Enemy Visuals** outside Play Mode to generate reusable visual prefabs and attach them to the existing six enemy prefabs. It preserves their root scales, colliders and gameplay components. Visuals are centered on the existing hitboxes; airborne source height offsets are compensated so Unity navigation remains the altitude authority.

Sizing uses an enabled root capsule, sphere or box collider when available. Prefabs with logical targeting and no physical collider use their original root mesh bounds, then NavMeshAgent dimensions as a fallback. Import never adds a collider.

Body hopping, swaying and wing flapping use the source `WaveManager` frequencies on the session gameplay clock. Motion affects only imported child transforms and resets on pool reuse. Ma Giap Thong Linh has no generic locomotion pose in that source and retains its neutral visual hierarchy. Yaksha, towers and allied minions are outside this import step. No GUI automation or Play Mode QA is performed by this importer.

## Yaksha

Pass `yaksha_boss` as the fourth exporter argument to export `YakshaBossMesh.js` separately. Run **TREBRAVE > Defense 3D > Import Vongxuyen Yaksha Visual** to attach it to `DefenseYaksha3D.prefab`. This preserves the existing warning ring, FSM, phase timings, health, damage and root transform.

Yaksha's body, horns, bronze armor and Guan Dao use the source mesh hierarchy. The visual child raises the weapon and lifts during the existing Slam warning, using read-only warning progress; source walking bob/sway is shown during AssaultBase. The actor root and hit location do not follow the animated body. Web auras/projectiles, source AI, new attack types and destruction animations are not imported. Materials use the shared Unity toon shader; web GLSL lighting is not reproduced exactly. Visual QA remains unperformed.

## Village shop and friendly units

Export `danbinh,phutapdich,thaylang` to `Assets/Art/Minions/Vongxuyen` using the fourth exporter argument. Run **TREBRAVE > Defense 3D > Install Vongxuyen Village Shop** outside Play Mode. The installer creates three editable definition assets, source visual prefabs, a reusable friendly actor prefab per role, and 12 inactive scene slots per role (36 total). Rerunning replaces only its own shop root and reuses existing tower/support bindings. This fixed capacity is an explicit Unity pooling limit, shown in the UI; the web recruitment method has no equivalent hard cap. Add authored slots and update the shop/roster arrays to raise it.

The shop panel uses TextMeshPro, safe-area anchors and a shared wood wallet. Recruitment costs are Dân Binh 40, Phu Tạp Dịch 30, Thầy Lang 35. Source HP/speed are respectively 260/1.85, 115/2.95 and 135/2.25. The initial wallet is set to the source DefenseRules default of 100 wood. Transactions are blocked when the game gate is closed, the hero is down or the base is destroyed; failed NavMesh deployment consumes no wood. All roles are registered as support damage targets for the existing enemy AI. Units are reused without runtime Instantiate/Destroy.

Each role has Defend/Follow/Advance commands. Militia use the existing session target index, require a complete NavMesh path and a direct NavMesh attack line, attack grounded enemies, slow for 2.2s at 50%, and stun for 1.25s on every fourth hit. The fourth hit deals 14 damage instead of 9. Knockback uses the existing enemy status adapter (0.325 normalized strength against its default 2m envelope). Yaksha is excluded from militia targeting. Gatherers collect physical wood up to 60 per trip, deliver it to the living hero and drop undelivered cargo when killed. Healers restore 28 hero HP or 45 unit HP every 2.6s without reviving dead actors. Injured structures receive 25 tower HP or 15 base HP every 0.95s; gatherers perform these repairs when they have no cargo or available wood target. Frontline healers follow the most advanced living militia. Units and their pathing freeze with the gameplay gate and reset on cleanup/restart.

Tower construction uses the nearest authored socket, costs 50 wood, and cannot redirect an occupied nearest socket to a different pedestal. Existing towers receive support health (250), can be destroyed, and can be upgraded to level 3 for 40 then 80 wood. Each level adds 100 max/current HP, 10 base damage and 1m base range; fire interval is 1.2 / (1 + 0.3 * (level - 1)). The initial range/damage become the source values 7/18. Global run upgrades continue to apply on top. Shop changes do not overwrite map layout, waves, hero stats or boss phase rules.

This ports the core shop and role loop, not every web subsystem: three separate militia commander squads, draggable rally flags, cone piercing/bridge-specific combo tuning, breakable jars, physical EXP cargo, and tower arrow VFX remain outside this slice. Unity's existing EXP/reward ownership remains unchanged to avoid awarding XP twice. The installer does not enter Play Mode or run GUI QA. Device movement, combat feel, visual layout and performance remain unverified.

### Proximity interactions

The fixed global shop toggle and tower buttons inside the recruitment panel are replaced by one contextual button projected over the actual interaction point. The hut at source X/Z (-5.4, -2.2) is inclusive within 1.6m; tower pedestals require strictly less than 1.7m. Detection uses XZ distances. Hut recruitment takes priority in overlapping zones. Leaving the hut closes its panel and recruitment is rejected by gameplay code outside the zone; leaving all zones, pausing or hero defeat hides the context button. Unaffordable tower actions remain visible but disabled with the shortage, and level-three towers display their maximum level. Keyboard B routes through the same current context. Pointer clicks are revalidated to reject a stale pedestal/action after moving or building.

Use **TREBRAVE > Defense 3D > Apply Source Proximity Interactions** to patch existing shop UI and author eleven ground zone rings without rebuilding units or changing balances. The full shop installer also applies this configuration. The recruitment marker remains scene-editable. No GUI or Play Mode acceptance is implied by this patch.

## Basic attack and visible ground

**TREBRAVE > Defense 3D > Apply Source Basic Attack and Rebake Indicator Ground** applies source press-to-attack / hold-to-repeat controls without a drag/cancel aiming pad for basic attacks. It sets the hit delay to 0.18s, repeat interval to 0.25s and animation duration to 0.85s. Basic auto-facing uses max(radius * 1.8, 4.5) and the source distance/facing score, without extending damage range. Facing locks during windup, movement slows by the source 2.1/3.8 ratio during recovery, and the actor's 0.85s animation plays at normal speed rather than being compressed into the repeat interval. Holding restarts it when the next attack begins. Dash/skill/relic aim controls keep their existing adapters.

The broad idle attack fan is removed. Idle shows the source-sized mint foot ring/arrow; a short-lived annular slash appears at waist height after the hit, with the source 0.65–1.4m annulus and 234-degree shape. The damage cone uses the exact source dot threshold -0.26 (210.14012 degrees). Basic hits apply scoped knockback through the existing 3D status adapter. The Unity authored damage and 3D airborne-height restrictions remain unchanged; this is a controls/presentation correction, not a random-damage or air-targeting rebalance.

Ground placement now combines collider hits with world-space triangles baked from visible banks, slopes, paths, courtyard and bridge decks. Terrain props, roofs, actors and decorative grass clusters are excluded; gameplay colliders are neither added nor modified. Height is interpolated inside each triangle and sampled over the indicator footprint, rather than assuming Y=0 or taking an arbitrary mesh bounding-box top. Re-run the menu command after moving/replacing terrain art to refresh the serialized surface data. No Play Mode, native pointer interaction, shader rendering or visual acceptance is claimed by standalone compilation/math checks.
