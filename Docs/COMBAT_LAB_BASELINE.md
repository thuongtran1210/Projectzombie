# Combat baseline and isolated measurement lab

Status: source audit and measurement foundation implemented; no balance changes. Live capture has not yet been validated in Unity Play Mode. The lab is not a completed controlled dummy arena.

## Current Defense3D attack

- `Skill_BasicAttack.asset`: damage 35, cooldown .25 s, windup .18 s, radius/range 2.05. These are asset values, not guaranteed effective runtime values.
- `DefenseHeroAutoAttack3D` receives the configured definition; damage is baseline multiplied by (1 + HeroAttackDamage modifier). Interval is baseline divided by (1 + HeroAttackSpeed modifier), through `DefenseRunStats`.
- Manual casting shares the attack ability cooldown. Successful prepare commits cooldown before queueing the attack. The optional auto-attack path also commits cooldown before queueing.
- Only one pending windup is allowed. With uninterrupted requests, the nominal single-target ceiling is damage / max(interval, windup), excluding frame scheduling and input gaps. Baseline is 140 requested damage/s. At +100% attack speed the interval is .125 s but windup is still .18 s: the ideal ceiling is ~194.44, not 280.
- Recovery is .85 s from attack start. It slows movement by 2.1/3.8 but does not gate CanPrepare. Visual action also uses .85 s. This is a movement/visual window, not the damage cycle.
- Resolve uses the hero position at release and direction locked at start, with a cone of ~210.14 degrees. Every eligible enemy within the query buffer (256) may be hit once per resolve. There is no separate small target cap per swing in this path.
- Normal attack supplies EffectiveDamage without setting critical. HealthCore subtracts Amount directly and clamps HP at zero. No armor or critical-roll calculation exists in these two parts of the inspected path; do not import formulas from the separate legacy combat system.
- Confirmed-hit feedback carries requested damage, not HP lost. A 35-damage killing hit on 10 HP removes 10 HP with 25 overkill.
- Dash, death and closed gameplay gate cancel pending attacks. Cooldown is not refunded in CancelPendingAttack. Manual attack preparation resolves auto-aim again rather than using the requested aim verbatim.

## Findings to measure before changing behavior

1. Attack-speed gains become windup-limited; upgrades can show diminishing realized benefit.
2. Long movement recovery can stay active during repeated attacks even though attacks occur every ~.25 s.
3. Broad multi-target coverage plus repeated knockback can dominate balance beyond nominal single-target DPS.
4. Animation timing and repeated hit cadence need a live comparison; source inspection alone cannot establish how they feel.
5. Compare continuous attack, movement, target switching and dash cancellation at fixed upgrade values.

## Isolation

`Assets/Features/CombatLab/Runtime` is a pure C# assembly with no Unity references and autoReferenced disabled. The Editor assembly references only this core; Defense3D is accessed using an opt-in reflection adapter because an asmdef cannot directly reference the predefined Assembly-CSharp. No production assembly or scene references the lab. All subscriptions are removed on Stop, window close, Play Mode exit and assembly reload. No runtime component is added to a hero or enemy.

The Editor adapter allocates through reflection and is a diagnostic tool, not an Android performance benchmark. No automatic per-frame scene discovery is used: targets are discovered once per capture.

## Use

1. Unity menu `Tools > Combat Lab > Create Isolated Scene Copy` copies the prototype scene into this module without opening it or modifying the source. Existing gameplay assets remain shared: do not edit referenced definitions to tune a baseline copy. Duplicate any definitions before experimenting.
2. Open the copied scene manually. Enter Play Mode and wait until the hero is bound and enemies exist.
3. Open `Tools > Combat Lab > Open Measurements`; drag the hero DefenseHeroAutoAttack3D component into Hero Attack.
4. Record conditions (movement, equipment, upgrades, enemy type), then Start capture. Keep upgrades constant. Stop after a fixed gameplay-time window and export the report.
5. The cohort contains only enemies active at Start belonging to the same gameplay context. Spawned-later enemies are excluded. Reuse of a tracked pooled enemy stops capture. Run changes stop capture. This is a fixed-cohort probe, not whole-wave DPS.
6. Capture counts all hero attack starts, but accepted hits and HP damage only for tracked enemies whose damage Source equals the chosen hero Attack component. Tower, minion and relic damage are excluded. Other sources still update cached HP so their damage is not attributed to Attack.
7. HP damage uses the existing Damaged event after HP subtraction and cached pre-hit HP maintained by HealthChanged/Damaged. This assumes synchronous, non-reentrant health observers; a future nested health mutation needs a richer production damage-result event.
8. The denominator is the explicit Start-to-Stop session gameplay window, including movement and idle time; session pause is excluded. One isolated enemy is required for single-target DPS; multiple tracked enemies produce group DPS. TTK, mitigation percentages and hit-rate are not inferred by this version.

## Verification and next milestone

Core tests cover overkill, idle denominator, group damage, mitigation separation, windup ceiling, attack interval and invalid values. Compile core and Editor independently against Unity 2022.3 references. A real Play Mode pass remains required to validate reflection binding, subscriptions and HP attribution.

Next: controlled stationary/moving/group fixtures with stable loadout and reproducible spawn conditions; measure baseline, then propose timing and damage changes. Proceed to the agreed balancing phase after validating measurements and record before/after results.

## Applied gameplay update (2026-10-29)

The audit above preserves the pre-change baseline. Attack now retains hold-to-repeat but uses .65 s default interval (~1.54 starts/s, ~53.85 nominal single-target requested DPS at 35 damage before upgrades). Damage, windup and movement recovery are unchanged. The source rule, skill asset, source installer and prototype/copy scene fallback are aligned.

Relic is now throw -> flying -> anchored. It queries current targets and applies the existing 180 damage once at flight completion (definition Duration, currently .2 s). A visible slipper sprite follows the arc and remains at the anchor. The same skill button shows LAO TOI (Vietnamese label in game) for the second press during a 4 s anchor window. Recast uses the existing safe dash traversal and a .3 s dash; blocked routes may stop short. Failed traversal keeps the anchor available until expiry. It adds no second damage hit and does not restart the 12 s cooldown committed on throw. The standalone Dash skill remains separate. Death, disable, cleanup and reset discard the relic state; session pauses suspend gameplay time.

Verification: six pure state-machine tests passed (flight gate, one-use recast, exact expiry, late tick, reset, invalid timings). Runtime/Editor compilation and source parity checks are recorded separately; actual visual/input/physics Play Mode validation remains required.
