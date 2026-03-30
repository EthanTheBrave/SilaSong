using HarmonyLib;
using System;

namespace SilksongRando.Hooks
{
    /// <summary>
    /// Hooks GameManager.BeginSceneTransition to trigger randomization
    /// on the first scene load of a new rando game.
    /// BeginSceneTransition(SceneLoadInfo) is the confirmed public entry point
    /// from decompiled Assembly-CSharp.
    /// </summary>
    [HarmonyPatch]
    internal static class GameManagerHook
    {
        private static bool _randoStarted;

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.BeginSceneTransition))]
        [HarmonyPrefix]
        static void OnBeginSceneTransition(GameManager.SceneLoadInfo info)
        {
            if (!RandoPlugin.Instance.IsRandoActive) return;
            if (_randoStarted) return;
            if (RandoPlugin.Instance.SaveData.Placements.Count > 0) return; // loaded save

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

        /// <summary>
        /// Reset when a new game is started so a fresh rando can be generated.
        /// Hooks PlayerData.CreateNewSingleton since GameManager has no ClearSaveFile.
        /// </summary>
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.CreateNewSingleton))]
        [HarmonyPostfix]
        static void OnNewGame()
        {
            _randoStarted = false;
        }
    }
}
