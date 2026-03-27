using HarmonyLib;
using SilksongIC;
using SilksongIC.Locations;

namespace SilksongRando.Hooks
{
    /// <summary>
    /// Prevents CollectableItemManager from treating randomized pickups as
    /// "already collected" before the player reaches them.
    ///
    /// Without this patch the game reads its master list on scene load and
    /// hides any pickup whose item is already in the player's inventory —
    /// which would make randomized locations invisible.
    /// </summary>
    [HarmonyPatch]
    internal static class CollectableItemManagerHook
    {
        [HarmonyPatch(typeof(CollectableItemManager), nameof(CollectableItemManager.IsItemInMasterList))]
        [HarmonyPrefix]
        static bool OverrideIsItemInMasterList(string itemId, ref bool __result)
        {
            if (!RandoPlugin.Instance.IsRandoActive) return true;

            // If this original item ID belongs to a randomized location, pretend
            // it is NOT in the master list so the pickup stays visible.
            if (IsTrackedItemId(itemId))
            {
                __result = false;
                return false; // skip original method
            }

            return true;
        }

        private static bool IsTrackedItemId(string itemId)
        {
            foreach (var loc in ItemManager.Instance.Locations.Values)
            {
                if (loc is CollectableItemPickupLocation cip && cip.OriginalItemId == itemId)
                    return true;
            }
            return false;
        }
    }
}
