using UnityEngine;

namespace AshesOfRyu
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public Health Health { get; private set; }
        public float InvulnDuration = 0.5f;
        public float RespawnInvulnDuration = 1.5f;

        public PlayerStats Stats => controller != null ? controller.Stats : null;

        private PlayerController controller;
        private PlayerVisuals visuals;
        private PlayerAnimator animator;
        private float invulnUntil;
        private bool lastFury;
        private bool initialized;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            visuals = GetComponent<PlayerVisuals>();
            animator = GetComponent<PlayerAnimator>();
            InitializeHealth();
        }

        private void InitializeHealth()
        {
            if (GameManager.Instance != null && GameManager.Instance.Meta != null && controller != null && controller.Stats != null)
            {
                Health = new Health(controller.Stats.EffectiveMaxHealth(GameManager.Instance.Meta));
                Health.IsInvulnerable = true;
                invulnUntil = Time.time + RespawnInvulnDuration;
                initialized = true;
            }
        }

        private void Start()
        {
            if (!initialized)
            {
                InitializeHealth();
            }
        }

        private void Update()
        {
            if (!initialized || Health == null) return;

            float ratio = Health.Max > 0f ? Health.Current / Health.Max : 0f;
            bool fury = ratio <= controller.Stats.furyThreshold;
            if (fury != lastFury)
            {
                lastFury = fury;
                visuals.SetFuryActive(fury);
            }

            if (Health.IsInvulnerable && Time.time >= invulnUntil)
            {
                Health.IsInvulnerable = false;
                visuals.SetInvuln(false);
            }
        }

        public void TakeDamage(DamageInfo info)
        {
            if (Health == null || Health.IsInvulnerable || GameManager.Instance?.PlayerIsDead == true) return;
            Health.TakeDamage(info);
            if (Health.Current <= 0f)
            {
                Die();
                return;
            }

            controller.AddKnockback(info.Knockback);
            visuals.FlashDamage();
            if (animator != null) animator.PlayDamage();
            FloatingText.Spawn(transform.position, Mathf.RoundToInt(info.Amount).ToString(), Color.white, 56f);

            invulnUntil = Time.time + InvulnDuration;
            Health.IsInvulnerable = true;
            visuals.SetInvuln(true);
        }

        public void Die()
        {
            GameManager.Instance?.PlayerDied(this);
        }

        public void Recover()
        {
            if (Health != null && controller != null && controller.Stats != null && GameManager.Instance?.Meta != null)
            {
                Health.SetMax(controller.Stats.EffectiveMaxHealth(GameManager.Instance.Meta), true);
            }
            invulnUntil = Time.time + RespawnInvulnDuration;
            if (Health != null) Health.IsInvulnerable = true;
            visuals.SetInvuln(false);
            lastFury = false;
            visuals.SetFuryActive(false);
        }
    }
}
