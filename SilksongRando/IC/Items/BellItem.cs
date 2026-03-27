using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// One of the five Bells required to unlock Act 2.
    /// Same pattern as TrinketItem — CollectableItem added via CollectableItemManager.AddItem.
    /// </summary>
    public class BellItem : AbstractItem
    {
        public string ItemAssetName { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            var item = UnityEngine.Resources.Load<CollectableItem>(ItemAssetName);
            if (item != null)
            {
                CollectableItemManager.AddItem(item, 1);
                RandoPlugin.Logger.LogInfo($"[BellItem] Added bell: {ItemAssetName}");
            }
            else
            {
                RandoPlugin.Logger.LogWarning($"[BellItem] Asset not found: {ItemAssetName}");
            }
        }

        public override bool AlreadyObtained()
        {
            var item = UnityEngine.Resources.Load<CollectableItem>(ItemAssetName);
            return item != null && !item.CanGetMore();
        }
    }
}
