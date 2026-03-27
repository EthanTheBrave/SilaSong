using System.Collections.Generic;

namespace SilksongIC.SaveData
{
    /// <summary>
    /// The portion of save data owned by SilksongIC.
    /// Stored via Silksong.DataManager's ISaveDataMod interface.
    /// </summary>
    public class ICSaveData
    {
        /// <summary>Location name → item name. Set by the randomizer before a run.</summary>
        public Dictionary<string, string> Placements { get; set; } = new();

        /// <summary>Set of location names the player has already collected.</summary>
        public HashSet<string> CollectedLocations { get; set; } = new();

        public void MarkCollected(string locationName) => CollectedLocations.Add(locationName);
        public bool IsCollected(string locationName) => CollectedLocations.Contains(locationName);
    }
}
