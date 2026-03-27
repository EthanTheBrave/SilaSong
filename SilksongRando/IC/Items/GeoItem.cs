using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>Grants a fixed amount of geo (currency).</summary>
    public class GeoItem : AbstractItem
    {
        public int Amount { get; init; }

        public override void GiveItem(GiveInfo info)
        {
            // HeroController.instance.AddGeo is the standard HK pattern;
            // Silksong likely uses the same or a renamed equivalent.
            HeroController.instance.AddGeo(Amount);
            RandoPlugin.Logger.LogInfo($"[GeoItem] Gave {Amount} geo.");
        }
    }
}
