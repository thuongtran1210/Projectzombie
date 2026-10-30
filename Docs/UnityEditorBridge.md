# Unity Editor bridge

Project-local file bridge, not an MCP server. Unity imports `Assets/Editor/ProjectEditorBridge.cs` and processes a single JSON request on its main thread every 0.5 seconds. No external package, network listener, arbitrary method execution, scene saving, or Play Mode changes. Editor-only; excluded from Android builds.

From the project root:

```powershell
./Tools/Invoke-UnityBridge.ps1 status
./Tools/Invoke-UnityBridge.ps1 hierarchy
./Tools/Invoke-UnityBridge.ps1 capture-camera
./Tools/Invoke-UnityBridge.ps1 validate-enemy-health -TimeoutSeconds 60
./Tools/Invoke-UnityBridge.ps1 validate-app-architecture -TimeoutSeconds 60
./Tools/Invoke-UnityBridge.ps1 smoke-bootstrap
./Tools/Invoke-UnityBridge.ps1 smoke-direct-map
```

Requests/responses and readiness marker live in ignored `Library/ProjectEditorBridge`. One client/request at a time. Replies must match the request GUID. A timeout does not cancel queued work; inspect the response before retrying. A ready marker can survive an Editor shutdown, so only a matching response proves connectivity.

`capture-camera` renders the active MainCamera at 960×540 and restores its target. It is a camera render, not a Game View screenshot: Screen Space Overlay UI and editor chrome are not included, and aspect may differ from the current Game View. Images are returned as local paths for inspection.

`validate-enemy-health` requires Edit Mode and invokes the existing isolated preview/test tool. It writes `scratch/EnemyHealthPreview.png` and validation reports. It does not establish that a live combat session works.

If the bridge is not ready, allow Unity to import and compile the new Editor script. Existing compile errors or disabled auto-refresh can prevent loading. The client does not operate Unity's UI to force this. To disable the bridge, remove its Editor script and let Unity recompile.
