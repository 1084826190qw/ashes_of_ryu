using UnityEngine;

namespace AshesOfRyu
{
    public static class ArenaBuilder
    {
        private const float TileSize = 1f;
        private const float DirtChance = 0.12f;
        private const float BushChance = 0.035f;
        private const float SkullChance = 0.006f;

        public static void Build(Transform parent, out float halfW, out float halfH, out Vector2 spawnPoint)
        {
            halfW = 16f;
            halfH = 9f;
            spawnPoint = new Vector2(-halfW * 0.6f, 0f);

            EnvironmentArt.Load();
            var rng = new System.Random(20260820);

            BuildFloor(parent, halfW, halfH, rng);

            float thickness = 1f;
            AddWall(parent, "MuroNorte", new Vector2(0f, halfH + thickness * 0.5f), new Vector2(halfW * 2f + thickness * 2f, thickness));
            AddWall(parent, "MuroSur", new Vector2(0f, -halfH - thickness * 0.5f), new Vector2(halfW * 2f + thickness * 2f, thickness));
            AddWall(parent, "MuroOeste", new Vector2(-halfW - thickness * 0.5f, 0f), new Vector2(thickness, halfH * 2f + thickness * 2f));
            AddWall(parent, "MuroEste", new Vector2(halfW + thickness * 0.5f, 0f), new Vector2(thickness, halfH * 2f + thickness * 2f));

            AddRock(parent, new Vector2(-4f, 2.5f), rng);
            AddRock(parent, new Vector2(3f, -3f), rng);
            AddRock(parent, new Vector2(6f, 2f), rng);
            AddRock(parent, new Vector2(-7f, -2f), rng);
            AddRock(parent, new Vector2(0.5f, 4.5f), rng);
            AddRock(parent, new Vector2(-2f, -5f), rng);
            AddRock(parent, new Vector2(9f, -4f), rng);

            AddTreeObstacle(parent, new Vector2(-11f, 5.5f));
            AddTreeObstacle(parent, new Vector2(12f, -6f));

            AddBorderTrees(parent, halfW, halfH, rng);
        }

        private static void BuildFloor(Transform parent, float halfW, float halfH, System.Random rng)
        {
            var floor = new GameObject("Suelo");
            floor.transform.SetParent(parent, false);
            floor.transform.position = new Vector3(0f, 0f, 1f);

            // Los tiles de suelo miden 1 x 0.5 unidades (64x32 px @ PPU 64):
            // paso horizontal de 1 y vertical de 0.5 para cubrir sin huecos.
            int minX = Mathf.FloorToInt(-halfW - 1f);
            int maxX = Mathf.CeilToInt(halfW + 1f);
            int minY = Mathf.FloorToInt((-halfH - 1f) * 2f);
            int maxY = Mathf.CeilToInt((halfH + 1f) * 2f);

            for (int row = minY; row < maxY; row++)
            {
                float yPos = row * 0.5f + 0.25f;
                for (int x = minX; x < maxX; x++)
                {
                    var tileGo = new GameObject($"Tile_{x}_{row}");
                    tileGo.transform.SetParent(floor.transform, false);
                    tileGo.transform.position = new Vector3(x + 0.5f, yPos, 0f);
                    var sr = tileGo.AddComponent<SpriteRenderer>();
                    var isDirt = rng.NextDouble() < DirtChance;
                    sr.sprite = isDirt ? DirtTiles(rng) : FloorTiles(rng);
                    if (isDirt && sr.sprite != null)
                        sr.color = new Color(1.3f, 1.1f, 0.9f); // aclarar tierra
                    sr.sortingOrder = -10;

                    double roll = rng.NextDouble();
                    if (roll < SkullChance)
                    {
                        AddDecal(floor.transform, EnvironmentArt.Skull, new Vector3(x + 0.5f, yPos, 0f), 0.45f);
                    }
                    else if (roll < SkullChance + BushChance)
                    {
                        AddDecal(floor.transform, EnvironmentArt.Bush, new Vector3(x + 0.5f, yPos, 0f), 0.35f);
                    }
                }
            }
        }

        private static Sprite FloorTiles(System.Random rng)
        {
            var tiles = EnvironmentArt.FloorTiles;
            return tiles == null || tiles.Length == 0 ? null : tiles[rng.Next(tiles.Length)];
        }

        private static Sprite DirtTiles(System.Random rng)
        {
            var tiles = EnvironmentArt.DirtTiles;
            return tiles == null || tiles.Length == 0 ? null : tiles[rng.Next(tiles.Length)];
        }

        private static void AddDecal(Transform parent, Sprite sprite, Vector3 pos, float scale)
        {
            if (sprite == null) return;
            var go = new GameObject("Decal");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -9;
        }

        private static void AddWall(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Solid(new Color(0.3f, 0.24f, 0.18f));
            sr.sortingOrder = -5;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private static readonly Vector2[] RockColliderSizes =
        {
            new Vector2(0.95f, 0.8f),
            new Vector2(0.75f, 0.55f),
        };

        private static void AddRock(Transform parent, Vector2 pos, System.Random rng)
        {
            var rocks = EnvironmentArt.Rocks;
            if (rocks == null || rocks.Length == 0) return;

            int index = rng.Next(rocks.Length);
            var go = new GameObject("Roca");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = rocks[index];
            sr.sortingOrder = -2;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = RockColliderSizes[index];
        }

        private static void AddTreeObstacle(Transform parent, Vector2 pos)
        {
            if (EnvironmentArt.DeadTree == null) return;

            var go = new GameObject("ArbolMuerto");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = EnvironmentArt.DeadTree;
            sr.sortingOrder = -2;
            var col = go.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(0f, -1.1f);
            col.size = new Vector2(0.55f, 0.7f);
        }

        private static void AddBorderTrees(Transform parent, float halfW, float halfH, System.Random rng)
        {
            if (EnvironmentArt.DeadTree == null) return;

            float[] xs = { -14f, -8.5f, -3f, 2.5f, 8f, 13.5f };
            foreach (float x in xs)
            {
                AddBorderTree(parent, new Vector2(x + (float)(rng.NextDouble() * 1.2 - 0.6), halfH - 1.4f), rng);
                if (rng.NextDouble() < 0.45)
                    AddBorderTree(parent, new Vector2(x + (float)(rng.NextDouble() * 1.2 - 0.6), -halfH + 1.4f), rng);
            }
        }

        private static void AddBorderTree(Transform parent, Vector2 pos, System.Random rng)
        {
            var go = new GameObject("ArbolMuertoBorde");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = EnvironmentArt.DeadTree;
            sr.flipX = rng.NextDouble() < 0.5;
            sr.sortingOrder = -8;
        }
    }
}
