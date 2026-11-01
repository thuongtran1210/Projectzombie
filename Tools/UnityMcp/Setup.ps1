param([string]$Python = 'python')
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$environmentRoot = Join-Path $projectRoot 'Library/UnityMcpPython'
$environmentPython = Join-Path $environmentRoot 'Scripts/python.exe'
if (!(Test-Path -LiteralPath $environmentPython)) {
    & $Python -m venv --without-pip $environmentRoot
    if ($LASTEXITCODE -ne 0) { throw 'Could not create Python environment.' }
}
& $Python -m pip --python $environmentPython install -r (Join-Path $PSScriptRoot 'requirements.txt')
if ($LASTEXITCODE -ne 0) { throw 'Could not install MCP SDK.' }
$server = Join-Path $PSScriptRoot 'server.py'
$clientConfig = @{mcpServers = @{ 'projectzombie-unity' = @{command = $environmentPython; args = @($server, '--project', $projectRoot)}}}
$configPath = Join-Path $projectRoot 'Library/UnityMcp.client.json'
$clientConfig | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $configPath -Encoding utf8
Write-Output "Unity MCP environment ready. Generic MCP client configuration: $configPath"
Write-Output "Command: $environmentPython"
Write-Output "Arguments: $server --project $projectRoot"
