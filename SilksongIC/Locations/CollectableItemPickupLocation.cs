using HarmonyLib;
using UnityEngine;

namespace SilksongIC.Locations
{
    /// <summary>
    /// Location for standard CollectableItemPickup components.
    ///
    /// From decompiled Assembly-CSharp:
    ///   - CollectableItemPickup.DoPickup() is the private method that fires on interaction.
    ///   - CollectableItemPickup.Item returns a SavedItem (the ScriptableObject asset).
    ///   - SavedItem.name (inherited from UnityEngine.Object) is the asset name.
    ///   - OnPickedUp is a UnityEvent (not directly patchable as a method entry point).
    ///
    /// Strategy: patch DoPickup with a prefix. If the pickup maps to a rando location,
    /// suppress the vanilla give and deliver the randomized item instead.
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
            // Patch DoPickup — the private method that fires when the player
            // activates a CollectableItemPickup (confirmed from decompiled source).
            [HarmonyPatch(typeof(CollectableItemPickup), "DoPickup")]
            [HarmonyPrefix]
            static bool InterceptDoPickup(CollectableItemPickup __instance)
            {
                var locationName = LocationResolver.Resolve(__instance);
                if (locationName == null)
                    return true; // not a rando location — run vanilla

                ItemManager.Instance.DeliverItem(locationName, new GiveInfo(
                    LocationName: locationName,
                    Fling: FlingType.Gentle,
                    Container: nameof(CollectableItemPickupLocation)
                ));

                return false; // suppress vanilla give
            }
        }
    }
}
