using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Silksong.DataManager;
using Silksong.GameModeManager;
using SilksongIC;
using SilksongRando.IC;
using SilksongRando.Logging;
using SilksongRando.RC;
using SilksongRando.Settings;
using SilksongRando.UI;
using SilksongRando.Logging;

namespace SilksongRando
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency(SilksongICPlugin.GUID)]
    public class RandoPlugin : BaseUnityPlugin, ISaveDataMod<RandoSaveData>
    {
        public const string GUID    = "com.silksongrando.rando";
        public const string Name    = "SilksongRando";
        public const string Version = "0.1.0";

        public static RandoPlugin Instance { get; private set; } = null!;
        public static new ManualLogSource Logger { get; private set; } = null!;

        public RandoSettings Settings { get; private set; } = new();
        public RandoSaveData SaveData { get; private set; } = new();
        public RandoController? Controller { get; private set; }

        private Harmony _harmony = null!;

        private void Awake()
        {
            Instance = this;
            Logger   = base.Logger;

            // Register all items + locations with SilksongIC before anything else
            ICRegistrar.RegisterAll();

            // Register the "Randomiser" game mode on the new-game screen
            GameModeManager.RegisterGameMode(new GameMode(
                id:          "Randomiser",
                displayName: "Randomiser",
                description: "Items are shuffled throughout the world.",
                onStart:     OnRandoModeSelected
            ));

            // Initialize map overlay
            RandoMap.Initialize();

            // Hook item collection to show notification UI and refresh tracker log
            ItemManager.Instance.OnItemCollected += ItemNotification.Show;
            ItemManager.Instance.OnItemCollected += OnItemCollected;

            _harmony = new Harmony(GUID);
            _harmony.PatchAll(typeof(RandoPlugin).Assembly);

            Logger.LogInfo($"[SilksongRando] Loaded v{Version}");
        }

        // ── ISaveDataMod ──────────────────────────────────────────────────

        public RandoSaveData OnSave() => SaveData;

        public RandoSaveData OnLoad(RandoSaveData data)
        {
            SaveData = data ?? new RandoSaveData();

            if (SaveData.IsActive)
            {
                // Re-apply placements from save
                ItemManager.Instance.ApplyPlacements(SaveData.Placements, SaveData.CollectedLocations);
                Controller = new RandoController(Settings, SaveData);
                Logger.LogInfo($"[SilksongRando] Loaded rando save (seed {SaveData.Seed})");
            }

            return SaveData;
        }

        public void OnNewSave()
        {
            SaveData = new RandoSaveData();
            Controller = null;
        }

        // ── Game mode callback ─────────────────────────────────────────────

        private void OnRandoModeSelected()
        {
            // Seed will be set from the seed menu; fall back to random if not set
            if (Settings.Seed == 0)
                Settings.Seed = GenerateRandomSeed();
        }

        // ── Called by NewGameHook ─────────────────────────────────────────

        public void StartRando()
        {
            SaveData.Seed     = Settings.Seed;
            SaveData.IsActive = true;

            Logger.LogInfo($"[SilksongRando] Starting rando with seed {SaveData.Seed}");

            Controller = new RandoController(Settings, SaveData);
            Controller.Run();

            // Persist placements into save data immediately
            SaveData.Placements = new System.Collections.Generic.Dictionary<string, string>(
                Controller.Placements);

            // Apply to ItemManager
            ItemManager.Instance.ApplyPlacements(SaveData.Placements, SaveData.CollectedLocations);

            // Write logs
            new RandoLogger(SaveData.Seed, SaveData.Placements).Write();
        }

        public bool IsRandoActive => SaveData.IsActive;

        private static int GenerateRandomSeed()
        {
            return (int)(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() & 0x7FFFFFFF);
        }

        private void OnItemCollected(string locationName, SilksongIC.AbstractItem item)
        {
            // Persist to save data
            SaveData.MarkCollected(locationName);

            // Update controller's progression manager for reachability queries
            Controller?.RebuildProgressionManager();

            // Refresh tracker log on disk
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
