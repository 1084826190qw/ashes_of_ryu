using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AshesOfRyu.EditorTools
{
    public static class FogataAnimatorBuilder
    {
        private const string MenuPath = "Tools/Ashes of Ryu/Animar Fogatas";
        private const string SheetPath = "Assets/fogata.png";
        private static readonly Regex FogataName = new Regex(@"^fogata_6( \(\d+\))?$");

        // Solo los frames cortados en la hoja: 6 es el primero y 21 el ultimo.
        private static readonly int[] FrameOrder = { 6, 11, 17, 19, 20, 21 };

        [MenuItem(MenuPath)]
        public static void AnimateCampfires()
        {
            Sprite[] frames = LoadFrames();
            if (frames.Length != FrameOrder.Length)
            {
                Debug.LogError($"[FogataAnimator] Se esperaban {FrameOrder.Length} frames y se encontraron {frames.Length} en {SheetPath}.");
                return;
            }

            var scene = EditorSceneManager.GetActiveScene();
            var targets = new List<GameObject>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (FogataName.IsMatch(transform.name)) targets.Add(transform.gameObject);
                }
            }

            foreach (var target in targets)
            {
                var animator = target.GetComponent<FogataAnimator>();
                if (animator == null) animator = target.AddComponent<FogataAnimator>();

                var serialized = new SerializedObject(animator);
                var framesProperty = serialized.FindProperty("frames");
                framesProperty.arraySize = frames.Length;
                for (int i = 0; i < frames.Length; i++)
                {
                    framesProperty.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
                }
                serialized.FindProperty("framesPerSecond").floatValue = 7.6923077f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[FogataAnimator] {targets.Count} fogata(s) animadas con {frames.Length} frames.");
        }

        private static Sprite[] LoadFrames()
        {
            var byIndex = new Dictionary<int, Sprite>();
            foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>())
            {
                int index = FrameIndex(sprite.name);
                if (FrameOrder.Contains(index)) byIndex[index] = sprite;
            }

            return FrameOrder
                .Where(byIndex.ContainsKey)
                .Select(index => byIndex[index])
                .ToArray();
        }

        private static int FrameIndex(string spriteName)
        {
            int separator = spriteName.LastIndexOf('_');
            return separator >= 0 && int.TryParse(spriteName.Substring(separator + 1), out int index)
                ? index
                : -1;
        }
    }
}
