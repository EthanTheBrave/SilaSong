using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace SilksongIC
{
    /// <summary>
    /// Central registry for items and locations.
    /// Other mods (connection mods) can register additional items/locations here.
    /// </summary>
    public class ItemManager
    {
        public static ItemManager Instance { get; private set; } = new();

        private readonly Dictionary<string, AbstractItem> _items = new();
        private readonly Dictionary<string, AbstractLocation> _locations = new();

        // locationName -> itemName mapping set by the randomizer before a run
        private Dictionary<string, string> _placements = new();

        // Which locations have already been collected this save
        private HashSet<string> _collected = new();

        private ManualLogSource? _log;

        public static void Initialize(ManualLogSource log)
        {
            Instance = new ItemManager();
            Instance._log = log;
        }

        // ── Registration ────────────────────────────────────────────────────

        public void RegisterItem(AbstractItem item)
        {
            if (_items.ContainsKey(item.Name))
                _log?.LogWarning($"[SilksongIC] Duplicate item registered: {item.Name}");
            else
                _items[item.Name] = item;
        }

        public void RegisterLocation(AbstractLocation location)
        {
            if (_locations.ContainsKey(location.Name))
                _log?.LogWarning($"[SilksongIC] Duplicate location registered: {location.Name}");
            else
                _locations[location.Name] = location;
        }

        public IReadOnlyDictionary<string, AbstractItem> Items => _items;
        public IReadOnlyDictionary<string, AbstractLocation> Locations => _locations;

        // ── Placement (set by randomizer) ────────────────────────────────────

        /// <summary>Apply a full placement map: location → item name.</summary>
        public void ApplyPlacements(Dictionary<string, string> placements, HashSet<string> alreadyCollected)
        {
            _placements = placements;
            _collected = alreadyCollected;
        }

        /// <summary>Returns the item placed at a location, or null if not randomized.</summary>
        public AbstractItem? GetPlacedItem(string locationName)
        {
            if (_placements.TryGetValue(locationName, out var itemName) &&
                _items.TryGetValue(itemName, out var item))
                return item;
            return null;
        }

        // ── Delivery ─────────────────────────────────────────────────────────

        /// <summary>
        /// Called by a location hook when the player triggers a pickup.
        /// Delivers the randomized item (or vanilla item if not randomized).
        /// </summary>
        public void DeliverItem(string locationName, GiveInfo info)
        {
            if (_collected.Contains(locationName))
            {
                _log?.LogDebug($"[SilksongIC] Location already collected, skipping: {locationName}");
                return;
            }

            var item = GetPlacedItem(locationName);
            if (item == null)
            {
                _log?.LogWarning($"[SilksongIC] No item placed at location: {locationName}");
                return;
            }

            _log?.LogInfo($"[SilksongIC] Giving '{item.Name}' from '{locationName}'");
            item.GiveItem(info);
            _collected.Add(locationName);
            OnItemCollected?.Invoke(locationName, item);
        }

        public bool IsCollected(string locationName) => _collected.Contains(locationName);

        // ── Events ───────────────────────────────────────────────────────────

        /// <summary>Fires after an item is delivered. Use for UI notifications, logging, etc.</summary>
        public event Action<string, AbstractItem>? OnItemCollected;
    }
}
