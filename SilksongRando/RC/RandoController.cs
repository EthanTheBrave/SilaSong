using System;
using System.Collections.Generic;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Randomization;
using SilksongIC;
using SilksongRando.Settings;

namespace SilksongRando.RC
{
    public class RandoController
    {
        private readonly RandoSettings _settings;
        private readonly RandoSaveData _saveData;
        private Dictionary<string, string> _placements = new();

        // Kept alive to answer IsReachable() queries during a session
        private LogicManager? _lm;
        private ProgressionManager? _pm;

        public IReadOnlyDictionary<string, string> Placements => _placements;

        public RandoController(RandoSettings settings, RandoSaveData saveData)
        {
            _settings = settings;
            _saveData  = saveData;
        }

        // ── Run (new game) ────────────────────────────────────────────────

        public void Run()
        {
            _lm = RCData.GetLogicManager();
            var ctx = BuildContext(_lm);

            _placements = Randomize(ctx, _settings.Seed);
            RandoPlugin.Logger.LogInfo($"[RandoController] Done. {_placements.Count} placements.");

            // Build a fresh ProgressionManager for reachability queries
            RebuildProgressionManager();
        }

        // ── Reachability ─────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the given location is currently reachable
        /// based on items the player has already collected.
        /// </summary>
        public bool IsReachable(string locationName)
        {
            if (_lm == null || _pm == null) return false;
            try
            {
                var loc = _lm.GetLocationStrict(locationName);
                return loc.CanGet(_pm);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Rebuild the ProgressionManager from collected items in SaveData.</summary>
        public void RebuildProgressionManager()
        {
            if (_lm == null) return;

            _pm = new ProgressionManager(_lm, null);

            // Add all items the player has collected
            foreach (var locName in _saveData.CollectedLocations)
            {
                if (!_placements.TryGetValue(locName, out var itemName)) continue;
                var item = _lm.GetItemStrict(itemName);
                _pm.Add(item);
            }

            // Add start items
            foreach (var itemName in _saveData.StartItems)
            {
                var item = _lm.GetItemStrict(itemName);
                _pm.Add(item);
            }
        }

        // ── Context building ──────────────────────────────────────────────

        private RandoContext BuildContext(LogicManager lm)
        {
            var rb = new RequestBuilder(_settings, lm);

            if (_settings.Pools.Abilities)   rb.AddPool("Abilities");
            if (_settings.Pools.Melodies)    rb.AddPool("Melodies");
            if (_settings.Pools.Bellshrines) rb.AddPool("Bells");
            if (_settings.Pools.HeartPieces) rb.AddPool("HeartPieces");
            if (_settings.Pools.Mossberries) rb.AddPool("Mossberries");
            if (_settings.Pools.PollipBulbs) rb.AddPool("PollipBulbs");
            if (_settings.Pools.ShopItems)   rb.AddPool("ShopItems");
            if (_settings.Pools.GeoRocks)    rb.AddPool("GeoRocks");
            if (_settings.Pools.Crests)      rb.AddPool("Crests");

            foreach (var item in _settings.Start.StartItems)
                rb.AddStartItem(item);

            // Allow connection mods to inject custom content
            var connCtx = new Connections.ConnectionContext(rb);
            Connections.ConnectionsAPI.ApplyAll(connCtx);

            return rb.Build();
        }

        // ── Algorithm ─────────────────────────────────────────────────────

        private static Dictionary<string, string> Randomize(RandoContext ctx, int seed)
        {
            var rng        = new Random(seed);
            var randomizer = new Randomizer(rng, ctx);
            var groups     = randomizer.Run();

            var result = new Dictionary<string, string>();
            foreach (var group in groups)
                foreach (var placement in group.Placements)
                    result[placement.Location.Name] = placement.Item.Name;

            return result;
        }
    }
}
