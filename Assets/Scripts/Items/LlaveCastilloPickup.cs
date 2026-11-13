using UnityEngine;

namespace AshesOfRyu
{
    /// <summary>
    /// Llave que suelta el jefe del primer nivel al morir.
    /// Se recoge al entrar en contacto con el jugador.
    /// </summary>
    public class LlaveCastilloPickup : MonoBehaviour
    {
        // 32 px a 100 pixels per unit.
        private const float TargetWorldSize = 0.32f;

        private static Sprite fallbackSprite;

        private float baseY;

        public static void Create(Vector2 position, Sprite sprite)
        {
            var go = new GameObject("llave_castillo");
            go.transform.position = position;

            var visualGo = new GameObject("Visual");
            visualGo.transform.SetParent(go.transform, false);

            Sprite spriteToUse = sprite != null ? sprite : FallbackSprite();
            var sr = visualGo.AddComponent<SpriteRenderer>();
            sr.sprite = spriteToUse;
            sr.sortingOrder = 7;

            // Escala la llave para que ocupe como maximo 32x32 px manteniendo su proporcion.
            Bounds bounds = spriteToUse.bounds;
            float maxSide = Mathf.Max(bounds.size.x, bounds.size.y);
            float scale = maxSide > 0f ? TargetWorldSize / maxSide : 1f;
            visualGo.transform.localScale = Vector3.one * scale;
            visualGo.transform.localPosition = -bounds.center * scale;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.12f;

            go.AddComponent<LlaveCastilloPickup>();
        }

        private static Sprite FallbackSprite()
        {
            if (fallbackSprite == null)
                fallbackSprite = SpriteFactory.Circle(new Color(1f, 0.85f, 0.25f), 16);
            return fallbackSprite;
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
            transform.Rotate(0f, 0f, 35f * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerHealth>() == null) return;
            FloatingText.Spawn(transform.position, "Llave del Castillo", new Color(1f, 0.85f, 0.25f), 72f);
            Destroy(gameObject);
        }
    }
}
