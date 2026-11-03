using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AshesOfRyu
{
    public class ScreenFader : MonoBehaviour
    {
        private static ScreenFader _instance;
        private CanvasGroup _canvasGroup;
        private Image _image;
        private bool _isFading;

        public static ScreenFader Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ScreenFader");
                    _instance = go.AddComponent<ScreenFader>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFader();
        }

        private void CreateFader()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            GraphicRaycaster gr = gameObject.AddComponent<GraphicRaycaster>();
            gr.enabled = false;

            GameObject imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(transform, false);
            _image = imageGO.AddComponent<Image>();
            _image.color = Color.black;
            _image.raycastTarget = false;

            _canvasGroup = imageGO.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            RectTransform rect = _image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public void FadeToScene(string sceneName, float duration = 0.25f)
        {
            if (_isFading)
            {
                return;
            }
            StartCoroutine(FadeToSceneRoutine(sceneName, duration));
        }

        private IEnumerator FadeToSceneRoutine(string sceneName, float duration)
        {
            _isFading = true;
            duration = Mathf.Clamp(duration, 0.18f, 0.32f);
            _image.raycastTarget = true;
            _canvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return null;

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;

            _image.raycastTarget = false;
            _canvasGroup.blocksRaycasts = false;
            _isFading = false;
        }
    }
}
