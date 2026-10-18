using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AshesOfRyu
{
    /// <summary>
    /// Un fotograma de la cinemática: imagen, texto de narración y duración en segundos.
    /// </summary>
    [System.Serializable]
    public class CinematicSlide
    {
        public Sprite image;

        [TextArea(2, 6)]
        public string text;

        [Tooltip("Duración total del fotograma en segundos (incluye la transición de entrada).")]
        public float duration = 6f;
    }

    /// <summary>
    /// Cinemática de prólogo: muestra una secuencia de imágenes con transición lenta
    /// (crossfade), narración en cursiva y música de fondo. Al terminar carga la escena siguiente.
    /// El botón "Siguiente" (o Enter / Espacio / botón A del mando) pasa de escena;
    /// Esc salta toda la cinemática.
    /// </summary>
    public class CinematicSequence : MonoBehaviour
    {
        [Header("Contenido")]
        [SerializeField] private CinematicSlide[] slides;
        [SerializeField] private Image[] slideImages;

        [Header("Narración")]
        [SerializeField] private Text narrationText;

        [Header("Fundido final")]
        [SerializeField] private Image fadeOverlay;

        [Header("Música")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip music;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.7f;

        [Header("Tiempos")]
        [Tooltip("Duración del crossfade entre imágenes (transición lenta).")]
        [SerializeField] private float imageFadeDuration = 1.5f;
        [SerializeField] private float textFadeIn = 0.8f;
        [SerializeField] private float textFadeOut = 0.8f;
        [SerializeField] private float textStartDelay = 0.2f;
        [Tooltip("Fundido a negro al terminar antes de cargar la escena siguiente.")]
        [SerializeField] private float endFadeDuration = 1.2f;

        [Header("Escena siguiente")]
        [SerializeField] private string nextScene = "4_scene_game";

        [Header("Control")]
        [SerializeField] private bool allowSkip = true;

        private bool _skip;
        private bool _advance;
        private bool _ending;
        private int _lastAdvanceFrame = -1;

        private void Awake()
        {
            if (narrationText != null)
            {
                narrationText.fontStyle = FontStyle.Italic;
                SetAlpha(narrationText, 0f);
            }

            if (fadeOverlay != null)
                SetAlpha(fadeOverlay, 0f);

            if (slideImages != null)
            {
                foreach (Image image in slideImages)
                    if (image != null)
                        SetAlpha(image, 0f);
            }
        }

        private void Start()
        {
            StartCoroutine(Play());
        }

        private void Update()
        {
            if (_ending || _skip)
                return;

            // Esc / Start = saltar toda la cinemática.
            if (allowSkip && SkipPressed())
            {
                _skip = true;
                return;
            }

            // Enter / Espacio / botón de mando = pasar a la siguiente escena.
            if (NextPressed())
                NextSlide();
        }

        /// <summary>Pasa a la siguiente escena. Lo llama el botón "Siguiente" (clic) y la tecla Enter.</summary>
        public void NextSlide()
        {
            if (_ending || _skip)
                return;

            // Evita avanzar dos veces en el mismo frame (Enter + Submit del botón).
            if (Time.frameCount == _lastAdvanceFrame)
                return;

            _lastAdvanceFrame = Time.frameCount;
            _advance = true;
        }

        private IEnumerator Play()
        {
            if (musicSource != null && music != null)
            {
                musicSource.clip = music;
                musicSource.loop = false;
                musicSource.volume = musicVolume;
                musicSource.Play();
            }

            for (int i = 0; i < slides.Length && !_skip; i++)
                yield return PlaySlide(i);

            // Fundido a negro final y bajada de la música (sin saltar, para que quede suave).
            _ending = true;
            _skip = false;
            _advance = false;

            yield return FadeGraphic(fadeOverlay, 0f, 1f, endFadeDuration);
            yield return FadeMusic(musicSource, musicVolume, 0f, endFadeDuration);

            SceneManager.LoadScene(nextScene);
        }

        private IEnumerator PlaySlide(int index)
        {
            _advance = false;

            CinematicSlide slide = slides[index];
            float duration = Mathf.Max(0.1f, slide.duration);

            Image image = (slideImages != null && index < slideImages.Length) ? slideImages[index] : null;
            if (image != null)
            {
                if (slide.image != null)
                    image.sprite = slide.image;
                SetAlpha(image, 0f);
            }

            if (narrationText != null)
            {
                narrationText.text = slide.text ?? string.Empty;
                SetAlpha(narrationText, 0f);
            }

            float elapsed = 0f;

            // 1) Aparece la imagen -> crossfade lento con la anterior.
            yield return FadeGraphic(image, 0f, 1f, imageFadeDuration);
            elapsed += imageFadeDuration;

            // La imagen anterior ya está tapada: la apagamos.
            HidePreviousSlide(index);

            // 2) Aparece la narración (se omite si el usuario pidió pasar de escena).
            if (!_advance)
            {
                yield return WaitFor(textStartDelay);
                elapsed += textStartDelay;
                yield return FadeGraphic(narrationText, 0f, 1f, textFadeIn);
                elapsed += textFadeIn;
            }

            // 3) Se mantiene.
            if (!_advance)
            {
                float hold = duration - elapsed - textFadeOut;
                if (hold > 0f)
                    yield return WaitFor(hold);
            }

            // 4) Se va la narración.
            if (!_advance)
                yield return FadeGraphic(narrationText, 1f, 0f, textFadeOut);
            else if (narrationText != null)
                SetAlpha(narrationText, 0f);
        }

        private void HidePreviousSlide(int index)
        {
            if (slideImages == null || index - 1 < 0 || index - 1 >= slideImages.Length)
                return;

            Image previous = slideImages[index - 1];
            if (previous != null)
            {
                SetAlpha(previous, 0f);
                previous.gameObject.SetActive(false);
            }
        }

        // ------------------------------------------------------------------ Helpers

        private IEnumerator FadeGraphic(Graphic graphic, float from, float to, float duration)
        {
            if (graphic == null)
                yield break;

            if (duration <= 0f)
            {
                SetAlpha(graphic, to);
                yield break;
            }

            SetAlpha(graphic, from);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (_skip || _advance)
                    break;

                elapsed += Time.deltaTime;
                SetAlpha(graphic, Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }

            SetAlpha(graphic, to);
        }

        private IEnumerator FadeMusic(AudioSource source, float from, float to, float duration)
        {
            if (source == null)
                yield break;

            if (duration <= 0f)
            {
                source.volume = to;
                yield break;
            }

            source.volume = from;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (_skip || _advance)
                    break;

                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            source.volume = to;
        }

        private IEnumerator WaitFor(float seconds)
        {
            if (seconds <= 0f)
                yield break;

            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (_skip || _advance)
                    yield break;

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
                return;

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        /// <summary>Saltar toda la cinemática: Esc o el botón Start del mando.</summary>
        private static bool SkipPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                return true;

            var gamepad = UnityEngine.InputSystem.Gamepad.current;
            if (gamepad != null && gamepad.startButton.wasPressedThisFrame)
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape))
                return true;
#endif
            return false;
        }

        /// <summary>Pasar a la siguiente escena: Enter, Espacio o el botón A/Sur del mando.</summary>
        private static bool NextPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null &&
                (keyboard.enterKey.wasPressedThisFrame ||
                 keyboard.numpadEnterKey.wasPressedThisFrame ||
                 keyboard.spaceKey.wasPressedThisFrame))
                return true;

            var gamepad = UnityEngine.InputSystem.Gamepad.current;
            if (gamepad != null && gamepad.aButton.wasPressedThisFrame)
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Space))
                return true;
#endif
            return false;
        }
    }
}
