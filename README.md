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

## ⚠️ TODO: Location Data Needs In-Game Discovery

The biggest remaining task before this mod is playable is filling in the actual **scene names, GameObject names, FSM names, and trigger states** in:

```
SilksongRando/Resources/Data/locations.json
```

All game API calls (ability unlocks, item gives, hooks) are implemented with confirmed method/field names from the decompiled assembly. The location data is the only part that still contains placeholders.

### What needs to be done

For each location entry in `locations.json`, the following fields need real values verified in-game:

| Field | Description | How to find it |
|---|---|---|
| `scene` | The Unity scene name the location is in | Use a scene dumper tool or check scene transition logs |
| `gameObject` | The GameObject path in the scene hierarchy | Use the in-game debug mod or a runtime inspector |
| `fsmName` | The PlayMaker FSM name on that GameObject | Use [FsmUtil](https://thunderstore.io/c/hollow-knight-silksong/p/PimDeWitte/FsmUtil/) or a FSM viewer |
| `triggerState` | The FSM state that gives the item | Step through FSM states with a debug tool |
| `originalItemId` | The `SavedItem` asset name for collectable pickups | Log `pickup.Item.name` in a test patch |

### Recommended approach

1. Install [Silksong.DebugMod](https://github.com/hk-speedrunning/Silksong.DebugMod) for scene/object inspection.
2. Install [FsmUtil](https://thunderstore.io/c/hollow-knight-silksong/p/PimDeWitte/FsmUtil/) to view PlayMaker FSMs at runtime.
3. For each item location in the game, record the scene, GameObject, and FSM details.
4. Update `locations.json` and the matching `Resources/Logic/locations.json` logic expressions.

The existing [Silksong-Rando](https://github.com/timothymarriott/Silksong-Rando) by timothymarriott has a `LocationFinder` tool (F11+Y) that scans all scenes automatically — that output can be used as a reference for `CollectableItemPickup` locations.
