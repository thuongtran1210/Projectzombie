# Menu-to-gameplay loading

The production cover is `AppLoadingView` inside `Assets/Core/Architecture/Assets/AppBootstrap.prefab`. It belongs to the application and remains visible while outgoing menu objects are unloaded. The older `LoadingScreenView` prefabs are not the owner of this transition.

The updated design uses the existing forest artwork, a slowly rotating bronze/jade seal, stage title, gameplay tip, three phase labels and a filled progress track. It scales to 16:9 and wide landscape screens using the persistent canvas scaler. The fill has an authored white sprite so Unity's Filled image mode clips its geometry correctly. Decorative graphics do not intercept input; a full-screen shade and the canvas group block clicks while the cover is visible.

Progress is phase-weighted work completion rather than an elapsed-time estimate or a download-byte counter:

| Range | Work |
| --- | --- |
| 0–10% | Unload outgoing content scene |
| 10–20% | Resolve stage configuration |
| 20–62% | Addressables scene-operation progress, including activation |
| 62–98% | Bind scene, prepare selected minions, build tower inventory/tier visuals and prewarm projectiles, initialize the session |
| 100% | Composition has explicitly reported ready; finish the cover |

Returning to the hub uses scene-load progress through 96%, then completion. Tower preparation reports each completed actor, so maps with longer inventory preparation do not hide a completed scene load behind a false 100%. Percentages are monotonic within one transition. Display smoothing approaches the latest completed work and cannot run ahead of it. `CompleteAsync` catches up, shows 100% and fades out over a short unscaled-time finish; it introduces no artificial multi-second loading timer. Errors stay blocking, do not report completion and provide a dismiss button only after recovery to the hub.

`IProgressContentSceneLoader` is an optional capability. Existing `IContentSceneLoader` implementations remain compatible; loaders without progress still report stage boundaries. The rules and map lifetimes, navigation lock, resource ownership and error recovery stay with `AppFlowController`.

## Authoring and verification

- **Tools > ProjectZombie > Architecture > Build Progress Loading UI** / bridge `build-loading-ui` updates only the persistent cover prefab. It preserves services, route references and the current open scene. It reuses the Vietnamese font and existing forest image, plus a tiny baked white UI sprite.
- Isolated previews: `scratch/LoadingPreview.png` (1920×1080) and `scratch/LoadingWidePreview.png` (2340×1080).
- **Run Loading UI Smoke** / `smoke-loading-ui` runs bootstrap → menu → actual gameplay → hub. It records real progress in `scratch/LoadingProgress.txt`, checks readiness before 100%, checks that displayed progress stays behind work and checks input-cover release. It restores the previous Play start-scene preference.
- `scratch/LoadingTransition.png` is the real loading Game View capture. `scratch/LoadingError.png` is a deliberately simulated recovered-error view used to verify the close button and input release; it is not evidence of a failed production load.
- `AppLoadingViewTests` and `AppFlowTests` cover monotonic/reset/error behavior, readiness gating, the progress-capability adapter and the authored sprite/renderer references. They are included in `validate-towers` alongside the existing match/tower regressions.

Editor checks establish integration and layout, not loading latency or frame rate on an Android device. Addressables operation progress may remain flat during indivisible work; the UI stays animated without inventing extra completed work.
