using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SilksongIC;
using SilksongRando.IC;
using SilksongRando.Logging;
using SilksongRando.RC;
using SilksongRando.Settings;
using SilksongRando.UI;

namespace SilksongRando
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency(SilksongICPlugin.GUID)]
    public class RandoPlugin : BaseUnityPlugin
    {
        public const string GUID    = "com.silksongrando.rando";
        public const string Name    = "SilksongRando";
        public const string Version = "0.1.0";

        public static RandoPlugin Instance { get; private set; } = null!;
        public static new ManualLogSource Logger { get; private set; } = null!;

        public RandoSettings Settings { get; private set; } = new();
        public RandoSaveData SaveData { get; private set; } = new();
        public RandoController? Controller { get; private set; }

        // Config entries — edit BepInEx/config/com.silksongrando.rando.cfg to change these.
        private ConfigEntry<bool> _cfgEnabled  = null!;
        private ConfigEntry<int>  _cfgSeed     = null!;

        private Harmony _harmony = null!;

        private void Awake()
        {
            Instance = this;
            Logger   = base.Logger;

            _cfgEnabled = Config.Bind("General", "Enabled", false,
                "Set to true to activate the randomizer for the next new game.");
            _cfgSeed = Config.Bind("General", "Seed", 0,
                "Seed for the randomizer. 0 = random seed each run.");

            Settings.Seed = _cfgSeed.Value;

            // Register all items + locations with SilksongIC
            ICRegistrar.RegisterAll();

            // Initialize map overlay
            RandoMap.Initialize();

            // Hook item collection for notification UI and tracker log refresh
            ItemManager.Instance.OnItemCollected += ItemNotification.Show;
            ItemManager.Instance.OnItemCollected += OnItemCollected;

            _harmony = new Harmony(GUID);
            _harmony.PatchAll(typeof(RandoPlugin).Assembly);

            Logger.LogInfo($"[SilksongRando] Loaded v{Version}");
        }

        // ── Save / Load ───────────────────────────────────────────────────────
        // Called by SaveHook / LoadHook Harmony patches.

        public void OnSave()
        {
            SaveStateManager.Save(SaveData);
        }

        public void OnLoad()
        {
            var loaded = SaveStateManager.Load();
            if (loaded != null)
            {
                SaveData = loaded;
                if (SaveData.IsActive)
                {
                    ItemManager.Instance.ApplyPlacements(SaveData.Placements, SaveData.CollectedLocations);
                    Controller = new RandoController(Settings, SaveData);
                    Logger.LogInfo($"[SilksongRando] Loaded rando save (seed {SaveData.Seed})");
                }
            }
        }

        public void OnNewSave()
        {
            SaveData   = new RandoSaveData();
            Controller = null;
            SaveStateManager.Delete();
        }

        // ── Called by NewGameHook when rando mode is active ──────────────────

        public void StartRando()
        {
            if (Settings.Seed == 0)
                Settings.Seed = GenerateRandomSeed();

            SaveData.Seed     = Settings.Seed;
            SaveData.IsActive = true;

            Logger.LogInfo($"[SilksongRando] Starting rando with seed {SaveData.Seed}");

            Controller = new RandoController(Settings, SaveData);
            Controller.Run();

            SaveData.Placements = Controller.Placements.ToDictionary(kv => kv.Key, kv => kv.Value);
            ItemManager.Instance.ApplyPlacements(SaveData.Placements, SaveData.CollectedLocations);

            new RandoLogger(SaveData.Seed, SaveData.Placements).Write();
        }

        public bool IsRandoActive   => SaveData.IsActive;
        public bool IsRandoEnabled  => _cfgEnabled.Value;

        private static int GenerateRandomSeed() =>
            (int)(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() & 0x7FFFFFFF);

        private void OnItemCollected(string locationName, AbstractItem item)
        {
            SaveData.MarkCollected(locationName);
            Controller?.RebuildProgressionManager();

            if (SaveData.Placements.Count > 0)
                new RandoLogger(SaveData.Seed, SaveData.Placements).Write();
        }

        private void OnDestroy()
        {
            ItemManager.Instance.OnItemCollected -= ItemNotification.Show;
            ItemManager.Instance.OnItemCollected -= OnItemCollected;
            _harmony.UnpatchSelf();
        }
    }
}
