using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AshesOfRyu
{
    public class HUD : MonoBehaviour
    {
        private UIHelpers.Bar healthBar;
        private UIHelpers.Bar fireBar;
        private UIHelpers.Bar dragonBar;
        private Text furyText;
        private Text memoriesText;
        private Text deathsText;
        private Text areaText;
        private Text memoryLine;
        private CanvasGroup memoryGroup;
        private float memoryLineShownUntil;
        private PlayerHealth playerHealth;
        private bool furyVisible;

        public void Build(Canvas canvas)
        {
            var root = new GameObject("HUD", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            healthBar = UIHelpers.CreateBar(root.transform, "Vida", new Vector2(0f, 0f), new Vector2(30f, 30f),
                new Vector2(320f, 26f), new Color(0f, 0f, 0f, 0.6f), new Color(0.85f, 0.15f, 0.15f));

            furyText = UIHelpers.Label(root.transform, "Furia", new Vector2(0f, 0f), new Vector2(30f, 70f),
                new Vector2(320f, 24f), "FURIA INMORTAL", 16, new Color(1f, 0.3f, 0.1f));
            furyText.gameObject.SetActive(false);

            memoriesText = UIHelpers.Label(root.transform, "Recuerdos", new Vector2(0f, 1f), new Vector2(30f, -30f),
                new Vector2(400f, 28f), "", 20, Color.white, TextAnchor.MiddleLeft);
            deathsText = UIHelpers.Label(root.transform, "Muertes", new Vector2(0f, 1f), new Vector2(30f, -62f),
                new Vector2(400f, 28f), "", 20, new Color(0.8f, 0.8f, 0.8f), TextAnchor.MiddleLeft);
            areaText = UIHelpers.Label(root.transform, "Zona", new Vector2(0f, 1f), new Vector2(30f, -94f),
                new Vector2(400f, 28f), "", 20, new Color(1f, 0.7f, 0.4f), TextAnchor.MiddleLeft);

            fireBar = UIHelpers.CreateBar(root.transform, "Aliento", new Vector2(1f, 0f), new Vector2(-30f, 78f),
                new Vector2(220f, 22f), new Color(0f, 0f, 0f, 0.6f), new Color(1f, 0.45f, 0.1f));
            UIHelpers.Label(root.transform, "AlientoLabel", new Vector2(1f, 0f), new Vector2(-165f, 78f),
                new Vector2(160f, 22f), "Q · Aliento", 16, Color.white, TextAnchor.MiddleRight);

            dragonBar = UIHelpers.CreateBar(root.transform, "Forma", new Vector2(1f, 0f), new Vector2(-30f, 104f),
                new Vector2(220f, 22f), new Color(0f, 0f, 0f, 0.6f), new Color(0.9f, 0.6f, 0.1f));
            UIHelpers.Label(root.transform, "FormaLabel", new Vector2(1f, 0f), new Vector2(-165f, 104f),
                new Vector2(160f, 22f), "R · Forma Dragón", 16, Color.white, TextAnchor.MiddleRight);

            var memoryPanel = UIHelpers.Panel(root.transform, "LineaRecuerdo", new Vector2(0.5f, 1f),
                new Vector2(0f, -130f), new Vector2(1000f, 70f), new Color(0f, 0f, 0f, 0.45f));
            memoryLine = UIHelpers.Label(memoryPanel, "Texto", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(980f, 60f), "", 22, new Color(0.25f, 0.9f, 1f));
            memoryGroup = memoryPanel.gameObject.AddComponent<CanvasGroup>();
            memoryGroup.alpha = 0f;

            UIHelpers.Label(root.transform, "Controles", new Vector2(0.5f, 0f), new Vector2(0f, 10f),
                new Vector2(1100f, 22f),
                "Mover: WASD · Mirar: Ratón / Stick derecho · Atacar: Click / J · Aliento: Q · Forma de Dragón: R",
                16, new Color(1f, 1f, 1f, 0.55f));
        }

        public void Bind(GameManager game, PlayerHealth playerHealth, PlayerAbilities abilities)
        {
            this.playerHealth = playerHealth;

            playerHealth.Health.OnChanged += OnHealthChanged;
            OnHealthChanged(playerHealth.Health.Current, playerHealth.Health.Max);

            abilities.FireBreath.OnCooldownChanged += t => UIHelpers.SetBar(fireBar, 1f - t);
            abilities.DragonForm.OnCooldownChanged += t => UIHelpers.SetBar(dragonBar, 1f - t);

            game.Meta.OnMemoriesChanged += SetMemoriesText;
            SetMemoriesText(game.Meta.Memories);
            SetDeaths(game.Meta.Deaths);
            SetArea(game.CurrentAreaName);
        }

        private void Update()
        {
            if (playerHealth == null || playerHealth.Health == null) return;
            bool fury = playerHealth.Health.Max > 0f &&
                        playerHealth.Health.Current / playerHealth.Health.Max <= playerHealth.Stats.furyThreshold;
            if (fury != furyVisible)
            {
                furyVisible = fury;
                furyText.gameObject.SetActive(fury);
            }
        }

        private void OnHealthChanged(float current, float max)
        {
            UIHelpers.SetBar(healthBar, max > 0f ? current / max : 0f);
        }

        public void SetMemoriesText(int memories)
        {
            memoriesText.text = "Recuerdos: " + memories;
        }

        public void SetDeaths(int deaths)
        {
            deathsText.text = "Muertes: " + deaths;
        }

        public void SetArea(string area)
        {
            areaText.text = "Zona: " + area;
        }

        public void ShowMemoryLine(string line)
        {
            memoryLine.text = line;
            memoryGroup.alpha = 1f;
            memoryLineShownUntil = Time.time + 4f;
            StopAllCoroutines();
            StartCoroutine(FadeMemoryLine());
        }

        private IEnumerator FadeMemoryLine()
        {
            while (Time.time < memoryLineShownUntil) yield return null;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.6f;
                memoryGroup.alpha = 1f - t;
                yield return null;
            }
            memoryGroup.alpha = 0f;
        }
    }
}
