using UnityEngine;

namespace AshesOfRyu
{
    public static class VFX
    {
        public static void ClawSwing(Vector2 pos, Vector2 facing, Color color)
        {
            var go = new GameObject("Garra");
            go.transform.position = pos;
            go.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Solid(color);
            sr.sortingOrder = 4;
            sr.flipX = true;
            var fx = go.AddComponent<TempEffect>();
            fx.Play(0.14f, new Vector3(0f, 0f, 1f), new Vector3(2.8f, 1.1f, 1f));
        }

        public static void Explosion(Vector2 pos, float radius)
        {
            var go = new GameObject("Explosion");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Circle(new Color(1f, 0.5f, 0.1f, 1f));
            sr.sortingOrder = 5;
            var fx = go.AddComponent<TempEffect>();
            fx.Play(0.3f, Vector3.zero, Vector3.one * radius * 2f);
        }

        public static void HitSpark(Vector2 pos)
        {
            var go = new GameObject("Chispa");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Solid(Color.white);
            sr.sortingOrder = 6;
            var fx = go.AddComponent<TempEffect>();
            fx.Play(0.12f, Vector3.zero, Vector3.one * 0.4f);
        }
    }

    public class TempEffect : MonoBehaviour
    {
        private float duration;
        private Vector3 start;
        private Vector3 end;
        private float t;
        private SpriteRenderer sr;

        public void Play(float duration, Vector3 startScale, Vector3 endScale)
        {
            this.duration = duration;
            start = startScale;
            end = endScale;
            sr = GetComponent<SpriteRenderer>();
            t = 0f;
        }

        private void Update()
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            transform.localScale = Vector3.Lerp(start, end, k);
            var c = sr.color;
            c.a = 1f - k;
            sr.color = c;
            if (t >= duration) Destroy(gameObject);
        }
    }
}
