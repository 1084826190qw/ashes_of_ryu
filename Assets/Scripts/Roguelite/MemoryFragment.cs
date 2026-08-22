using UnityEngine;

namespace AshesOfRyu
{
    public class MemoryFragment : MonoBehaviour
    {
        private float baseY;

        public static void Create(Vector2 pos)
        {
            var go = new GameObject("Recuerdo");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Circle(new Color(0.25f, 0.9f, 1f), 14);
            sr.sortingOrder = 6;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.28f;
            go.AddComponent<MemoryFragment>();
        }

        private void Start()
        {
            baseY = transform.position.y;
        }

        private void Update()
        {
            var p = transform.position;
            p.y = baseY + Mathf.Sin(Time.time * 3f) * 0.15f;
            transform.position = p;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerHealth>() == null) return;
            GameManager.Instance.CollectMemory();
            FloatingText.Spawn(transform.position, "+1 Recuerdo", new Color(0.25f, 0.9f, 1f), 72f);
            Destroy(gameObject);
        }
    }
}
