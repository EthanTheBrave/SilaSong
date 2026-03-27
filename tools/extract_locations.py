"""
extract_locations.py
Scans all Silksong scene bundles and extracts:
  - CollectableItemPickup locations (with resolved item asset names)
  - PlayMakerFSM components with item-giving states
  - BellShrineTuningFork locations

Outputs:
  locations_raw.json  - all found locations, ready to edit into locations.json
  fsm_data.json       - all FSMs found, for manual inspection
  errors.json         - any bundles/objects that failed to parse
"""

import os
import json
import UnityPy
import UnityPy.config

UnityPy.config.FALLBACK_UNITY_VERSION = "6000.0.50f1"

BASE = r"C:\repos\ss\ss\Hollow Knight Silksong_Data\StreamingAssets\aa\StandaloneWindows64"
SCENE_DIR   = os.path.join(BASE, "scenes_scenes_scenes")
MONO_BUNDLE = os.path.join(BASE, "94696d22b6ed0a74097d1bd58feb4dce_monoscripts.bundle")
DATA_BASE   = os.path.join(BASE, "dataassets_assets_assets", "dataassets")

# Bundles that contain item ScriptableObject assets
ITEM_BUNDLES = [
    os.path.join(DATA_BASE, "collectables", "collectableitems.bundle"),
    os.path.join(DATA_BASE, "collectables", "relics.bundle"),
    os.path.join(DATA_BASE, "tools", "toolitems.bundle"),
    os.path.join(DATA_BASE, "tools", "crestitems.bundle"),
]

OUT_DIR = os.path.join(os.path.dirname(__file__), "output")
os.makedirs(OUT_DIR, exist_ok=True)

GIVE_KEYWORDS = {"give", "reward", "collect", "pickup", "get item", "received", "obtain"}

# ── Pre-load item bundles into a shared environment ───────────────────────────
print("Loading item asset bundles...")
item_env = UnityPy.Environment()
with open(MONO_BUNDLE, "rb") as f:
    item_env.load_file(f, name=MONO_BUNDLE)
for bundle_path in ITEM_BUNDLES:
    if os.path.exists(bundle_path):
        with open(bundle_path, "rb") as f:
            item_env.load_file(f, name=bundle_path)
        print(f"  Loaded: {os.path.basename(bundle_path)}")
    else:
        print(f"  Not found (skipped): {bundle_path}")

# Build path_id → asset name map from loaded item bundles
item_name_map = {}  # (file_id, path_id) is not stable across envs; use path_id only
for obj in item_env.objects:
    if obj.type.name in ("MonoBehaviour", "ScriptableObject"):
        try:
            data = obj.read()
            name = getattr(data, "m_Name", "")
            if name:
                item_name_map[obj.path_id] = name
        except Exception:
            pass

print(f"  {len(item_name_map)} item assets indexed.\n")

# ── Scan scenes ───────────────────────────────────────────────────────────────
locations_raw = []
fsm_data      = []
errors        = []

scene_files = sorted(f for f in os.listdir(SCENE_DIR) if f.endswith(".bundle"))
total = len(scene_files)
print(f"Scanning {total} scene bundles...")

for i, bundle_file in enumerate(scene_files):
    scene_name  = bundle_file.replace(".bundle", "")
    bundle_path = os.path.join(SCENE_DIR, bundle_file)

    if i % 50 == 0:
        print(f"  {i}/{total} - {scene_name}")

    try:
        env = UnityPy.Environment()
        with open(MONO_BUNDLE, "rb") as f:
            env.load_file(f, name=MONO_BUNDLE)
        with open(bundle_path, "rb") as f:
            env.load_file(f, name=bundle_path)
    except Exception as e:
        errors.append({"scene": scene_name, "error": f"load: {e}"})
        continue

    for obj in env.objects:
        if obj.type.name != "MonoBehaviour":
            continue
        try:
            data = obj.read()
            script_name = data.m_Script.read().m_ClassName
            if not script_name:
                continue

            go_name = ""
            try:
                go_name = data.m_GameObject.read().m_Name
            except Exception:
                pass

            # ── CollectableItemPickup ──────────────────────────────────
            if script_name == "CollectableItemPickup":
                raw = obj.read_typetree()
                item_asset_name = ""
                item_pptr = raw.get("item", {})
                if isinstance(item_pptr, dict):
                    path_id = item_pptr.get("m_PathID", 0)
                    item_asset_name = item_name_map.get(path_id, "")

                locations_raw.append({
                    "name":           f"{scene_name}_{go_name}",
                    "scene":          scene_name,
                    "locationType":   "collectable",
                    "gameObject":     go_name,
                    "originalItemId": item_asset_name,
                })

            # ── PlayMakerFSM ───────────────────────────────────────────
            elif script_name == "PlayMakerFSM":
                raw    = obj.read_typetree()
                fsm    = raw.get("fsm", {})
                fsm_name = fsm.get("name", "")
                states   = [s.get("name", "") for s in fsm.get("states", []) if s.get("name")]

                fsm_data.append({
                    "scene":      scene_name,
                    "gameObject": go_name,
                    "fsmName":    fsm_name,
                    "states":     states,
                })

                give_states = [s for s in states
                               if any(kw in s.lower() for kw in GIVE_KEYWORDS)]
                if give_states:
                    locations_raw.append({
                        "name":           f"{scene_name}_{go_name}_{fsm_name}",
                        "scene":          scene_name,
                        "locationType":   "fsm",
                        "gameObject":     go_name,
                        "fsmName":        fsm_name,
                        "triggerState":   give_states[0],
                        "_allGiveStates": give_states,
                        "_note":          "Review triggerState — pick the state that actually gives the item",
                    })

            # ── BellShrineTuningFork ───────────────────────────────────
            elif script_name == "BellShrineTuningFork":
                locations_raw.append({
                    "name":         f"{scene_name}_Bellshrine",
                    "scene":        scene_name,
                    "locationType": "bellshrine",
                    "gameObject":   go_name,
                    "_note":        "Confirmed BellShrineTuningFork",
                })

        except Exception as e:
            errors.append({"scene": scene_name, "path_id": str(obj.path_id), "error": str(e)})

def write_json(path, data):
    with open(path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

write_json(os.path.join(OUT_DIR, "locations_raw.json"), locations_raw)
write_json(os.path.join(OUT_DIR, "fsm_data.json"),      fsm_data)
write_json(os.path.join(OUT_DIR, "errors.json"),         errors)

print(f"\nDone.")
print(f"  {len(locations_raw)} potential locations -> output/locations_raw.json")
print(f"  {len(fsm_data)} FSMs found           -> output/fsm_data.json")
print(f"  {len(errors)} errors               -> output/errors.json")
