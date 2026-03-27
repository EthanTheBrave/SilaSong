using HarmonyLib;
using UnityEngine;

namespace SilksongIC.Locations
{
    /// <summary>
    /// Location for standard CollectableItemPickup components —
    /// the most common item pickup type in Silksong.
    /// </summary>
    public class CollectableItemPickupLocation : AbstractLocation
    {
        /// <summary>
        /// Name of the GameObject that has the CollectableItemPickup component.
        /// If null, matches by scene + any CollectableItemPickup with a matching item ID.
        /// </summary>
        public string? GameObjectName { get; init; }

        /// <summary>The original item ID this pickup gives (used to identify it).</summary>
        public string OriginalItemId { get; init; } = string.Empty;

        public override void OnSceneLoad(LocationLoadContext ctx)
        {
            // Hooks are handled globally by Patches below.
            // Nothing to do per-scene here; the patch intercepts all pickups
            // and routes them through ItemManager.
        }

        [HarmonyPatch]
        internal static class Patches
        {
            // Patch the method that fires when the player picks up a CollectableItemPickup.
            // Replace with the actual method name from the decompiled game.
            [HarmonyPatch(typeof(CollectableItemPickup), "OnPickedUp")]
            [HarmonyPrefix]
            static bool InterceptPickup(CollectableItemPickup __instance)
            {
                var locationName = LocationResolver.Resolve(__instance);
                if (locationName == null)
                    return true; // not a randomized location — run vanilla

                ItemManager.Instance.DeliverItem(locationName, new GiveInfo(
                    LocationName: locationName,
                    Fling: FlingType.Gentle,
                    Container: nameof(CollectableItemPickupLocation)
                ));

                // Suppress the vanilla item give (we replaced it)
                return false;
            }
        }
    }
}
