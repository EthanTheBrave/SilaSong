using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Randomization;
using SilksongRando.Settings;

namespace SilksongRando.RC
{
    /// <summary>
    /// Reads items/locations/pools from embedded JSON and assembles the
    /// RandomizationGroup list that RandomizerCore will shuffle.
    /// </summary>
    public class RequestBuilder
    {
        private readonly RandoSettings _settings;
        private readonly LogicManager  _lm;

        private readonly List<string> _enabledPools = new();
        private readonly List<string> _startItems   = new();

        // Extra items/locations injected by connection mods
        private readonly Dictionary<string, List<string>> _extraItemsByPool     = new();
        private readonly Dictionary<string, List<string>> _extraLocationsByPool = new();

        private static PoolData[]?     _pools;
        private static ItemData[]?     _items;
        private static LocationData[]? _locations;

        public RequestBuilder(RandoSettings settings, LogicManager lm)
        {
            _settings = settings;
            _lm       = lm;
            EnsureLoaded();
        }

        public void AddPool(string name)      => _enabledPools.Add(name);
        public void AddStartItem(string name) => _startItems.Add(name);

        public void AddItemToPool(string itemName, string poolName, string logic)
        {
            if (!_extraItemsByPool.TryGetValue(poolName, out var list))
                _extraItemsByPool[poolName] = list = new List<string>();
            list.Add(itemName);
        }

        public void AddLocationToPool(string locationName, string poolName, string logic)
        {
            if (!_extraLocationsByPool.TryGetValue(poolName, out var list))
                _extraLocationsByPool[poolName] = list = new List<string>();
            list.Add(locationName);
        }

        public RandoContext Build()
        {
            var groups = new List<RandomizationGroup>();

            foreach (var pool in _pools!)
            {
                if (!_enabledPools.Contains(pool.name)) continue;

                var items     = new List<IRandoItem>();
                var locations = new List<IRandoLocation>();

                foreach (var iName in pool.items)
                {
                    // Start items are pre-collected — skip them from the shuffle pool
                    if (RemoveOneStartItem(iName)) continue;
                    items.Add(new RandoItem(_lm.GetItemStrict(iName)));
                }

                foreach (var lName in pool.locations)
                    locations.Add(new RandoLocation(_lm.GetLocationStrict(lName)));

                if (items.Count > 0 && locations.Count > 0)
                    groups.Add(new RandomizationGroup
                    {
                        Label     = pool.name,
                        Items     = items.ToArray(),
                        Locations = locations.ToArray(),
                        Strategy  = new DefaultGroupPlacementStrategy(new DefaultGroupPlacementStrategy.PlacementOptions()),
                    });
            }

            // Build start state: one term increment per start item
            var pm = new ProgressionManager(_lm, null);
            foreach (var name in _startItems)
            {
                var item = _lm.GetItemStrict(name);
                pm.Add(item);
            }

            return new RandoContext(_lm)
            {
                Groups     = groups.ToArray(),
                InitialProgressionManager = pm,
            };
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private bool RemoveOneStartItem(string name)
        {
            int idx = _startItems.IndexOf(name);
            if (idx < 0) return false;
            _startItems.RemoveAt(idx);
            return true;
        }

        // ── Data loading ──────────────────────────────────────────────────

        private static void EnsureLoaded()
        {
            if (_pools != null) return;
            _pools     = Read<PoolData[]>("Data.pools.json");
            _items     = Read<ItemData[]>("Data.items.json");
            _locations = Read<LocationData[]>("Data.locations.json");
        }

        private static T Read<T>(string name)
        {
            var asm      = Assembly.GetExecutingAssembly();
            var fullName = $"SilksongRando.Resources.{name}";
            using var stream = asm.GetManifestResourceStream(fullName)
                ?? throw new InvalidOperationException($"Embedded resource not found: {fullName}");
            using var reader = new StreamReader(stream);
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd())
                ?? throw new InvalidOperationException($"Failed to deserialize: {fullName}");
        }

        // ── JSON schemas ──────────────────────────────────────────────────

#pragma warning disable CS8618
        private class PoolData
        {
            public string   name;
            public string[] items;
            public string[] locations;
        }
        private class ItemData
        {
            public string  name;
            public string  type;
            public string? uiName;
        }
        private class LocationData
        {
            public string  name;
            public string? scene;
            public string  locationType;
        }
#pragma warning restore CS8618
    }
}
