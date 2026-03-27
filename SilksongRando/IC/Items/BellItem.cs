using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// One of the five Bells required to unlock Act 2.
    /// Stored as a collectable item in CollectableItemManager.
    /// </summary>
    public class BellItem : AbstractItem
    {
        public string BellId { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            CollectableItemManager.instance.AddItemToMasterList(BellId);
            RandoPlugin.Logger.LogInfo($"[BellItem] Gave bell: {BellId}");
        }

        public override bool AlreadyObtained()
        {
            return CollectableItemManager.instance.IsItemInMasterList(BellId);
        }
    }
}
