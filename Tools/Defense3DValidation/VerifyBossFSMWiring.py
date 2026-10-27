"""Offline shared-health/phase wiring audit. Never invokes Unity."""
from pathlib import Path
import json, hashlib, re
ROOT = Path(__file__).resolve().parents[2]
BASE = Path((ROOT / 'Temp/defense-boss-fsm-baseline-path.txt').read_text(encoding='utf-8-sig').strip())
manifest = json.loads((BASE / 'manifest.json').read_text())
protected = 0
for name, expected in manifest.items():
    if name.startswith('Assets/Resources/Defense3D/') or name == 'Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity':
        assert hashlib.sha256((ROOT / name).read_bytes()).hexdigest() == expected, 'Protected asset changed ' + name
        protected += 1
source = ROOT / 'Assets/Features/Defense3D'
encounter = (source / 'DefenseYakshaEncounter3D.cs').read_text()
arena = (source / 'DefenseYakshaArenaCombat3D.cs').read_text()
bridge = (source / 'DefenseEnemyBridgeWalker3D.cs').read_text()
fsm = (source / 'DefenseYakshaBehaviorFSM.cs').read_text()
enemy = (source / 'DefenseEnemy3D.cs').read_text()
assert 'new DefenseYakshaBehaviorFSM(enemy.HealthAuthority, _arenaDuration, cycle)' in encounter
assert 'internal DefenseHealthCore HealthAuthority => Health;' in enemy
assert 'ReferenceEquals(state.Health, _enemy.HealthAuthority)' in arena
assert 'new DefenseHealthCore' not in encounter + arena
for legacy in ['_spawnedAt', 'HasSpawned { get; private set; }', 'AssaultStarted { get; private set; }', 'IsDefeated { get; private set; }']:
    assert legacy not in encounter, 'Duplicate encounter state ' + legacy
for legacy in ['activeArena', 'bool warning', 'nextBlastAt', 'detonateAt', 'private void Update()', 'private void LateUpdate()']:
    assert legacy not in arena, 'Autonomous arena state ' + legacy
assert 'arenaMode' not in bridge
assault = bridge[bridge.index('public void BeginBossAssault'):bridge.index('public void StopBossCombat')]
assert 'SetMaxHealth' not in assault and 'Initialize(' not in assault
assert 'Health.Defeated -= OnBossDefeated' in fsm
assert 'IsStopped = true; ClearTelegraph(); CombatStopped?.Invoke();' in fsm
assert 'DefenseYakshaPhase.AssaultBase' in encounter and 'Boss.BeginBossAssault(_base)' in encounter
assert 'Boss.StopBossCombat()' in encounter and '_state.Dispose()' in encounter
assert '_state.Tick(delta, false)' in encounter
assert 'ReferenceEquals(_enemy.Lifetime, _lifetime)' in arena and 'Gameplay.RunGeneration == _generation' in arena
assert 'ring.enabled = false' in arena and 'WarningActive && CanExecute' in arena
assert (source / 'DefenseMatchRuntime.cs').read_bytes() == (BASE / 'Assets/Features/Defense3D/DefenseMatchRuntime.cs').read_bytes()
for name in ['DefenseWaterNavigation3D', 'DefenseCannonBattery3D']:
    assert not re.search(r'private\s+(?:readonly\s+)?NavMeshPath\s+\w+\s*=\s*new', (source / (name+'.cs')).read_text())
print('PASS Shared actor health injected into FSM; duplicate encounter/arena/bridge phase timers removed')
print('PASS FSM-to-native warning, assault, stop, lease/generation and disposal wiring')
print('PASS All protected resources and scene unchanged:', protected)
print('NOT_RUN Unity lifecycle/defeat callback order, warning rendering, assault NavMesh and full-run result timing')
