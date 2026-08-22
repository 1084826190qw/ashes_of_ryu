using System.Collections.Generic;
using UnityEngine;

namespace AshesOfRyu
{
    public class EnemySpawner : MonoBehaviour
    {
        private Transform container;
        private Vector2 center;
        private float halfW;
        private float halfH;
        private float spawnTimer;
        private int runIndex;

        private readonly List<Enemy> alive = new List<Enemy>();

        public void Configure(Transform parent, Vector2 center, float halfW, float halfH)
        {
            this.center = center;
            this.halfW = halfW;
            this.halfH = halfH;
            container = new GameObject("Enemigos").transform;
            container.SetParent(parent, false);
        }

        public void ResetForRun(int deaths)
        {
            ClearAll();
            runIndex = deaths;
            spawnTimer = 1.2f;
            int count = 1 + Mathf.Min(runIndex, 6);
            for (int i = 0; i < count; i++) SpawnRandom();
        }

        public void ClearAll()
        {
            foreach (var enemy in alive)
            {
                if (enemy != null) Destroy(enemy.gameObject);
            }
            alive.Clear();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.PlayerIsDead) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f) return;

            int maxAlive = 5 + Mathf.Min(runIndex, 10);
            spawnTimer = Mathf.Max(1.2f, 3.5f - runIndex * 0.2f);
            if (alive.Count < maxAlive) SpawnRandom();
        }

        private void SpawnRandom()
        {
            var player = GameManager.Instance.Player;
            Vector2 pos;
            int attempts = 0;
            do
            {
                pos = new Vector2(
                    center.x + Random.Range(-halfW, halfW),
                    center.y + Random.Range(-halfH, halfH));
                attempts++;
            }
            while (player != null && Vector2.Distance(pos, player.Position) < 3f && attempts < 20);

            Enemy enemy = Random.value < 0.3f
                ? RangedEnemy.Create(pos, container)
                : ChaserEnemy.Create(pos, container);

            enemy.Configure(GameManager.Instance.DifficultyScale);
            enemy.Died += OnEnemyDied;
            alive.Add(enemy);
        }

        private void OnEnemyDied(Enemy enemy)
        {
            alive.Remove(enemy);
        }
    }
}
