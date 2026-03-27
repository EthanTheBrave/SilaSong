using System.Collections;
using SilksongIC;
using TMPro;
using UnityEngine;

namespace SilksongRando.UI
{
    /// <summary>
    /// Displays a brief on-screen popup when the player collects a randomized item.
    /// Creates a simple canvas-based notification anchored to the top of the screen.
    /// </summary>
    public static class ItemNotification
    {
        private static GameObject? _canvas;
        private static TextMeshProUGUI? _label;
        private static Coroutine? _hideRoutine;

        private const float DisplayTime = 3f;

        public static void Show(string locationName, AbstractItem item)
        {
            EnsureCanvas();
            if (_label == null) return;

            _label.text = $"Found: {item.UIName}";
            _canvas!.SetActive(true);

            if (_hideRoutine != null)
                RandoPlugin.Instance.StopCoroutine(_hideRoutine);
            _hideRoutine = RandoPlugin.Instance.StartCoroutine(HideAfter(DisplayTime));
        }

        private static IEnumerator HideAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _canvas?.SetActive(false);
        }

        private static void EnsureCanvas()
        {
            if (_canvas != null) return;

            _canvas = new GameObject("RandoNotification");
            Object.DontDestroyOnLoad(_canvas);
            _canvas.SetActive(false);

            var canvasComp = _canvas.AddComponent<Canvas>();
            canvasComp.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvasComp.sortingOrder = 100;

            _canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            _canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Background panel
            var bg = new GameObject("Background");
            bg.transform.SetParent(_canvas.transform, false);
            var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.6f);

            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 1f);
            bgRect.anchorMax = new Vector2(0.5f, 1f);
            bgRect.pivot     = new Vector2(0.5f, 1f);
            bgRect.anchoredPosition = new Vector2(0f, -20f);
            bgRect.sizeDelta        = new Vector2(400f, 60f);

            // Text
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(bg.transform, false);
            _label = textGo.AddComponent<TextMeshProUGUI>();
            _label.alignment  = TextAlignmentOptions.Center;
            _label.fontSize   = 20f;
            _label.color      = Color.white;

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
