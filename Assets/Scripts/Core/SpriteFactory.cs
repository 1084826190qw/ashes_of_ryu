using UnityEngine;

namespace AshesOfRyu
{
    public static class SpriteFactory
    {
        public static Sprite Solid(Color color, int size = 32)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite Circle(Color color, int size = 32)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];
            float center = (size - 1) / 2f;
            float radius = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    pixels[y * size + x] = dx * dx + dy * dy <= radius * radius ? color : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite[] CreateStrip(Texture2D tex, int frameWidth, int frameHeight, int count, float ppu = 100f)
        {
            var sprites = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                var rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight);
                sprites[i] = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), ppu);
            }
            return sprites;
        }
    }
}
