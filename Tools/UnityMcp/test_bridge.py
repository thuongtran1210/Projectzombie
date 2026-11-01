import asyncio
import json
import tempfile
import unittest
from datetime import datetime, timedelta, timezone
from pathlib import Path

from bridge_client import BridgeClient, atomic_json


class BridgeTests(unittest.IsolatedAsyncioTestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.project = Path(self.temporary.name)
        (self.project / 'ProjectSettings').mkdir()
        (self.project / 'ProjectSettings/ProjectVersion.txt').touch()
        self.bridge = BridgeClient(self.project)

    def heartbeat(self, age=0, available=True):
        atomic_json(self.bridge.root / 'heartbeat.json', {
            'utc': (datetime.now(timezone.utc) - timedelta(seconds=age)).isoformat(), 'available': available})

    async def test_offline_does_not_enqueue(self):
        with self.assertRaises(RuntimeError):
            await self.bridge.submit('playmode_set', {'playing': True})
        self.assertEqual(list((self.bridge.root / 'requests').iterdir()), [])

    async def test_stale_and_unavailable_heartbeat(self):
        self.heartbeat(age=60)
        self.assertFalse(self.bridge.connection()['online'])
        self.heartbeat(available=False)
        self.assertFalse(self.bridge.connection()['online'])

    async def test_timeout_keeps_exactly_one_request(self):
        self.heartbeat()
        job = await self.bridge.submit('tests_run', {'mode': 'EditMode'}, wait_seconds=0.01)
        self.assertEqual(job['state'], 'queued')
        requests = list((self.bridge.root / 'requests').glob('*.json'))
        self.assertEqual(len(requests), 1)
        self.assertEqual(json.loads(requests[0].read_text())['id'], job['id'])
        self.assertEqual(self.bridge.job_status(job['id'])['state'], 'queued')

    async def test_concurrent_requests_have_independent_results(self):
        self.heartbeat()
        jobs = await asyncio.gather(*[self.bridge.submit('editor_status', {}, 0) for _ in range(8)])
        self.assertEqual(len({job['id'] for job in jobs}), 8)
        for job in jobs:
            atomic_json(self.bridge.root / 'jobs' / (job['id'] + '.json'),
                        {**job, 'state': 'succeeded', 'result': job['id']})
        for job in jobs:
            self.assertEqual(self.bridge.job_status(job['id'])['result'], job['id'])

    async def test_job_id_rejects_path_traversal(self):
        for value in ['../heartbeat', '../../secrets', 'a' * 32, 'A' * 32]:
            if value == 'a' * 32:
                self.assertEqual(self.bridge.job_status(value)['state'], 'unknown')
            else:
                with self.assertRaises(ValueError):
                    self.bridge.job_status(value)

    async def test_artifact_is_confined_to_bridge(self):
        job_id = 'b' * 32
        atomic_json(self.bridge.root / 'jobs' / (job_id + '.json'), {
            'id': job_id, 'command': 'capture_game_view', 'state': 'succeeded',
            'result': {'path': str(self.project / 'outside.png')}})
        with self.assertRaises(ValueError):
            self.bridge.artifact(job_id)


if __name__ == '__main__':
    unittest.main()
