using System;
using System.IO;
using System.Reflection;
using RandomizerCore.Json;
using RandomizerCore.Logic;

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
            var fmt = new JsonLogicFormat();

            ldb.DeserializeFile(LogicFileType.Terms,      fmt, OpenResource("Logic.terms.json"));
            ldb.DeserializeFile(LogicFileType.Macros,     fmt, OpenResource("Logic.macros.json"));
            ldb.DeserializeFile(LogicFileType.Waypoints,  fmt, OpenResource("Logic.waypoints.json"));
            ldb.DeserializeFile(LogicFileType.Transitions, fmt, OpenResource("Logic.transitions.json"));
            ldb.DeserializeFile(LogicFileType.Locations,  fmt, OpenResource("Logic.locations.json"));
            ldb.DeserializeFile(LogicFileType.ItemStrings, fmt, OpenResource("Logic.items.json"));
            ldb.DeserializeFile(LogicFileType.StateData,  fmt, OpenResource("Logic.state.json"));

            return new LogicManager(ldb);
        }

        private static Stream OpenResource(string name)
        {
            var asm      = Assembly.GetExecutingAssembly();
            var fullName = $"SilksongRando.Resources.{name}";
            return asm.GetManifestResourceStream(fullName)
                ?? throw new InvalidOperationException($"Embedded resource not found: {fullName}");
        }
    }
}
