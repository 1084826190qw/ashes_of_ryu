namespace AshesOfRyu
{
    public class ImmortalFury
    {
        private readonly float threshold;
        private readonly float multiplier;

        public ImmortalFury(float threshold, float multiplier)
        {
            this.threshold = threshold;
            this.multiplier = multiplier;
        }

        public float GetMultiplier(float current, float max)
        {
            if (max <= 0f) return 1f;
            return current / max <= threshold ? multiplier : 1f;
        }
    }
}
