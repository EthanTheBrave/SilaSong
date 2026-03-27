using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Grants a fixed amount of geo (currency).
    /// PlayerData.geo is the confirmed field name from decompiled Assembly-CSharp.
    /// </summary>
    public class GeoItem : AbstractItem
    {
        public int Amount { get; init; }

        public override void GiveItem(GiveInfo info)
        {
            PlayerData.instance.IncrementInt("geo");
            // IncrementInt only adds 1, so use SetInt for arbitrary amounts
            int current = PlayerData.instance.GetInt("geo");
            PlayerData.instance.SetInt("geo", current + Amount);
            RandoPlugin.Logger.LogInfo($"[GeoItem] Added {Amount} geo.");
        }
    }
}
