using UnityEngine;

namespace AshesOfRyu
{
    /// <summary>
    /// Jefe del primer nivel. Usa las animaciones de Assets/Scripts/jefe_primer_nivel:
    /// idle_sheet, walk_sheet, run_sheet, attack_sheet y jump_sheet.
    /// Se mueve dentro de los limites marcados por el mapa.
    /// </summary>
    public class BossPrimerNivel : Enemy
    {
        private enum BossState
        {
            Idle,
            Walk,
            Run,
            Attack,
            Jump
        }

        [Header("Animaciones")]
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private Sprite[] runFrames;
        [SerializeField] private Sprite[] attackFrames;
        [SerializeField] private Sprite[] jumpFrames;
        [SerializeField] private float framesPerSecond = 10f;

        [Header("Limites del mapa")]
        [Tooltip("Hierba_23_9")]
        [SerializeField] private float minX = 23.5f;
        [Tooltip("Hierba_31_4 (3)")]
        [SerializeField] private float minY = 4.5f;
        [Tooltip("MuroEste")]
        [SerializeField] private float maxX = 63.13f;
        [Tooltip("MuroNorte")]
        [SerializeField] private float maxY = 13.5f;
        [SerializeField] private float edgeMargin = 0.75f;

        [Header("Comportamiento")]
        [SerializeField] private float patrolRadius = 4f;
        [SerializeField] private float patrolSpeed = 1.595f;
        [SerializeField] private float chaseSpeed = 3.915f;
        [SerializeField] private float detectionRadius = 11f;
        [Tooltip("Distancia a la que se detiene para atacar. Debe ser pequena para que su cuerpo alcance al jugador.")]
        [SerializeField] private float attackRadius = 0.45f;
        [Tooltip("Alcance del golpe durante la animacion de ataque.")]
        [SerializeField] private float attackHitRadius = 1.0f;
        [SerializeField] private float attackCooldown = 1.6f;
        [SerializeField] private float jumpCooldown = 5f;
        [SerializeField] private float jumpDuration = 0.8f;
        [SerializeField] private float jumpHeight = 0.75f;

        [Header("Recompensa")]
        [SerializeField] private Sprite keySprite;

        private SpriteRenderer visualRenderer;
        private Transform visual;
        private Vector2 patrolOrigin;
        private float patrolDirection = 1f;
        private float patrolPause;
        private float attackTimer;
        private float attackDuration;
        private bool attackDamageApplied;
        private float attackCooldownTimer;
        private float jumpTimer;
        private float jumpCooldownTimer;
        private float animationTimer;
        private int animationFrame;
        private Sprite[] currentFrames;
        private BossState state;
        private bool facingLeft;

        protected override float BaseHealth() => 480f;

        protected override void Awake()
        {
            base.Awake();

            if (Health == null) Configure(1f);
            ContactDamage = 30.6f;
            moveSpeed = chaseSpeed;
            patrolOrigin = rb != null ? rb.position : (Vector2)transform.position;

            visualRenderer = GetComponentInChildren<SpriteRenderer>();
            if (visualRenderer != null)
            {
                visual = visualRenderer.transform;
                visualRenderer.sortingOrder = 3;
            }
        }

        protected override void OnDeath(Vector2 deathPosition)
        {
            base.OnDeath(deathPosition);
            LlaveCastilloPickup.Create(deathPosition, keySprite);
        }

        protected override bool ShouldDropMemory()
        {
            return false;
        }

        private void TryDealAttackDamage()
        {
            if (attackDamageApplied || rb == null) return;

            var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (player == null || GameManager.Instance.PlayerIsDead) return;
            if (Vector2.Distance(rb.position, player.Position) > attackHitRadius) return;

            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null) return;

            Vector2 direction = ((Vector2)player.transform.position - rb.position).normalized;
            playerHealth.TakeDamage(new DamageInfo
            {
                Amount = ContactDamage,
                Knockback = direction * 4f,
                Source = gameObject
            });
            attackDamageApplied = true;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            attackCooldownTimer -= dt;
            jumpCooldownTimer -= dt;
            patrolPause -= dt;
            if (attackTimer > 0f) attackTimer -= dt;
            if (jumpTimer > 0f) jumpTimer -= dt;

            if (rb == null) return;

            var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            bool chasing = false;
            Vector2 playerPosition = Vector2.zero;

            if (player != null && !GameManager.Instance.PlayerIsDead)
            {
                playerPosition = player.Position;
                float distance = Vector2.Distance(rb.position, playerPosition);
                chasing = IsInsideArena(playerPosition) && distance <= detectionRadius;

                if (distance <= detectionRadius)
                {
                    facingLeft = playerPosition.x < rb.position.x;
                }

                if (chasing && distance <= attackRadius && attackTimer <= 0f && attackCooldownTimer <= 0f)
                {
                    attackDuration = AnimationDuration(attackFrames);
                    attackTimer = attackDuration;
                    attackCooldownTimer = attackCooldown;
                    attackDamageApplied = false;
                }

                if (chasing && distance > attackRadius * 2.5f && jumpTimer <= 0f && jumpCooldownTimer <= 0f)
                {
                    jumpTimer = jumpDuration;
                    jumpCooldownTimer = jumpCooldown;
                }
            }

