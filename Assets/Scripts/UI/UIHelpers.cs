using UnityEngine;
using UnityEngine.UI;

namespace AshesOfRyu
{
    public static class UIHelpers
    {
        private static Font cachedFont;

        public static Font Font
        {
            get
            {
                if (cachedFont == null)
                {
                    cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return cachedFont;
            }
        }

        public static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static RectTransform Panel(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return rt;
        }

        public static Text Label(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, string text,
            int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var label = go.GetComponent<Text>();
            label.font = Font;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            return label;
        }

        public struct Bar
        {
            public RectTransform Fill;
            public float BaseWidth;
        }

        public static Bar CreateBar(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size,
            Color background, Color fillColor)
        {
            var bg = Panel(parent, name, anchor, pos, size, background);

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fill = fillGo.GetComponent<RectTransform>();
            fill.SetParent(bg, false);
            fill.anchorMin = new Vector2(0f, 0.5f);
            fill.anchorMax = new Vector2(0f, 0.5f);
            fill.pivot = new Vector2(0f, 0.5f);
            fill.anchoredPosition = Vector2.zero;
            fill.sizeDelta = new Vector2(size.x, size.y - 4f);
            fillGo.GetComponent<Image>().color = fillColor;

            return new Bar { Fill = fill, BaseWidth = size.x };
        }

        public static void SetBar(Bar bar, float normalized)
        {
            var sd = bar.Fill.sizeDelta;
            sd.x = Mathf.Max(0f, bar.BaseWidth * Mathf.Clamp01(normalized));
            bar.Fill.sizeDelta = sd;
        }
    }
}
