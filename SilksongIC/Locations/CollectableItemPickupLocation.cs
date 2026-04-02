using HarmonyLib;
using UnityEngine;

namespace SilksongIC.Locations
{
    /// <summary>
    /// Location for standard CollectableItemPickup components.
    ///
    /// From decompiled Assembly-CSharp:
    ///   - CollectableItemPickup.DoPickup() fires on player interaction.
    ///   - CollectableItemPickup.DoPickupInstant() fires for auto/proximity pickups.
    ///   - Both are private void methods that ultimately call DoPickupAction().
    ///   - CollectableItemPickup.Item returns a SavedItem (ScriptableObject asset).
    ///   - SavedItem.name (inherited from UnityEngine.Object) is the asset name.
    ///
    /// Strategy: prefix both DoPickup and DoPickupInstant. If the pickup maps to
    /// a rando location, suppress the vanilla give and deliver the rando item.
    /// </summary>
    public class CollectableItemPickupLocation : AbstractLocation
    {
        /// <summary>Name of the GameObject in the scene that has the CollectableItemPickup.</summary>
        public string? GameObjectName { get; init; }

        /// <summary>The SavedItem asset name this pickup originally gives.</summary>
        public string OriginalItemId { get; init; } = string.Empty;

        public override void OnSceneLoad(LocationLoadContext ctx)
        {
            // All interception is handled globally in Patches below.
        }

        [HarmonyPatch]
        internal static class Patches
        {
            [HarmonyPatch(typeof(CollectableItemPickup), "DoPickup")]
            [HarmonyPrefix]
            static bool InterceptDoPickup(CollectableItemPickup __instance)
                => Intercept(__instance);

            // Also intercept auto/proximity pickups (triggered without player interaction).
            [HarmonyPatch(typeof(CollectableItemPickup), "DoPickupInstant")]
            [HarmonyPrefix]
            static bool InterceptDoPickupInstant(CollectableItemPickup __instance)
                => Intercept(__instance);

            static bool Intercept(CollectableItemPickup instance)
            {
                var locationName = LocationResolver.Resolve(instance);
                if (locationName == null)
                    return true; // not a rando location — run vanilla

                ItemManager.Instance.DeliverItem(locationName, new GiveInfo(
                    LocationName: locationName,
                    Fling:        FlingType.Gentle,
                    Container:    nameof(CollectableItemPickupLocation)
                ));

                return false; // suppress vanilla give
            }
        }
    }
}
