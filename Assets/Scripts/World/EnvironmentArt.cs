using System;
using UnityEngine;

namespace AshesOfRyu
{
    public static class EnvironmentArt
    {
        private const float Ppu = 32f;

        public static Sprite PathTile { get; private set; }
        public static Sprite GrassTile { get; private set; }
        public static Sprite GrassLeftEdge { get; private set; }
        public static Sprite GrassRightEdge { get; private set; }
        public static Sprite GrassTopEdge { get; private set; }
        public static Sprite GrassBottomEdge { get; private set; }
        public static Sprite GrassTopLeftCorner { get; private set; }
        public static Sprite Skull { get; private set; }
        public static Sprite Bush { get; private set; }
        public static Sprite[] Rocks { get; private set; }
        public static Sprite DeadTree { get; private set; }

        public static void Load(Sprite[] groundTiles)
        {
            if (groundTiles == null || groundTiles.Length <= 10)
                throw new ArgumentException("La matriz de suelo 1_suelo debe incluir los tiles 0 a 10.", nameof(groundTiles));

            int[] requiredTiles = { 0, 1, 2, 4, 5, 6, 9 };
            foreach (int index in requiredTiles)
            {
                if (groundTiles[index] == null)
                    throw new ArgumentException("Falta el tile " + index + " de 1_suelo.", nameof(groundTiles));
            }

            PathTile = groundTiles[0];
            GrassTile = groundTiles.Length > 23 && groundTiles[23] != null
                ? groundTiles[23]
                : groundTiles[1];
            GrassLeftEdge = groundTiles[2];
            GrassRightEdge = groundTiles[4];
            GrassTopEdge = groundTiles[5];
            GrassBottomEdge = groundTiles[6];
            GrassTopLeftCorner = groundTiles[9];

            var forestRock = Resources.Load<Texture2D>("Art/Environment/forest_rock");
            var darkWorldRock = Resources.Load<Texture2D>("Art/Environment/darkworld_rock");
            var deadTree = Resources.Load<Texture2D>("Art/Environment/darkworld_tree");

            if (forestRock != null) forestRock.filterMode = FilterMode.Point;
            if (darkWorldRock != null) darkWorldRock.filterMode = FilterMode.Point;
            if (deadTree != null) deadTree.filterMode = FilterMode.Point;

            Skull = null;
            Bush = null;

            if (forestRock != null)
            {
                Rocks = new[]
                {
                    Create(forestRock, new Rect(0f, 0f, 32f, 32f)),
                    Create(forestRock, new Rect(32f, 0f, 32f, 32f)),
                };
            }
            else if (darkWorldRock != null)
            {
                Rocks = new[]
                {
                    Create(darkWorldRock, new Rect(0f, 0f, 48f, 32f)),
                };
            }

            if (deadTree != null) DeadTree = Create(deadTree, new Rect(0f, 0f, 80f, 96f));
        }

        private static Sprite Create(Texture2D tex, Rect rect, float ppu = Ppu)
        {
            return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), ppu);
        }
    }
}