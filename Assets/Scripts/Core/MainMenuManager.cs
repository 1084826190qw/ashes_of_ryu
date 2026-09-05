using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AshesOfRyu
{
    public class MainMenuManager : MonoBehaviour
    {
        private void Awake()
        {
            var playButton = GameObject.Find("Canvas/PlayButton")?.GetComponent<Button>();
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(PlayGame);
            }

            var creditsButton = GameObject.Find("Canvas/CreditsButton")?.GetComponent<Button>();
            if (creditsButton != null)
            {
                creditsButton.onClick.RemoveAllListeners();
                creditsButton.onClick.AddListener(OpenCredits);
            }
        }

        public void PlayGame()
        {
            Debug.Log("[MainMenuManager] PlayGame() llamado - cargando SampleScene (Bosque Oscuro)");
            SceneManager.LoadScene("SampleScene");
        }

        public void OpenCredits()
        {
            Debug.Log("[MainMenuManager] Créditos - pendiente de implementar");
        }
    }
}