using UnityEngine;
using UnityEngine.UI;

namespace AshesOfRyu
{
    public class FogAnimator : MonoBehaviour
    {
        private const string SheetPath = "Art/Effects/fog_animated";
        private const int FrameWidth = 640;
        private const int FrameHeight = 480;
        private const float UIWorldWidth = 1920f;

        [SerializeField] private float opacity = 0.5f;
        [SerializeField] private float framesPerSecond = 5f;
        [SerializeField] private float worldWidth = UIWorldWidth;
        [SerializeField] private Sprite[] customFrames;

        private SpriteRenderer sr;
        private Image uiImage;
        private Sprite[] frames;
        private int currentFrame;
        private float timer;
        private float frameTime = 0.09f;

        public static FogAnimator CreateLayer(Transform parent, float worldWidth, float opacity, float framesPerSecond = 11f)
        {
            var sprites = BuildFrames(worldWidth);
            if (sprites == null) return null;

            var go = new GameObject("Niebla");
            go.SetActive(false);
            go.transform.SetParent(parent, false);
            go.transform.position = Vector3.zero;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprites[0];
            renderer.color = new Color(1f, 1f, 1f, Mathf.Clamp01(opacity));
            renderer.sortingOrder = 3;

            var anim = go.AddComponent<FogAnimator>();
            anim.Init(sprites, framesPerSecond);
            anim.opacity = Mathf.Clamp01(opacity);

            go.SetActive(true);
            return anim;
        }

        // Capa de niebla para UI (dentro de un Canvas, Screen Space Overlay).
        // Va entre el fondo y los botones segun su posicion en la jerarquia.
        public static FogAnimator CreateUILayer(Transform parent, float opacity, float framesPerSecond = 5f)
        {
            var sprites = BuildFrames(UIWorldWidth);
            if (sprites == null) return null;

            var go = new GameObject("Niebla");
            go.SetActive(false);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.sprite = sprites[0];
            img.color = new Color(1f, 1f, 1f, Mathf.Clamp01(opacity));
            img.raycastTarget = false;

            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var anim = go.AddComponent<FogAnimator>();
            anim.Init(sprites, framesPerSecond);
            anim.opacity = Mathf.Clamp01(opacity);

            go.SetActive(true);
            return anim;
        }

        // Si el componente esta puesto a mano en una escena (nadie llamo a
        // CreateLayer/CreateUILayer), carga la hoja igual que en gameplay.
        private void Awake()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (uiImage == null) uiImage = GetComponent<Image>();

            if (frames == null || frames.Length == 0)
            {
                // Si la escena trae sus propios frames (tira de sprites), se usan esos;
                // si no, se carga la hoja por defecto de Resources.
                if (customFrames != null && customFrames.Length > 0)
                {
                    Init(customFrames, framesPerSecond);
                }
                else
                {
                    if (worldWidth <= 0f) worldWidth = UIWorldWidth;
                    var sprites = BuildFrames(worldWidth);
                    if (sprites != null) Init(sprites, framesPerSecond);
                }
            }

            ApplyOpacity();
        }

        public void Init(Sprite[] sprites, float framesPerSecond)
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (uiImage == null) uiImage = GetComponent<Image>();

            frames = sprites;
            frameTime = Mathf.Max(0.01f, 1f / Mathf.Max(1f, framesPerSecond));
            currentFrame = 0;
            timer = 0f;
            if (frames != null && frames.Length > 0) ApplyFrame(0);
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;

            timer += Time.deltaTime;
            if (timer < frameTime) return;
            timer = 0f;

            currentFrame = (currentFrame + 1) % frames.Length;
            ApplyFrame(currentFrame);
        }

        private void ApplyFrame(int index)
        {
            var next = frames[index];
            if (next == null) return;
            if (sr != null) sr.sprite = next;
            if (uiImage != null) uiImage.sprite = next;
        }

        private void ApplyOpacity()
        {
            var color = new Color(1f, 1f, 1f, Mathf.Clamp01(opacity));
            if (sr != null) sr.color = color;
            if (uiImage != null) uiImage.color = color;
        }

        private static Sprite[] BuildFrames(float worldWidth)
        {
            var tex = Resources.Load<Texture2D>(SheetPath);
            if (tex == null)
            {
                Debug.LogWarning("FogAnimator: no se encontro la textura en Resources: " + SheetPath);
                return null;
            }
            tex.filterMode = FilterMode.Point;

            int cols = tex.width / FrameWidth;
            int rows = tex.height / FrameHeight;
            float ppu = FrameWidth / worldWidth;

            var sprites = new Sprite[cols * rows];
            for (int i = 0; i < sprites.Length; i++)
            {
                int x = (i % cols) * FrameWidth;
                int y = (i / cols) * FrameHeight;
                sprites[i] = Sprite.Create(tex, new Rect(x, y, FrameWidth, FrameHeight), new Vector2(0.5f, 0.5f), ppu);
            }
            return sprites;
        }
    }
}
