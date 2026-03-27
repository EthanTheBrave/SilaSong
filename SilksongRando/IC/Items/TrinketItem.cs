using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Generic inventory item — Rosary pieces, Bone Necklace, Seal Chit, Ward Key, etc.
    ///
    /// CollectableItemManager.AddItem(CollectableItem, int) is the confirmed
    /// public static method. Items are loaded by asset name via Resources.Load.
    /// </summary>
    public class TrinketItem : AbstractItem
    {
        /// <summary>The CollectableItem ScriptableObject asset name.</summary>
        public string ItemAssetName { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            var item = UnityEngine.Resources.Load<CollectableItem>(ItemAssetName);
            if (item != null)
            {
                CollectableItemManager.AddItem(item, 1);
                RandoPlugin.Logger.LogInfo($"[TrinketItem] Added: {ItemAssetName}");
            }
            else
            {
                RandoPlugin.Logger.LogWarning($"[TrinketItem] Asset not found: {ItemAssetName}");
            }
        }

        public override bool AlreadyObtained()
        {
            var item = UnityEngine.Resources.Load<CollectableItem>(ItemAssetName);
            return item != null && !item.CanGetMore();
        }
    }
}
