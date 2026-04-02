using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// One of the six Bells required to unlock Act 2.
    /// Uses CollectableItemManager.GetItemByName to resolve the asset from the game's
    /// master list, then AddItem to give it to the player.
    /// </summary>
    public class BellItem : AbstractItem
    {
        public string ItemAssetName { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            var item = CollectableItemManager.GetItemByName(ItemAssetName);
            if (item != null)
            {
                CollectableItemManager.AddItem(item, 1);
                RandoPlugin.Logger.LogInfo($"[BellItem] Added bell: {ItemAssetName}");
            }
            else
            {
                RandoPlugin.Logger.LogWarning($"[BellItem] Bell not found in master list: {ItemAssetName}");
            }
        }

        public override bool AlreadyObtained()
        {
            var item = CollectableItemManager.GetItemByName(ItemAssetName);
            return item != null && !item.CanGetMore();
        }
    }
}
