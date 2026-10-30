param(
    [ValidateSet('status', 'hierarchy', 'capture-camera', 'validate-enemy-health', 'install-app-architecture', 'validate-app-architecture', 'smoke-app-architecture', 'smoke-bootstrap', 'smoke-direct-map', 'install-towers', 'validate-towers', 'smoke-towers', 'refresh')]
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
