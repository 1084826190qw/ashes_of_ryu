using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AshesOfRyu
{
    /// <summary>
    /// Controla el menú principal. Enlaza los botones por nombre y ejecuta sus acciones.
    /// Los sprites normal/seleccionado de cada botón se configuran desde la escena.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Escenas")]
        [SerializeField] private string playSceneName = "3_scene_cinematic_1";
        [SerializeField] private string creditsSceneName = "5_scene_credits";
        [SerializeField] private string optionsSceneName = "6_scene_options";
        [SerializeField] private string confirmationSceneName = "6_scene_confirmation";

        [Header("Selección inicial")]
        [Tooltip("Si está activo, se resalta el primer botón al abrir el menú. " +
                 "Déjalo desactivado para que no haya ningún botón seleccionado por defecto.")]
        [SerializeField] private bool selectFirstButtonOnStart = false;

        private void Awake()
        {
            WireButton("PlayButton", PlayGame);
            WireButton("CreditsButton", OpenCredits);
            WireButton("OptionsButton", OpenOptions);
            WireButton("QuitButton", OpenQuitConfirmation);
        }

        private void Start()
        {
            // Por defecto NO se selecciona ningún botón (nada resaltado al abrir el menú).
            if (!selectFirstButtonOnStart)
            {
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(null);
                return;
            }

            // Opcional: resalta el primer botón para navegación con teclado/mando.
            var first = GetComponentInChildren<Button>(true);
            if (first != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(first.gameObject);
        }

        private void WireButton(string buttonName, UnityAction action)
        {
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                if (button.name != buttonName)
                    continue;

                button.onClick.RemoveListener(action);
                button.onClick.AddListener(action);
            }
        }

        public void PlayGame()
        {
            Debug.Log("[MainMenuManager] Jugar -> " + playSceneName);
            SceneManager.LoadScene(playSceneName);
        }

        public void OpenCredits()
        {
            LoadSceneIfAvailable(creditsSceneName, "Créditos");
        }

        public void OpenOptions()
        {
            LoadSceneIfAvailable(optionsSceneName, "Opciones");
        }

        /// <summary>
        /// Salir: en lugar de cerrar el juego directamente, abre la escena de
        /// confirmación (Sí/No). Si esa escena aún no existe, sale directamente.
        /// </summary>
        public void OpenQuitConfirmation()
        {
            if (!string.IsNullOrEmpty(confirmationSceneName) &&
                Application.CanStreamedLevelBeLoaded(confirmationSceneName))
            {
                SceneManager.LoadScene(confirmationSceneName);
                return;
            }

            Debug.Log("[MainMenuManager] Sin escena de confirmación; saliendo directamente.");
            QuitGame();
        }

        public void QuitGame()
        {
            Debug.Log("[MainMenuManager] Salir");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void LoadSceneIfAvailable(string sceneName, string label)
        {
            if (!string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.Log("[MainMenuManager] " + label + ": la escena '" + sceneName +
                          "' todavía no existe o no está en Build Settings.");
            }
        }
    }
}
