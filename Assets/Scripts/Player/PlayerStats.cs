using UnityEngine;

namespace AshesOfRyu
{
    [System.Serializable]
    public class PlayerStats
    {
        public float maxHealth = 100f;
        public float moveSpeed = 2.9f;

        public float clawDamage = 16f;
        public float clawRange = 3.4f;
        public float clawHalfAngle = 55f;
        public float clawCooldown = 0.4f;

        public float fireBreathCooldown = 1.1f;
        public float fireBreathDamage = 14f;
        public float fireBreathSpeed = 9f;
        public float fireBreathRange = 7f;

        public float dragonFormDuration = 8f;
        public float dragonFormCooldown = 28f;
        public float dragonFormDamageMultiplier = 3f;
        public float dragonFormMoveMultiplier = 1.6f;

        public float furyThreshold = 0.3f;
        public float furyDamageMultiplier = 1.8f;

        public float rebirthRadius = 7f;
        public float rebirthDamage = 45f;

        public float EffectiveMaxHealth(MetaProgression meta) => maxHealth + meta.BonusMaxHealth;
        public float EffectiveClawDamage(MetaProgression meta) => clawDamage + meta.BonusDamage;
        public float EffectiveFireDamage(MetaProgression meta) => fireBreathDamage + meta.BonusDamage * 0.8f;
    }
}
