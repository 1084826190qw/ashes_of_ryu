using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AshesOfRyu
{
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (GameManager.Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            EnsureGlobalLight();
            SetupCamera();

            var meta = new MetaProgression();
            var gameManager = new GameObject("GameManager").AddComponent<GameManager>();
            gameManager.Meta = meta;

            var input = new GameObject("InputReader").AddComponent<InputReader>();

            var baked = GameObject.Find("Mundo");
            if (baked != null) Destroy(baked);

            ArenaBuilder.Build(transform, out float halfW, out float halfH, out Vector2 spawn);

            var player = BuildPlayer(spawn);

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
            cameraFollow.Target = player.transform;

            spawner.ResetForRun(0);
        }

        private static GameObject BuildPlayer(Vector2 spawn)
        {
            var go = new GameObject("Ryu");
            go.transform.position = spawn;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.8f;

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
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
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
