using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AshesOfRyu
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private Sprite[] groundTiles;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField, Range(0f, 1f)] private float gameplayMusicVolume = 0.45f;

        private void Awake()
        {
            if (GameManager.Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            EnsureGlobalLight();
            SetupCamera();
            StartGameplayMusic();

            var meta = new MetaProgression();
            var gameManager = new GameObject("GameManager").AddComponent<GameManager>();
            gameManager.Meta = meta;

            var input = new GameObject("InputReader").AddComponent<InputReader>();

            var baked = GameObject.Find("Mundo");
            float halfW = 16f;
            float halfH = 9f;
            Vector2 spawn = new Vector2(-14.5f, 6.5f);
            if (baked == null)
                ArenaBuilder.Build(transform, groundTiles, out halfW, out halfH, out spawn);

            var player = BuildPlayer(spawn);
            FairyCompanion.Create(transform, player.transform);

            var spawner = new GameObject("EnemySpawner").AddComponent<EnemySpawner>();
            spawner.Configure(transform, Vector2.zero, halfW, halfH);

            var canvas = UIHelpers.CreateCanvas("UICanvas");
            var hud = new GameObject("HUD").AddComponent<HUD>();
            hud.Build(canvas);
            var deathScreen = new GameObject("DeathScreen").AddComponent<DeathScreen>();
            deathScreen.Build(canvas);

            gameManager.Player = player.GetComponent<PlayerController>();
            gameManager.PlayerHealth = player.GetComponent<PlayerHealth>();
            gameManager.PlayerVisuals = player.GetComponent<PlayerVisuals>();
            gameManager.RespawnPoint = spawn;

            gameManager.Initialize(meta, input, spawner, hud, deathScreen);
            hud.Bind(gameManager, player.GetComponent<PlayerHealth>(), player.GetComponent<PlayerAbilities>());

            var cameraFollow = Camera.main.gameObject.AddComponent<CameraFollow>();
            cameraFollow.SetTarget(player.transform);

            spawner.ResetForRun(0);
        }

        private void StartGameplayMusic()
        {
            if (gameplayMusic == null)
            {
                Debug.LogError("[GameBootstrap] No hay música de juego asignada.", this);
                return;
            }

            var source = gameObject.AddComponent<AudioSource>();
            source.clip = gameplayMusic;
            source.loop = true;
            source.volume = gameplayMusicVolume;
            source.playOnAwake = false;
            source.Play();
        }

        private static GameObject BuildPlayer(Vector2 spawn)
        {
            var go = new GameObject("Ryu");
            go.transform.position = spawn;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;
            go.transform.localScale = Vector3.one * 1.3f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;

            go.AddComponent<PlayerController>();
            go.AddComponent<PlayerVisuals>();
            go.AddComponent<PlayerHealth>();
            go.AddComponent<FireBreath>();
            go.AddComponent<DragonForm>();
            go.AddComponent<PlayerCombat>();
            go.AddComponent<PlayerAbilities>();

            var animator = go.AddComponent<PlayerAnimator>();
            var tex = Resources.Load<Texture2D>("Elder Dragon Hatchling Sprite Sheet");
            var json = Resources.Load<TextAsset>("Elder Dragon Hatchling Sprite Sheet");
            if (tex != null && json != null)
            {
                var readable = new Texture2D(tex.width, tex.height, tex.format, false);
                readable.LoadRawTextureData(tex.GetRawTextureData());
                readable.Apply();
                readable.filterMode = FilterMode.Point;
                animator.Init(readable, json.text);
            }

            return go;
        }

        private static void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.AddComponent<UniversalAdditionalCameraData>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 3.2f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        private static void EnsureGlobalLight()
        {
            if (FindAnyObjectByType<Light2D>() != null) return;
            var lightGo = new GameObject("Global Light 2D");
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1.5f;
            light.color = new Color(0.6f, 0.55f, 0.5f);
        }
    }
}
