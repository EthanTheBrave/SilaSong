using System.Collections.Generic;

namespace SilksongIC.Locations
{
    /// <summary>
    /// Location for items sold in shops (e.g. the Weavers, traveling merchants).
    /// Each slot in a shop is a separate location with its own price.
    /// </summary>
    public class ShopLocation : AbstractLocation
    {
        /// <summary>Name of the NPC/shop owner GameObject.</summary>
        public string ShopOwnerName { get; init; } = string.Empty;

        /// <summary>The original item this slot sold (used to identify the slot).</summary>
        public string OriginalItemId { get; init; } = string.Empty;

        /// <summary>Geo cost to purchase from this slot (can be overridden by settings).</summary>
        public int Cost { get; init; }

        public override void OnSceneLoad(LocationLoadContext ctx)
        {
            // Shop slots are patched globally — nothing per-scene needed.
        }

        /// <summary>
        /// Returns all shop locations in the current scene, for use by a shop UI patch.
        /// </summary>
        public static IEnumerable<ShopLocation> GetForScene(string sceneName)
        {
            foreach (var loc in ItemManager.Instance.Locations.Values)
                if (loc is ShopLocation shop && shop.SceneName == sceneName)
                    yield return shop;
        }
    }
}
