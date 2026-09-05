using UnityEngine;

namespace AshesOfRyu
{
    public class RangedEnemy : Enemy
    {
        public float PreferredRange = 5f;
        public float FireInterval = 2.4f;
        public float ProjectileSpeed = 6.5f;

        private float fireTimer;

        protected override float BaseHealth() => 18f;

        protected override Vector2 GetMoveDirection()
        {
            var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (player == null) return Vector2.zero;
            Vector2 to = player.Position - rb.position;
            float dist = to.magnitude;
            if (dist > PreferredRange) return to.normalized;
            if (dist < PreferredRange * 0.55f) return -to.normalized;
            return Vector2.zero;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.PlayerIsDead) return;
            var player = GameManager.Instance.Player;
            if (player == null) return;

            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f && Vector2.Distance(rb.position, player.Position) < PreferredRange * 1.5f)
            {
                fireTimer = FireInterval;
                Vector2 dir = (player.Position - rb.position).normalized;
                Projectile.Create(rb.position + dir * 0.4f, dir, ProjectileSpeed, ContactDamage * 0.8f, 8f,
                    0, 0, new Color(0.65f, 0.25f, 0.9f), false, 0.5f);
            }
        }

        public static RangedEnemy Create(Vector2 pos, Transform parent)
        {
            var enemy = Spawn<RangedEnemy>("Acechador", pos, parent, new Color(0.2f, 0.35f, 0.7f), 0.32f, 3);
            enemy.moveSpeed = 1.35f;
            enemy.ContactDamage = 8f;
            enemy.MemoryDropChance = 30;
            return enemy;
        }
    }
}
