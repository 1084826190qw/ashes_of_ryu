using System;
using UnityEngine;

namespace AshesOfRyu
{
    public class FireBreath : MonoBehaviour
    {
        public float CooldownRemaining { get; private set; }
        public event Action<float> OnCooldownChanged;

        private PlayerController controller;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (CooldownRemaining <= 0f) return;
            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - Time.deltaTime);
            OnCooldownChanged?.Invoke(CooldownRemaining / controller.Stats.fireBreathCooldown);
        }

        public void TryFire()
        {
            var stats = controller.Stats;
            if (CooldownRemaining > 0f) return;
            CooldownRemaining = stats.fireBreathCooldown;
            OnCooldownChanged?.Invoke(1f);

            int level = GameManager.Instance.Meta.FireBreathLevel;
            Color color = new Color(1f, 0.45f, 0.1f);
            if (level >= 2) color = new Color(0.3f, 0.6f, 1f);
            if (level >= 3) color = new Color(0.12f, 0.04f, 0.22f);

            float damage = stats.EffectiveFireDamage(GameManager.Instance.Meta) * (level >= 2 ? 1.5f : 1f);
            int pierce = level >= 2 ? 1 : 0;
            int bounces = level >= 3 ? 1 : 0;

            Projectile.Create(
                controller.Position + controller.Facing * 1.0f,
                controller.Facing,
                stats.fireBreathSpeed,
                damage,
                stats.fireBreathRange,
                pierce,
                bounces,
                color,
                true,
                1.25f + level * 0.15f); // cada nivel de poder es mas grande

            var animator = GetComponent<PlayerAnimator>();
            if (animator != null) animator.PlayAttack();
        }
    }
}
