using System;
using UnityEngine;

namespace AshesOfRyu
{
    public abstract class Enemy : MonoBehaviour, IDamageable
    {
        public event Action<Enemy> Died;

        public Health Health { get; protected set; }
        public float ContactDamage = 10f;

        protected SpriteRenderer sr;
        protected Rigidbody2D rb;
        protected float moveSpeed = 1.5f;
        protected int MemoryDropChance = 30;

        private Vector2 knockback;
        private bool died;

        protected virtual void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        public void Configure(float difficultyScale)
        {
            Health = new Health(BaseHealth() * difficultyScale);
            ContactDamage *= difficultyScale;
        }

        protected abstract float BaseHealth();
        protected abstract Vector2 GetMoveDirection();

        private void FixedUpdate()
        {
            Vector2 move = GetMoveDirection() * moveSpeed;
            if (knockback.sqrMagnitude > 0.04f)
            {
                move = knockback;
                knockback = Vector2.Lerp(knockback, Vector2.zero, Time.fixedDeltaTime * 8f);
            }
            rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerHealth>();
            if (player == null) return;
            Vector2 dir = ((Vector2)player.transform.position - rb.position).normalized;
            player.TakeDamage(new DamageInfo { Amount = ContactDamage, Knockback = dir * 4f, Source = gameObject });
        }

        public void TakeDamage(DamageInfo info)
        {
            if (Health == null || Health.Current <= 0f) return;
            Health.TakeDamage(info);
            if (Health.Current <= 0f)
            {
                Die();
                return;
            }
            knockback = info.Knockback;
            FloatingText.Spawn(transform.position, Mathf.RoundToInt(info.Amount).ToString(), Color.yellow, 48f);
            VFX.HitSpark(transform.position);
        }

        private void Die()
        {
            if (died) return;
            died = true;
            if (UnityEngine.Random.Range(0, 100) < MemoryDropChance) MemoryFragment.Create(transform.position);
            VFX.Explosion(transform.position, 0.9f);
            Died?.Invoke(this);
            Destroy(gameObject);
        }

        protected static T Spawn<T>(string name, Vector2 pos, Transform parent, Color color, float colliderRadius,
            int sortingOrder) where T : Enemy
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Solid(color, 24);
            sr.sortingOrder = sortingOrder;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = colliderRadius;

            return go.AddComponent<T>();
        }
    }
}
