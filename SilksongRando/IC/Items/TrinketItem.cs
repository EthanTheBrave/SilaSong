using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Generic inventory item — Rosary pieces, Bone Necklace, Seal Chit, Ward Key, etc.
    ///
    /// CollectableItemManager.GetItemByName(string) looks up the asset in the game's
    /// master list (confirmed from decompiled Assembly-CSharp).
    /// CollectableItemManager.AddItem(CollectableItem, int) adds it to the player inventory.
    /// </summary>
    public class TrinketItem : AbstractItem
    {
        /// <summary>The CollectableItem asset name.</summary>
        public string ItemAssetName { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            var item = CollectableItemManager.GetItemByName(ItemAssetName);
            if (item != null)
            {
                CollectableItemManager.AddItem(item, 1);
                RandoPlugin.Logger.LogInfo($"[TrinketItem] Added: {ItemAssetName}");
            }
            else
            {
                RandoPlugin.Logger.LogWarning($"[TrinketItem] Item not found in master list: {ItemAssetName}");
            }
        }

        public override bool AlreadyObtained()
        {
            var item = CollectableItemManager.GetItemByName(ItemAssetName);
            return item != null && !item.CanGetMore();
        }
    }
}
