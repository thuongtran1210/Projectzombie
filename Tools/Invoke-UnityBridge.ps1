param(
    [ValidateSet('status', 'audit-map-art', 'build-map-art', 'build-combat-hud', 'migrate-skill-data', 'validate-skill-data', 'test-skill-data', 'build-soul-pickup', 'smoke-soul-pickup', 'validate-soul-pickup', 'smoke-combat-hud', 'preview-map-art', 'hierarchy', 'capture-camera', 'validate-card-progression', 'preview-card-feedback', 'apply-courtyard', 'preview-courtyard', 'validate-courtyard', 'install-loadout-screen', 'apply-clean-review', 'validate-clean-review', 'validate-enemy-health', 'install-app-architecture', 'validate-app-architecture', 'smoke-app-architecture', 'smoke-bootstrap', 'smoke-direct-map', 'install-towers', 'build-tower-ordnance', 'build-structure-art', 'build-ice-laser', 'build-loading-ui', 'build-thunder', 'validate-thunder', 'validate-map-collision', 'smoke-map-collision', 'smoke-thunder', 'smoke-loading-ui', 'validate-towers', 'smoke-towers', 'refresh', 'save-scenes', 'clean-tmp-submeshes', 'optimize-ui-raycasts', 'build-pipe', 'preview-pipe', 'smoke-pipe', 'migrate-roster', 'smoke-roster', 'clear-validation-layout')]
    [string]$Command = 'status',
    [int]$TimeoutSeconds = 30
)
$ErrorActionPreference = 'Stop'
$bridgeRoot = Join-Path (Split-Path $PSScriptRoot -Parent) 'Library/ProjectEditorBridge'
if (!(Test-Path (Join-Path $bridgeRoot 'ready.json'))) {
    throw 'Unity bridge has not loaded. Let Unity import/compile ProjectEditorBridge.cs first.'
}
$requestPath = Join-Path $bridgeRoot 'request.json'
if (Test-Path $requestPath) { throw 'Another request is pending. Wait for Unity to process it.' }
$requestId = [Guid]::NewGuid().ToString('N')
$temporaryPath = Join-Path $bridgeRoot 'request.tmp'
@{id=$requestId; command=$Command} | ConvertTo-Json | Set-Content -LiteralPath $temporaryPath -Encoding UTF8
Move-Item -LiteralPath $temporaryPath -Destination $requestPath
$deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
$responsePath = Join-Path $bridgeRoot 'response.json'
while ([DateTime]::UtcNow -lt $deadline) {
    if (Test-Path $responsePath) {
        try { $reply = Get-Content -LiteralPath $responsePath -Raw | ConvertFrom-Json } catch { $reply = $null }
        if ($reply -and $reply.id -eq $requestId) {
            if (!$reply.ok) { throw $reply.error }
            $reply.result
            exit 0
        }
    }
    Start-Sleep -Milliseconds 250
}
throw "Unity did not answer within $TimeoutSeconds seconds. Request $requestId remains pending; do not resubmit a validation blindly."
