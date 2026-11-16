using System.Collections;
using UnityEngine;

namespace AshesOfRyu
{
    public class HUD : MonoBehaviour
    {
        private UIHelpers.Bar healthBar;
        private UIHelpers.Bar fireBar;
        private UIHelpers.Bar dragonBar;
        private PlayerHealth playerHealth;

        public void Build(Canvas canvas)
        {
            var root = new GameObject("HUD", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            healthBar = UIHelpers.CreateBar(root.transform, "Vida", new Vector2(0f, 0f), new Vector2(30f, 90f),
                new Vector2(320f, 26f), new Color(0f, 0f, 0f, 0.6f), new Color(0.85f, 0.15f, 0.15f));

            fireBar = UIHelpers.CreateBar(root.transform, "Aliento", new Vector2(0f, 0f), new Vector2(30f, 58f),
                new Vector2(220f, 22f), new Color(0f, 0f, 0f, 0.6f), new Color(1f, 0.45f, 0.1f));

            dragonBar = UIHelpers.CreateBar(root.transform, "Forma", new Vector2(0f, 0f), new Vector2(30f, 30f),
                new Vector2(220f, 22f), new Color(0f, 0f, 0f, 0.6f), new Color(0.9f, 0.6f, 0.1f));
        }

        public void Bind(GameManager game, PlayerHealth playerHealth, PlayerAbilities abilities)
        {
            this.playerHealth = playerHealth;

            playerHealth.Health.OnChanged += OnHealthChanged;
            OnHealthChanged(playerHealth.Health.Current, playerHealth.Health.Max);

            abilities.FireBreath.OnCooldownChanged += t => UIHelpers.SetBar(fireBar, 1f - t);
            abilities.DragonForm.OnCooldownChanged += t => UIHelpers.SetBar(dragonBar, 1f - t);
        }

        private void OnHealthChanged(float current, float max)
        {
            UIHelpers.SetBar(healthBar, max > 0f ? current / max : 0f);
        }

        public void SetMemoriesText(int memories) { }

        public void SetDeaths(int deaths) { }

        public void SetArea(string area) { }

        public void ShowMemoryLine(string line) { }
    }
}
