using HarmonyLib;

namespace SilksongRando.Hooks
{
    /// <summary>
    /// Marks the save as a rando save when a new game is created.
    /// PlayerData.CreateNewSingleton is the public entry point that fires
    /// when the player starts a new game (confirmed from decompiled Assembly-CSharp).
    /// </summary>
    [HarmonyPatch]
    internal static class NewGameHook
    {
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.CreateNewSingleton))]
        [HarmonyPostfix]
        static void OnNewSingleton()
        {
            if (Silksong.GameModeManager.GameModeManager.CurrentMode?.Id != "Randomiser")
                return;

            RandoPlugin.Instance.SaveData.IsActive = true;

            if (RandoPlugin.Instance.Settings.Seed == 0)
                RandoPlugin.Instance.Settings.Seed = (int)(
                    System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() & 0x7FFFFFFF);
        }
    }
}
