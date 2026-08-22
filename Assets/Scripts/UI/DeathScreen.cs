using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AshesOfRyu
{
    public class DeathScreen : MonoBehaviour
    {
        private CanvasGroup group;
        private Text title;
        private Text sub;

        public void Build(Canvas canvas)
        {
            var root = UIHelpers.Panel(canvas.transform, "Muerte", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(1920f, 1080f), new Color(0f, 0f, 0f, 0.75f));
            var rt = root;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            group = root.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            title = UIHelpers.Label(root, "Titulo", new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(1200f, 90f),
                "HAS MUERTO", 64, new Color(1f, 0.25f, 0.1f));
            sub = UIHelpers.Label(root, "Subtitulo", new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(1400f, 120f),
                "", 24, new Color(1f, 0.8f, 0.6f));
        }

        public void Show(int deaths, int memories)
        {
            title.text = "HAS MUERTO";
            sub.text = "Tu cuerpo se convierte en cenizas...\n\nMuertes: " + deaths + "  ·  Recuerdos: " + memories +
                       "\n\nRenaciendo en el inicio del ciclo...";
            StartCoroutine(Fade(group, 1f, 0.25f));
        }

        public void Hide()
        {
            StartCoroutine(Fade(group, 0f, 0.4f));
        }

        private static IEnumerator Fade(CanvasGroup g, float target, float time)
        {
            float start = g.alpha;
            float t = 0f;
            while (t < time)
            {
                t += Time.deltaTime;
                g.alpha = Mathf.Lerp(start, target, t / time);
                yield return null;
            }
            g.alpha = target;
        }
    }
}
