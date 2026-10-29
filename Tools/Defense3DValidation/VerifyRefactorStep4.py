"""Compare against the pre-step-4 snapshot; performs no Unity operations."""
import hashlib
import json
from pathlib import Path
import sys

root = Path(__file__).resolve().parents[2]
snapshot = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(
    (root / 'Temp/defense-refactor-step4-baseline-path.txt').read_text().strip())
manifest = json.loads((snapshot / 'manifest.json').read_text())
assets = 0
metas = 0
for entry in manifest:
    relative = Path(entry['path'])
    current = root / relative
    assert current.is_file(), f'Missing baseline file: {relative}'
    if relative.suffix == '.meta' or str(relative).replace('\\', '/').startswith(
            ('Assets/Resources/Defense3D/', 'Assets/Scenes/Defense3D/')):
        assert hashlib.sha256(current.read_bytes()).hexdigest() == entry['sha256'], f'Changed asset/meta: {relative}'
        if relative.suffix == '.meta':
            metas += 1
        else:
            assets += 1

folder = root / 'Assets/Features/Defense3D'
for name in ('DefenseHeroMotor3D', 'DefenseTowerBuildPrototype3D'):
    source = (folder / (name + '.cs')).read_text()
    assert 'InputSystem' not in source and 'Keyboard.current' not in source and 'Gamepad.current' not in source
    assert 'IDefenseHeroCommands' in source
attack = (folder / 'DefenseHeroAutoAttack3D.cs').read_text()
assert 'nextAttackAt' not in attack and '_ability.TryActivate' in attack
composition = (folder / 'DefenseSceneCompositionRoot.cs').read_text()
assert '_session.Own(_heroCommands)' in composition
assert '_heroHealth.Defeated += ClearHeroCommands' in composition
for name in ('DefenseHeroCommands', 'DefenseAbilityRuntime'):
    assert 'UnityEngine' not in (folder / (name + '.cs')).read_text()
for name in ('DefenseHeroCommands', 'DefenseAbilityRuntime', 'DefenseHeroDeviceInput'):
    assert (folder / (name + '.cs.meta')).is_file()
    assert 'new GameObject' not in (folder / (name + '.cs')).read_text()
assert 'new GameObject' not in composition and 'AddComponent' not in composition

for name, expected in [('DefenseSessionChecks', 23), ('DefenseCombatChecks', 35), ('DefenseAbilityInputChecks', 27)]:
    log = (root / f'Logs/TraversalQA/defense-refactor-step4-{name}.txt').read_text(encoding='utf-8-sig')
    assert f'TOTAL {expected} PASS' in log, f'Missing successful check summary: {name}'

report = '\n'.join([
    f'PASS {assets} baseline scene/resource files unchanged byte-for-byte',
    f'PASS {metas} existing metadata files unchanged byte-for-byte',
    'PASS motor/builder consume injected commands; device access isolated in adapter',
    'PASS auto attack delegates cooldown to common ability runtime',
    'PASS session owns command channel; hero defeat clears commands',
    'PASS pure cores have no UnityEngine dependency; new sources have metadata',
    'PASS no runtime GameObject/component creation in added composition path',
    'PASS 85 standalone checks: lifecycle/spawn 23, combat 35, input/ability 27',
    'NOT_RUN Unity Editor, Unity test runner, physics, NavMesh, touch, rendering, Android',
])
(root / 'Logs/TraversalQA/defense-refactor-step4-static-checks.txt').write_text(report + '\n')
print(report)
