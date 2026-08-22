using UnityEngine;

namespace AshesOfRyu
{
    public class PlayerCombat : MonoBehaviour
    {
        private PlayerController controller;
        private PlayerHealth health;
        private DragonForm dragonForm;
        private ImmortalFury fury;
        private PlayerAnimator animator;
        private float clawReadyAt;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            health = GetComponent<PlayerHealth>();
            dragonForm = GetComponent<DragonForm>();
            fury = new ImmortalFury(controller.Stats.furyThreshold, controller.Stats.furyDamageMultiplier);
            animator = GetComponent<PlayerAnimator>();
        }

        private void Update()
        {
            var input = InputReader.Instance;
            if (input == null || GameManager.Instance.PlayerIsDead) return;
            if (input.AttackPressed && Time.time >= clawReadyAt) PerformClaw();
        }

        private void PerformClaw()
        {
            var stats = controller.Stats;
            clawReadyAt = Time.time + stats.clawCooldown;

            Vector2 facing = controller.Facing;
            Vector2 origin = controller.Position;
            Vector2 center = origin + facing * (stats.clawRange * 0.45f);

            float damage = EffectiveDamage();
            var hits = Physics2D.OverlapCircleAll(center, stats.clawRange * 0.6f);
            bool hit = false;
            foreach (var collider in hits)
            {
                var enemy = collider.GetComponentInParent<Enemy>();
                if (enemy == null) continue;

                Vector2 toEnemy = (Vector2)enemy.transform.position - origin;
                if (Vector2.Angle(facing, toEnemy.normalized) > stats.clawHalfAngle) continue;

                enemy.TakeDamage(new DamageInfo { Amount = damage, Knockback = facing * 7f, Source = gameObject });
                hit = true;
            }

            VFX.ClawSwing(center, facing, hit ? Color.white : new Color(1f, 0.9f, 0.7f, 0.6f));
            if (animator != null) animator.PlayAttack();
        }

        public float EffectiveDamage()
        {
            var stats = controller.Stats;
            float damage = stats.EffectiveClawDamage(GameManager.Instance.Meta);
            float multiplier = 1f;
            if (dragonForm != null && dragonForm.IsActive) multiplier *= dragonForm.DamageMultiplier;
            if (fury != null) multiplier *= fury.GetMultiplier(health.Health.Current, health.Health.Max);
            return damage * multiplier;
        }
    }
}
