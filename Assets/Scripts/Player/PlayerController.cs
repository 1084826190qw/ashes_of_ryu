using UnityEngine;

namespace AshesOfRyu
{
    public class PlayerController : MonoBehaviour
    {
        public PlayerStats Stats { get; private set; }
        public Vector2 Facing { get; private set; } = Vector2.right;
        public Vector2 Position => rb.position;
        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;

        private Rigidbody2D rb;
        private Vector2 knockback;
        private DragonForm dragon;

        private DragonForm Dragon => dragon != null ? dragon : (dragon = GetComponent<DragonForm>());

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            Stats = new PlayerStats();
        }

        private void Update()
        {
            var input = InputReader.Instance;
            if (input == null) return;

            Vector2 aim = Vector2.zero;
            if (input.RightStick.sqrMagnitude > 0.09f) aim = input.RightStick;
            else if (input.MouseActive) aim = (Vector2)MouseWorld() - Position;
            else if (input.MoveAxis.sqrMagnitude > 0.04f) aim = input.MoveAxis;
            if (aim.sqrMagnitude > 0.01f) Facing = aim.normalized;
        }

        private void FixedUpdate()
        {
            var input = InputReader.Instance;
            float speed = Stats.moveSpeed;
            if (Dragon != null && Dragon.IsActive) speed *= Dragon.MoveMultiplier;

            Vector2 velocity = Vector2.zero;
            if (input != null && !GameManager.Instance.PlayerIsDead)
            {
                velocity = input.MoveAxis.normalized * speed;
            }
            velocity += knockback;
            knockback = Vector2.Lerp(knockback, Vector2.zero, Time.fixedDeltaTime * 10f);
            rb.linearVelocity = velocity;
        }

        public void AddKnockback(Vector2 force)
        {
            knockback += force;
        }

        private Vector2 MouseWorld()
        {
            var cam = Camera.main;
            var input = InputReader.Instance;
            if (cam == null || input == null) return rb.position;
            var screen = new Vector3(input.MousePosition.x, input.MousePosition.y, Mathf.Abs(cam.transform.position.z));
            return cam.ScreenToWorldPoint(screen);
        }
    }
}
