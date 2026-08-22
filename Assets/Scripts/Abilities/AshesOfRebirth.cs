using UnityEngine;

namespace AshesOfRyu
{
    public static class AshesOfRebirth
    {
        public static void Explode(Vector2 position, float radius, float damage)
        {
            var hits = Physics2D.OverlapCircleAll(position, radius);
            foreach (var collider in hits)
            {
                var enemy = collider.GetComponentInParent<Enemy>();
                if (enemy == null) continue;
                Vector2 dir = ((Vector2)enemy.transform.position - position).normalized;
                enemy.TakeDamage(new DamageInfo { Amount = damage, Knockback = dir * 6f, Source = null });
            }

            VFX.Explosion(position, radius);
            FloatingText.Spawn(position, "CENIZAS DEL RENACIMIENTO", new Color(1f, 0.5f, 0.1f), 64f);
        }
    }
}
