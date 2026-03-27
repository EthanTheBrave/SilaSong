using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SilksongIC.Locations;

namespace SilksongIC.Locations
{
    /// <summary>
    /// Maps CollectableItemPickup instances → location names.
    /// Built once per scene load from registered CollectableItemPickupLocations.
    /// </summary>
    public static class LocationResolver
    {
        // gameObject instance ID -> location name
        private static readonly Dictionary<int, string> _cache = new();

        public static void RebuildForScene(string sceneName)
        {
            _cache.Clear();
            foreach (var loc in ItemManager.Instance.Locations.Values)
            {
                if (loc is not CollectableItemPickupLocation cipLoc) continue;
                if (cipLoc.SceneName != sceneName) continue;

                // Find the matching GameObject in the loaded scene
                var go = cipLoc.GameObjectName != null
                    ? GameObject.Find(cipLoc.GameObjectName)
                    : FindByItemId(cipLoc.OriginalItemId);

                if (go == null) continue;

                var pickup = go.GetComponent<CollectableItemPickup>();
                if (pickup == null) continue;

                _cache[pickup.GetInstanceID()] = loc.Name;
            }
        }

        public static string? Resolve(CollectableItemPickup pickup)
        {
            _cache.TryGetValue(pickup.GetInstanceID(), out var name);
            return name;
        }

        private static GameObject? FindByItemId(string itemId)
        {
            // Iterate all CollectableItemPickups in the scene and match by item ID
            foreach (var pickup in Object.FindObjectsOfType<CollectableItemPickup>())
            {
                // Update this property name once the game's assembly is decompiled
                if (pickup.ItemID == itemId)
                    return pickup.gameObject;
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

                var fsm = go.GetComponent<PlayMakerFSM>();
                if (fsm == null || fsm.FsmName != fsmLoc.FSMName) continue;

                _cache[fsm.GetInstanceID()] = loc.Name;
            }
        }

        public static string? Resolve(PlayMakerFSM fsm)
        {
            _cache.TryGetValue(fsm.GetInstanceID(), out var name);
            return name;
        }
    }
}
