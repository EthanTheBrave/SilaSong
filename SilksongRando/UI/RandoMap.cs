using System.Collections.Generic;
using HarmonyLib;
using SilksongIC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SilksongRando.UI
{
    /// <summary>
    /// In-game map overlay showing check status.
    /// Cycle modes with [M] (configurable):
    ///   Off → Checks → Logic → Spoiler → Off
    ///
    /// Off:     no overlay
    /// Checks:  shows original item icon at each check position
    /// Logic:   reachable checks opaque, unreachable semi-transparent, collected hidden
    /// Spoiler: shows the randomized item at each position
    /// </summary>
    public class RandoMap : MonoBehaviour
    {
        public enum MapMode { Off, Checks, Logic, Spoiler }

        private MapMode _mode = MapMode.Off;
        private readonly List<CheckPin> _pins = new();

        private static RandoMap? _instance;

        public static void Initialize()
        {
            var go = new GameObject("RandoMap");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<RandoMap>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
                CycleMode();
        }

        private void CycleMode()
        {
            _mode = _mode switch
            {
                MapMode.Off     => MapMode.Checks,
                MapMode.Checks  => MapMode.Logic,
                MapMode.Logic   => MapMode.Spoiler,
                MapMode.Spoiler => MapMode.Off,
                _               => MapMode.Off,
            };
            RefreshPins();
        }

        public void RebuildForScene(string sceneName)
        {
            ClearPins();
            if (!RandoPlugin.Instance.IsRandoActive) return;

            foreach (var loc in ItemManager.Instance.Locations.Values)
            {
                if (loc.SceneName != sceneName) continue;

                var item = ItemManager.Instance.GetPlacedItem(loc.Name);
                if (item == null) continue;

                var pin = CreatePin(loc, item);
                _pins.Add(pin);
            }

            RefreshPins();
        }

        private void RefreshPins()
        {
            bool anyVisible = _mode != MapMode.Off;

            foreach (var pin in _pins)
            {
                if (_mode == MapMode.Off)
                {
                    pin.SetVisible(false);
                    continue;
                }

                bool collected = RandoPlugin.Instance.SaveData.IsCollected(pin.LocationName);
                if (collected)
                {
                    pin.SetVisible(false);
                    continue;
                }

                pin.SetVisible(true);

                switch (_mode)
                {
                    case MapMode.Checks:
                        pin.SetLabel(pin.OriginalItemName);
                        pin.SetAlpha(1f);
                        break;

                    case MapMode.Logic:
                        pin.SetLabel(pin.OriginalItemName);
                        pin.SetAlpha(IsReachable(pin.LocationName) ? 1f : 0.35f);
                        break;

                    case MapMode.Spoiler:
                        pin.SetLabel(pin.PlacedItemName);
                        pin.SetAlpha(1f);
                        break;
                }
            }
        }

        private static bool IsReachable(string locationName)
        {
            // Ask the RandoController's cached reachability state.
            // Simplified: if the location's logic passes with current inventory.
            // Full implementation would use a ProgressionManager query.
            return RandoPlugin.Instance.Controller?.IsReachable(locationName) ?? false;
        }

        private CheckPin CreatePin(SilksongIC.AbstractLocation loc, SilksongIC.AbstractItem item)
        {
            var go = new GameObject($"Pin_{loc.Name}");
            go.transform.SetParent(transform, false);

            var pin = go.AddComponent<CheckPin>();
            pin.Initialize(
                locationName:     loc.Name,
                originalItemName: loc.Name,       // shown in Checks mode
                placedItemName:   item.UIName      // shown in Spoiler mode
            );

            return pin;
        }

        private void ClearPins()
        {
            foreach (var pin in _pins)
                if (pin != null) Destroy(pin.gameObject);
            _pins.Clear();
        }

        [HarmonyPatch]
        internal static class ScenePatch
        {
            [HarmonyPatch(typeof(GameManager), nameof(GameManager.BeginSceneTransition))]
            [HarmonyPostfix]
            static void OnSceneChange()
            {
                // Rebuild pins after scene transitions
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                _instance?.RebuildForScene(scene.name);
            }
        }
    }

    /// <summary>A single map pin for one check location.</summary>
    public class CheckPin : MonoBehaviour
    {
        public string LocationName     { get; private set; } = string.Empty;
        public string OriginalItemName { get; private set; } = string.Empty;
        public string PlacedItemName   { get; private set; } = string.Empty;

        private TextMeshProUGUI? _label;
        private CanvasGroup?     _group;

        public void Initialize(string locationName, string originalItemName, string placedItemName)
        {
            LocationName     = locationName;
            OriginalItemName = originalItemName;
            PlacedItemName   = placedItemName;

            _group = gameObject.AddComponent<CanvasGroup>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            _label            = labelGo.AddComponent<TextMeshProUGUI>();
            _label.fontSize   = 12f;
            _label.color      = Color.white;
            _label.alignment  = TextAlignmentOptions.Center;
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);
        public void SetLabel(string text)    => _label!.text   = text;
        public void SetAlpha(float alpha)    => _group!.alpha  = alpha;
    }
}
