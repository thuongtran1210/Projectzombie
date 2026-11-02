# Compact combat HUD

The gameplay controls now share bronze frames, dark ink plates and transparent vector-drawn glyphs. The generator is `Assets/Editor/UI/generate_combat_hud_art.py`; it creates new assets without editing any existing icon images.

## Layout and touch

At a 1080 reference height, Attack is 144 units, signature 108, relic 100 and dash 92. The smaller actions retain input rectangles of at least 100 units. A separate Face child contains the art and press response; pressing never scales the input rectangle or changes aim coordinates. The four actions form a compact two-row arc at the lower right. The skill canvas has an authored safe-area root and scales by screen height.

The village canvas retains its 720 reference resolution so recruitment controls keep their existing authored dimensions. Its height-based scaling matches the combat canvas proportionally. The command template uses a 48-unit plate (72 at 1080) within a 64-unit input rectangle (96 at 1080). It shows portrait, count badge and a short order, with the unit name/current order displayed briefly above the dock when commanded. Additional equipped types remain in the horizontal scroll list. Recruitment rows are unchanged.

## Identity and states

- Attack uses a common pair of brush-like attack strokes; the white-backed weapon image is removed from the HUD.
- Scholar signature and lightning use separate brush/lightning glyphs. Slipper and pipe likewise use separate glyphs. Runtime loadout binding selects the alternate glyph by its authored identity token.
- Center text is reserved for numeric cooldown. Recast/action words move to the small status line.
- Cooldown uses a dark radial fill. Ready uses a faint accent ring plus a brief pulse when cooldown completes. Locked has a dark overlay and small padlock. Aiming changes the border; pressing scales only the Face to 95%.
- Labels use compact title case and fit one line. The Attack slot has no extra status line.

Sprites have alpha transparency, disabled mipmaps and Android ASTC 6x6 settings. Frame/panel sprites retain border metadata. Round plates render with uniform scaling to preserve their circles; the rectangular dock uses sliced rendering. Pipe VFX rebuilding respects the compact skin's existing status/duration/aim visuals.

## Tools and verification

```powershell
python Assets/Editor/UI/generate_combat_hud_art.py
./Tools/Invoke-UnityBridge.ps1 -Command refresh
./Tools/Invoke-UnityBridge.ps1 -Command build-combat-hud
./Tools/Invoke-UnityBridge.ps1 -Command smoke-combat-hud
./Tools/Invoke-UnityBridge.ps1 -Command smoke-roster
./Tools/Invoke-UnityBridge.ps1 -Command validate-towers
./Tools/Invoke-UnityBridge.ps1 -Command validate-thunder
```

The builder updates the controls prefab, command-row template and authored layout in Song Que/Combat Lab scenes, preserving gameplay bindings. It refuses dirty scenes/Play Mode and restores the previous scene setup.

The HUD Play check verifies real match entry, four non-overlapping input rectangles, raycast routes, touch dimensions at 1080-equivalent height, command art size, radial/numeric cooldown, Scholar/Lightning casts and dynamic lightning/pipe glyphs. The existing roster Play check verifies four equipped types, recruitment payments, independent commands, pool reuse, pause/restart and unloading. No physical-device usability or mobile performance measurement is claimed.

Evidence: `scratch/CombatHudBuild.txt`, `CombatHudSmoke.txt`, `CombatHudGameplay.png`, `CombatHudCooldown.png`, `CombatHudAlternate.png`, and `MinionRosterMatchCheck.txt`.
