using System;
using System.Collections.Generic;
using SilksongIC;
using SilksongRando.RC;

namespace SilksongRando.Connections
{
    /// <summary>
    /// Public API for connection mods — BepInEx plugins that want to inject
    /// custom items and locations into the randomizer.
    ///
    /// Call Register() from your plugin's Awake(), before SilksongRando
    /// builds the context (i.e. before a new game starts).
    ///
    /// Example:
    ///   ConnectionsAPI.Register(ctx => {
    ///       ctx.AddItem(new MyCustomItem());
    ///       ctx.AddLocation(new MyCustomLocation(), logic: "Ability_Silkspear");
    ///   });
    /// </summary>
    public static class ConnectionsAPI
    {
        private static readonly List<Action<ConnectionContext>> _registrations = new();

        /// <summary>Register a callback that adds items/locations to the rando context.</summary>
        public static void Register(Action<ConnectionContext> setup)
        {
            _registrations.Add(setup);
        }

        /// <summary>Called internally by RequestBuilder to apply all registered connections.</summary>
        internal static void ApplyAll(ConnectionContext ctx)
        {
            foreach (var reg in _registrations)
            {
                try { reg(ctx); }
                catch (Exception ex)
                {
                    RandoPlugin.Logger.LogError($"[ConnectionsAPI] Error in connection registration: {ex}");
                }
            }
        }
    }

    /// <summary>
    /// Context provided to connection mod callbacks.
    /// Provides methods to inject items, locations, and logic overrides.
    /// </summary>
    public class ConnectionContext
    {
        private readonly RequestBuilder _rb;
        private readonly List<(AbstractItem item, string logic)> _extraItems = new();
        private readonly List<(AbstractLocation loc, string logic)> _extraLocations = new();

        internal ConnectionContext(RequestBuilder rb)
        {
            _rb = rb;
        }

        /// <summary>Add a custom item to the shuffle pool with an optional logic expression.</summary>
        public void AddItem(AbstractItem item, string logic = "TRUE")
        {
            ItemManager.Instance.RegisterItem(item);
            _extraItems.Add((item, logic));
        }

        /// <summary>Add a custom location to the shuffle pool.</summary>
        public void AddLocation(AbstractLocation location, string logic = "TRUE")
        {
            ItemManager.Instance.RegisterLocation(location);
            _extraLocations.Add((location, logic));
        }

        /// <summary>Add a custom item to an existing pool.</summary>
        public void AddItemToPool(AbstractItem item, string poolName, string logic = "TRUE")
        {
            ItemManager.Instance.RegisterItem(item);
            _rb.AddItemToPool(item.Name, poolName, logic);
        }

        /// <summary>Add a custom location to an existing pool.</summary>
        public void AddLocationToPool(AbstractLocation location, string poolName, string logic = "TRUE")
        {
            ItemManager.Instance.RegisterLocation(location);
            _rb.AddLocationToPool(location.Name, poolName, logic);
        }

        internal IReadOnlyList<(AbstractItem, string)> ExtraItems       => _extraItems;
        internal IReadOnlyList<(AbstractLocation, string)> ExtraLocations => _extraLocations;
    }
}
