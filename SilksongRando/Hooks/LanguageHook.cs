using HarmonyLib;

namespace SilksongRando.Hooks
{
    /// <summary>
    /// Injects item display names from SilksongIC into the game's localisation
    /// system so the in-game pickup popup shows the correct randomized item name.
    ///
    /// All rando strings live in the "Rando" sheet to avoid collisions.
    /// </summary>
    [HarmonyPatch]
    internal static class LanguageHook
    {
        private const string Sheet = "Rando";

        [HarmonyPatch(typeof(Language), nameof(Language.Has))]
        [HarmonyPrefix]
        static bool Has(string key, string sheet, ref bool __result)
        {
            if (sheet != Sheet) return true;
            __result = SilksongIC.ItemManager.Instance.Items.ContainsKey(key);
            return false;
        }

        [HarmonyPatch(typeof(Language), nameof(Language.Get))]
        [HarmonyPrefix]
        static bool Get(string key, string sheet, ref string __result)
        {
            if (sheet != Sheet) return true;

            if (SilksongIC.ItemManager.Instance.Items.TryGetValue(key, out var item))
            {
                __result = item.UIName;
                return false;
            }

            return true;
        }
    }
}
