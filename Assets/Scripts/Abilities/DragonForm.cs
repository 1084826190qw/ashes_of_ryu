using System;
using UnityEngine;

namespace AshesOfRyu
{
    public class DragonForm : MonoBehaviour
    {
        public bool IsActive { get; private set; }
        public float DamageMultiplier => controller.Stats.dragonFormDamageMultiplier;
        public float MoveMultiplier => controller.Stats.dragonFormMoveMultiplier;
        public float CooldownRemaining { get; private set; }

        public event Action<float> OnCooldownChanged;

        private PlayerController controller;
        private PlayerVisuals visuals;
        private float activeUntil;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            visuals = GetComponent<PlayerVisuals>();
        }

        private void Update()
        {
            if (IsActive)
            {
                if (Time.time >= activeUntil) EndForm();
                else OnCooldownChanged?.Invoke(0f);
                return;
            }

            if (CooldownRemaining <= 0f) return;
            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - Time.deltaTime);
            OnCooldownChanged?.Invoke(CooldownRemaining / controller.Stats.dragonFormCooldown);
        }

        public void TryActivate()
        {
            var stats = controller.Stats;
            if (IsActive || CooldownRemaining > 0f) return;

            IsActive = true;
            activeUntil = Time.time + stats.dragonFormDuration;
            visuals.SetDragonForm(true);
            OnCooldownChanged?.Invoke(0f);
            FloatingText.Spawn(controller.Position + Vector2.up * 0.8f, "FORMA DE DRAGÓN", new Color(1f, 0.55f, 0.1f), 72f);
        }

        private void EndForm()
        {
            IsActive = false;
            CooldownRemaining = controller.Stats.dragonFormCooldown;
            visuals.SetDragonForm(false);
            OnCooldownChanged?.Invoke(1f);
        }
    }
}
