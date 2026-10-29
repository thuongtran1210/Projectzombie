# CardCodex Defense loadout

Entry: MainHub Btn_CardCodex_Wood_Frame -> CardCodex.

- Passives (serialized enum value 2) now opens Minions; Towers is value 3.
- CardCodexUI now contains authored references for all four tabs, equip/status controls, ten grouped icon slots, and a reusable CodexCard prefab. No fixed controls are cloned at runtime.
- The responsive layout respects Screen.safeArea and recalculates four/five grid columns only when width changes. Text names use two lines; equipped and inspected states are separate. Shard counts appear in the detail panel rather than every card.
- Inspecting a card updates only the previous/current selection. Equipping updates badges and slot bindings without reconstructing the card list.
- Unit portraits are baked from existing 3D prefabs into Assets/Art/UI/Codex; no runtime model rendering is needed for these icons.
- Rebuild using ProjectZombie > CardCodex > Build and Preview (Ctrl+Shift+J). This updates both prefab copies and exports sample-data renders into Logs/CardCodexQA. It uses temporary scenes and does not save MainMenu.
- Selection and slot counts read LoadoutRules.Default (1 hero, 1 relic, max 4 distinct minions / towers). Existing minimum of 1 minion / tower is retained.
- Equipment is saved through DefenseLoadoutRepository. Inspecting cards does not equip them. Legacy cards without a supported catalog ID are visible for collection/upgrade, with equipping disabled.
- Catalog-only baseline entries are shown even if the old card database has no matching asset. New playable content still requires catalog entries and runtime definitions/adapters.
- Stage selection opens DefensePreparation as a review. Edit opens CardCodex; Back returns to review, retaining the selected stage. Review reloads the saved loadout each time it opens.
- Scene navigation still targets the existing MapSongQue3D_Prototype; adding support for multiple gameplay scenes is outside this change.

## Validation

Runtime sources compiled using the project's Unity compiler response file, with new partial sources included and output redirected into Temp.
14 DefenseLoadoutCoreTests executed with NUnit assertions through a standalone Mono runner, including four distinct types accepted, fifth rejected, duplicates rejected, snapshot isolation and rule overrides.

## Unity manual check

1. Open MainMenu, enter Play Mode, click Btn_CardCodex_Wood_Frame once; only one screen should open.
2. Check all four tabs, Vietnamese labels, loadout slot row and equip button at target mobile resolutions.
3. Click a loadout slot, inspect a card, equip/remove; inspect must not change equipment. Unsupported cards must not silently equip a fallback.
4. Return to hub; check updated summary, reopen Codex and restart Play Mode to confirm persistence.
5. Select a stage, inspect review, edit loadout, press Back and confirm the review reloads the saved selection.
6. Start battle and check allowed minion roles, towers and hero/relic kit in gameplay.

Visual layout rendered and inspected at 1920x1080, 2400x1080 and 1440x1080. Preview images use sample card ownership/progression to exercise visual states; they do not represent the player's inventory.
Physical-device GPU/CPU profiling, notch safe-area simulation and full scene-transition testing still require device QA.
