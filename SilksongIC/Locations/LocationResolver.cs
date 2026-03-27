using System.Collections.Generic;
using UnityEngine;

namespace SilksongIC.Locations
{
    /// <summary>
    /// Maps CollectableItemPickup instances → location names at runtime.
    ///
    /// From decompiled Assembly-CSharp:
    ///   - CollectableItemPickup.Item returns a SavedItem (ScriptableObject).
    ///   - SavedItem.name (UnityEngine.Object.name) is the asset name we use
    ///     as the identifier in locations.json (originalItemId field).
    ///   - We also match by GameObject name as a fallback when originalItemId
    ///     is ambiguous (multiple pickups with same item in one scene).
    /// </summary>
    public static class LocationResolver
    {
        // CollectableItemPickup instance ID → location name
        private static readonly Dictionary<int, string> _cache = new();

        public static void RebuildForScene(string sceneName)
        {
            _cache.Clear();

            foreach (var pickup in Object.FindObjectsOfType<CollectableItemPickup>())
            {
                var locationName = FindLocationName(pickup, sceneName);
                if (locationName != null)
                    _cache[pickup.GetInstanceID()] = locationName;
            }
        }

        public static string? Resolve(CollectableItemPickup pickup)
        {
            _cache.TryGetValue(pickup.GetInstanceID(), out var name);
            return name;
        }

        private static string? FindLocationName(CollectableItemPickup pickup, string sceneName)
        {
            var assetName = pickup.Item?.name;
            var goName    = pickup.gameObject.name;

            foreach (var loc in ItemManager.Instance.Locations.Values)
            {
                if (loc is not CollectableItemPickupLocation cip) continue;
                if (cip.SceneName != sceneName) continue;
                if (ItemManager.Instance.IsCollected(loc.Name)) continue;

                // Match by GameObject name if specified, otherwise by item asset name
                bool nameMatch = cip.GameObjectName != null
                    ? cip.GameObjectName == goName
                    : cip.OriginalItemId == assetName;

                if (nameMatch)
                    return loc.Name;
            }

            return null;
        }
    }

    /// <summary>
    /// Maps PlayMakerFSM instances → location names for FSMLocations.
    /// </summary>
    public static class FSMLocationResolver
    {
        private static readonly Dictionary<int, string> _cache = new();

        public static void RebuildForScene(string sceneName)
        {
            _cache.Clear();

            foreach (var loc in ItemManager.Instance.Locations.Values)
            {
                if (loc is not FSMLocation fsmLoc) continue;
                if (fsmLoc.SceneName != sceneName) continue;

                var go = GameObject.Find(fsmLoc.GameObjectName);
                if (go == null) continue;

                // Find the FSM by name on this GameObject
                foreach (var fsm in go.GetComponents<PlayMakerFSM>())
                {
                    if (fsm.FsmName == fsmLoc.FSMName)
                    {
                        _cache[fsm.GetInstanceID()] = loc.Name;
                        break;
                    }
                }
            }
        }

        public static string? Resolve(PlayMakerFSM fsm)
        {
            _cache.TryGetValue(fsm.GetInstanceID(), out var name);
            return name;
        }
    }
}
