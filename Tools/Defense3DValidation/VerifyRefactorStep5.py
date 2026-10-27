"""Static step-5 source/authoring checks. Does not launch or communicate with Unity."""
import hashlib
import json
from pathlib import Path
import re
import sys

root = Path(__file__).resolve().parents[2]
snapshot = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(
    (root / 'Temp/defense-refactor-step5-baseline-path.txt').read_text().strip())
manifest = json.loads((snapshot / 'manifest.json').read_text())
assets = metas = 0
for entry in manifest:
    relative = Path(entry['path'])
    current = root / relative
    assert current.is_file(), f'Missing baseline file: {relative}'
    if relative.suffix == '.meta' or relative.as_posix().startswith(
            ('Assets/Resources/Defense3D/', 'Assets/Scenes/Defense3D/')):
        assert hashlib.sha256(current.read_bytes()).hexdigest() == entry['sha256'], f'Changed baseline asset/meta: {relative}'
        if relative.suffix == '.meta':
            metas += 1
        else:
            assets += 1

folder = root / 'Assets/Features/Defense3D'
pure_paths = [folder / 'DefenseAimResolver.cs', folder / 'DefenseSkillGestureSession.cs',
              root / 'Assets/Features/UI/Controls/SkillPointerGesture.cs']
for source in pure_paths:
    assert 'UnityEngine' not in source.read_text()
legacy = (root / 'Assets/Features/UI/Controls/SmartSkillDragHandler.cs').read_text(encoding='utf-8-sig')
assert 'SkillPointerGesture' in legacy and 'Coroutine' not in legacy and 'WaitForSeconds' not in legacy
for field in ('_dragThreshold', '_maxDragDistance', '_requireHoldOrDrag', '_holdDurationThreshold'):
    assert f'private ' in legacy and field in legacy
for event in ('OnAimStarted', 'OnAimUpdated', 'OnAimReleased', 'OnAimDetailedReleased', 'OnAimCancelled'):
    assert event in legacy
button = (folder / 'UI/DefenseSkillAimButton3D.cs').read_text()
projector = (folder / 'DefenseAimProjector3D.cs').read_text()
view = (folder / 'UI/DefenseAimIndicator3D.cs').read_text()
for source in (button, projector, view):
    for forbidden in ('new GameObject', 'AddComponent', 'new Material', 'Physics2D', 'UICancelSkillZone.Instance', 'Camera.main'):
        assert forbidden not in source, forbidden
assert '_bindingVersion' in button and 'sample.RunGeneration != _gameplay.RunGeneration' in button
assert 'TrySurface(geometry' in projector and 'QueryTriggerInteraction.Ignore' in projector

prefab = (root / 'Assets/Resources/Defense3D/DefenseAimIndicator3D.prefab').read_text()
headers = list(re.finditer(r'^--- !u!(\d+) &(\d+)$', prefab, re.M))
ids = {int(h[2]) for h in headers}
assert len(ids) == len(headers) == 9
for line in prefab.splitlines():
    if 'guid:' not in line:
        for ref in re.findall(r'fileID: (\d+)', line):
            assert int(ref) == 0 or int(ref) in ids, f'Broken local reference: {line}'
blocks = {int(h[2]): prefab[h.end():headers[i + 1].start() if i + 1 < len(headers) else len(prefab)]
          for i, h in enumerate(headers)}
for identifier, count in [(2002, 64), (3002, 2)]:
    block = blocks[identifier]
    positions = block.split('  m_Positions:\n')[1].split('  m_Parameters:')[0]
    assert len(re.findall(r'^  - \{x:', positions, re.M)) == count
    assert '  m_Enabled: 0' in block
assert '  m_UseWorldSpace: 0' in blocks[2002] and '  m_Loop: 1' in blocks[2002]
assert '  m_UseWorldSpace: 1' in blocks[3002] and '  m_Loop: 0' in blocks[3002]
assert '_targetRing: {fileID: 2002}' in blocks[1002] and '_directionLine: {fileID: 3002}' in blocks[1002]
material = (root / 'Assets/Resources/Defense3D/DefenseAimIndicator.mat').read_text()
assert '4b78b114f7497cf4aad8e8f6123060b4' in material and '_BaseColor: {r: 1, g: 1, b: 1, a: 1}' in material

for name, expected in [('DefenseSessionChecks', 23), ('DefenseCombatChecks', 35),
                       ('DefenseAbilityInputChecks', 27), ('DefenseGestureAimChecks', 39)]:
    log = (root / f'Logs/TraversalQA/defense-refactor-step5-{name}.txt').read_text(encoding='utf-8-sig')
    assert f'TOTAL {expected} PASS' in log
for suffix in ('runtime-compile', 'source-compatibility-compile'):
    assert 'PASS ' in (root / f'Logs/TraversalQA/defense-refactor-step5-{suffix}.txt').read_text(encoding='utf-8-sig')

report = '\n'.join([
    f'PASS {assets} baseline scene/resource files and {metas} metadata files unchanged byte-for-byte',
    'PASS shared pointer policy used by both Legacy and Defense adapters; Legacy event/serialized names retained',
    'PASS pure geometry/gesture cores have no UnityEngine dependency',
    'PASS Defense adapter uses explicit camera/ground/cancel bindings; no singleton or runtime object/material creation',
    'PASS authored indicator: 9 unique YAML IDs, valid local references, unit 64-point ring, 2-point direction line',
    'PASS indicator material reuses existing URP shader with vertex-color support',
    'PASS 124 standalone core checks and runtime/editor/fixture source compilation',
    'NOT_RUN Unity import/test runner, graphics/shader compile, real touch, raycasts, physics/NavMesh, Android',
    'NOT_WIRED button/presenter/executor/indicator scene path; new controls remain unbound until authored integration',
])
(root / 'Logs/TraversalQA/defense-refactor-step5-static-checks.txt').write_text(report + '\n')
print(report)
