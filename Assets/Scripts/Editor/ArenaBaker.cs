using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AshesOfRyu.EditorTools
{
    public static class ArenaBaker
    {
        private const string WorldRootName = "Mundo";
        private const string GameScenePath = "Assets/Scenes/4_scene_game.unity";

        [MenuItem("Ashes/Generar Mundo en Escena")]
        public static void GenerateWorld()
        {
            if (EditorSceneManager.GetActiveScene().path != GameScenePath)
                EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

            Sprite[] groundTiles = LoadGroundTiles();
            ClearWorld();

            var root = new GameObject(WorldRootName);
            Undo.RegisterCreatedObjectUndo(root, "Generar Mundo");
            ArenaBuilder.Build(root.transform, groundTiles, out _, out _, out _);

            if (!EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene()))
                throw new InvalidOperationException("No se pudo guardar la escena con el mundo generado.");
            Selection.activeGameObject = root;
            Debug.Log("Primera etapa del mundo generada y guardada en la escena (semilla fija 20260820).");
        }

        private static Sprite[] LoadGroundTiles()
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/1_suelo.png");
            var groundTiles = new Sprite[27];
            foreach (UnityEngine.Object asset in sprites)
            {
                if (!(asset is Sprite sprite) || !sprite.name.StartsWith("1_suelo_", StringComparison.Ordinal))
                    continue;

                if (int.TryParse(sprite.name.Substring("1_suelo_".Length), out int index) &&
                    index >= 0 && index < groundTiles.Length)
                    groundTiles[index] = sprite;
            }

            for (int i = 0; i < groundTiles.Length; i++)
            {
                if (groundTiles[i] == null)
                    throw new InvalidOperationException("Falta el tile " + i + " de Assets/1_suelo.png.");
            }

            return groundTiles;
        }

        [MenuItem("Ashes/Borrar Mundo Generado")]
        public static void ClearWorld()
        {
            var existing = GameObject.Find(WorldRootName);
            if (existing == null) return;
            Undo.DestroyObjectImmediate(existing);
        }
    }
}
