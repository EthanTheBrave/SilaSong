# SilaSong — Hollow Knight: Silksong Randomizer

A full-featured item randomizer for Hollow Knight: Silksong, built from scratch with a clean architecture inspired by the HK1 Randomizer 4.

## Architecture

Two projects:

- **SilksongIC** — Item placement library (Silksong's "ItemChanger" equivalent). Handles intercepting pickups, delivering randomized items, and scene hooks. Can be used independently by other mods.
- **SilksongRando** — Main randomizer plugin. Uses [RandomizerCore](https://github.com/homothetyhk/RandomizerCore) for the shuffle algorithm, with logic files defining item requirements.

## Tech Stack

- BepInEx 5 + HarmonyX
- .NET Standard 2.1
- [RandomizerCore](https://github.com/homothetyhk/RandomizerCore) (NuGet)
- Silksong.GameLibs, Silksong.DataManager, Silksong.GameModeManager (NuGet — samboy feed)

## Features

- Logic-based randomization (no softlocks)
- Seed input on the new game screen
- On-screen item pickup notifications
- In-game map overlay (`[M]` cycles: Off → Checks → Logic → Spoiler)
- Spoiler log + live tracker log written to `%APPDATA%\SilksongRando\Logs\`
- Connection mod API — other BepInEx plugins can inject custom items/locations
- Full save/load support via Silksong.DataManager

## Building

1. Update `<GameDir>` in both `.csproj` files to point to your Silksong install.
2. Restore NuGet packages (add `https://nuget.samboy.dev/v3/index.json` as a source).
3. Build — outputs go directly to `BepInEx/plugins/`.

---

## Location Data Status

`SilksongRando/Resources/Data/locations.json` now contains **354 real locations** extracted directly from the game bundles using `tools/extract_locations.py` (UnityPy):

- **160 collectable pickups** — `CollectableItemPickup` components with confirmed `SavedItem` asset names
- **188 FSM locations** — ability shrines, boss drops, quest rewards, shops, and NPC dialogues with give-states
- **6 bellshrine locations** — one per bellshrine scene, hooked on `Bell Shrine Lever → Activate Delayed → Hit Lever`

All scene names, GameObject names, FSM names, and item asset names come from the real game bundles.

## ⚠️ TODO: What Still Needs Work

### 1. Logic expressions
`SilksongRando/Resources/Logic/locations.json` currently uses `"logic": "TRUE"` for all 354 locations — meaning nothing is locked behind progression yet. Real logic (which abilities/items are required to reach each location) needs to be filled in based on in-game testing.

### 2. Item pool mapping
`items.json` has 25 items defined. The 354 locations need to be mapped to pools in `pools.json` so the randomizer knows which locations contain which categories of items. Many of the 354 extracted locations give trinkets/relics that aren't yet represented in `items.json`.

### 3. FSM trigger state verification
Some FSM `triggerState` values were auto-selected from the first matching "give" state keyword. A handful may need correction after in-game testing — especially quest-chain locations where multiple states contain give-keywords.

### 4. In-game testing
All game API calls (ability unlocks, item gives, scene hooks) use confirmed field/method names from the decompiled assembly. The mod compiles cleanly but hasn't been run in-game yet.

### How to re-extract location data

```
cd tools
pip install UnityPy
python extract_locations.py
```

Outputs go to `tools/output/`. The script scans all 590 scene bundles and extracts `CollectableItemPickup`, `PlayMakerFSM`, and bellshrine data automatically.