            // El golpe se aplica a un tercio de la animacion, cuando la garra ya esta extendida.
            if (attackTimer > 0f && !attackDamageApplied && attackDuration > 0f)
            {
                float elapsed = attackDuration - attackTimer;
                if (elapsed >= attackDuration / 3f) TryDealAttackDamage();
            }

            if (attackTimer > 0f) state = BossState.Attack;
            else if (jumpTimer > 0f) state = BossState.Jump;
            else if (chasing) state = BossState.Run;
            else if (patrolPause > 0f) state = BossState.Idle;
            else state = BossState.Walk;

            UpdateAnimation(dt);
        }

        protected override Vector2 GetMoveDirection()
        {
            if (rb == null) return Vector2.zero;
            if (attackTimer > 0f) return Vector2.zero;
            if (GameManager.Instance != null && GameManager.Instance.PlayerIsDead) return Vector2.zero;

            var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (player != null && IsInsideArena(player.Position))
            {
                Vector2 toPlayer = player.Position - rb.position;
                if (toPlayer.magnitude <= detectionRadius)
                {
                    if (toPlayer.magnitude <= attackRadius) return Vector2.zero;
                    return toPlayer.normalized;
                }
            }

            if (patrolPause > 0f) return Vector2.zero;

            float targetX = patrolOrigin.x + patrolDirection * patrolRadius;
            if (Mathf.Abs(rb.position.x - targetX) < 0.25f)
            {
                patrolDirection *= -1f;
                patrolPause = Random.Range(0.8f, 1.6f);
                return Vector2.zero;
            }

            float speedFactor = patrolSpeed / Mathf.Max(0.01f, moveSpeed);
            Vector2 patrol = new Vector2(patrolDirection * speedFactor, 0f);
            if (Mathf.Abs(patrol.x) > 0.01f) facingLeft = patrol.x < 0f;
            return patrol;
        }

        protected override Vector2 ClampPosition(Vector2 position)
        {
            position.x = Mathf.Clamp(position.x, minX + edgeMargin, maxX - edgeMargin);
            position.y = Mathf.Clamp(position.y, minY + edgeMargin, maxY - edgeMargin);
            return position;
        }

        private bool IsInsideArena(Vector2 position)
        {
            return position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY;
        }

        private float AnimationDuration(Sprite[] frames)
        {
            int count = frames != null ? frames.Length : 0;
            return count > 0 ? count / Mathf.Max(1f, framesPerSecond) : 0.5f;
        }

        private void UpdateAnimation(float dt)
        {
            if (visualRenderer == null || visual == null) return;

            Sprite[] frames;
            bool loop;
            switch (state)
            {
                case BossState.Attack:
                    frames = attackFrames;
                    loop = false;
                    break;
                case BossState.Jump:
                    frames = jumpFrames;
                    loop = false;
                    break;
                case BossState.Run:
                    frames = runFrames;
                    loop = true;
                    break;
                case BossState.Walk:
                    frames = walkFrames;
                    loop = true;
                    break;
                default:
                    frames = idleFrames;
                    loop = true;
                    break;
            }

            if (frames == null || frames.Length == 0) frames = idleFrames;
            if (frames == null || frames.Length == 0) return;

            if (frames != currentFrames)
            {
                currentFrames = frames;
                animationFrame = 0;
                animationTimer = 0f;
            }

            animationTimer += dt;
            float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);
            while (animationTimer >= frameDuration)
            {
                animationTimer -= frameDuration;
                if (loop) animationFrame = (animationFrame + 1) % frames.Length;
                else animationFrame = Mathf.Min(animationFrame + 1, frames.Length - 1);
            }

            Sprite sprite = frames[Mathf.Clamp(animationFrame, 0, frames.Length - 1)];
            visualRenderer.sprite = sprite;

            // Ancla la parte inferior central de cada frame al origen del jefe
            // para que las animaciones no salten al cambiar de sprite.
            Bounds bounds = sprite.bounds;
            Vector3 offset = new Vector3(-bounds.center.x, -bounds.min.y, 0f);
            float bob = 0f;
            if (jumpTimer > 0f && jumpDuration > 0f)
            {
                float progress = 1f - Mathf.Clamp01(jumpTimer / jumpDuration);
                bob = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
            }
            visual.localPosition = offset + new Vector3(0f, bob, 0f);
            visualRenderer.flipX = facingLeft;
        }
    }
}
