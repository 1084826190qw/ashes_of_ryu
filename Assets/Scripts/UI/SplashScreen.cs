using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AshesOfRyu
{
    /// <summary>
    /// Splash del logo. El logo aparece pequeño y crece hasta su tamaño final
    /// mientras su opacidad sube de 0 a 1 (efecto de aparición), se mantiene unos
    /// segundos y luego se desvanece antes de cargar el menú principal.
    /// Se coloca en un GameObject "SplashManager" dentro de la escena 1_scene_logo.
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        [Header("Configuración")]
        [Tooltip("Duración total en pantalla (segundos).")]
        [SerializeField] private float duration = 3f;

        [Tooltip("Escena que se carga al terminar el splash.")]
        [SerializeField] private string nextScene = "2_scene_menu";

        [Header("Animación de entrada (escala)")]
        [Tooltip("Duración del crecimiento del logo (escala).")]
        [SerializeField] private float introDuration = 1.2f;

        [Tooltip("Escala inicial del logo respecto a su tamaño final (0.4 = 40%).")]
        [Range(0.05f, 1f)]
        [SerializeField] private float startScale = 0.4f;

        [Header("Animación de entrada (opacidad)")]
        [Tooltip("Duración del efecto de opacidad al aparecer. Puede ser mayor que el crecimiento.")]
        [SerializeField] private float fadeInDuration = 1.6f;

        [Tooltip("Curva de la opacidad durante la aparición (0 = invisible, 1 = visible).")]
        [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Animación de salida (opacidad)")]
        [Tooltip("Duración del desvanecido final. 0 = sin desvanecido.")]
        [SerializeField] private float fadeOutDuration = 0.5f;

        [Tooltip("Imagen del logo (se asigna solo desde el editor).")]
        [SerializeField] private Image logo;

        private void Start()
        {
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            if (logo != null)
            {
                float intro = Mathf.Max(introDuration, fadeInDuration);
                yield return AnimateLogo(intro);

                // Espera el tiempo restante de la duración total.
                float wait = Mathf.Max(0f, duration - intro - fadeOutDuration);
                if (wait > 0f)
                    yield return new WaitForSecondsRealtime(wait);

                yield return FadeOut();
            }
            else
            {
                // Sin logo, solo respeta la duración total.
                yield return new WaitForSecondsRealtime(duration);
            }

            SceneManager.LoadScene(nextScene);
        }

        /// <summary>
        /// Anima el logo de pequeño a su tamaño final (escala) y de opacidad 0 a 1,
        /// cada uno con su propia duración y curva.
        /// </summary>
        private IEnumerator AnimateLogo(float intro)
        {
            Transform tr = logo.transform;
            Vector3 finalScale = tr.localScale;
            Color color = logo.color;

            tr.localScale = finalScale * startScale;
            color.a = 0f;
            logo.color = color;

            float elapsed = 0f;
            while (elapsed < intro)
            {
                elapsed += Time.unscaledDeltaTime;

                // Escala: de startScale a 1 con suavizado.
                float scaleT = introDuration > 0f ? Mathf.Clamp01(elapsed / introDuration) : 1f;
                float scaleEased = Mathf.SmoothStep(0f, 1f, scaleT);
                tr.localScale = finalScale * Mathf.Lerp(startScale, 1f, scaleEased);

                // Opacidad: de 0 a 1 con su propia curva y duración.
                float fadeT = fadeInDuration > 0f ? Mathf.Clamp01(elapsed / fadeInDuration) : 1f;
                color.a = fadeInCurve != null ? Mathf.Clamp01(fadeInCurve.Evaluate(fadeT)) : fadeT;
                logo.color = color;

                yield return null;
            }

            tr.localScale = finalScale;
            color.a = 1f;
            logo.color = color;
        }

        /// <summary>
        /// Desvanece el logo hasta hacerlo invisible.
        /// </summary>
        private IEnumerator FadeOut()
        {
            if (logo == null || fadeOutDuration <= 0f)
                yield break;

            Color color = logo.color;
            float startAlpha = color.a;
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(elapsed / fadeOutDuration));
                logo.color = color;
                yield return null;
            }

            color.a = 0f;
            logo.color = color;
        }
    }
}
