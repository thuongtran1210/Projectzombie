# Scholar signature: Ha But Thanh An

The Defense3D Scholar signature uses the existing Started / Released / HitConfirmed / Ended event flow and effect pool. Combat definition is unchanged: 0.38 s windup, radius 6, damage 52.5, cooldown 30 s.

- Cast: ink strokes and a progressively revealed jade seal; inward energy accents. The sustained profile explicitly opts into normalized lifetime animation, while other sustained effects retain their existing progress behavior.
- Release: ivory flash, immediate full-footprint outline on the damage frame, eased outgoing aftershock and ink decay over 0.48 s.
- Hit: dedicated 0.2 s brush spark, still limited to 16 accepted targets per cast.
- Camera: 0.075 world-unit impulse over 0.16 s, configurable on FXSet_ScholarSignature. Amplitude 0 disables the impulse. Presenter clears it on pause, cleanup, dash, death and disable.

The shared URP shader renders one transparent quad per ground cue (see air-brush revision below), depth-tested against scene geometry. It uses no texture, global shader clock, per-frame material instance or runtime mesh allocation. Abstract seal marks are ornament rather than readable text. Mobile frame time remains to be measured on a device.

Unity menu: Tools > VFX Generator > Preview Scholar Ink Signature renders scratch/ScholarInkPreview.png. The top row shows cast at 12%, 55%, 95%; the bottom shows release at 0%, 30%, 75%. Validate Scholar Ink Signature also runs the selected synchronous NUnit fixtures without changing the currently open scene and writes scratch/ScholarInkValidation.xml and .txt.

Visual acceptance in gameplay: test on light and dark ground, near walls, when moving during windup, and amid dense enemies. Check hit sparks, one camera impulse per release, no residual visuals after dash/death and no stale shader state on repeated casts. The preview is an isolated render, not proof of Android performance or final in-battle readability.


## Air-brush revision

The primary silhouette now uses baked curved 3D ribbons above ground: two gathering ribbons around the torso and three large crossing release strokes. Release includes ten short ink fragments with deterministic outward flight and gravity. The ground seal retains its outer ring, inner circle and ornamental marks at 32% opacity weighting. It combines with airborne ink strokes; the large center glyph stays removed to keep the hero readable. Hit flashes stand upright rather than lying on the floor. Cast/release orientation captures the owner's facing direction.

Build/rebuild: Tools > VFX Generator > Build Scholar Air Signature. This updates only the Scholar prefabs, brush material and named baked mesh assets, then renders and validates. No gameplay or open scene is saved. Preview now includes the actual ThuSinhVisual prefab: rows show cast, release, and the same release with the ground renderer disabled. It is an isolated visual preview, not an in-match performance capture.

The release uses 14 mesh renderers (one ground, three ribbons, ten fragments). The effect service still budgets instances, not individual draw calls; measure this on target Android hardware. Ribbons and fragments share one material, but this does not guarantee a single draw call. No extra mesh or GameObject is created per cast after pool warm-up.
