using System.Collections.Generic;

namespace SilksongIC
{
    /// <summary>
    /// Represents an item that can be placed at any location.
    /// Subclass this to define how a specific item is given to the player.
    /// </summary>
    public abstract class AbstractItem
    {
        public string Name { get; init; } = string.Empty;

        /// <summary>Display name shown in UI / pickup messages.</summary>
        public string UIName { get; init; } = string.Empty;

        /// <summary>Sprite key used to look up the item's icon.</summary>
        public string? SpriteKey { get; init; }

        /// <summary>
        /// Called when the player collects this item at a location.
        /// This is where you apply the item effect (unlock ability, add to inventory, etc).
        /// </summary>
        public abstract void GiveItem(GiveInfo info);

        /// <summary>
        /// Returns true if the player already has this item (used for duplicate handling).
        /// </summary>
        public virtual bool AlreadyObtained() => false;

        /// <summary>
        /// Extra tags for connection mods or special behavior hints.
        /// </summary>
        public List<IItemTag> Tags { get; } = new();

        public T? GetTag<T>() where T : IItemTag
        {
            foreach (var tag in Tags)
                if (tag is T t) return t;
            return default;
        }

        public override string ToString() => Name;
    }

    /// <summary>Context passed to GiveItem to describe how/where the item was collected.</summary>
    public record GiveInfo(
        string LocationName,
        FlingType Fling,
        string? Container
    );

    public enum FlingType
    {
        /// <summary>Item pops directly into player inventory with no animation.</summary>
        Immediate,
        /// <summary>Item spawns as a collectible that flies toward the player.</summary>
        Gentle,
    }

    public interface IItemTag { }
}
