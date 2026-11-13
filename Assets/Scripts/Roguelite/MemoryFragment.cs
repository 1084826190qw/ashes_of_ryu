using UnityEngine;

namespace AshesOfRyu
{
    public class MemoryFragment : MonoBehaviour
    {
        private static readonly Color ParticleColor = new Color(0.25f, 0.9f, 1f, 1f);
        private const float ParticleScale = 0.16f;
        private const float ParticleOrbitRadius = 0.09f;

        private Transform particleA;
        private Transform particleB;
        private float baseY;
        private float phase;

        public static void Create(Vector2 pos)
        {
            var go = new GameObject("Recuerdo");
            go.transform.position = pos;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.28f;

            var fragment = go.AddComponent<MemoryFragment>();
            var sprite = SpriteFactory.Circle(ParticleColor, 8);
            fragment.particleA = CreateParticle(go.transform, "ParticulaA", sprite);
            fragment.particleB = CreateParticle(go.transform, "ParticulaB", sprite);
        }

        private static Transform CreateParticle(Transform parent, string name, Sprite sprite)
        {
            var particle = new GameObject(name);
            particle.transform.SetParent(parent, false);
            particle.transform.localScale = Vector3.one * ParticleScale;

            var sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 6;

            return particle.transform;
        }

        private void Start()
        {
            baseY = transform.position.y;
            phase = Random.Range(0f, Mathf.PI * 2f);
            UpdateParticles(0f);
        }

        private void Update()
        {
            var p = transform.position;
            p.y = baseY + Mathf.Sin(Time.time * 3f) * 0.15f;
            transform.position = p;

            UpdateParticles(Time.time * 2.5f + phase);
        }

        private void UpdateParticles(float t)
        {
            if (particleA != null)
            {
                particleA.localPosition = new Vector3(
                    Mathf.Cos(t) * ParticleOrbitRadius,
                    Mathf.Sin(t) * ParticleOrbitRadius * 0.6f,
                    0f);
                particleA.localScale = Vector3.one * (ParticleScale * (0.9f + 0.1f * Mathf.Sin(t * 3f)));
            }

            if (particleB != null)
            {
                float opposite = t + Mathf.PI;
                particleB.localPosition = new Vector3(
                    Mathf.Cos(opposite) * ParticleOrbitRadius,
                    Mathf.Sin(opposite) * ParticleOrbitRadius * 0.6f,
                    0f);
                particleB.localScale = Vector3.one * (ParticleScale * (0.9f + 0.1f * Mathf.Sin(opposite * 3f)));
            }
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
