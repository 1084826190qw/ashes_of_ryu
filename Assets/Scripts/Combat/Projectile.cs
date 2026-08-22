using System;
using UnityEngine;

namespace AshesOfRyu
{
    public class Projectile : MonoBehaviour
    {
        public float Damage;
        public float Speed;
        public float MaxRange;
        public int Pierce;
        public int Bounces;
        public bool FromPlayer;
        public Vector2 Direction;

        private float travelled;
        private Rigidbody2D rb;
        private static Sprite[] fireballFrames;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            Vector2 step = Direction * Speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + step);
            travelled += step.magnitude;
            if (travelled >= MaxRange) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (FromPlayer)
            {
                if (other.GetComponentInParent<PlayerHealth>() != null) return;

                var enemy = other.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(new DamageInfo { Amount = Damage, Knockback = Direction * 1.6f, Source = gameObject });
                    if (Pierce > 0)
                    {
                        Pierce--;
                        return;
                    }
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                var player = other.GetComponentInParent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(new DamageInfo { Amount = Damage, Knockback = Direction * 2f, Source = gameObject });
                    Destroy(gameObject);
                    return;
                }
                if (other.GetComponentInParent<Enemy>() != null) return;
            }

            if (Bounces > 0 && TryBounce(other))
            {
                Bounces--;
                return;
            }
            Destroy(gameObject);
        }

        private bool TryBounce(Collider2D other)
        {
            Vector2 closest = other.ClosestPoint(transform.position);
            Vector2 normal = (Vector2)transform.position - closest;
            if (normal.sqrMagnitude < 0.0001f) normal = -Direction;
            normal.Normalize();
            Direction = Vector2.Reflect(Direction, normal).normalized;
            return true;
        }

        private static void LoadFireballFrames()
        {
            // Sin cache entre sesiones: los sprites de Sprite.Create se destruyen al salir
            // del modo Play; recrearlos es barato y evita proyectiles invisibles.
            var tex = Resources.Load<Texture2D>("Art/Environment/fireball");
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                // Use the known fireball frames from the sprite sheet (from meta file):
                // _20: rect(24, 24, 26, 18)
                // _21: rect(86, 24, 28, 18)
                // _22: rect(148, 24, 30, 18)
                // These three form a horizontal sequence at y=24 with similar heights
                var frames = new System.Collections.Generic.List<Sprite>();
                float ppu = 32f; // spritePixelsToUnits from meta

                var rects = new[]
                {
                    new Rect(24f, 24f, 26f, 18f),   // _20
                    new Rect(86f, 24f, 28f, 18f),   // _21
                    new Rect(148f, 24f, 30f, 18f),  // _22
                };

                foreach (var rect in rects)
                {
                    var sprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), ppu);
                    frames.Add(sprite);
                }

                fireballFrames = frames.ToArray();
            }
        }

        public static Projectile Create(Vector2 origin, Vector2 direction, float speed, float damage, float range,
            int pierce, int bounces, Color color, bool fromPlayer, float scale = 0.55f)
        {
            var go = new GameObject(fromPlayer ? "AlientoDeFuego" : "ProyectilEnemigo");
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            if (fromPlayer)
            {
                LoadFireballFrames();
                if (fireballFrames != null && fireballFrames.Length > 0 && fireballFrames[0] != null)
                {
                    sr.sprite = fireballFrames[0];
                    sr.color = color; // tinte por nivel: naranja / azul / negro
                    var animator = go.AddComponent<FireballAnimator>();
                    animator.Init(fireballFrames, 0.08f);
                }
                else
                {
                    sr.sprite = SpriteFactory.Circle(color, 16);
                }
            }
            else
            {
                sr.sprite = SpriteFactory.Circle(color, 16);
            }

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var p = go.AddComponent<Projectile>();
            p.Damage = damage;
            p.Speed = speed;
            p.MaxRange = range;
            p.Pierce = pierce;
            p.Bounces = bounces;
            p.FromPlayer = fromPlayer;
            p.Direction = direction.normalized;
            return p;
        }
    }
}
