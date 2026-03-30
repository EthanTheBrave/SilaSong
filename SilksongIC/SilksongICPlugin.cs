using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SilksongIC.Locations;
using UnityEngine.SceneManagement;

namespace SilksongIC
{
    /// <summary>
    /// BepInEx plugin entry point for SilksongIC.
    /// This plugin manages item/location registration and scene hooks.
    /// The randomizer (SilksongRando) depends on this and registers its content here.
    /// </summary>
    [BepInPlugin(GUID, Name, Version)]
    public class SilksongICPlugin : BaseUnityPlugin
    {
        public const string GUID = "com.silksongrando.ic";
        public const string Name = "SilksongIC";
        public const string Version = "0.1.0";

        public static SilksongICPlugin Instance { get; private set; } = null!;
        public static new ManualLogSource Logger { get; private set; } = null!;

        private Harmony _harmony = null!;

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            ItemManager.Initialize(Logger);
            SceneHookManager.Initialize(Logger);
            FSMPatcher.Initialize(Logger);

            _harmony = new Harmony(GUID);
            _harmony.PatchAll();

            // Rebuild location index whenever a scene loads
            SceneManager.sceneLoaded += OnSceneLoaded;

            Logger.LogInfo($"[SilksongIC] Loaded v{Version}");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LocationResolver.RebuildForScene(scene.name);
            FSMLocationResolver.RebuildForScene(scene.name);
            SceneHookManager.OnSceneLoaded(scene.name);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _harmony.UnpatchSelf();
        }
    }
}
