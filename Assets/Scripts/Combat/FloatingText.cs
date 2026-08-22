using UnityEngine;
using UnityEngine.UI;

namespace AshesOfRyu
{
    public static class FloatingText
    {
        private static Font cachedFont;

        public static void Spawn(Vector2 position, string text, Color color, float fontSize = 60f)
        {
            if (cachedFont == null)
            {
                cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            var go = new GameObject("TextoFlotante");
            go.transform.position = position + Vector2.up * 0.5f;

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 200;
            canvas.worldCamera = Camera.main;

            var canvasRt = go.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(2f, 0.7f);
            go.transform.localScale = Vector3.one * 0.01f;

            var textGo = new GameObject("Texto", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var label = textGo.AddComponent<Text>();
            label.font = cachedFont;
            label.text = text;
            label.fontSize = (int)fontSize;
            label.fontStyle = FontStyle.Bold;
            label.color = color;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;

            var rt = textGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var anim = go.AddComponent<FloatingTextAnim>();
            anim.Configure(position, label);
        }
    }

    public class FloatingTextAnim : MonoBehaviour
    {
        private Text label;
        private Vector2 origin;
        private float t;

        public void Configure(Vector2 origin, Text label)
        {
            this.origin = origin;
            this.label = label;
        }

        private void Update()
        {
            t += Time.deltaTime;
            float duration = 0.7f;
            var p = origin + Vector2.up * (0.6f + t * 1.2f);
            transform.position = new Vector3(p.x, p.y, 0f);

            var c = label.color;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            label.color = c;

            if (t >= duration) Destroy(gameObject);
        }
    }
}
