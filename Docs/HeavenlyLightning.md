# Thiên Lôi Giáng Thế

Đạo Sĩ's signature is bound dynamically through `DefenseLoadoutRuntimeAdapter` and the scene composition root. `Skill_DaoSi_Signature.asset` now references `FXSet_HeavenlyLightning.asset`, which owns the cast, release and confirmed-hit profiles. A missing/invalid cosmetic style leaves the presentation unbound instead of failing map initialization; the gameplay effect remains usable. Validation checks the shipped reference so a missing visual does not silently ship unnoticed.

## Presentation

- **Cast:** a target-position thunder seal marks the 4.5m area during the existing 0.25-second windup. Short electrical arcs gather around the seal and its central flash. This is world-positioned at the aim point, rather than attached to the caster.
- **Release:** seven branching bolt paths fall from up to 12m above the target. The main core is white with a cyan/indigo fringe. The first flash and two smaller electrical echoes are visual pulses; damage still resolves once. A ground shockwave and radial fractures fade through the 0.8-second aftermath.
- **Hit:** a small arc/flash is emitted only for confirmed accepted damage, capped at 12 targets per cast. The camera receives one bounded impulse at release, independent of target count.

`DefenseHeavenlyLightningVisual3D` caches its authored line points and property blocks before use, reconstructs positions from those original points and resets every managed renderer on return. Ground is XZ and bolt height is Y. Shader animation uses the effect service's session progress, not `_Time`; pause/interruption clears this signature's owned handles. Ground effects retain ordinary depth testing. The flash uses the existing camera-facing glow shader, with a small surface offset rather than `ZTest Always`.

The common `DefenseSignatureFeedbackAdapter` gains an optional `castAtTarget` argument. Its default remains caster-anchored for Thư Sinh; the lightning presenter opts into the targeted path. The presenter handles source events, death/dash/pause interruption and camera reset. The original direct `Instantiate`/`Destroy` lightning fallback is removed. The old serialized `LightningVfxPrefab` property remains for compatibility and is no longer the production rendering path.

## Gameplay and lifetime

The authored damage 95, radius 4.5m, range 10m, cooldown 18 seconds and windup 0.25 seconds are retained. Damage continues to scale from hero attack. Stun remains 1.5 seconds and the concurrent slow remains 40% for one second. Windup now resolves against the session clock, matching its feedback and allowing deterministic tests. Pause, death, disable, cleanup and restart cancel pending windup. Cancelling during a resolving callback still emits one cancellation event.

Area targets and enemy actor leases are captured before damage callbacks. A recycled enemy instance cannot receive pending damage or status from an old lease. VFX rejection does not suppress damage. The shared effect service owns the pools and active budget: two instances per phase are prewarmed, with normal service growth/rejection rules afterwards. This is not an allocation-free-system or Android frame-rate claim.

## Tools and evidence

- **Build Heavenly Lightning** / `build-thunder`: updates only thunder assets, wires the skill definition and renders `scratch/ThunderPreview.png`. No open scene is saved. The top row shows cast, strike and aftermath with ground; the bottom row isolates the air/flash layers.
- **Validate Heavenly Lightning** / `validate-thunder`: uses Unity Test Runner for the lightning and shared signature fixtures, including the existing `LogAssert` observer-failure test. Report: `scratch/ThunderValidation.txt/.xml`.
- **Run Heavenly Lightning Smoke** / `smoke-thunder`: launches a real Đạo Sĩ match, casts from the actual signature button, checks AoE/stun/slow, interrupts windup with pause/death and checks restart cleanup and return to hub. Reports: `scratch/ThunderSmoke.txt`; captures: `scratch/ThunderWindup.png`, `scratch/ThunderCombat.png`.
- `validate-towers` includes the lightning authoring/gameplay and common adapter regressions alongside the existing app/tower tests. The Scholar fixture requiring Unity's log scope is run by `validate-thunder`, not the lightweight NUnit helper.

`DefenseTestAbilitySource` moved from `Tests/Editor` to `Tests/RuntimeSupport` with its GUID retained and an `UNITY_EDITOR` guard. This allows Unity to attach the helper during the existing Scholar gameplay tests without including the helper in a player build.

The preview uses authored VFX and an enemy model. The Play capture is required to judge timing and scale against the actual scene/hero. Device rendering cost, overdraw and audio design have not been measured or expanded by this visual update.
