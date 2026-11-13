using System.Collections.Generic;
using UnityEngine;

namespace AshesOfRyu
{
    /// <summary>
    /// Carga las hojas de sprites del esqueleto desde Resources, las corta en
    /// frames y elimina el fondo blanco que rodea a cada figura.
    /// </summary>
    public static class SkeletonSpriteLoader
    {
        private const string ResourceFolder = "Enemies/Skeleton/";

        private static readonly Dictionary<string, Sprite[]> cache = new Dictionary<string, Sprite[]>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            cache.Clear();
        }

        public static Sprite[] GetFrames(string sheetName, int frameWidth, int frameHeight)
        {
            string key = sheetName + "_" + frameWidth + "x" + frameHeight;
            if (cache.TryGetValue(key, out Sprite[] cached) && cached != null && cached.Length > 0 && cached[0] != null)
                return cached;

            Texture2D source = Resources.Load<Texture2D>(ResourceFolder + sheetName);
            if (source == null)
            {
                Debug.LogError("[SkeletonSpriteLoader] No se encontro la textura: " + ResourceFolder + sheetName);
                return null;
            }

            Color[] pixels = source.GetPixels();
            RemoveBorderWhite(pixels, source.width, source.height);

            var texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            texture.name = sheetName + "_Runtime";
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(pixels);
            texture.Apply(false, false);

            int frameCount = Mathf.Max(1, source.width / frameWidth);
            var frames = new Sprite[frameCount];

            // Un frame de 32 px ocupa una unidad de mundo, igual que el cuadrado anterior.
            float pixelsPerUnit = frameWidth;
            for (int i = 0; i < frameCount; i++)
            {
                var rect = new Rect(i * frameWidth, 0f, frameWidth, frameHeight);
                frames[i] = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit, 0,
                    SpriteMeshType.FullRect);
                frames[i].name = sheetName + "_" + i;
            }

            cache[key] = frames;
            return frames;
        }

        private static void RemoveBorderWhite(Color[] pixels, int width, int height)
        {
            var visited = new bool[pixels.Length];
            var queue = new Queue<int>();

            for (int x = 0; x < width; x++)
            {
                EnqueueIfWhite(queue, visited, pixels, width, x, 0);
                EnqueueIfWhite(queue, visited, pixels, width, x, height - 1);
            }
            for (int y = 0; y < height; y++)
            {
                EnqueueIfWhite(queue, visited, pixels, width, 0, y);
                EnqueueIfWhite(queue, visited, pixels, width, width - 1, y);
            }

            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                int x = index % width;
                int y = index / width;
                if (!IsWhite(pixels[index])) continue;

                Color color = pixels[index];
                color.a = 0f;
                pixels[index] = color;

                if (x > 0) EnqueueIfWhite(queue, visited, pixels, width, x - 1, y);
                if (x < width - 1) EnqueueIfWhite(queue, visited, pixels, width, x + 1, y);
                if (y > 0) EnqueueIfWhite(queue, visited, pixels, width, x, y - 1);
                if (y < height - 1) EnqueueIfWhite(queue, visited, pixels, width, x, y + 1);
            }
        }

        private static void EnqueueIfWhite(Queue<int> queue, bool[] visited, Color[] pixels, int width, int x, int y)
        {
            int index = y * width + x;
            if (visited[index]) return;
            if (!IsWhite(pixels[index])) return;

            visited[index] = true;
            queue.Enqueue(index);
        }

        private static bool IsWhite(Color color)
        {
            // Solo blanco puro (255,255,255), para no borrar los tonos claros del esqueleto.
            return color.r >= 0.999f && color.g >= 0.999f && color.b >= 0.999f;
        }
    }
}
