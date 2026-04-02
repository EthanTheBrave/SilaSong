using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// One of the three melodies (Vault, Architect, Conductor).
    ///
    /// Melodies are CollectableItems tracked by CollectableItemManager.
    /// GetItemByName resolves the asset from the game's master ScriptableObject list.
    /// </summary>
    public class MelodyItem : AbstractItem
    {
        /// <summary>The CollectableItem asset name for this melody.</summary>
        public string ItemAssetName { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            var item = CollectableItemManager.GetItemByName(ItemAssetName);
            if (item != null)
            {
                CollectableItemManager.AddItem(item, 1);
                RandoPlugin.Logger.LogInfo($"[MelodyItem] Added melody: {ItemAssetName}");
            }
            else
            {
                RandoPlugin.Logger.LogWarning($"[MelodyItem] Melody not found in master list: {ItemAssetName}");
            }
        }

        public override bool AlreadyObtained()
        {
            var item = CollectableItemManager.GetItemByName(ItemAssetName);
            return item != null && !item.CanGetMore();
        }
    }
}
