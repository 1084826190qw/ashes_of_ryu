using System;
using UnityEngine;

namespace AshesOfRyu
{
    public class Health
    {
        public float Max { get; private set; }
        public float Current { get; private set; }
        public bool IsInvulnerable { get; set; }

        public event Action<float, float> OnChanged;
        public event Action<Health> OnDepleted;

        public Health(float max)
        {
            Max = max;
            Current = max;
        }

        public void SetMax(float max, bool refill = true)
        {
            Max = max;
            if (refill) Current = max;
            else Current = Mathf.Min(Current, max);
            OnChanged?.Invoke(Current, Max);
        }

        public void Heal(float amount)
        {
            Current = Mathf.Min(Max, Current + amount);
            OnChanged?.Invoke(Current, Max);
        }

        public void TakeDamage(DamageInfo info)
        {
            if (IsInvulnerable || Current <= 0f) return;
            Current = Mathf.Max(0f, Current - info.Amount);
            OnChanged?.Invoke(Current, Max);
            if (Current <= 0f) OnDepleted?.Invoke(this);
        }
    }
}
