"""Projectzombie Unity MCP. Run with the project-local Python environment."""
from __future__ import annotations

import argparse
from pathlib import Path
from typing import Any, Literal

from mcp.server.fastmcp import FastMCP, Image
from mcp.types import ToolAnnotations
from bridge_client import BridgeClient

mcp = FastMCP('Projectzombie Unity')
bridge: BridgeClient
READ = ToolAnnotations(readOnlyHint=True, destructiveHint=False, idempotentHint=True, openWorldHint=False)
WRITE = ToolAnnotations(readOnlyHint=False, destructiveHint=False, idempotentHint=False, openWorldHint=False)


@mcp.tool(annotations=READ)
async def editor_status() -> dict:
    """Read live Editor state, scenes and compiler diagnostics. Offline state does not enqueue work."""
    if not bridge.connection()['online']:
        return bridge.connection()
    return await bridge.submit('editor_status', {})


@mcp.tool(annotations=READ)
async def console_read(limit: int = 100, severity: Literal['all', 'error', 'warning', 'log'] = 'all') -> dict:
    """Read the bounded log buffer captured since bridge load, including stack traces; does not clear Console."""
    return await bridge.submit('console_read', {'limit': limit, 'severity': severity})


@mcp.tool(annotations=READ)
async def hierarchy_query(name_contains: str = '', limit: int = 200, offset: int = 0) -> dict:
    """Query loaded scene objects, including inactive objects; returns stable IDs and component IDs."""
    return await bridge.submit('hierarchy_query', locals())


@mcp.tool(annotations=READ)
async def component_read(object_id: str, property_path: str = '', limit: int = 100) -> dict:
    """Read serialized fields by ID returned by hierarchy_query or asset_find. Supports scalar, vector, color and reference values."""
    return await bridge.submit('component_read', locals())


@mcp.tool(annotations=WRITE)
async def serialized_property_set(object_id: str, property_path: str, value: Any,
                                  expected_value: Any) -> dict:
    """Edit one serialized field in Edit Mode with Undo and an expected-value conflict check. Does not save files. Asset edits are limited to ScriptableObjects; prefab edits use scene instances."""
    return await bridge.submit('serialized_property_set', locals())


@mcp.tool(annotations=READ)
async def asset_find(query: str, limit: int = 100, offset: int = 0) -> dict:
    """Search AssetDatabase in Assets using Unity search syntax, e.g. 't:Prefab Health'."""
    return await bridge.submit('asset_find', locals())


@mcp.tool(annotations=WRITE)
async def capture_game_view() -> dict:
    """Capture a rendered Game View frame including overlay UI. Requires running, unpaused Play Mode and visible Game View. Poll job_status; use job_status(include_image=True) for image content."""
    return await bridge.submit('capture_game_view', {})


@mcp.tool(annotations=WRITE)
async def playmode_set(playing: bool) -> dict:
    """Enter or exit Play Mode. Entering requires saved clean scenes; completion waits for the transition."""
    return await bridge.submit('playmode_set', locals())


@mcp.tool(annotations=WRITE)
async def tests_run(mode: Literal['EditMode', 'PlayMode'] = 'EditMode', test_names: list[str] | None = None,
                    assembly_names: list[str] | None = None) -> dict:
    """Start Unity Test Framework tests asynchronously. Requires clean saved scenes and Edit Mode. Poll returned job ID; failures remain available across reloads."""
    return await bridge.submit('tests_run', locals())


@mcp.tool(annotations=READ)
async def job_status(job_id: str, include_image: bool = False) -> Any:
    """Read durable queued/running/succeeded/failed/interrupted/expired job state. A client timeout never cancels or resubmits the job. Optionally return a completed capture as native MCP image."""
    if include_image:
        return Image(data=bridge.artifact(job_id), format='png')
    return bridge.job_status(job_id)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--project', type=Path, default=Path(__file__).resolve().parents[2])
    args = parser.parse_args()
    bridge = BridgeClient(args.project)
    mcp.run(transport='stdio')
