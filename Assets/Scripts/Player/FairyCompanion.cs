using System.Collections;
using UnityEngine;

namespace AshesOfRyu
{
    public class FairyCompanion : MonoBehaviour
    {
        private Transform target;
        private SpriteRenderer sr;
        private Sprite[] idleFrames;
        private Sprite[] flyFrames;
        private Vector2 offset = new Vector2(0.75f, 0.9f);
        private float followSpeed = 4f;
        private float animTimer;
        private int frameIndex;
        private bool flying;

        public void Init(Transform followTarget)
        {
            target = followTarget;
            transform.position = target.position + (Vector3)offset;
        }

        public static FairyCompanion Create(Transform parent, Transform followTarget)
        {
            var go = new GameObject("Hada");
            go.transform.SetParent(parent, false);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 6;
            sr.color = new Color(0.85f, 0.95f, 1f);

            var fairy = go.AddComponent<FairyCompanion>();
            fairy.sr = sr;
            fairy.idleFrames = LoadStrip("Idle_Flying (32 x 32)", 4);
            fairy.flyFrames = LoadStrip("Flying_Forward_Movement (32 x 32)", 4);
            fairy.Init(followTarget);
            return fairy;
        }

        private static Sprite[] LoadStrip(string sheetName, int count)
        {
            var tex = Resources.Load<Texture2D>("Art/Characters/Fairy/" + sheetName);
            if (tex == null) return null;
            tex.filterMode = FilterMode.Point;
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                frames[i] = Sprite.Create(tex, new Rect(i * 32f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f), 32f);
            }
            return frames;
        }

        private void Update()
        {
            Animate();
            BobAndFollow();
        }

        private void Animate()
        {
            var frames = flying && flyFrames != null ? flyFrames : idleFrames != null ? idleFrames : flyFrames;
            if (frames == null || frames.Length == 0) return;

            animTimer -= Time.deltaTime;
            if (animTimer > 0f) return;
            animTimer = flying ? 0.09f : 0.16f;
            frameIndex = (frameIndex + 1) % frames.Length;
            sr.sprite = frames[frameIndex];
        }

        private void BobAndFollow()
        {
            if (target == null) return;

            bool facingLeft = false;
            var controller = target.GetComponent<PlayerController>();
            if (controller != null) facingLeft = controller.Facing.x < -0.01f;
            offset.x = Mathf.Abs(offset.x) * (facingLeft ? -1f : 1f);

            float bob = Mathf.Sin(Time.time * 3f) * 0.08f;
            Vector2 desired = (Vector2)target.position + offset + new Vector2(0f, bob);
            Vector2 previous = transform.position;
            transform.position = Vector2.Lerp(transform.position, desired, Time.deltaTime * followSpeed);

            float dx = transform.position.x - previous.x;
            if (Mathf.Abs(dx) > 0.001f)
            {
                sr.flipX = dx < 0f;
                bool nowFlying = Mathf.Abs(dx) > 0.008f;
                if (nowFlying != flying)
                {
                    flying = nowFlying;
                    frameIndex = 0;
                }
            }
        }
    }
}
