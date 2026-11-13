using UnityEngine;

namespace AshesOfRyu
{
    /// <summary>
    /// Animacion visual sencilla para los enemigos esqueleto.
    /// No modifica movimiento, dano ni colision: solo decide que frame mostrar.
    /// </summary>
    public class SkeletonEnemyVisuals : MonoBehaviour
    {
        private const int FrameSize = 32;
        private const float WalkFps = 10f;
        private const float IdleFps = 8f;
        private const float ActionFps = 10f;

        private SpriteRenderer sr;
        private Enemy enemy;
        private Health health;

        private Sprite[] idleFrames;
        private Sprite[] twitchFrames;
        private Sprite[] walkFrames;
        private Sprite[] hurtFrames;
        private Sprite[] spawnFrames;
        private Sprite[] deathFrames;
        private Sprite[] attackFrames;
        private Sprite[] pileFrames;

        private Sprite[] currentFrames;
        private int currentFrame;
        private float frameTimer;
        private float frameDuration;
        private bool loop;
        private bool oneShot;
        private bool moving;
        private float lastHealth;
        private Vector2 lastPosition;
        private float idleTwitchTimer;
        private float twitchTimer;
        private bool twitching;
        private bool dead;

        public static void Attach(GameObject target)
        {
            if (target == null) return;
            if (target.GetComponent<SkeletonEnemyVisuals>() == null)
                target.AddComponent<SkeletonEnemyVisuals>();
        }

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            enemy = GetComponent<Enemy>();

            if (sr != null) sr.color = Color.white;

            idleFrames = SkeletonSpriteLoader.GetFrames("Standing_Idle", FrameSize, FrameSize);
            twitchFrames = SkeletonSpriteLoader.GetFrames("Twitching_Idle", FrameSize, FrameSize);
            walkFrames = SkeletonSpriteLoader.GetFrames("Limping_Movement", FrameSize, FrameSize);
            hurtFrames = SkeletonSpriteLoader.GetFrames("Hurt", FrameSize, FrameSize);
            spawnFrames = SkeletonSpriteLoader.GetFrames("Bone_Pile_Wakeup", FrameSize, FrameSize);
            deathFrames = SkeletonSpriteLoader.GetFrames("Crumbling_Into_Bone_Pile", FrameSize, FrameSize);
            attackFrames = SkeletonSpriteLoader.GetFrames("Bone_Toss", FrameSize, FrameSize);
            pileFrames = SkeletonSpriteLoader.GetFrames("Bone_Pile", FrameSize, FrameSize);
        }

        private void Start()
        {
            lastPosition = transform.position;
            BindHealth();

            if (spawnFrames != null && spawnFrames.Length > 0)
                PlayOneShot(spawnFrames, ActionFps);
            else
                PlayBaseAnimation();
        }

        private void Update()
        {
            if (dead) return;

            if (health == null) BindHealth();

            UpdateMovement();
            UpdateTwitch();
            AdvanceFrames();
        }

        public void PlayAttack()
        {
            if (dead) return;
            PlayOneShot(attackFrames, ActionFps);
        }

        private void BindHealth()
        {
            if (enemy == null || enemy.Health == null) return;

            health = enemy.Health;
            lastHealth = health.Current;
            health.OnChanged += OnHealthChanged;
            health.OnDepleted += OnHealthDepleted;
        }

        private void OnDestroy()
        {
            if (health == null) return;
            health.OnChanged -= OnHealthChanged;
            health.OnDepleted -= OnHealthDepleted;
        }

        private void OnHealthChanged(float current, float max)
        {
            if (dead) return;

            bool damaged = current < lastHealth - 0.001f;
            lastHealth = current;

            if (damaged && current > 0f)
                PlayOneShot(hurtFrames, ActionFps);
        }

        private void OnHealthDepleted(Health depleted)
        {
            if (dead) return;

            dead = true;
            SpawnCorpse();
        }

        private void SpawnCorpse()
        {
            if (deathFrames == null || deathFrames.Length == 0) return;

            var corpseGo = new GameObject("SkeletonCorpse");
            corpseGo.transform.position = transform.position;
            corpseGo.transform.localScale = transform.localScale;

            var corpseRenderer = corpseGo.AddComponent<SpriteRenderer>();
            corpseRenderer.sortingOrder = sr != null ? sr.sortingOrder : 3;
            corpseRenderer.flipX = sr != null && sr.flipX;

            Sprite pile = pileFrames != null && pileFrames.Length > 0 ? pileFrames[0] : null;
            var corpse = corpseGo.AddComponent<SkeletonCorpseAnimator>();
            corpse.Init(deathFrames, pile, ActionFps, 12f);
        }

        private void UpdateMovement()
        {
            Vector2 position = transform.position;
            Vector2 delta = position - lastPosition;
            bool nowMoving = delta.sqrMagnitude > 0.000001f;

            if (Mathf.Abs(delta.x) > 0.0001f && sr != null)
                sr.flipX = delta.x < 0f;

            lastPosition = position;

            if (nowMoving == moving) return;

            moving = nowMoving;
            if (!oneShot) PlayBaseAnimation();
        }

        private void UpdateTwitch()
        {
            if (moving)
            {
                twitching = false;
                idleTwitchTimer = Random.Range(5f, 10f);
                return;
            }

            if (oneShot) return;

            if (twitching)
            {
                twitchTimer -= Time.deltaTime;
                if (twitchTimer <= 0f)
                {
                    twitching = false;
                    PlayBaseAnimation();
                }
                return;
            }

            idleTwitchTimer -= Time.deltaTime;
            if (idleTwitchTimer <= 0f)
            {
                twitching = true;
                twitchTimer = Random.Range(0.8f, 1.6f);
                idleTwitchTimer = Random.Range(5f, 10f);
                PlayLoop(twitchFrames, IdleFps);
            }
        }

        private void AdvanceFrames()
        {
            if (currentFrames == null || currentFrames.Length == 0) return;

            frameTimer += Time.deltaTime;
            while (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;

                if (loop)
                {
                    currentFrame = (currentFrame + 1) % currentFrames.Length;
                }
                else
                {
                    if (currentFrame < currentFrames.Length - 1)
                    {
                        currentFrame++;
                    }
                    else
                    {
                        oneShot = false;
                        PlayBaseAnimation();
                        return;
                    }
                }
            }

            if (sr != null)
                sr.sprite = currentFrames[Mathf.Clamp(currentFrame, 0, currentFrames.Length - 1)];
        }

        private void PlayBaseAnimation()
        {
            if (dead) return;

            if (moving)
            {
                PlayLoop(walkFrames, WalkFps);
                return;
            }

            PlayLoop(twitching ? twitchFrames : idleFrames, IdleFps);
        }

        private void PlayLoop(Sprite[] frames, float fps)
        {
            if (frames == null || frames.Length == 0) return;
            if (currentFrames == frames && loop) return;

            currentFrames = frames;
            currentFrame = 0;
            frameTimer = 0f;
            frameDuration = 1f / Mathf.Max(1f, fps);
            loop = true;
            oneShot = false;

            if (sr != null) sr.sprite = currentFrames[0];
        }

        private void PlayOneShot(Sprite[] frames, float fps)
        {
            if (frames == null || frames.Length == 0) return;

            currentFrames = frames;
            currentFrame = 0;
            frameTimer = 0f;
            frameDuration = 1f / Mathf.Max(1f, fps);
            loop = false;
            oneShot = true;

            if (sr != null) sr.sprite = currentFrames[0];
        }
    }
}
