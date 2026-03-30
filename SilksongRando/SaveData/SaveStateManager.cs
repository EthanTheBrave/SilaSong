using System;
using System.IO;
using BepInEx;
using Newtonsoft.Json;

namespace SilksongRando
{
    /// <summary>
    /// Persists RandoSaveData to a JSON file alongside the game's save files.
    /// One file per save slot is the eventual goal; currently uses a single file.
    /// </summary>
    internal static class SaveStateManager
    {
        private static string SavePath =>
            Path.Combine(Paths.GameRootPath, "BepInEx", "data", "SilksongRando", "rando_save.json");

        public static void Save(RandoSaveData data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
                File.WriteAllText(SavePath, JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            catch (Exception ex)
            {
                RandoPlugin.Logger.LogError($"[SaveStateManager] Failed to save: {ex.Message}");
            }
        }

        public static RandoSaveData? Load()
        {
            if (!File.Exists(SavePath)) return null;
            try
            {
                return JsonConvert.DeserializeObject<RandoSaveData>(File.ReadAllText(SavePath));
            }
            catch (Exception ex)
            {
                RandoPlugin.Logger.LogError($"[SaveStateManager] Failed to load: {ex.Message}");
                return null;
            }
        }

        public static void Delete()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
    }
}
