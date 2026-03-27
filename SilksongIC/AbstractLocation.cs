using System.Collections.Generic;

namespace SilksongIC
{
    /// <summary>
    /// Represents a location in the world where an item can be placed.
    /// Subclass this to define how a specific type of pickup is intercepted.
    /// </summary>
    public abstract class AbstractLocation
    {
        public string Name { get; init; } = string.Empty;

        /// <summary>The scene (room) this location is in.</summary>
        public string SceneName { get; init; } = string.Empty;

        /// <summary>
        /// Called once when the scene containing this location loads.
        /// Hook game objects here to intercept the pickup.
        /// </summary>
        public abstract void OnSceneLoad(LocationLoadContext ctx);

        /// <summary>
        /// Delivers the randomized item when the player triggers this location.
        /// Call this from your hook when the original pickup fires.
        /// </summary>
        protected void DeliverItem(string locationName)
        {
            ItemManager.Instance.DeliverItem(locationName, new GiveInfo(
                LocationName: locationName,
                Fling: FlingType.Gentle,
                Container: GetType().Name
            ));
        }

        /// <summary>Extra tags for connection mods or special behavior.</summary>
        public List<ILocationTag> Tags { get; } = new();

        public T? GetTag<T>() where T : ILocationTag
        {
            foreach (var tag in Tags)
                if (tag is T t) return t;
            return default;
        }

        public override string ToString() => Name;
    }

    /// <summary>Context provided to OnSceneLoad containing Unity scene info.</summary>
    public class LocationLoadContext
    {
        public string SceneName { get; init; } = string.Empty;
    }

    public interface ILocationTag { }
}
