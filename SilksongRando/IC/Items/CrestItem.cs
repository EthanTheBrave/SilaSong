using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Unlocks an equipment crest.
    /// ToolItemManager.GetCrestByName(string) and ToolCrest.Unlock() confirmed
    /// from decompiled Assembly-CSharp. IsUnlocked property also confirmed.
    /// </summary>
    public class CrestItem : AbstractItem
    {
        public string CrestName { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            var crest = ToolItemManager.GetCrestByName(CrestName);
            if (crest != null)
            {
                crest.Unlock();
                RandoPlugin.Logger.LogInfo($"[CrestItem] Unlocked crest: {CrestName}");
            }
            else
            {
                RandoPlugin.Logger.LogWarning($"[CrestItem] Crest not found: {CrestName}");
            }
        }

        public override bool AlreadyObtained()
        {
            var crest = ToolItemManager.GetCrestByName(CrestName);
            return crest?.IsUnlocked ?? false;
        }
    }
}
