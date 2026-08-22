using UnityEditor;
using UnityEngine;

namespace AshesOfRyu.EditorTools
{
    public static class ArenaBaker
    {
        private const string WorldRootName = "Mundo";

        [MenuItem("Ashes/Generar Mundo en Escena")]
        public static void GenerateWorld()
        {
            ClearWorld();

            var root = new GameObject(WorldRootName);
            Undo.RegisterCreatedObjectUndo(root, "Generar Mundo");
            ArenaBuilder.Build(root.transform, out _, out _, out _);

            Selection.activeGameObject = root;
            Debug.Log("Mundo generado en la escena (semilla fija 20260820: identico al del juego). Guarda la escena (Ctrl+S) para conservarlo.");
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
