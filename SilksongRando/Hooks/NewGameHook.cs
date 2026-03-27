using HarmonyLib;

namespace SilksongRando.Hooks
{
    /// <summary>
    /// Marks the save as a rando save when "Randomiser" game mode is selected
    /// and a new game begins.
    /// </summary>
    [HarmonyPatch]
    internal static class NewGameHook
    {
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.SetupNewPlayerData))]
        [HarmonyPostfix]
        static void OnNewPlayerData()
        {
            // Only activate if the player selected the Randomiser game mode.
            // GameModeManager.CurrentMode is set before SetupNewPlayerData fires.
            if (Silksong.GameModeManager.GameModeManager.CurrentMode?.Id != "Randomiser")
                return;

            RandoPlugin.Instance.SaveData.IsActive = true;

            // Apply seed from settings (set via seed menu or default random)
            if (RandoPlugin.Instance.Settings.Seed == 0)
                RandoPlugin.Instance.Settings.Seed = (int)(
                    System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() & 0x7FFFFFFF);
        }
    }
}
