// SeedMenu: seed is configured via BepInEx config file (BepInEx/config/com.silksongrando.rando.cfg).
// The in-game seed input UI requires hooking the Silksong new-game menu, which has not yet
// been identified in the game's class hierarchy. This is a known TODO.
//
// To set a specific seed: edit the config file and set Seed = <your number>.
// Seed = 0 means random (default).
namespace SilksongRando.UI
{
    public static class SeedMenu { }
}
