using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Grants a movement/combat ability.
    ///
    /// Field names confirmed from decompiled Assembly-CSharp.dll:
    ///   hasSilkSpecial  = Silkspear (silk special projectile)
    ///   hasDash         = Sprint/Dash
    ///   hasWalljump     = Walljump
    ///   hasNeedolin     = Needolin
    ///   hasThreadSphere = Silk Sphere
    ///   hasBrolly       = Brolly
    ///   hasSilkCharge   = Silk Charge (mapped to Faydown ability unlock)
    /// </summary>
    public class AbilityItem : AbstractItem
    {
        /// <summary>The PlayerData bool field name for this ability.</summary>
        public string PlayerDataField { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            PlayerData.instance.SetBool(PlayerDataField, true);
            RandoPlugin.Logger.LogInfo($"[AbilityItem] Set {PlayerDataField} = true");
        }

        public override bool AlreadyObtained() =>
            PlayerData.instance.GetBool(PlayerDataField);
    }
}
