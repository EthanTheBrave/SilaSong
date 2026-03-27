using HarmonyLib;
using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Grants one of the three melodies (Vault, Architect, Conductor).
    ///
    /// Melodies are tracked by CollectableItemManager. We add the melody ID
    /// directly to the master list so the game treats it as collected.
    /// This mirrors the approach in Silksong-Rando's CollectableItemManager hook.
    /// </summary>
    public class MelodyItem : AbstractItem
    {
        public string MelodyId { get; init; } = string.Empty;

        public override void GiveItem(GiveInfo info)
        {
            // Add to CollectableItemManager's persistent collected set
            CollectableItemManager.instance.AddItemToMasterList(MelodyId);

            // Also set a PlayerData flag so UI correctly shows it
            PlayerData.instance.SetBool($"hasCollected_{MelodyId}", true);

            RandoPlugin.Logger.LogInfo($"[MelodyItem] Gave melody: {MelodyId}");
        }

        public override bool AlreadyObtained()
        {
            return CollectableItemManager.instance.IsItemInMasterList(MelodyId);
        }
    }
}
