using System;

namespace AshesOfRyu
{
    public class MetaProgression
    {
        public int Deaths { get; set; }
        public int Memories { get; private set; }

        public float BonusMaxHealth { get; private set; }
        public float BonusDamage { get; private set; }
        public int FireBreathLevel { get; private set; } = 1;

        public const int FireBreathLevelUpEvery = 3;

        public event Action<int> OnMemoriesChanged;

        public void CollectMemory()
        {
            Memories++;
            BonusMaxHealth += 8f;
            BonusDamage += 1.2f;
            if (FireBreathLevel < 3 && Memories % FireBreathLevelUpEvery == 0) FireBreathLevel++;
            OnMemoriesChanged?.Invoke(Memories);
        }

        public void ResetRun()
        {
            Deaths = 0;
            Memories = 0;
            BonusMaxHealth = 0f;
            BonusDamage = 0f;
            FireBreathLevel = 1;
        }
    }
}
