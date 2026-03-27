using HarmonyLib;
using System;

namespace SilksongRando.Hooks
{
    /// <summary>
    /// Hooks into GameManager to:
    ///   • Start randomization on new game (first scene transition)
    ///   • Clear rando state when a save file is wiped
    /// </summary>
    [HarmonyPatch]
    internal static class GameManagerHook
    {
        private static bool _randoStarted;

        /// <summary>
        /// The first BeginSceneTransition on a new rando save triggers randomization.
        /// We wait until here (rather than SetupNewPlayerData) because DataManager
        /// may not have finished initializing ISaveDataMod instances at that point.
        /// </summary>
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.BeginSceneTransition))]
        [HarmonyPrefix]
        static void OnBeginSceneTransition()
        {
            if (!RandoPlugin.Instance.IsRandoActive) return;
            if (_randoStarted) return;
            if (RandoPlugin.Instance.SaveData.Placements.Count > 0) return; // loaded from save

            try
            {
                _randoStarted = true;
                RandoPlugin.Instance.StartRando();
            }
            catch (Exception ex)
            {
                RandoPlugin.Logger.LogError($"[GameManagerHook] Failed to start rando: {ex}");
            }
        }

        /// <summary>Reset flag when a save file is cleared.</summary>
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.ClearSaveFile))]
        [HarmonyPostfix]
        static void OnClearSaveFile()
        {
            _randoStarted = false;
        }
    }
}
