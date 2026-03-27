using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SilksongRando.UI
{
    /// <summary>
    /// Adds a seed input field to the new-game / game-mode selection screen.
    /// Player can type a specific seed or leave blank for a random one.
    ///
    /// Hooks into the game's mode selection UI. Exact hook target may need
    /// adjustment once the UI class hierarchy is confirmed from decompilation.
    /// </summary>
    public static class SeedMenu
    {
        private static TMP_InputField? _seedInput;
        private static TextMeshProUGUI? _seedLabel;

        [HarmonyPatch]
        internal static class Patches
        {
            // Fires when the game mode selection menu is shown.
            // Update target class/method to match actual game UI once decompiled.
            [HarmonyPatch(typeof(GameModeMenuController), "OnEnable")]
            [HarmonyPostfix]
            static void OnMenuOpen(GameModeMenuController __instance)
            {
                if (!IsRandoModeSelected()) return;
                BuildSeedUI(__instance.gameObject);
            }
        }

        private static void BuildSeedUI(GameObject parent)
        {
            if (parent.transform.Find("RandoSeedUI") != null) return;

            var container = new GameObject("RandoSeedUI");
            container.transform.SetParent(parent.transform, false);

            var rect = container.AddComponent<RectTransform>();
            rect.anchorMin        = new Vector2(0.5f, 0f);
            rect.anchorMax        = new Vector2(0.5f, 0f);
            rect.pivot            = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 60f);
            rect.sizeDelta        = new Vector2(350f, 80f);

            // Label
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(container.transform, false);
            _seedLabel = labelGo.AddComponent<TextMeshProUGUI>();
            _seedLabel.text      = "Seed (blank = random)";
            _seedLabel.fontSize  = 16f;
            _seedLabel.color     = Color.white;
            _seedLabel.alignment = TextAlignmentOptions.Center;

            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin        = new Vector2(0f, 0.5f);
            labelRect.anchorMax        = new Vector2(1f, 1f);
            labelRect.offsetMin        = Vector2.zero;
            labelRect.offsetMax        = Vector2.zero;

            // Input field
            var inputGo = new GameObject("SeedInput");
            inputGo.transform.SetParent(container.transform, false);
            _seedInput = inputGo.AddComponent<TMP_InputField>();
            _seedInput.contentType      = TMP_InputField.ContentType.IntegerNumber;
            _seedInput.characterLimit   = 10;
            _seedInput.text             = string.Empty;
            _seedInput.onValueChanged.AddListener(OnSeedChanged);

            var inputBg = inputGo.AddComponent<Image>();
            inputBg.color = new Color(1f, 1f, 1f, 0.15f);

            var inputRect = inputGo.GetComponent<RectTransform>();
            inputRect.anchorMin        = new Vector2(0f, 0f);
            inputRect.anchorMax        = new Vector2(1f, 0.5f);
            inputRect.offsetMin        = Vector2.zero;
            inputRect.offsetMax        = Vector2.zero;
        }

        private static void OnSeedChanged(string value)
        {
            if (int.TryParse(value, out int seed))
                RandoPlugin.Instance.Settings.Seed = seed;
            else
                RandoPlugin.Instance.Settings.Seed = 0; // random
        }

        private static bool IsRandoModeSelected() =>
            Silksong.GameModeManager.GameModeManager.CurrentMode?.Id == "Randomiser";
    }
}
