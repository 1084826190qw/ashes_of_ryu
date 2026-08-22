using UnityEngine;

namespace AshesOfRyu
{
    public class ChaserEnemy : Enemy
    {
        protected override float BaseHealth() => 30f;

        protected override Vector2 GetMoveDirection()
        {
            var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (player == null) return Vector2.zero;
            Vector2 to = player.Position - rb.position;
            return to.sqrMagnitude > 0.1f ? to.normalized : Vector2.zero;
        }

        public static ChaserEnemy Create(Vector2 pos, Transform parent)
        {
            var enemy = Spawn<ChaserEnemy>("EspectroDelBosque", pos, parent, new Color(0.55f, 0.1f, 0.45f), 0.32f, 3);
            enemy.moveSpeed = 2.1f;
            enemy.ContactDamage = 12f;
            enemy.MemoryDropChance = 35;
            return enemy;
        }
    }
}
