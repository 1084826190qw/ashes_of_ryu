using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AshesOfRyu.EditorTools
{
    public static class BossPrimerNivelBuilder
    {
        private const string MenuPath = "Tools/Ashes of Ryu/Crear Jefe Primer Nivel";
        private const string UpdateMenuPath = "Tools/Ashes of Ryu/Actualizar Jefe Primer Nivel";
        private const string BossName = "JefePrimerNivel";
        private const string ArtFolder = "Assets/Scripts/jefe_primer_nivel";
        private const string KeySpritePath = "Assets/llave_castillo.png";

        // Hierba_31_-2 (2) en el mapa actual.
        private static readonly Vector3 SpawnPosition = new Vector3(48.49f, 10.485f, 0f);

        [MenuItem(MenuPath)]
        public static void CreateBoss()
        {
            var existing = GameObject.Find(BossName);
            if (existing != null)
            {
                Debug.LogWarning($"[BossPrimerNivel] Ya existe un '{BossName}' en la escena. No se crea otro.");
                Selection.activeGameObject = existing;
                return;
            }

            var bossGo = new GameObject(BossName);
            Undo.RegisterCreatedObjectUndo(bossGo, "Crear Jefe Primer Nivel");
            bossGo.transform.position = SpawnPosition;
            bossGo.transform.localScale = Vector3.one * 0.72f;

            var rb = bossGo.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var collider = bossGo.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.30f;
            collider.offset = new Vector2(0f, 0.45f);

            var visualGo = new GameObject("Visual");
            visualGo.transform.SetParent(bossGo.transform, false);
            var visualRenderer = visualGo.AddComponent<SpriteRenderer>();
            visualRenderer.sortingOrder = 3;

            var boss = bossGo.AddComponent<BossPrimerNivel>();

            var serialized = new SerializedObject(boss);
            SetSpriteArray(serialized, "idleFrames", LoadFrames("idle_sheet.png"));
            SetSpriteArray(serialized, "walkFrames", LoadFrames("walk_sheet.png"));
            SetSpriteArray(serialized, "runFrames", LoadFrames("run_sheet.png"));
            SetSpriteArray(serialized, "attackFrames", LoadFrames("attack_sheet.png"));
            SetSpriteArray(serialized, "jumpFrames", LoadFrames("jump_sheet.png"));
            SetSprite(serialized, "keySprite", LoadKeySprite());
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var firstFrame = AssetDatabase.LoadAllAssetsAtPath($"{ArtFolder}/idle_sheet.png")
                .OfType<Sprite>()
                .OrderBy(sprite => FrameIndex(sprite.name))
                .FirstOrDefault();
            if (firstFrame != null)
            {
                visualRenderer.sprite = firstFrame;
                Bounds bounds = firstFrame.bounds;
                visualGo.transform.localPosition = new Vector3(-bounds.center.x, -bounds.min.y, 0f);
            }

            EditorSceneManager.MarkSceneDirty(bossGo.scene);
            Selection.activeGameObject = bossGo;
            Debug.Log($"[BossPrimerNivel] Jefe creado en {SpawnPosition} y limitado al area del mapa.");
        }

        [MenuItem(UpdateMenuPath)]
        public static void UpdateBoss()
        {
            var bossGo = GameObject.Find(BossName);
            if (bossGo == null)
            {
                Debug.LogWarning($"[BossPrimerNivel] No existe un '{BossName}' en la escena.");
                return;
            }

            var boss = bossGo.GetComponent<BossPrimerNivel>();
            if (boss == null)
            {
                Debug.LogWarning($"[BossPrimerNivel] El objeto '{BossName}' no tiene el componente BossPrimerNivel.");
                return;
            }

            var serialized = new SerializedObject(boss);
            SetSprite(serialized, "keySprite", LoadKeySprite());
            serialized.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(bossGo.scene);
            Selection.activeGameObject = bossGo;
            Debug.Log("[BossPrimerNivel] Recompensa del jefe actualizada (llave_castillo).");
        }

        private static Sprite[] LoadFrames(string fileName)
        {
            return AssetDatabase.LoadAllAssetsAtPath($"{ArtFolder}/{fileName}")
                .OfType<Sprite>()
                .OrderBy(sprite => FrameIndex(sprite.name))
                .ToArray();
        }

        private static Sprite LoadKeySprite()
        {
            return AssetDatabase.LoadAllAssetsAtPath(KeySpritePath)
                .OfType<Sprite>()
                .FirstOrDefault(sprite => sprite.name == "llave_castillo_0");
        }

        private static void SetSprite(SerializedObject serialized, string propertyName, Sprite sprite)
        {
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[BossPrimerNivel] No se encontro la propiedad '{propertyName}'.");
                return;
            }

            property.objectReferenceValue = sprite;
        }

        private static int FrameIndex(string spriteName)
        {
            int separator = spriteName.LastIndexOf('_');
            if (separator < 0) return int.MaxValue;
            return int.TryParse(spriteName.Substring(separator + 1), out int index) ? index : int.MaxValue;
        }

        private static void SetSpriteArray(SerializedObject serialized, string propertyName, Sprite[] sprites)
        {
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[BossPrimerNivel] No se encontro la propiedad '{propertyName}'.");
                return;
            }

            property.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }
    }
}
