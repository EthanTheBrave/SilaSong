using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using SilksongIC;
using SilksongIC.Locations;
using SilksongRando.IC.Items;

namespace SilksongRando.IC
{
    /// <summary>
    /// Reads items.json and locations.json and registers all content with SilksongIC's ItemManager.
    ///
    /// Ability PlayerData field names confirmed from decompiled Assembly-CSharp.dll:
    ///   Silkspear  → hasSilkSpecial
    ///   Sprint     → hasDash
    ///   Walljump   → hasWalljump
    ///   Needolin   → hasNeedolin
    ///   SilkSphere → hasThreadSphere
    ///   Brolly     → hasBrolly
    ///   Faydown    → hasSilkCharge
    /// </summary>
    public static class ICRegistrar
    {
        // Maps ability name → confirmed PlayerData bool field
        private static readonly Dictionary<string, string> AbilityFields = new()
        {
            ["Silkspear"]  = "hasSilkSpecial",
            ["Sprint"]     = "hasDash",
            ["Walljump"]   = "hasWalljump",
            ["Needolin"]   = "hasNeedolin",
            ["SilkSphere"] = "hasThreadSphere",
            ["Brolly"]     = "hasBrolly",
            ["Faydown"]    = "hasSilkCharge",
        };

        public static void RegisterAll()
        {
            RegisterItems();
            RegisterLocations();
            SceneHookManager.RebuildIndex();
        }

        private static void RegisterItems()
        {
            var defs = ReadJson<ItemDef[]>("Data.items.json");
            foreach (var def in defs)
                ItemManager.Instance.RegisterItem(CreateItem(def));
        }

        private static void RegisterLocations()
        {
            var defs = ReadJson<LocationDef[]>("Data.locations.json");
            foreach (var def in defs)
                ItemManager.Instance.RegisterLocation(CreateLocation(def));
        }

        private static AbstractItem CreateItem(ItemDef def)
        {
            var name = def.name;
            var ui   = def.uiName ?? def.name;
            return def.type switch
            {
                "ability" => new AbilityItem
                {
                    Name             = name,
                    UIName           = ui,
                    PlayerDataField  = AbilityFields.TryGetValue(def.abilityId ?? name, out var field)
                                       ? field
                                       : $"has{def.abilityId ?? name}",
                },
                "melody"     => new MelodyItem    { Name = name, UIName = ui, ItemAssetName = def.itemId ?? name },
                "bell"       => new BellItem      { Name = name, UIName = ui, ItemAssetName = def.itemId ?? name },
                "heartpiece" => new HeartPieceItem { Name = name, UIName = ui },
                "geo"        => new GeoItem       { Name = name, UIName = ui, Amount = def.amount ?? 0 },
                "crest"      => new CrestItem     { Name = name, UIName = ui, CrestName = def.itemId ?? name },
                "trinket"    => new TrinketItem   { Name = name, UIName = ui, ItemAssetName = def.itemId ?? name },
                _            => new TrinketItem   { Name = name, UIName = ui, ItemAssetName = name },
            };
        }

        private static AbstractLocation CreateLocation(LocationDef def)
        {
            return def.locationType switch
            {
                "collectable" => new CollectableItemPickupLocation
                {
                    Name           = def.name,
                    SceneName      = def.scene ?? string.Empty,
                    GameObjectName = def.gameObject,
                    OriginalItemId = def.originalItemId ?? string.Empty,
                },
                "fsm" => new FSMLocation
                {
                    Name           = def.name,
                    SceneName      = def.scene ?? string.Empty,
                    GameObjectName = def.gameObject ?? string.Empty,
                    FSMName        = def.fsmName ?? string.Empty,
                    TriggerState   = def.triggerState ?? string.Empty,
                },
                "shop" => new ShopLocation
                {
                    Name           = def.name,
                    SceneName      = def.scene ?? string.Empty,
                    ShopOwnerName  = def.shopOwner ?? string.Empty,
                    OriginalItemId = def.originalItemId ?? string.Empty,
                    Cost           = def.cost ?? 0,
                },
                _ => new CollectableItemPickupLocation
                {
                    Name           = def.name,
                    SceneName      = def.scene ?? string.Empty,
                    OriginalItemId = def.originalItemId ?? string.Empty,
                },
            };
        }

        private static T ReadJson<T>(string name)
        {
            var asm      = Assembly.GetExecutingAssembly();
            var fullName = $"SilksongRando.Resources.{name}";
            using var stream = asm.GetManifestResourceStream(fullName)
                ?? throw new InvalidOperationException($"Embedded resource not found: {fullName}");
            using var reader = new StreamReader(stream);
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd())
                ?? throw new InvalidOperationException($"Failed to deserialize: {fullName}");
        }

#pragma warning disable CS8618
        private class ItemDef
        {
            public string  name;
            public string  type;
            public string? uiName;
            public string? abilityId;
            public string? itemId;
            public int?    amount;
        }

        private class LocationDef
        {
            public string  name;
            public string? scene;
            public string  locationType;
            public string? gameObject;
            public string? originalItemId;
            public string? fsmName;
            public string? triggerState;
            public string? shopOwner;
            public int?    cost;
        }
#pragma warning restore CS8618
    }
}
