using System;
using System.Collections.Generic;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Randomization;
using SilksongRando.Settings;

namespace SilksongRando.RC
{
    public class RandoController
    {
        private readonly RandoSettings _settings;
        private readonly RandoSaveData _saveData;
        private Dictionary<string, string> _placements = new Dictionary<string, string>();

        // Kept alive to answer IsReachable() queries during a session
        private LogicManager? _lm;
        private ProgressionManager? _pm;
        private List<string> _startItemNames = new List<string>();

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
            var (ctx, stages) = BuildContext(_lm);

            _placements = Randomize(ctx, stages, _settings.Seed);
            RandoPlugin.Logger.LogInfo($"[RandoController] Done. {_placements.Count} placements.");

            RebuildProgressionManager();
        }

        // ── Reachability ─────────────────────────────────────────────────

        public bool IsReachable(string locationName)
        {
            if (_lm == null || _pm == null) return false;
            try
            {
                if (!_lm.LogicLookup.TryGetValue(locationName, out var loc))
                    return false;
                return loc.CanGet(_pm);
            }
            catch
            {
                return false;
            }
        }

        public void RebuildProgressionManager()
        {
            if (_lm == null) return;

            _pm = new ProgressionManager(_lm, null);

            foreach (var locName in _saveData.CollectedLocations)
            {
                if (!_placements.TryGetValue(locName, out var itemName)) continue;
                if (_lm.ItemLookup.TryGetValue(itemName, out var item))
                    _pm.Add(item);
            }

            foreach (var itemName in _startItemNames)
            {
                if (_lm.ItemLookup.TryGetValue(itemName, out var item))
                    _pm.Add(item);
            }
        }

        // ── Context building ──────────────────────────────────────────────

        private (SilksongRandoContext ctx, RandomizationStage[] stages) BuildContext(LogicManager lm)
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
            if (_settings.Pools.Crests)        rb.AddPool("Crests");
            if (_settings.Pools.Collectables)  rb.AddPool("Collectables");

            foreach (var item in _settings.Start.StartItems)
                rb.AddStartItem(item);

            var connCtx = new Connections.ConnectionContext(rb);
            Connections.ConnectionsAPI.ApplyAll(connCtx);

            _startItemNames = new List<string>(_settings.Start.StartItems);
            return rb.Build();
        }

        // ── Algorithm ─────────────────────────────────────────────────────

        private static Dictionary<string, string> Randomize(
            SilksongRandoContext ctx, RandomizationStage[] stages, int seed)
        {
            var rng        = new Random(seed);
            var randomizer = new Randomizer(rng, ctx, stages);
            var allResults = randomizer.Run();

            var result = new Dictionary<string, string>();
            foreach (var stageResult in allResults)
                foreach (var groupPlacements in stageResult)
                    foreach (var p in groupPlacements)
                        result[p.Location.Name] = p.Item.Name;

            return result;
        }
    }
}
