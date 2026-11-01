"""Durable per-request file transport. A wait timeout never resubmits work."""
from __future__ import annotations

import asyncio
import json
import os
import time
import uuid
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


def read_json(path: Path) -> dict:
    # Windows CRT file opens omit FILE_SHARE_DELETE, which can block Unity's
    # atomic File.Replace while the client polls a job. Share delete explicitly.
    if os.name != 'nt':
        return json.loads(path.read_text(encoding='utf-8-sig'))
    import ctypes
    import msvcrt
    from ctypes import wintypes
    kernel = ctypes.WinDLL('kernel32', use_last_error=True)
    create = kernel.CreateFileW
    create.argtypes = [wintypes.LPCWSTR, wintypes.DWORD, wintypes.DWORD, ctypes.c_void_p,
                       wintypes.DWORD, wintypes.DWORD, wintypes.HANDLE]
    create.restype = wintypes.HANDLE
    for attempt in range(20):
        handle = create(str(path), 0x80000000, 7, None, 3, 0x80, None)
        if handle != ctypes.c_void_p(-1).value or ctypes.get_last_error() not in (32, 33):
            break
        # ReplaceFile can briefly hold an exclusive metadata lock even when the
        # readers share delete; retry the read, never the Unity command itself.
        time.sleep(0.005)
    if handle == ctypes.c_void_p(-1).value:
        code = ctypes.get_last_error()
        if code in (2, 3):
            raise FileNotFoundError(code, 'File not found', str(path))
        raise ctypes.WinError(code)
    descriptor = msvcrt.open_osfhandle(handle, os.O_RDONLY)
    with os.fdopen(descriptor, 'r', encoding='utf-8-sig') as stream:
        return json.load(stream)


def atomic_json(path: Path, value: dict) -> None:
    temporary = path.with_name(path.name + '.' + uuid.uuid4().hex + '.tmp')
    temporary.write_text(json.dumps(value, ensure_ascii=False), encoding='utf-8')
    os.replace(temporary, path)


class BridgeClient:
    def __init__(self, project: Path):
        self.project = project.resolve()
        if not (self.project / 'ProjectSettings/ProjectVersion.txt').is_file():
            raise ValueError('Project must be a Unity project root.')
        self.root = self.project / 'Library/UnityMcpBridge'
        for name in ('requests', 'jobs', 'artifacts'):
            (self.root / name).mkdir(parents=True, exist_ok=True)

    def connection(self) -> dict:
        try:
            heartbeat = read_json(self.root / 'heartbeat.json')
            stamp = datetime.fromisoformat(heartbeat['utc'].replace('Z', '+00:00'))
            age = max(0, (datetime.now(timezone.utc) - stamp).total_seconds())
            return {**heartbeat, 'online': age < 10 and heartbeat.get('available', False), 'heartbeatAgeSeconds': round(age, 2)}
        except (OSError, ValueError, KeyError):
            return {'online': False, 'reason': 'No current Editor heartbeat. Open Unity and allow script compilation.'}

    def job_status(self, job_id: str) -> dict:
        if uuid.UUID(job_id).hex != job_id:
            raise ValueError('job_id must be 32 lowercase hexadecimal characters.')
        try:
            job = read_json(self.root / 'jobs' / (job_id + '.json'))
        except FileNotFoundError:
            return {'id': job_id, 'state': 'unknown', 'connection': self.connection()}
        return {**job, 'connection': self.connection()}

    async def submit(self, command: str, arguments: dict[str, Any], wait_seconds: float = 2) -> dict:
        connection = self.connection()
        if not connection['online']:
            raise RuntimeError(connection.get('reason', 'Unity heartbeat is stale; no request was queued.'))
        job_id = uuid.uuid4().hex
        now = datetime.now(timezone.utc)
        request = {'id': job_id, 'command': command, 'arguments': arguments,
                   'createdUtc': now.isoformat(), 'expiresUtc': datetime.fromtimestamp(now.timestamp() + 120, timezone.utc).isoformat()}
        atomic_json(self.root / 'jobs' / (job_id + '.json'),
                    {'id': job_id, 'command': command, 'state': 'queued', 'createdUtc': now.isoformat()})
        atomic_json(self.root / 'requests' / (job_id + '.json'), request)
        deadline = time.monotonic() + wait_seconds
        while time.monotonic() < deadline:
            job = self.job_status(job_id)
            if job['state'] in ('succeeded', 'failed', 'interrupted', 'expired'):
                return job
            await asyncio.sleep(0.1)
        return self.job_status(job_id)

    def artifact(self, job_id: str) -> bytes:
        job = self.job_status(job_id)
        if job['state'] != 'succeeded' or job.get('command') != 'capture_game_view':
            raise ValueError('Capture has not succeeded. Poll job_status first.')
        path = Path(job['result']['path']).resolve()
        if not path.is_relative_to((self.root / 'artifacts').resolve()) or path.suffix != '.png':
            raise ValueError('Artifact path is outside the bridge artifact directory.')
        if path.stat().st_size > 20 * 1024 * 1024:
            raise ValueError('Image exceeds 20 MiB.')
        return path.read_bytes()
