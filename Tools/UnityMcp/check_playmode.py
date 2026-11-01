"""Optional live check: enter Play Mode, capture Game View, then restore Edit Mode."""
import asyncio
import json
from pathlib import Path
from bridge_client import BridgeClient, read_json


async def wait(bridge, job, timeout=100):
    for _ in range(timeout * 2):
        if job['state'] not in ('queued', 'running'):
            break
        await asyncio.sleep(0.5)
        job = bridge.job_status(job['id'])
    print(json.dumps(job), flush=True)
    if job['state'] != 'succeeded':
        raise RuntimeError('Job did not succeed: ' + job['id'])
    return job


async def main():
    bridge = BridgeClient(Path(__file__).resolve().parents[2])
    state = await wait(bridge, await bridge.submit('editor_status', {}))
    assert not state['result']['playing'], 'Start this check in Edit Mode.'
    enter = await bridge.submit('playmode_set', {'playing': True})
    try:
        await wait(bridge, enter)
        await asyncio.sleep(2)
        image = await wait(bridge, await bridge.submit('capture_game_view', {}), 25)
        assert bridge.artifact(image['id']).startswith(b'\x89PNG\r\n\x1a\n')
        print('Play Mode transition and Game View PNG: PASS', flush=True)
    finally:
        # Never abandon a pending transition or capture before attempting restoration.
        for _ in range(50):
            jobs = [read_json(path)
                    for path in (bridge.root / 'jobs').glob('*.json')]
            if not any(job.get('state') == 'running' for job in jobs):
                break
            await asyncio.sleep(0.5)
        for _ in range(120):
            if bridge.connection()['online']:
                break
            await asyncio.sleep(0.5)
        await wait(bridge, await bridge.submit('playmode_set', {'playing': False}))


if __name__ == '__main__':
    asyncio.run(main())
