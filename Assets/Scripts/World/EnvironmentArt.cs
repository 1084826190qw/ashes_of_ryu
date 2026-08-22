using UnityEngine;

namespace AshesOfRyu
{
    public static class EnvironmentArt
    {
        private const float Ppu = 32f;
        private const float GroundPpu = 64f;

        public static Sprite[] FloorTiles { get; private set; }
        public static Sprite[] DirtTiles { get; private set; }
        public static Sprite Skull { get; private set; }
        public static Sprite Bush { get; private set; }
        public static Sprite[] Rocks { get; private set; }
        public static Sprite DeadTree { get; private set; }

        public static void Load()
        {
            // Sin cache entre sesiones: los sprites creados con Sprite.Create se destruyen
            // al salir del modo Play, asi que siempre recreamos (es barato, ~10 sprites).
            var tiles = Resources.Load<Texture2D>("Art/Environment/death_forest_tiles");
            var forestRock = Resources.Load<Texture2D>("Art/Environment/forest_rock");
            var darkWorldRock = Resources.Load<Texture2D>("Art/Environment/darkworld_rock");
            var deadTree = Resources.Load<Texture2D>("Art/Environment/darkworld_tree");

            if (tiles != null) tiles.filterMode = FilterMode.Point;
            if (forestRock != null) forestRock.filterMode = FilterMode.Point;
            if (darkWorldRock != null) darkWorldRock.filterMode = FilterMode.Point;
            if (deadTree != null) deadTree.filterMode = FilterMode.Point;

            if (tiles != null)
            {
                // death_forest_tiles (256x128) layout verified pixel-by-pixel:
                //   Columna 0 (x=0..64):  suelo GRIS, dos variantes de 64x32
                //   Columna 1 (x=64..128): suelo MARRON/tierra, dos variantes de 64x32
                //   Objeto blanco-plateado abajo-izquierda: CALAVERA (64x40 aprox)
                //   Objeto grande derecha: ARBUSTO/vegetacion (128x124)
                // GroundPpu=64 hace que cada tile de suelo mida exactamente 1 x 0.5 unidades.

                FloorTiles = new[]
                {
                    Create(tiles, new Rect(0f, 96f, 64f, 32f), GroundPpu),  // 0 - gris variante A
                    Create(tiles, new Rect(0f, 64f, 64f, 32f), GroundPpu),  // 0 - gris variante B
                };

                DirtTiles = new[]
                {
                    Create(tiles, new Rect(64f, 96f, 64f, 32f), GroundPpu), // 1 - marron variante A
                    Create(tiles, new Rect(64f, 64f, 64f, 32f), GroundPpu), // 1 - marron variante B
                };

                Skull = Create(tiles, new Rect(0f, 16f, 64f, 40f));    // 2 - calavera
                Bush = Create(tiles, new Rect(128f, 4f, 128f, 124f));  // 3 - arbusto
            }

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