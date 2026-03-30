namespace SilksongRando.Settings
{
    public class RandoSettings
    {
        public int Seed { get; set; }
        public PoolSettings Pools { get; set; } = new();
        public SkipSettings Skips { get; set; } = new();
        public StartSettings Start { get; set; } = new();
    }

    public class PoolSettings
    {
        /// <summary>Randomize abilities (Silkspear, Sprint, Walljump, etc).</summary>
        public bool Abilities { get; set; } = true;

        /// <summary>Randomize the three melodies (win conditions).</summary>
        public bool Melodies { get; set; } = true;

        /// <summary>Randomize Bell items across Bellshrine locations.</summary>
        public bool Bellshrines { get; set; } = true;

        /// <summary>Randomize Heart Pieces.</summary>
        public bool HeartPieces { get; set; } = true;

        /// <summary>Randomize Mossberries.</summary>
        public bool Mossberries { get; set; } = true;

        /// <summary>Randomize Pollip Bulbs (Shell Flowers).</summary>
        public bool PollipBulbs { get; set; } = true;

        /// <summary>Randomize shop items.</summary>
        public bool ShopItems { get; set; } = false; // off by default until fully implemented

        /// <summary>Randomize Geo Rocks.</summary>
        public bool GeoRocks { get; set; } = false;

        /// <summary>Randomize equipment crests.</summary>
        public bool Crests { get; set; } = false;

        /// <summary>Randomize all CollectableItemPickup locations (trinkets, relics, keys, etc).</summary>
        public bool Collectables { get; set; } = true;
    }

    public class SkipSettings
    {
        /// <summary>Assume the player can perform precise platforming without Sprint.</summary>
        public bool PrecisePlatforming { get; set; } = false;

        /// <summary>Assume the player can use Silkspear movement tech.</summary>
        public bool SpearMovement { get; set; } = false;

        /// <summary>Assume the player can reach elevated areas without Walljump via other means.</summary>
        public bool EarlyElevated { get; set; } = false;
    }

    public class StartSettings
    {
        /// <summary>Give the player these items at the start of the run.</summary>
        public string[] StartItems { get; set; } = System.Array.Empty<string>();
    }
}
