using HarmonyLib;
using SilksongIC;
using SilksongIC.Locations;

namespace SilksongRando.Hooks
{
    /// <summary>
    /// Prevents randomized pickups from being hidden on scene load.
    ///
    /// From decompiled Assembly-CSharp:
    ///   - CollectableItemPickup.Start() calls GameManager.instance.CanPickupsExist()
    ///     and then checks the item's persistence state.
    ///   - CollectableItemPickup.Item returns a SavedItem; SavedItem.CanGetMore()
    ///     returns false if already at max (i.e. already collected).
    ///   - If CanGetMore() is false, the pickup hides itself.
    ///
    /// Strategy: patch SavedItem.CanGetMore on CollectableItem so that items
    /// that are rando-tracked always report they can be picked up, until we
    /// actually deliver the rando item (at which point they're suppressed by
    /// our DoPickup prefix returning false).
    /// </summary>
    [HarmonyPatch]
    internal static class CollectableItemManagerHook
    {
        [HarmonyPatch(typeof(CollectableItem), nameof(CollectableItem.CanGetMore))]
        [HarmonyPostfix]
        static void ForceCanGetMore(CollectableItem __instance, ref bool __result)
        {
            if (!RandoPlugin.Instance.IsRandoActive) return;
            if (__result) return; // already true, no need to override

            // If this item's asset name is the original item of a tracked location
            // that hasn't been collected yet, force it to appear as collectible.
            if (IsTrackedAndUncollected(__instance.name))
                __result = true;
        }

        [HarmonyPatch(typeof(CollectableItem), "IsAtMax")]
        [HarmonyPostfix]
        static void ForceNotAtMax(CollectableItem __instance, ref bool __result)
        {
            if (!RandoPlugin.Instance.IsRandoActive) return;
            if (IsTrackedAndUncollected(__instance.name))
                __result = false; // pretend not at max so pickup stays visible
        }

        private static bool IsTrackedAndUncollected(string assetName)
        {
            foreach (var loc in ItemManager.Instance.Locations.Values)
            {
                if (loc is CollectableItemPickupLocation cip &&
                    cip.OriginalItemId == assetName &&
                    !ItemManager.Instance.IsCollected(loc.Name))
                    return true;
            }
            return false;
        }
    }
}
