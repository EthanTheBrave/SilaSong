using HarmonyLib;

namespace SilksongRando.Hooks
{
    /// <summary>
    /// Marks the save as a rando save when a new game is created.
    /// PlayerData.CreateNewSingleton is the public entry point that fires
    /// when the player starts a new game (confirmed from decompiled Assembly-CSharp).
    ///
    /// Rando is activated when "Enabled = true" in the BepInEx config file.
    /// </summary>
    [HarmonyPatch]
    internal static class NewGameHook
    {
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.CreateNewSingleton))]
        [HarmonyPostfix]
        static void OnNewSingleton()
        {
            if (!RandoPlugin.Instance.IsRandoEnabled)
                return;

            RandoPlugin.Instance.OnNewSave();
            RandoPlugin.Instance.SaveData.IsActive = true;
        }
    }
}
