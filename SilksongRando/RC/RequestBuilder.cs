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
    /// RandomizationStage list that RandomizerCore will shuffle.
    /// </summary>
    public class RequestBuilder
    {
        private readonly RandoSettings _settings;
        private readonly LogicManager  _lm;

        private readonly List<string> _enabledPools = new List<string>();
        private readonly List<string> _startItems   = new List<string>();

        // Extra items/locations injected by connection mods
        private readonly Dictionary<string, List<string>> _extraItemsByPool     = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> _extraLocationsByPool = new Dictionary<string, List<string>>();

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

        public (SilksongRandoContext ctx, RandomizationStage[] stages) Build()
        {
            var stages = new List<RandomizationStage>();

            foreach (var pool in _pools!)
            {
                if (!_enabledPools.Contains(pool.name)) continue;

                var items     = new List<IRandoItem>();
                var locations = new List<IRandoLocation>();

                foreach (var iName in pool.items)
                {
                    if (RemoveOneStartItem(iName)) continue;
                    if (!_lm.ItemLookup.TryGetValue(iName, out var logicItem)) continue;
                    items.Add(new RandoItem { item = logicItem });
                }

                foreach (var lName in pool.locations)
                {
                    if (!_lm.LogicLookup.TryGetValue(lName, out var logicDef)) continue;
                    locations.Add(new RandoLocation { logic = logicDef });
                }

                if (items.Count == 0 || locations.Count == 0) continue;

                stages.Add(new RandomizationStage
                {
                    label  = pool.name,
                    groups = new[]
                    {
                        new RandomizationGroup
                        {
                            Label     = pool.name,
                            Items     = items.ToArray(),
                            Locations = locations.ToArray(),
                            Strategy  = new DefaultGroupPlacementStrategy(0f),
                        }
                    },
                    strategy = new StagePlacementStrategy(),
                });
            }

            // Initial progression from start items
            var ctx = new SilksongRandoContext(_lm);
            if (_startItems.Count > 0)
            {
                // Build a compound logic item for start items using first matching item
                // For simplicity: apply them to the ProgressionManager externally in RandoController
                // The ILogicItem on InitialProgression can be null-like; we leave it null
            }

            return (ctx, stages.ToArray());
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
                ?? throw new System.InvalidOperationException($"Embedded resource not found: {fullName}");
            using var reader = new StreamReader(stream);
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd())
                ?? throw new System.InvalidOperationException($"Failed to deserialize: {fullName}");
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

    /// <summary>
    /// Minimal concrete subclass of RandoContext for SilksongRando.
    /// </summary>
    public class SilksongRandoContext : RandoContext
    {
        public SilksongRandoContext(LogicManager lm) : base(lm) { }

        public override System.Collections.Generic.IEnumerable<GeneralizedPlacement> EnumerateExistingPlacements()
        {
            yield break; // No pre-existing placements
        }
    }
}
