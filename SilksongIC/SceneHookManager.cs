using System.Collections.Generic;
using BepInEx.Logging;

namespace SilksongIC
{
    /// <summary>
    /// Listens for scene loads and calls OnSceneLoad on all registered locations
    /// whose SceneName matches the loaded scene.
    ///
    /// Scene notification comes from SilksongICPlugin via SceneManager.sceneLoaded,
    /// which fires after all scene-object Awake/OnEnable calls complete.
    /// </summary>
    public static class SceneHookManager
    {
        private static ManualLogSource? _log;
        // sceneName -> locations in that scene
        private static Dictionary<string, List<AbstractLocation>> _byScene = new();

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        /// <summary>Rebuild scene index from current ItemManager registrations.</summary>
        public static void RebuildIndex()
        {
            _byScene.Clear();
            foreach (var loc in ItemManager.Instance.Locations.Values)
            {
                if (!_byScene.TryGetValue(loc.SceneName, out var list))
                    _byScene[loc.SceneName] = list = new List<AbstractLocation>();
                list.Add(loc);
            }
        }

        /// <summary>Called by SilksongICPlugin.OnSceneLoaded (SceneManager.sceneLoaded event).</summary>
        public static void OnSceneLoaded(string sceneName)
        {
            if (!_byScene.TryGetValue(sceneName, out var locations))
                return;

            var ctx = new LocationLoadContext { SceneName = sceneName };
            foreach (var loc in locations)
            {
                if (ItemManager.Instance.IsCollected(loc.Name))
                    continue;
                if (ItemManager.Instance.GetPlacedItem(loc.Name) == null)
                    continue;

                _log?.LogDebug($"[SilksongIC] Loading location: {loc.Name}");
                loc.OnSceneLoad(ctx);
            }
        }
    }
}
