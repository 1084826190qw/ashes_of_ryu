using System.Collections;
using UnityEngine;

namespace AshesOfRyu
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public MetaProgression Meta { get; set; }
        public PlayerController Player { get; set; }
        public PlayerHealth PlayerHealth { get; set; }
        public PlayerVisuals PlayerVisuals { get; set; }
        public Vector2 RespawnPoint { get; set; }
        public bool PlayerIsDead { get; private set; }

        public event System.Action OnPlayerDied;
        public event System.Action OnRunRestarted;

        public float DifficultyScale => 1f + Meta.Deaths * 0.15f;
        public string CurrentAreaName => "Bosque Oscuro";

        private readonly string[] memoryLines =
        {
            "El mago temía el poder de los dragones... y por ello los exterminó.",
            "Cada muerte convierte tu cuerpo en cenizas y te arrebata un recuerdo.",
            "La maldición fue lanzada para que jamás pudieras descansar.",
            "Aunque lo olvides... el castillo del mago espera al final del camino.",
            "Tus garras recuerdan el fuego de tus ancestros.",
            "Si algún día pudieras morir... ¿de verdad querrías hacerlo?",
        };

        private InputReader input;
        private EnemySpawner spawner;
        private HUD hud;
        private DeathScreen deathScreen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(MetaProgression meta, InputReader input, EnemySpawner spawner, HUD hud, DeathScreen deathScreen)
        {
            Meta = meta;
            this.input = input;
            this.spawner = spawner;
            this.hud = hud;
            this.deathScreen = deathScreen;
        }

        public void PlayerDied(PlayerHealth health)
        {
            if (PlayerIsDead) return;
            PlayerIsDead = true;
            OnPlayerDied?.Invoke();
            PlayerVisuals.SetDead(true);

            var stats = health.Stats;
            AshesOfRebirth.Explode(health.transform.position, stats.rebirthRadius, stats.rebirthDamage);

            spawner.ClearAll();
            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            // Primero se ve la animacion de muerte y la explosion de cenizas...
            yield return new WaitForSeconds(1.0f);

            deathScreen.Show(Meta.Deaths + 1, Meta.Memories);
            yield return new WaitForSeconds(1.9f);

            Meta.Deaths++;
            PlayerIsDead = false;
            Player.transform.position = RespawnPoint;
            PlayerHealth.Recover();
            PlayerVisuals.SetDead(false);
            deathScreen.Hide();
            spawner.ResetForRun(Meta.Deaths);
            OnRunRestarted?.Invoke();
        }

        public void CollectMemory()
        {
            int before = Meta.FireBreathLevel;
            Meta.CollectMemory();
            int after = Meta.FireBreathLevel;

            string line = memoryLines[(Meta.Memories - 1) % memoryLines.Length];
            if (after > before)
            {
                line += "\n" + (after == 2 ? "¡Aliento de fuego azul despertado!" : "¡Aliento de fuego negro despertado!");
            }
            hud.ShowMemoryLine(line);
        }
    }
}
