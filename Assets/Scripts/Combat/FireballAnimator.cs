using UnityEngine;

namespace AshesOfRyu
{
    public class FireballAnimator : MonoBehaviour
    {
        private SpriteRenderer sr;
        private Sprite[] frames;
        private int currentFrame;
        private float timer;
        private float frameTime = 0.08f;

        public void Init(Sprite[] animFrames, float frameRate = 0.08f)
        {
            sr = GetComponent<SpriteRenderer>();
            frames = animFrames;
            frameTime = frameRate;
            if (frames != null && frames.Length > 0)
            {
                sr.sprite = frames[0];
            }
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;

            timer += Time.deltaTime;
            if (timer >= frameTime)
            {
                timer = 0f;
                currentFrame = (currentFrame + 1) % frames.Length;
                sr.sprite = frames[currentFrame];
            }
        }
    }
}