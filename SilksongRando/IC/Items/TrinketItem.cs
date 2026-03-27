using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Generic inventory item — Rosary pieces, Bone Necklace, Seal Chit, Ward Key, etc.
    /// Uses CollectableItemManager to add the item by ID, which is how the existing
    /// Silksong-Rando mod handles standard pickups.
    /// </summary>
    public class TrinketItem : AbstractItem
    {
        public string ItemId { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            CollectableItemManager.instance.AddItemToMasterList(ItemId);
            RandoPlugin.Logger.LogInfo($"[TrinketItem] Gave trinket: {ItemId}");
        }

        public override bool AlreadyObtained()
        {
            return CollectableItemManager.instance.IsItemInMasterList(ItemId);
        }
    }
}
