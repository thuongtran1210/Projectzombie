"""Real stdio MCP handshake/schema/offline checks; --live exercises read tools in an open Editor."""
import argparse
import asyncio
import json
import sys
from pathlib import Path

from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client


def payload(result):
    if result.structuredContent:
        return result.structuredContent
    for item in result.content:
        if item.type == 'text':
            return json.loads(item.text)
    return None


async def main(live, capture_job=None):
    server_path = Path(__file__).with_name('server.py').resolve()
    async with stdio_client(StdioServerParameters(command=sys.executable, args=[str(server_path)])) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            tools = (await session.list_tools()).tools
            assert len(tools) == 10, [tool.name for tool in tools]
            for tool in tools:
                assert tool.inputSchema['type'] == 'object'
            if live:
                from bridge_client import BridgeClient
                bridge = BridgeClient(server_path.parents[2])
                for _ in range(120):
                    if bridge.connection()['online']:
                        break
                    await asyncio.sleep(0.5)
                assert bridge.connection()['online'], bridge.connection()
            status = await session.call_tool('editor_status', {})
            assert not status.isError, status
            print('MCP stdio handshake and all 10 tool schemas: PASS')
            print('editor_status:', status.structuredContent or [item.text for item in status.content if item.type == 'text'])
            if capture_job:
                image = await session.call_tool('job_status', {'job_id': capture_job, 'include_image': True})
                assert not image.isError and any(item.type == 'image' and item.mimeType == 'image/png' for item in image.content)
                print('Native MCP PNG image response: PASS')
            if not live:
                return
            for name, args in [('console_read', {'limit': 5}), ('hierarchy_query', {'limit': 5}),
                               ('asset_find', {'query': 't:Scene', 'limit': 3})]:
                result = await session.call_tool(name, args)
                assert not result.isError, result
                data = payload(result)
                if data and 'result' in data and isinstance(data['result'], dict) and 'state' in data['result']:
                    data = data['result']
                for _ in range(30):
                    if not data or data.get('state') != 'queued' and data.get('state') != 'running':
                        break
                    await asyncio.sleep(0.2)
                    result = await session.call_tool('job_status', {'job_id': data['id']})
                    data = payload(result)
                assert data and data.get('state') == 'succeeded', result
                print(name + ': PASS', json.dumps(data.get('result'), ensure_ascii=True)[:500])


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--live', action='store_true')
    parser.add_argument('--capture-job')
    args = parser.parse_args()
    asyncio.run(main(args.live, args.capture_job))
