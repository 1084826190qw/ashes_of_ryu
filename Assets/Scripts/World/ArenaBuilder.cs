using UnityEngine;

namespace AshesOfRyu
{
    public static class ArenaBuilder
    {
        private const float TileSize = 1f;
        private const int PathStartX = -15;
        private const int PathEndX = 0;
        private const float WorldHalfWidth = 16f;
        private const float WorldHalfHeight = 9f;
        private static readonly Vector2 PlayerSpawn = new Vector2(-14.5f, 6.5f);

        public static void Build(Transform parent, Sprite[] groundTiles, out float halfW, out float halfH, out Vector2 spawnPoint)
        {
            halfW = WorldHalfWidth;
            halfH = WorldHalfHeight;
            spawnPoint = PlayerSpawn;

            EnvironmentArt.Load(groundTiles);
            var rng = new System.Random(20260820);

            BuildFloor(parent, halfW, halfH);

            var collisionLayer = CreateLayer(parent, "Colisiones");
            float thickness = 1f;
            AddWall(collisionLayer, "MuroNorte", new Vector2(0f, halfH + thickness * 0.5f), new Vector2(halfW * 2f + thickness * 2f, thickness));
            AddWall(collisionLayer, "MuroSur", new Vector2(0f, -halfH - thickness * 0.5f), new Vector2(halfW * 2f + thickness * 2f, thickness));
            AddWall(collisionLayer, "MuroOesteSuperior", new Vector2(-halfW - thickness * 0.5f, 8.5f), new Vector2(thickness, 2f));
            AddWall(collisionLayer, "MuroOesteInferior", new Vector2(-halfW - thickness * 0.5f, -1.5f), new Vector2(thickness, 15f));
            AddWall(collisionLayer, "MuroEste", new Vector2(halfW + thickness * 0.5f, 0f), new Vector2(thickness, halfH * 2f + thickness * 2f));

            var decorationLayer = CreateLayer(parent, "Decoracion");
            AddRock(decorationLayer, new Vector2(-11f, 2f), rng);
            AddRock(decorationLayer, new Vector2(-8f, -2f), rng);
            AddRock(decorationLayer, new Vector2(-5f, 5f), rng);
            AddRock(decorationLayer, new Vector2(-3f, -5f), rng);
            AddRock(decorationLayer, new Vector2(-1f, 4f), rng);

            AddTreeObstacle(decorationLayer, new Vector2(-12f, 0f));
            AddTreeObstacle(decorationLayer, new Vector2(-4f, -7f));
            AddBorderTrees(decorationLayer, halfW, halfH, rng);

            FogAnimator.CreateLayer(parent, halfW * 2f + 2f, 0.5f, 5f);
        }

        private static void BuildFloor(Transform parent, float halfW, float halfH)
        {
            var floor = new GameObject("Suelo");
            floor.transform.SetParent(parent, false);
            floor.transform.position = new Vector3(0f, 0f, 1f);
            var grassLayer = CreateLayer(floor.transform, "Hierba");
            var pathLayer = CreateLayer(floor.transform, "Camino");

            int minX = Mathf.FloorToInt(-halfW - 1f);
            int maxX = Mathf.CeilToInt(halfW + 1f);
            int minY = Mathf.FloorToInt(-halfH - 1f);
            int maxY = Mathf.CeilToInt(halfH + 1f);

            for (int row = minY; row < maxY; row++)
            {
                float yPos = row + TileSize * 0.5f;
                for (int x = minX; x < maxX; x++)
                {
                    bool isPath = IsPathCell(x, row);
                    float rotation = 0f;
                    var tileGo = new GameObject($"{(isPath ? "Camino" : "Hierba")}_{x}_{row}");
                    tileGo.transform.SetParent(isPath ? pathLayer : grassLayer, false);
                    tileGo.transform.localPosition = new Vector3(x + TileSize * 0.5f, yPos, 0f);
                    var sr = tileGo.AddComponent<SpriteRenderer>();
                    sr.sprite = isPath ? SelectPathTile(x, row, out rotation) : EnvironmentArt.GrassTile;
                    if (isPath)
                        tileGo.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);

                    Vector2 spriteSize = sr.sprite.bounds.size;
                    tileGo.transform.localScale = new Vector3(
                        TileSize / spriteSize.x,
                        TileSize / spriteSize.y,
                        1f);
                    sr.sortingOrder = -10;
                }
            }
        }

        private static bool IsPathCell(int x, int y)
        {
            if (x < PathStartX || x > PathEndX)
                return false;

            float progress = (float)(x - PathStartX) / (PathEndX - PathStartX);
            int centerY = Mathf.RoundToInt(Mathf.Lerp(7f, 0f, progress) + Mathf.Sin(progress * Mathf.PI) * 0.75f);
            int halfWidth = x >= PathEndX - 2 ? 3 : 1;
            return Mathf.Abs(y - centerY) <= halfWidth;
        }

        private static Sprite SelectPathTile(int x, int y, out float rotation)
        {
            bool grassAbove = !IsPathCell(x, y + 1);
            bool grassBelow = !IsPathCell(x, y - 1);
            bool grassRight = !IsPathCell(x + 1, y);
            bool grassLeft = !IsPathCell(x - 1, y);

            rotation = 0f;
            if (grassAbove && grassLeft)
                return EnvironmentArt.GrassTopLeftCorner;
            if (grassAbove && grassRight)
            {
                rotation = 270f;
                return EnvironmentArt.GrassTopLeftCorner;
            }
            if (grassBelow && grassRight)
            {
                rotation = 180f;
                return EnvironmentArt.GrassTopLeftCorner;
            }
            if (grassBelow && grassLeft)
            {
                rotation = 90f;
                return EnvironmentArt.GrassTopLeftCorner;
            }
            if (grassAbove)
                return EnvironmentArt.GrassTopEdge;
            if (grassBelow)
                return EnvironmentArt.GrassBottomEdge;
            if (grassLeft)
                return EnvironmentArt.GrassLeftEdge;
            if (grassRight)
                return EnvironmentArt.GrassRightEdge;
            return EnvironmentArt.PathTile;
        }

        private static Transform CreateLayer(Transform parent, string name)
        {
            var layer = new GameObject(name);
            layer.transform.SetParent(parent, false);
            return layer.transform;
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
            new Vector2(0.55f, 0.4f),
            new Vector2(0.55f, 0.4f),
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
