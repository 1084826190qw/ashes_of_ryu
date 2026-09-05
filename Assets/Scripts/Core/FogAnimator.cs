using UnityEngine;

namespace AshesOfRyu
{
    public class FogAnimator : MonoBehaviour
    {
        private const string SheetPath = "Art/Effects/fog_animated";
        private const int FrameWidth = 640;
        private const int FrameHeight = 480;

        private SpriteRenderer sr;
        private Sprite[] frames;
        private int currentFrame;
        private float timer;
        private float frameTime = 0.09f;

        public static FogAnimator CreateLayer(Transform parent, float worldWidth, float opacity, float framesPerSecond = 11f)
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

            var go = new GameObject("Niebla");
            go.transform.SetParent(parent, false);
            go.transform.position = Vector3.zero;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[0];
            sr.color = new Color(1f, 1f, 1f, Mathf.Clamp01(opacity));
            sr.sortingOrder = 3;

            var anim = go.AddComponent<FogAnimator>();
            anim.Init(sprites, framesPerSecond);
            return anim;
        }

        public void Init(Sprite[] sprites, float framesPerSecond)
        {
            sr = GetComponent<SpriteRenderer>();
            frames = sprites;
            frameTime = Mathf.Max(0.01f, 1f / Mathf.Max(1f, framesPerSecond));
            currentFrame = 0;
            timer = 0f;
            if (frames != null && frames.Length > 0) sr.sprite = frames[0];
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;

            timer += Time.deltaTime;
            if (timer < frameTime) return;
            timer = 0f;

            currentFrame = (currentFrame + 1) % frames.Length;
            var next = frames[currentFrame];
            if (next != null) sr.sprite = next;
        }
    }
}