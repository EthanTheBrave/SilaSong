using System;
using System.IO;
using System.Reflection;
using RandomizerCore.Logic;
using RandomizerCore.Json;

namespace SilksongRando.RC
{
    /// <summary>
    /// Loads and caches the LogicManager from the embedded JSON logic files.
    /// </summary>
    public static class RCData
    {
        private static LogicManager? _cached;

        public static LogicManager GetLogicManager()
        {
            return _cached ??= BuildLogicManager();
        }

        private static LogicManager BuildLogicManager()
        {
            var ldb = new LogicManagerBuilder();

            // Load logic files from embedded resources
            ldb.DeserializeJson(LogicManagerBuilder.JsonType.Terms,      ReadResource("Logic.terms.json"));
            ldb.DeserializeJson(LogicManagerBuilder.JsonType.Macros,     ReadResource("Logic.macros.json"));
            ldb.DeserializeJson(LogicManagerBuilder.JsonType.Waypoints,  ReadResource("Logic.waypoints.json"));
            ldb.DeserializeJson(LogicManagerBuilder.JsonType.Transitions, ReadResource("Logic.transitions.json"));
            ldb.DeserializeJson(LogicManagerBuilder.JsonType.Locations,  ReadResource("Logic.locations.json"));
            ldb.DeserializeJson(LogicManagerBuilder.JsonType.Items,      ReadResource("Logic.items.json"));
            ldb.DeserializeJson(LogicManagerBuilder.JsonType.StateData,  ReadResource("Logic.state.json"));

            return ldb.LogicManager;
        }

        private static string ReadResource(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            var fullName = $"SilksongRando.Resources.{name}";
            using var stream = asm.GetManifestResourceStream(fullName)
                ?? throw new InvalidOperationException($"Embedded resource not found: {fullName}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
