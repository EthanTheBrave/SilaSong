using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Equipment crest (tool/weapon slot). Unlocks a crest in the player's loadout.
    /// Based on SilksongSimpleRando's use of ToolItemManager.GetAllCrests().
    /// </summary>
    public class CrestItem : AbstractItem
    {
        public string CrestId { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            // Unlock the crest via ToolItemManager so it appears in the loadout screen
            var equipData = ToolItemManager.instance.GetCrestEquipData(CrestId);
            if (equipData != null)
            {
                equipData.unlocked = true;
                RandoPlugin.Logger.LogInfo($"[CrestItem] Unlocked crest: {CrestId}");
            }
            else
            {
                RandoPlugin.Logger.LogWarning($"[CrestItem] Crest not found: {CrestId}");
            }
        }

        public override bool AlreadyObtained()
        {
            var equipData = ToolItemManager.instance.GetCrestEquipData(CrestId);
            return equipData?.unlocked ?? false;
        }
    }
}
