using HarmonyLib;
using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Grants a movement/combat ability (Silkspear, Sprint, Walljump, etc).
    ///
    /// Based on Silksong-Rando's observation that abilities are stored as bools
    /// in PlayerData under names like "hasAbility_Sprint". Exact field names must
    /// be confirmed from the decompiled assembly.
    /// </summary>
    public class AbilityItem : AbstractItem
    {
        public string AbilityId { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            // Set the ability flag in PlayerData.
            // Field naming follows the pattern observed in Silksong-Rando hooks.
            var fieldName = $"hasAbility_{AbilityId}";
            PlayerData.instance.SetBool(fieldName, true);

            RandoPlugin.Logger.LogInfo($"[AbilityItem] Gave ability: {AbilityId} (field: {fieldName})");
        }

        public override bool AlreadyObtained()
        {
            return PlayerData.instance.GetBool($"hasAbility_{AbilityId}");
        }
    }
}
