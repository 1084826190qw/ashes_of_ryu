using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AshesOfRyu
{
    /// <summary>
    /// Escena de confirmación antes de salir del juego (se abre desde "Salir" del menú).
    /// Botón Sí = sale del juego; botón No = vuelve al menú principal.
    /// Esc hace lo mismo que No.
    /// </summary>
    public class ConfirmationScreen : MonoBehaviour
    {
        [Header("Escenas")]
        [SerializeField] private string menuSceneName = "2_scene_menu";

        private bool _goingBack;

        private void Awake()
        {
            WireButton("SiButton", ConfirmQuit);
            WireButton("NoButton", GoBack);
        }

        private void Start()
        {
            // Sin botón seleccionado por defecto (igual que el menú principal).
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private void Update()
        {
            if (_goingBack)
                return;

            if (BackPressed())
                GoBack();
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

        /// <summary>Sí: sale del juego (en el editor, detiene el modo Play).</summary>
        public void ConfirmQuit()
        {
            Debug.Log("[ConfirmationScreen] Sí -> salir del juego");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>No: vuelve al menú principal.</summary>
        public void GoBack()
        {
            if (_goingBack)
                return;

            if (string.IsNullOrEmpty(menuSceneName) || !Application.CanStreamedLevelBeLoaded(menuSceneName))
            {
                Debug.LogWarning("[ConfirmationScreen] La escena '" + menuSceneName + "' no está en Build Settings.");
                return;
            }

            _goingBack = true;
            SceneManager.LoadScene(menuSceneName);
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
    }
}
