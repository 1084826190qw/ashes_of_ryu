using System.Collections.Generic;
using UnityEngine;

namespace AshesOfRyu
{
    public class EnemySpawner : MonoBehaviour
    {
        // Ocho esqueletos fijos repartidos por todo el mapa.
        // Se quedan quietos, pero siguen animados y algunos lanzan proyectiles.
        private static readonly Vector2[] StaticSpawnPoints =
        {
            new Vector2(-11.5f, 9.0f),
            new Vector2(-1.0f, -9.0f),
            new Vector2(7.0f, 7.0f),
            new Vector2(10.5f, -4.0f),
            new Vector2(17.0f, 3.5f),
            new Vector2(28.0f, -10.5f),
            new Vector2(37.5f, 4.0f),
            new Vector2(55.0f, -5.0f),
            new Vector2(50.49f, 5.5f),
        };

        // Algunos esqueletos son los que lanzan huesos.
        private static readonly bool[] RangedSlots =
        {
            false,
            false,
            true,
            false,
            true,
            false,
            true,
            false,
            true,
        };

        private Transform container;

        private readonly List<Enemy> alive = new List<Enemy>();

        public void Configure(Transform parent, Vector2 center, float halfW, float halfH)
        {
            container = new GameObject("Enemigos").transform;
            container.SetParent(parent, false);
        }

        public void ResetForRun(int deaths)
        {
            ClearAll();
            SpawnStaticSkeletons();
        }

        public void ClearAll()
        {
            foreach (var enemy in alive)
            {
                if (enemy != null) Destroy(enemy.gameObject);
            }
            alive.Clear();
        }

        private void SpawnStaticSkeletons()
        {
            float difficulty = GameManager.Instance != null ? GameManager.Instance.DifficultyScale : 1f;

            for (int i = 0; i < StaticSpawnPoints.Length; i++)
            {
                Vector2 pos = StaticSpawnPoints[i];
                Enemy enemy = RangedSlots[i]
                    ? RangedEnemy.Create(pos, container)
                    : ChaserEnemy.Create(pos, container);

                enemy.SetMoveSpeed(0f);
                enemy.Configure(difficulty);
                enemy.Died += OnEnemyDied;
                alive.Add(enemy);
            }
        }

        private void OnEnemyDied(Enemy enemy)
        {
            alive.Remove(enemy);
        }
    }
}
