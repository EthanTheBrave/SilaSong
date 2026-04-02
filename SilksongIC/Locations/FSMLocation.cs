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
            // Patch PlayMakerFSM.OnEnable globally (same pattern as HK ItemChanger).
            // We do inline lookup instead of a pre-built cache because OnEnable fires
            // during scene setup, before SceneManager.sceneLoaded fires — so any
            // pre-built per-scene cache would be empty at patch time.
            [HarmonyPatch(typeof(PlayMakerFSM), "OnEnable")]
            [HarmonyPostfix]
            static void OnFSMEnabled(PlayMakerFSM __instance)
            {
                var goName    = __instance.gameObject.name;
                var fsmName   = __instance.FsmName;
                var sceneName = __instance.gameObject.scene.name;

                foreach (var loc in ItemManager.Instance.Locations.Values)
                {
                    if (loc is FSMLocation fsmLoc &&
                        fsmLoc.SceneName      == sceneName &&
                        fsmLoc.GameObjectName == goName    &&
                        fsmLoc.FSMName        == fsmName   &&
                        !ItemManager.Instance.IsCollected(loc.Name))
                    {
                        FSMPatcher.InsertDeliverAction(__instance, loc.Name);
                        return;
                    }
                }
            }
        }
    }
}
