using System.Linq;
using BepInEx.Logging;
using HutongGames.PlayMaker;

namespace SilksongIC.Locations
{
    /// <summary>
    /// Utilities for patching PlayMaker FSMs to intercept item-giving states.
    /// </summary>
    public static class FSMPatcher
    {
        private static ManualLogSource? _log;

        public static void Initialize(ManualLogSource log) => _log = log;

        /// <summary>
        /// Inserts a custom FsmStateAction before the item-give state in the FSM.
        /// The action delivers the randomized item before the vanilla give logic runs.
        /// </summary>
        public static void InsertDeliverAction(PlayMakerFSM fsm, string locationName)
        {
            var triggerState = GetTriggerState(locationName);
            if (triggerState == null)
            {
                _log?.LogWarning($"[SilksongIC] No trigger state found for location: {locationName}");
                return;
            }

            var state = fsm.FsmStates.FirstOrDefault(s => s.Name == triggerState);
            if (state == null)
            {
                _log?.LogWarning($"[SilksongIC] FSM state '{triggerState}' not found on '{fsm.gameObject.name}/{fsm.FsmName}'");
                return;
            }

            // Prepend our delivery action to the state's action array
            var existing = state.Actions ?? new FsmStateAction[0];
            var updated = new FsmStateAction[existing.Length + 1];
            updated[0] = new DeliverItemAction(locationName);
            existing.CopyTo(updated, 1);
            state.Actions = updated;
            _log?.LogDebug($"[SilksongIC] Patched FSM state '{triggerState}' for location '{locationName}'");
        }

        private static string? GetTriggerState(string locationName)
        {
            if (ItemManager.Instance.Locations.TryGetValue(locationName, out var loc) &&
                loc is FSMLocation fsmLoc)
                return fsmLoc.TriggerState;
            return null;
        }
    }

    /// <summary>
    /// A PlayMaker FsmStateAction that delivers the randomized item.
    /// </summary>
    internal class DeliverItemAction : FsmStateAction
    {
        private readonly string _locationName;

        public DeliverItemAction(string locationName)
        {
            _locationName = locationName;
        }

        public override void OnEnter()
        {
            ItemManager.Instance.DeliverItem(_locationName, new GiveInfo(
                LocationName: _locationName,
                Fling: FlingType.Gentle,
                Container: nameof(FSMLocation)
            ));
            Finish();
        }
    }
}
