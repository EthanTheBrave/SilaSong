using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Grants one of the three melodies (Vault, Architect, Conductor).
    ///
    /// Melodies are CollectableItems tracked by CollectableItemManager.
    /// CollectableItemManager.AddItem(CollectableItem, int) is the public static
    /// method confirmed from decompiled Assembly-CSharp.
    /// We look up the CollectableItem asset by name from the master list.
    /// </summary>
    public class MelodyItem : AbstractItem
    {
        /// <summary>The CollectableItem asset name for this melody.</summary>
        public string ItemAssetName { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            var item = GetCollectableItem();
            if (item != null)
            {
                CollectableItemManager.AddItem(item, 1);
                RandoPlugin.Logger.LogInfo($"[MelodyItem] Added collectable: {ItemAssetName}");
            }
            else
            {
                RandoPlugin.Logger.LogWarning($"[MelodyItem] CollectableItem not found: {ItemAssetName}");
            }
        }

        public override bool AlreadyObtained()
        {
            var item = GetCollectableItem();
            return item != null && !item.CanGetMore();
        }

        private CollectableItem? GetCollectableItem()
        {
            foreach (var ci in CollectableItemManager.GetCollectedItems())
                if (ci.name == ItemAssetName) return ci;

            // Also check master list via Resources if not yet collected
            return UnityEngine.Resources.Load<CollectableItem>(ItemAssetName);
        }
    }
}
