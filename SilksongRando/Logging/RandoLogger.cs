using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SilksongRando.Logging
{
    /// <summary>
    /// Writes spoiler log and tracker log to %APPDATA%\SilksongRando\Logs\
    /// after randomization completes.
    /// </summary>
    public class RandoLogger
    {
        private readonly int _seed;
        private readonly IReadOnlyDictionary<string, string> _placements;

        private static readonly string LogDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SilksongRando", "Logs");

        public RandoLogger(int seed, IReadOnlyDictionary<string, string> placements)
        {
            _seed       = seed;
            _placements = placements;
        }

        public void Write()
        {
            Directory.CreateDirectory(LogDir);
            WriteSpoilerLog();
            WriteTrackerLog();
        }

        // ── Spoiler log ───────────────────────────────────────────────────

        private void WriteSpoilerLog()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "SPOILER LOG");
            sb.AppendLine();

            // Group by area prefix (e.g. "Cave_01" → "Cave")
            var byArea = _placements
                .GroupBy(kv => AreaOf(kv.Key))
                .OrderBy(g => g.Key);

            foreach (var area in byArea)
            {
                sb.AppendLine($"[{area.Key}]");
                foreach (var kv in area.OrderBy(x => x.Key))
                    sb.AppendLine($"  {kv.Key,-48} <- {kv.Value}");
                sb.AppendLine();
            }

            Write(sb, "SpoilerLog");
        }

        // ── Tracker log ───────────────────────────────────────────────────

        private void WriteTrackerLog()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "TRACKER LOG");
            sb.AppendLine("(This file is overwritten every session — shows current state)");
            sb.AppendLine();
            sb.AppendLine("COLLECTED");
            sb.AppendLine("---------");

            var collected = RandoPlugin.Instance.SaveData.CollectedLocations;
            foreach (var loc in collected.OrderBy(x => x))
            {
                var item = _placements.TryGetValue(loc, out var i) ? i : "???";
                sb.AppendLine($"  {loc,-48} <- {item}");
            }

            sb.AppendLine();
            sb.AppendLine("REMAINING");
            sb.AppendLine("---------");

            foreach (var kv in _placements.OrderBy(x => x.Key))
            {
                if (collected.Contains(kv.Key)) continue;
                var reachable = RandoPlugin.Instance.Controller?.IsReachable(kv.Key) ?? false;
                var tag = reachable ? "[REACHABLE]" : "[LOCKED]   ";
                sb.AppendLine($"  {tag} {kv.Key,-44} <- {kv.Value}");
            }

            Write(sb, "TrackerLog");
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void AppendHeader(StringBuilder sb, string title)
        {
            sb.AppendLine($"SilksongRando {title}");
            sb.AppendLine($"Seed:      {_seed}");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(new string('=', 60));
        }

        private void Write(StringBuilder sb, string name)
        {
            var path = Path.Combine(LogDir, $"{name}_{_seed}.txt");
            File.WriteAllText(path, sb.ToString());
            RandoPlugin.Logger.LogInfo($"[RandoLogger] Wrote {name} to: {path}");
        }

        private static string AreaOf(string locationName)
        {
            // "Cave_01_HeartPiece" → "Cave", "Overworld_03_Sprint" → "Overworld"
            var parts = locationName.Split('_');
            return parts.Length > 0 ? parts[0] : locationName;
        }
    }
}
