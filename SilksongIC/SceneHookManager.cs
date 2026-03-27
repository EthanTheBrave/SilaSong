using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace SilksongIC
{
    /// <summary>
    /// Listens for scene loads and calls OnSceneLoad on all registered locations
    /// whose SceneName matches the loaded scene.
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

        /// <summary>Called by the scene-loaded Harmony patch.</summary>
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

        [HarmonyPatch]
        internal static class Patches
        {
            // Patch UnityEngine's scene loaded event via the game's scene transition system.
            // Update this target if the game uses a different scene management entry point.
            [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.LoadScene), new[] { typeof(string) })]
            [HarmonyPostfix]
            static void AfterSceneLoad(string sceneName)
            {
                OnSceneLoaded(sceneName);
            }
        }
    }
}
