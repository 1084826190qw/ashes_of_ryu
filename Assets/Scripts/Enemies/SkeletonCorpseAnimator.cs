using UnityEngine;

namespace AshesOfRyu
{
    /// <summary>
    /// Reproduce la animacion de desmoronamiento y deja el monton de huesos
    /// como resto visual, sin collider ni logica de enemigo.
    /// </summary>
    public class SkeletonCorpseAnimator : MonoBehaviour
    {
        private SpriteRenderer sr;
        private Sprite[] frames;
        private Sprite finalSprite;
        private int frame;
        private float timer;
        private float frameTime;
        private float holdTime;
        private float holdTimer;
        private bool finished;

        public void Init(Sprite[] crumblingFrames, Sprite pileSprite, float fps, float holdSeconds)
        {
            sr = GetComponent<SpriteRenderer>();
            frames = crumblingFrames;
            finalSprite = pileSprite;
            frameTime = 1f / Mathf.Max(1f, fps);
            holdTime = holdSeconds;

            if (sr == null) return;

            if (frames != null && frames.Length > 0) sr.sprite = frames[0];
            else if (finalSprite != null) sr.sprite = finalSprite;
        }

        private void Update()
        {
            if (sr == null) return;

            if (!finished && frames != null && frames.Length > 0)
            {
                timer += Time.deltaTime;
                while (timer >= frameTime)
                {
                    timer -= frameTime;
                    frame++;

                    if (frame >= frames.Length)
                    {
                        finished = true;
                        frame = frames.Length - 1;
                        sr.sprite = finalSprite != null ? finalSprite : frames[frame];
                        break;
                    }

                    sr.sprite = frames[frame];
                }
                return;
            }

            holdTimer += Time.deltaTime;
            if (holdTimer >= holdTime) Destroy(gameObject);
        }
    }
}
