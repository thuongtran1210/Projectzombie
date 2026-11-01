"""Run only bridge tests against the existing Editor; never saves user scenes."""
import asyncio
import json
from pathlib import Path
from bridge_client import BridgeClient


async def main():
    bridge = BridgeClient(Path(__file__).resolve().parents[2])
    job = await bridge.submit('tests_run', {'mode': 'EditMode',
                              'assembly_names': ['ProjectZombie.UnityMcp.Editor.Tests']})
    print('job:', job['id'], flush=True)
    for _ in range(600):
        if job['state'] not in ('queued', 'running'):
            break
        await asyncio.sleep(0.5)
        job = bridge.job_status(job['id'])
    print(json.dumps(job, indent=2))
    if job['state'] != 'succeeded' or job['result'].get('failed', 1) != 0 or job['result'].get('passed', 0) < 5:
        raise SystemExit(1)


if __name__ == '__main__':
    asyncio.run(main())
