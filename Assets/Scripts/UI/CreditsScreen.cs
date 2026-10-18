using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AshesOfRyu
{
    /// <summary>
    /// Pantalla de créditos: el texto no cabe entero, así que se muestra por partes.
    /// La flecha de la derecha pasa a la siguiente parte (la de la izquierda vuelve),
    /// y también se puede con las flechas del teclado (←/→ o A/D).
    /// La flecha de la esquina superior izquierda vuelve al menú.
    /// </summary>
    public class CreditsScreen : MonoBehaviour
    {
        [Header("Volver al menú")]
        [SerializeField] private string backSceneName = "2_scene_menu";
        [SerializeField] private Button backButton;

        [Header("Créditos por partes")]
        [SerializeField] private Text creditsText;
        [Tooltip("Una cadena por parte/pantalla de créditos.")]
        [SerializeField] private string[] pages;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;
        [Tooltip("Etiqueta \"1 / 5\" bajo el texto.")]
        [SerializeField] private Text pageLabel;
        [Tooltip("Duración del fundido de entrada al cambiar de parte, en segundos.")]
        [SerializeField] private float fadeTime = 0.18f;

        private int _index;
        private bool _goingBack;
        private CanvasGroup _group;
        private float _fade = 1f;

        private int PageCount => pages == null ? 0 : pages.Length;

        private void Start()
        {
            if (backButton != null)
                backButton.onClick.AddListener(GoBack);
            if (nextButton != null)
                nextButton.onClick.AddListener(NextPage);
            if (previousButton != null)
                previousButton.onClick.AddListener(PreviousPage);

            _group = creditsText != null ? creditsText.GetComponent<CanvasGroup>() : null;

            _index = 0;
            ApplyPage();
            _fade = 1f;
            if (_group != null)
                _group.alpha = 1f;
        }

        private void OnDestroy()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(GoBack);
            if (nextButton != null)
                nextButton.onClick.RemoveListener(NextPage);
            if (previousButton != null)
                previousButton.onClick.RemoveListener(PreviousPage);
        }

        private void Update()
        {
            // Fundido de entrada de la parte actual (simple, sin corrutinas).
            if (_fade < 1f)
            {
                _fade = fadeTime <= 0f
                    ? 1f
                    : Mathf.MoveTowards(_fade, 1f, Time.unscaledDeltaTime / fadeTime);

                if (_group != null)
                    _group.alpha = _fade;
            }

            if (_goingBack)
                return;

            if (BackPressed())
            {
                GoBack();
                return;
            }

            if (RightPressed())
                NextPage();
            else if (LeftPressed())
                PreviousPage();
        }

        /// <summary>Muestra la siguiente parte de créditos.</summary>
        public void NextPage()
        {
            if (_goingBack || PageCount == 0 || _index >= PageCount - 1)
                return; // ya se ve la última parte

            Show(_index + 1);
        }

        /// <summary>Vuelve a la parte anterior de créditos.</summary>
        public void PreviousPage()
        {
            if (_goingBack || PageCount == 0 || _index <= 0)
                return; // ya se ve la primera parte

            Show(_index - 1);
        }

        private void Show(int index)
        {
            _index = Mathf.Clamp(index, 0, PageCount - 1);
            ApplyPage();

            // Entra fundiendo desde negro.
            _fade = 0f;
            if (_group != null)
                _group.alpha = 0f;
        }

        /// <summary>Escribe la parte actual en pantalla y actualiza flechas y contador.</summary>
        private void ApplyPage()
        {
            if (creditsText != null && PageCount > 0)
                creditsText.text = pages[_index];

            if (pageLabel != null)
                pageLabel.text = PageCount > 0 ? (_index + 1) + " / " + PageCount : string.Empty;

            // Las flechas se atenúan cuando no hay adónde ir.
            if (nextButton != null)
                nextButton.interactable = _index < PageCount - 1;
            if (previousButton != null)
                previousButton.interactable = _index > 0;

            // Ningún botón seleccionado por defecto (evita que las flechas del
            // teclado se "coman" el foco tras hacer clic en una flecha).
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public void GoBack()
        {
            if (_goingBack)
                return;

            if (string.IsNullOrEmpty(backSceneName) || !Application.CanStreamedLevelBeLoaded(backSceneName))
            {
                Debug.LogWarning("[CreditsScreen] La escena '" + backSceneName + "' no está en Build Settings.");
                return;
            }

            _goingBack = true;
            SceneManager.LoadScene(backSceneName);
        }

        private static bool BackPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape))
                return true;
#endif
            return false;
        }

        private static bool RightPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null &&
                (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame))
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                return true;
#endif
            return false;
        }

        private static bool LeftPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null &&
                (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame))
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                return true;
#endif
            return false;
        }
    }
}
