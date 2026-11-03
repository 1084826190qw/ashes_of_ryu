using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AshesOfRyu.EditorTools
{
    /// <summary>
    /// Rehace el suelo interior de la escena de gameplay usando el tileset suelo_1.
    /// El camino tiene forma de C invertida (Ɔ), escalonada a base de bloques:
    /// barra superior con rama al borde derecho, lomo derecho y barra inferior.
    /// Solo se toca el interior del mapa: el anillo exterior (bajo los muros) se conserva.
    /// </summary>
    public static class Suelo1Baker
    {
        private const string ScenePath = "Assets/Scenes/4_scene_game.unity";
        private const string TexturePath = "Assets/suelo_1.png";
        private const string WorldRootName = "Mundo";
        private const string DiagPath = "/tmp/opencode/suelo1_result.txt";

        // Celdas interiores (coordenadas de nombre de tile). El anillo exterior queda intacto.
        private const int MinX = -16;
        private const int MaxX = 15;
        private const int MinY = -9;
        private const int MaxY = 8;

        // Ɔ escalonada: barra superior + rama al borde derecho, lomo derecho y barra inferior.
        // Barra superior/rama: y 3..5, x -10..15
        // Lomo derecho: x 2..4, y -9..2
        // Barra inferior: y -9..-7, x -10..4
        private const int TopBarMinX = -10;
        private const int TopBarMaxX = 15;
        private const int TopBarMinY = 3;
        private const int TopBarMaxY = 5;
        private const int SpineMinX = 2;
        private const int SpineMaxX = 4;
        private const int SpineMinY = -9;
        private const int SpineMaxY = 2;
        private const int BottomBarMinX = -10;
        private const int BottomBarMaxX = 4;
        private const int BottomBarMinY = -9;
        private const int BottomBarMaxY = -7;

        private const int InteriorCellCount = (MaxX - MinX + 1) * (MaxY - MinY + 1);

        private static readonly int[] TopVariants = { 1, 2, 3, 4, 5 };
        // Sin 19 ni 20 (4ª fila del tileset, columnas 4 y 6): solo se usa el 15.
        private static readonly int[] BottomVariants = { 15 };
        private static readonly int[] LeftVariants = { 6, 9 };
        private static readonly int[] RightVariants = { 7, 10, 17 };

        [MenuItem("Ashes/Rehacer Suelo con suelo_1")]
        public static void Rebuild()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                Debug.LogError("La escena activa no es " + ScenePath + " (es '" + scene.path + "').");
                return;
            }

            Sprite[] sprites = LoadSprites();
            if (sprites == null)
                return;

            GameObject world = GameObject.Find(WorldRootName);
            if (world == null)
            {
                Debug.LogError("No se encontró el objeto '" + WorldRootName + "' en la escena.");
                return;
            }

            Transform suelo = world.transform.Find("Suelo");
            Transform hierba = suelo != null ? suelo.Find("Hierba") : null;
            Transform camino = suelo != null ? suelo.Find("Camino") : null;
            if (hierba == null || camino == null)
            {
                Debug.LogError("No se encontraron Mundo/Suelo/Hierba y Mundo/Suelo/Camino.");
                return;
            }

            int borderBefore = CountBorderTiles(hierba) + CountBorderTiles(camino);
            int removedGrass = ClearInteriorTiles(hierba);
            int removedPath = ClearInteriorTiles(camino);

            var rng = new System.Random(20261031);
            int createdGrass = 0;
            int createdPath = 0;
            var map = new List<string>();

            for (int y = MaxY; y >= MinY; y--)
            {
                var line = new StringBuilder();
                for (int x = MinX; x <= MaxX; x++)
                {
                    bool isPath = IsPathCell(x, y);
                    Transform parent = isPath ? camino : hierba;
                    string prefix = isPath ? "Camino_" : "Hierba_";
                    var go = new GameObject(prefix + x + "_" + y);
                    go.transform.SetParent(parent, false);
                    go.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0f);

                    float rotation = 0f;
                    Sprite sprite = isPath
                        ? SelectPathTile(sprites, x, y, rng, out rotation)
                        : sprites[18]; // césped completo

                    if (sprite == null)
                    {
                        Debug.LogError("Sprite nulo en la celda (" + x + ", " + y + ").");
                        WriteDiagnostics(map, removedGrass, removedPath, createdGrass, createdPath, borderBefore, -1);
                        return;
                    }

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sortingOrder = -10;

                    if (!Mathf.Approximately(rotation, 0f))
                        go.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);

                    Vector2 size = sprite.bounds.size;
                    go.transform.localScale = new Vector3(
                        size.x > 0.0001f ? 1f / size.x : 1f,
                        size.y > 0.0001f ? 1f / size.y : 1f,
                        1f);

                    if (isPath)
                    {
                        createdPath++;
                        line.Append('C');
                    }
                    else
                    {
                        createdGrass++;
                        line.Append('H');
                    }
                }
                map.Add(y.ToString().PadLeft(3) + " " + line);
            }

            int borderAfter = CountBorderTiles(hierba) + CountBorderTiles(camino);
            int totalChildren = hierba.childCount + camino.childCount;
            int expectedChildren = borderBefore + InteriorCellCount;

            bool countsOk = createdGrass + createdPath == InteriorCellCount
                && borderAfter == borderBefore
                && totalChildren == expectedChildren;
            WriteDiagnostics(map, removedGrass, removedPath, createdGrass, createdPath, borderBefore, borderAfter);

            if (!countsOk)
            {
                Debug.LogError(
                    "El suelo no quedó completo. Creados=" + (createdGrass + createdPath) +
                    " (esperados " + InteriorCellCount + "), borde antes=" + borderBefore +
                    " después=" + borderAfter + ", hijos totales=" + totalChildren +
                    " (esperados " + expectedChildren + "). No se guarda la escena.");
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError("No se pudo guardar la escena con el nuevo suelo.");
                return;
            }

            Debug.Log(
                "Suelo rehecho con suelo_1: " + createdGrass + " césped + " + createdPath +
                " camino = " + (createdGrass + createdPath) + " tiles interiores. Borde conservado (" +
                borderBefore + "). Escena guardada.");
        }

        [MenuItem("Ashes/Verificar Suelo suelo_1")]
        public static void Verify()
        {
            GameObject world = GameObject.Find(WorldRootName);
            Transform suelo = world != null ? world.transform.Find("Suelo") : null;
            Transform hierba = suelo != null ? suelo.Find("Hierba") : null;
            Transform camino = suelo != null ? suelo.Find("Camino") : null;
            if (hierba == null || camino == null)
            {
                Debug.LogError("No se encontraron Mundo/Suelo/Hierba y Mundo/Suelo/Camino.");
                return;
            }

            int grass = CountInteriorTiles(hierba);
            int path = CountInteriorTiles(camino);
            int border = CountBorderTiles(hierba) + CountBorderTiles(camino);
            Debug.Log("Verificación suelo: césped interior=" + grass + ", camino interior=" + path +
                      ", borde=" + border + ", hijos Hierba=" + hierba.childCount +
                      ", hijos Camino=" + camino.childCount + ".");
        }

        private static Sprite[] LoadSprites()
        {
            var sprites = new Sprite[22];
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(TexturePath);
            const string prefix = "suelo_1_";

            foreach (UnityEngine.Object asset in assets)
            {
                if (!(asset is Sprite sprite) || !sprite.name.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                if (int.TryParse(sprite.name.Substring(prefix.Length), out int index) &&
                    index >= 0 && index < sprites.Length)
                {
                    sprites[index] = sprite;
                }
            }

            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null)
                {
                    Debug.LogError("Falta el sprite suelo_1_" + i + " en " + TexturePath + ".");
                    return null;
                }
            }

            return sprites;
        }

        private static int ClearInteriorTiles(Transform layer)
        {
            var toDelete = new List<GameObject>();
            foreach (Transform child in layer)
            {
                Vector3 p = child.localPosition;
                // Centros interiores: -15.5..15.5 y -8.5..8.5.
                // El anillo exterior está en ±16.5 y ±9.5, así que queda fuera.
                if (p.x > -16f && p.x < 16f && p.y > -9f && p.y < 9f)
                    toDelete.Add(child.gameObject);
            }

            foreach (GameObject go in toDelete)
                UnityEngine.Object.DestroyImmediate(go);

            return toDelete.Count;
        }

        private static int CountInteriorTiles(Transform layer)
        {
            int count = 0;
            foreach (Transform child in layer)
            {
                Vector3 p = child.localPosition;
                if (p.x > -16f && p.x < 16f && p.y > -9f && p.y < 9f)
                    count++;
            }
            return count;
        }

        private static int CountBorderTiles(Transform layer)
        {
            int count = 0;
            foreach (Transform child in layer)
            {
                Vector3 p = child.localPosition;
                if (p.x <= -16f || p.x >= 16f || p.y <= -9f || p.y >= 9f)
                    count++;
            }
            return count;
        }

        private static bool IsPathCell(int x, int y)
        {
            if (y >= TopBarMinY && y <= TopBarMaxY && x >= TopBarMinX && x <= TopBarMaxX)
                return true;
            if (x >= SpineMinX && x <= SpineMaxX && y >= SpineMinY && y <= SpineMaxY)
                return true;
            if (y >= BottomBarMinY && y <= BottomBarMaxY && x >= BottomBarMinX && x <= BottomBarMaxX)
                return true;
            return false;
        }

        private static Sprite SelectPathTile(Sprite[] sprites, int x, int y, System.Random rng, out float rotation)
        {
            bool grassAbove = !IsPathCell(x, y + 1);
            bool grassBelow = !IsPathCell(x, y - 1);
            bool grassLeft = !IsPathCell(x - 1, y);
            bool grassRight = !IsPathCell(x + 1, y);

            rotation = 0f;

            // Esquinas interiores (cóncavas): el tile va en la celda de camino
            // diagonal a la esquina de césped, con el césped mirando a esa esquina.
            if (x == 2 && y == 3) return sprites[13];  // hueco abajo-izquierda: césped inf-izq
            if (x == 2 && y == -7) return sprites[11]; // hueco arriba-izquierda: césped sup-izq

            // Esquinas exteriores (convexas).
            if (grassAbove && grassLeft) return sprites[0];                       // sup-izq
            if (grassAbove && grassRight) { rotation = 270f; return sprites[0]; }  // sup-der
            if (grassBelow && grassRight) return sprites[21];                    // inf-der
            if (grassBelow && grassLeft) { rotation = 270f; return sprites[21]; }  // inf-izq

            // Bordes rectos.
            if (grassAbove) return sprites[TopVariants[rng.Next(TopVariants.Length)]];
            if (grassBelow) return sprites[BottomVariants[rng.Next(BottomVariants.Length)]];
            if (grassLeft) return sprites[LeftVariants[rng.Next(LeftVariants.Length)]];
            if (grassRight) return sprites[RightVariants[rng.Next(RightVariants.Length)]];

            return sprites[8]; // tierra completa
        }

        private static void WriteDiagnostics(
            List<string> map,
            int removedGrass,
            int removedPath,
            int createdGrass,
            int createdPath,
            int borderBefore,
            int borderAfter)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("removedGrass=" + removedGrass);
                sb.AppendLine("removedPath=" + removedPath);
                sb.AppendLine("createdGrass=" + createdGrass);
                sb.AppendLine("createdPath=" + createdPath);
                sb.AppendLine("borderBefore=" + borderBefore);
                sb.AppendLine("borderAfter=" + borderAfter);
                sb.AppendLine("map (y, x=" + MinX + ".." + MaxX + "):");
                foreach (string line in map)
                    sb.AppendLine(line);
                File.WriteAllText(DiagPath, sb.ToString());
            }
            catch (Exception e)
            {
                Debug.LogWarning("No se pudo escribir el diagnóstico en " + DiagPath + ": " + e.Message);
            }
        }
    }
}
