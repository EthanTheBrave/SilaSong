using HarmonyLib;
using UnityEngine;

namespace SilksongIC.Locations
{
    /// <summary>
    /// Location driven by a PlayMaker FSM state action.
    /// Used for: Bellshrines, Mossberries, ability pickups, Brolly, Needolin, etc.
    /// </summary>
    public class FSMLocation : AbstractLocation
    {
        /// <summary>Name of the GameObject that owns the FSM.</summary>
        public string GameObjectName { get; init; } = string.Empty;

        /// <summary>Name of the PlayMaker FSM.</summary>
        public string FSMName { get; init; } = string.Empty;

        /// <summary>The FSM state that triggers the item give.</summary>
        public string TriggerState { get; init; } = string.Empty;

        public override void OnSceneLoad(LocationLoadContext ctx)
        {
            // Hooks are handled globally in Patches below.
        }

        [HarmonyPatch]
        internal static class Patches
        {
            // Patch PlayMakerFSM state entry to intercept item-giving states.
            // The actual type/method depends on the FSM library used by Silksong.
            // FsmUtil provides helpers — update this if the API differs.
            [HarmonyPatch(typeof(PlayMakerFSM), "OnEnable")]
            [HarmonyPostfix]
            static void OnFSMEnabled(PlayMakerFSM __instance)
            {
                var locationName = FSMLocationResolver.Resolve(__instance);
                if (locationName == null)
                    return;

                // Insert a custom action before the item-give state that
                // delivers the randomized item and suppresses the vanilla give.
                FSMPatcher.InsertDeliverAction(__instance, locationName);
            }
        }
    }
}
