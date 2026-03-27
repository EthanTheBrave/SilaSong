using System.Collections.Generic;

namespace SilksongRando
{
    /// <summary>
    /// All rando state persisted to the save file via Silksong.DataManager.
    /// </summary>
    public class RandoSaveData
    {
        /// <summary>Whether a rando run is active on this save slot.</summary>
        public bool IsActive { get; set; }

        /// <summary>The seed used to generate this run.</summary>
        public int Seed { get; set; }

        /// <summary>locationName → itemName. The full placement map for this run.</summary>
        public Dictionary<string, string> Placements { get; set; } = new();

        /// <summary>Location names the player has already collected.</summary>
        public HashSet<string> CollectedLocations { get; set; } = new();

        /// <summary>Items granted at game start (for tracker display).</summary>
        public List<string> StartItems { get; set; } = new();

        /// <summary>Hint data: locationName → hint text shown to the player.</summary>
        public Dictionary<string, string> Hints { get; set; } = new();

        /// <summary>Index into the start location list (for future start randomization).</summary>
        public string StartSceneName { get; set; } = string.Empty;

        public void MarkCollected(string locationName)
        {
            CollectedLocations.Add(locationName);
        }

        public bool IsCollected(string locationName) => CollectedLocations.Contains(locationName);
    }
}
