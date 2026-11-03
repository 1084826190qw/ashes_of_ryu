#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AshesOfRyu.EditorTools
{
    /// <summary>
    /// Herramienta de editor que monta la escena Splash y reconstruye el menú principal
    /// usando los sprites de Assets/Art/UI/logo_botones.png (logo_botones_0..8).
    /// Menú: Tools > Ashes > Setup Splash + Main Menu
    /// </summary>
    public static class SplashMainMenuBuilder
    {
        private const string SplashPath = "Assets/Scenes/1_scene_logo.unity";
        private const string MainMenuPath = "Assets/Scenes/2_scene_menu.unity";
        private const string CinematicPath = "Assets/Scenes/3_scene_cinematic_1.unity";
        private const string GameScenePath = "Assets/Scenes/4_scene_game.unity";
        private const string CreditsPath = "Assets/Scenes/5_scene_credits.unity";
        private const string ConfirmationPath = "Assets/Scenes/6_scene_confirmation.unity";

        // Imagen de fondo del menú.
        private const string MenuBackgroundPath = "Assets/Art/UI/menu_principal.jpg";

        // Imagen del menú de créditos.
        private const string CreditsImagePath = "Assets/Art/UI/menu_creditos.jpg";

        // Imagen de fondo de la escena de confirmación (antes de salir).
        private const string ConfirmationImagePath = "Assets/Art/UI/menu_confirmacion.jpg";

        // Spritesheet de los botones Sí/No: 0 = sí, 1 = sí seleccionado,
        // 2 = no, 3 = no seleccionado.
        private const string YesNoSheetPath = "Assets/Art/UI/si_no.psd";

        // Créditos en partes: una cadena por pantalla, para que se vayan mostrando
        // de uno en uno con las flechas laterales.
        private static readonly string[] CreditsPages =
        {
            // 1: portada.
            "<size=56><b>ASHES OF RYU</b></size>\n\n" +
            "<b>UN JUEGO DE</b>\n" +
            "Santiago Andrés Botina Ramos\n\n" +
            "<b>DISEÑO</b>\n" +
            "Santiago Andrés Botina Ramos\n",

            // 2: personajes.
            "<b>PERSONAJES PRINCIPALES</b>\n\n" +
            "Ryu, el último dragón\n" +
            "Arelia, el hada guía\n" +
            "Vareth, el mago del castillo\n",

            // 3: recursos de terceros.
            "<b>RECURSOS DE TERCEROS</b>\n\n" +
            "Arte: itch.io\n\n" +
            "<b>Música</b>\n" +
            "\"Horror Suspense\" — AtlasAudio\n" +
            "Pixabay\n",

            // 4: inspiración.
            "<b>INSPIRADO EN</b>\n\n" +
            "Dead Cells\n" +
            "Blasphemous\n" +
            "Soul Knight\n",

            // 5: cierre.
            "<i>\"Una vida limitada puede tener valor precisamente porque tiene un final.\"</i>\n\n\n" +
            "<size=44><b>GRACIAS POR JUGAR</b></size>\n"
        };

        // Música de la cinemática.
        private const string MusicPath = "Assets/Audio/musica_fondo.mp3";
        private const string S001SpriteSheetPath = "Assets/S001_nyknck.png";
        private const float S001AnimationFramesPerSecond = 5.5f;

        // Carpeta de las imágenes de la cinemática (ecena_1..8.jpg).
        private const string CinematicImagesFolder = "Assets/Art/Cinematic";

        // Rutas antiguas (se renombran automáticamente al ejecutar el Setup).
        private const string LegacySplashPath = "Assets/Scenes/Splash.unity";
        private const string LegacyMainMenuPath = "Assets/Scenes/MainMenu.unity";
        private const string LegacySampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const string LegacyGameScenePath = "Assets/Scenes/3_scene_game.unity";

        // Nombres de escena (los que usa SceneManager.LoadScene).
        private const string MainMenuSceneName = "2_scene_menu";
        private const string CinematicSceneName = "3_scene_cinematic_1";
        private const string GameSceneName = "4_scene_game";
        private const string CreditsSceneName = "5_scene_credits";
        private const string OptionsSceneName = "6_scene_options";
        private const string ConfirmationSceneName = "6_scene_confirmation";

        // Textos de la narración (cursiva) y duración de cada fotograma de la cinemática.
        private static readonly string[] CinematicTexts =
        {
            "Hace siglos, humanos y dragones libraron una guerra que lo consumió todo.",
            "Uno a uno, los dragones cayeron... hasta que solo quedó uno. Ryu, el último dragón verdadero.",
            "Para salvar a su especie, Ryu invocó un poder prohibido: el de la resurrección.",
            "El ritual funcionó... pero los dragones regresaron con forma humana. Ya no eran lo que habían sido.",
            "Ryu pagó el precio. Perdió su poder, su cuerpo se redujo... y la muerte le fue negada. Maldito a vivir para siempre.",
            "Pasaron los siglos. El mundo olvidó... pero Ryu no.",
            "Hasta que un hada, que nunca dejó de buscar, despertó una chispa de su antiguo fuego.",
            "El camino lleva al castillo de Vareth... y a la única esperanza de romper la maldición."
        };

        private static readonly float[] CinematicDurations = { 8f, 8f, 10f, 9f, 10f, 7f, 10f, 8f };

        // Desplazamiento vertical de cada imagen (píxeles de referencia; positivo = sube la imagen).
        // Índice 5 = escena 6, que va un poco más alta.
        private static readonly float[] CinematicImageOffsets = { 0f, 0f, 0f, 0f, 0f, 60f, 0f, 0f };

        // Tamaño de los botones del menú (ancho en píxeles de referencia; el alto se calcula solo).
        private const float ButtonWidth = 300f;

        // Franja vertical de la narración de la cinemática (0 = borde inferior, 1 = borde superior).
        private const float NarrationBottom = -0.09f;
        private const float NarrationTop = 0.20f;

        // Altura de la franja negra inferior donde va el texto.
        private const float NarrationBandTop = 0.11f;

        /// <summary>
        /// Ruta del spritesheet de logo/botones. Acepta .png o .psd (usa el que exista).
        /// </summary>
        private static string SpriteSheetPath
        {
            get
            {
                string[] candidates = { "Assets/Art/UI/logo_botones.png", "Assets/Art/UI/logo_botones.psd" };
                foreach (string path in candidates)
                {
                    if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                        return path;
                }

                return candidates[0];
            }
        }

        [MenuItem("Tools/Ashes/Setup Splash + Main Menu")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Ashes] Sal del modo Play antes de generar las escenas.");
                return;
            }

            ConfigureSpriteSheet();
            ConfigureMediaImports();

            Sprite[] sprites = LoadSprites();
            if (sprites == null || sprites.Length < 9)
            {
                Debug.LogError("[Ashes] No se pudieron cargar los 9 sprites de " + SpriteSheetPath +
                               ". Encontrados: " + (sprites == null ? 0 : sprites.Length));
                return;
            }

            EditorSceneManager.SaveOpenScenes();

            MigrateLegacyScenePaths();

            BuildSplashScene(sprites);
            BuildMainMenuScene(sprites);
            BuildCinematicScenes();
            BuildCreditsScene();
            BuildConfirmationScene();
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
            Debug.Log("[Ashes] Listo: 1_scene_logo -> 2_scene_menu -> 3_scene_cinematic_1..8 -> 4_scene_game -> 5_scene_credits -> 6_scene_confirmation. Sprites cargados: " + sprites.Length);
        }

        [MenuItem("Tools/Ashes/Open Splash Scene")]
        public static void OpenSplashScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Ashes] Sal del modo Play antes de abrir la escena Splash.");
                return;
            }

            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(SplashPath, OpenSceneMode.Single);
            Debug.Log("[Ashes] Escena 1_scene_logo abierta. Pulsa Play para verla.");
        }

        [MenuItem("Tools/Ashes/Open Cinematic Scene")]
        public static void OpenCinematicScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Ashes] Sal del modo Play antes de abrir la cinemática.");
                return;
            }

            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(CinematicPath, OpenSceneMode.Single);
            Debug.Log("[Ashes] Escena 3_scene_cinematic_1 abierta. Pulsa Play para verla.");
        }

        [MenuItem("Tools/Ashes/Open Credits Scene")]
        public static void OpenCreditsScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Ashes] Sal del modo Play antes de abrir los créditos.");
                return;
            }

            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(CreditsPath, OpenSceneMode.Single);
            Debug.Log("[Ashes] Escena 5_scene_credits abierta. Pulsa Play para verla.");
        }

        [MenuItem("Tools/Ashes/Open Confirmation Scene")]
        public static void OpenConfirmationScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Ashes] Sal del modo Play antes de abrir la escena de confirmación.");
                return;
            }

            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(ConfirmationPath, OpenSceneMode.Single);
            Debug.Log("[Ashes] Escena 6_scene_confirmation abierta. Pulsa Play para verla.");
        }

        [MenuItem("Tools/Ashes/Use Splash As Play Start")]
        public static void UseSplashAsPlayStart()
        {
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SplashPath);
            Debug.Log("[Ashes] Play iniciará siempre en la escena 1_scene_logo.");
        }

        [MenuItem("Tools/Ashes/Clear Play Start")]
        public static void ClearPlayStart()
        {
            EditorSceneManager.playModeStartScene = null;
            Debug.Log("[Ashes] Play volverá a iniciar en la escena abierta.");
        }

        // ------------------------------------------------------------------ Splash

        /// <summary>
        /// Renombra las escenas antiguas a la nomenclatura estándar conservando el GUID
        /// (así no se rompen referencias, Build Settings ni playModeStartScene).
        ///   Splash.unity      -> 1_scene_logo.unity
        ///   MainMenu.unity    -> 2_scene_menu.unity
        ///   SampleScene.unity -> 3_scene_game.unity -> 4_scene_game.unity
        /// </summary>
        private static void MigrateLegacyScenePaths()
        {
            RenameIfNeeded(LegacySplashPath, SplashPath, "1_scene_logo");
            RenameIfNeeded(LegacyMainMenuPath, MainMenuPath, "2_scene_menu");
            RenameIfNeeded(LegacySampleScenePath, LegacyGameScenePath, "3_scene_game");
            RenameIfNeeded(LegacyGameScenePath, GameScenePath, "4_scene_game");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void RenameIfNeeded(string legacyPath, string newPath, string newName)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(newPath) != null)
                return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(legacyPath) == null)
                return;

            string error = AssetDatabase.RenameAsset(legacyPath, newName);
            if (!string.IsNullOrEmpty(error))
                Debug.LogWarning("[Ashes] No se pudo renombrar " + legacyPath + ": " + error);
            else
                Debug.Log("[Ashes] Escena renombrada: " + legacyPath + " -> " + newPath);
        }

        private static void BuildSplashScene(Sprite[] sprites)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(Color.black);
            EnsureEventSystem();

            Canvas canvas = CreateCanvas();

            Image background = CreateImage(canvas.transform, "Background", null);
            Stretch(background.rectTransform);
            background.color = Color.black;
            background.raycastTarget = false;

            Image logo = CreateImage(canvas.transform, "Logo", sprites[0]);
            logo.preserveAspect = true;
            logo.raycastTarget = false;
            SetCenteredSize(logo, 900f);

            var managerGO = new GameObject("SplashManager");
            SplashScreen splash = managerGO.AddComponent<SplashScreen>();
            SetPrivate(splash, "logo", logo);
            SetPrivate(splash, "duration", 3f);
            SetPrivate(splash, "nextScene", MainMenuSceneName);
            SetPrivate(splash, "introDuration", 1.2f);
            SetPrivate(splash, "startScale", 0.4f);
            SetPrivate(splash, "fadeInDuration", 1.6f);
            SetPrivate(splash, "fadeOutDuration", 0.5f);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, SplashPath);
        }

        // --------------------------------------------------------------- Main Menu

        private static void BuildMainMenuScene(Sprite[] sprites)
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);

            Camera camera = Object.FindAnyObjectByType<Camera>();
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
            }

            EnsureEventSystem();

            GameObject canvasGO = GameObject.Find("Canvas");
            if (canvasGO == null)
                canvasGO = CreateCanvas().gameObject;

            MainMenuManager manager = canvasGO.GetComponent<MainMenuManager>();
            if (manager == null)
                manager = canvasGO.AddComponent<MainMenuManager>();
            SetPrivate(manager, "playSceneName", CinematicSceneName);
            SetPrivate(manager, "creditsSceneName", CreditsSceneName);
            SetPrivate(manager, "optionsSceneName", OptionsSceneName);
            SetPrivate(manager, "confirmationSceneName", ConfirmationSceneName);
            SetPrivate(manager, "selectFirstButtonOnStart", false);

            // Quita los botones antiguos basados en texto y cualquier reconstrucción previa.
            DestroyChild(canvasGO, "PlayButton");
            DestroyChild(canvasGO, "CreditsButton");
            DestroyChild(canvasGO, "MenuLogo");
            DestroyChild(canvasGO, "MenuButtons");

            // Fondo: llena la pantalla sin deformarse (cover).
            ConfigureBackground(canvasGO);

            // Panel de botones (centrado horizontalmente y pegado abajo).
            var panel = new GameObject("MenuButtons", typeof(RectTransform));
            panel.transform.SetParent(canvasGO.transform, false);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0f);
            prt.pivot = new Vector2(0.5f, 0f);
            prt.anchoredPosition = new Vector2(0f, 30f);
            prt.sizeDelta = new Vector2(500f, 420f);

            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            CreateButton(panel.transform, "PlayButton", sprites[1], sprites[2]);
            CreateButton(panel.transform, "CreditsButton", sprites[3], sprites[4]);
            CreateButton(panel.transform, "OptionsButton", sprites[5], sprites[6]);
            CreateButton(panel.transform, "QuitButton", sprites[7], sprites[8]);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuPath);
        }

        /// <summary>
        /// Prepara el fondo del menú para que llene la pantalla SIN deformarse (cover)
        /// usando el componente CoverBackground, alineado arriba (se ve el título).
        /// Si no existe el objeto "Background", lo crea con la imagen indicada.
        /// </summary>
        private static void ConfigureBackground(GameObject canvasGO)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MenuBackgroundPath);

            Transform existing = canvasGO.transform.Find("Background");
            Image image;

            if (existing == null)
            {
                image = CreateImage(canvasGO.transform, "Background", sprite);
                image.transform.SetAsFirstSibling();
            }
            else
            {
                image = existing.GetComponent<Image>();
                if (image == null)
                    image = existing.gameObject.AddComponent<Image>();
                if (image.sprite == null)
                    image.sprite = sprite;
            }

            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;

            RectTransform rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Quita el AspectRatioFitter antiguo (si quedara): ahora usamos CoverBackground.
            AspectRatioFitter oldFitter = image.GetComponent<AspectRatioFitter>();
            if (oldFitter != null)
                Object.DestroyImmediate(oldFitter, true);

            // Cover alineado arriba: muestra el título (borde superior) y recorta por abajo.
            CoverBackground cover = image.GetComponent<CoverBackground>();
            if (cover == null)
                cover = image.gameObject.AddComponent<CoverBackground>();
            SetPrivate(cover, "verticalAlign", 1f);
            SetPrivate(cover, "offsetY", 40f);
            cover.Apply();
        }

        private static void CreateButton(Transform parent, string name, Sprite normal, Sprite selected, float heightOverride = -1f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            Image image = go.AddComponent<Image>();
            image.sprite = normal;
            image.preserveAspect = true;
            image.type = Image.Type.Simple;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = selected,
                selectedSprite = selected,
                pressedSprite = selected,
                disabledSprite = normal
            };

            const float width = ButtonWidth;
            float height = heightOverride > 0f
                ? heightOverride
                : width * normal.rect.height / normal.rect.width;

            LayoutElement element = go.AddComponent<LayoutElement>();
            element.preferredWidth = width;
            element.preferredHeight = height;
            element.minWidth = width;
            element.minHeight = height;
        }

        /// <summary>
        /// Sube el tamaño máximo de textura de los fondos e imágenes grandes para no perder
        /// nitidez (2752 px necesitan 4096) y precarga la música para que no haya retardo.
        /// </summary>
        private static void ConfigureMediaImports()
        {
            string[] textures =
            {
                MenuBackgroundPath,
                CreditsImagePath,
                ConfirmationImagePath,
                YesNoSheetPath,
                CinematicImagesFolder + "/ecena_1.jpg", CinematicImagesFolder + "/ecena_2.jpg",
                CinematicImagesFolder + "/ecena_3.jpg", CinematicImagesFolder + "/ecena_4.jpg",
                CinematicImagesFolder + "/ecena_5.jpg", CinematicImagesFolder + "/ecena_6.jpg",
                CinematicImagesFolder + "/ecena_7.jpg", CinematicImagesFolder + "/ecena_8.jpg"
            };

            foreach (string path in textures)
            {
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                    continue;

                if (importer.maxTextureSize < 4096)
                {
                    importer.maxTextureSize = 4096;
                    importer.SaveAndReimport();
                    Debug.Log("[Ashes] Textura a 4096: " + path);
                }
            }

            if (AssetImporter.GetAtPath(MusicPath) is AudioImporter audio)
            {
                AudioImporterSampleSettings settings = audio.defaultSampleSettings;
                if (!settings.preloadAudioData)
                {
                    settings.preloadAudioData = true;
                    audio.defaultSampleSettings = settings;
                    audio.SaveAndReimport();
                    Debug.Log("[Ashes] Música precargada: " + MusicPath);
                }
            }
        }

        // ------------------------------------------------------------- Cinemática

        /// <summary>
        /// Construye 8 escenas cinematográficas individuales, una por imagen.
        /// La escena penúltima (índice 6) conserva las dos animaciones de destello
        /// (FogAnimator) que no deben moverse ni alterarse. Las demás escenas
        /// no incluyen estas animaciones para evitar el 45% de lentitud.
        /// </summary>
        private static void BuildCinematicScenes()
        {
            // Duraciones y textos originales de la cinemática.
            int count = Mathf.Min(CinematicTexts.Length, CinematicDurations.Length);
            float[] imageOffsets = CinematicImageOffsets;

            // Posiciones fijas de las dos animaciones en la escena penúltima.
            float[] effectXPositions = { -305f, 387f };
            float[] effectYPositions = { -10f, -192f };

            for (int i = 0; i < count; i++)
            {
                // Cada escena tiene una imagen de la cinemática.
                string imagePath = CinematicImagesFolder + "/ecena_" + (i + 1) + ".jpg";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
                if (sprite == null)
                {
                    Debug.LogWarning("[Ashes] Falta la imagen de la cinemática: " + imagePath);
                    continue;
                }

                // Nombre de la escena: 3_scene_cinematic_1 a 3_scene_cinematic_8
                string sceneName = "3_scene_cinematic_" + (i + 1);
                string scenePath = "Assets/Scenes/" + sceneName + ".unity";

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                CreateCamera(Color.black);
                Canvas canvas = CreateCanvas();

                // Imagen principal a pantalla completa.
                Image image = CreateImage(canvas.transform, "Slide1", sprite);
                image.raycastTarget = false;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;

                RectTransform rt = image.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.anchoredPosition = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);

                CoverBackground cover = image.gameObject.AddComponent<CoverBackground>();
                SetPrivate(cover, "verticalAlign", 0.5f);
                float imageOffset = (i < imageOffsets.Length) ? imageOffsets[i] : 0f;
                SetPrivate(cover, "offsetY", imageOffset);
                SetAlpha(image, 0f);
                cover.Apply();

                // Narración.
                Text narration = CreateNarrationText(canvas.transform);

                // Franja negra inferior.
                Image band = CreateImage(canvas.transform, "NarrationBand", null);
                band.raycastTarget = false;
                band.type = Image.Type.Simple;
                band.preserveAspect = false;
                band.color = Color.black;
                RectTransform bandRect = band.rectTransform;
                bandRect.anchorMin = new Vector2(0f, 0f);
                bandRect.anchorMax = new Vector2(1f, NarrationBandTop);
                bandRect.offsetMin = Vector2.zero;
                bandRect.offsetMax = Vector2.zero;

                // Botón "Siguiente".
                Button nextButton = CreateNextButton(canvas.transform);

                // Music.
                AudioClip music = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicPath);
                var managerGO = new GameObject("CinematicManager");
                AudioSource audioSource = managerGO.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.spatialBlend = 0f;
                audioSource.clip = music;
                audioSource.volume = 0.7f;

                CinematicSequence sequence = managerGO.AddComponent<CinematicSequence>();
                // Configurar slide individual.
                var so = new SerializedObject(sequence);
                SerializedProperty slides = so.FindProperty("slides");
                slides.arraySize = 1;
                SerializedProperty element = slides.GetArrayElementAtIndex(0);
                element.FindPropertyRelative("image").objectReferenceValue = sprite;
                element.FindPropertyRelative("text").stringValue = CinematicTexts[i];
                element.FindPropertyRelative("duration").floatValue = CinematicDurations[i];
                so.ApplyModifiedPropertiesWithoutUndo();
                SetPrivate(sequence, "narrationText", narration);
                SetPrivate(sequence, "fadeOverlay", CreateImage(canvas.transform, "FadeOverlay", null));
                SetPrivate(sequence, "musicSource", audioSource);
                SetPrivate(sequence, "music", music);
                SetPrivate(sequence, "musicVolume", 0.7f);
                SetPrivate(sequence, "imageFadeDuration", 0.6f);
                SetPrivate(sequence, "textFadeIn", 0.8f);
                SetPrivate(sequence, "textFadeOut", 0.8f);
                SetPrivate(sequence, "textStartDelay", 0.2f);
                SetPrivate(sequence, "endFadeDuration", 0.5f);
                SetPrivate(sequence, "nextScene", (i + 1 < count) ? "3_scene_cinematic_" + (i + 2) : GameSceneName);
                SetPrivate(sequence, "allowSkip", true);

                // Efectos especiales solo en la escena penúltima (i == 6).
                if (i == 6)  // Penúltima de 8 escenas
                {
                    GameObject[] slideEffects = CreateS001SlideEffects(canvas.transform);
                    SetPrivateObjectArray(sequence, "slideEffects", slideEffects);
                    SetPrivate(sequence, "effectsSlideIndex", 6);

                    // Colocar las dos animaciones en posiciones fijas.
                    for (int e = 0; e < slideEffects.Length; e++)
                    {
                        if (e < slideEffects.Length && e < effectXPositions.Length)
                        {
                            Image effectImg = slideEffects[e].GetComponent<Image>();
                            if (effectImg != null)
                            {
                                RectTransform effectRt = effectImg.rectTransform;
                                effectRt.anchorMin = new Vector2(0.5f, 0.5f);
                                effectRt.anchorMax = new Vector2(0.5f, 0.5f);
                                effectRt.pivot = new Vector2(0.5f, 0.5f);
                                effectRt.anchoredPosition = new Vector2(effectXPositions[e], effectYPositions[e]);
                            }
                        }
                    }
                }
                else
                {
                    // Sin efectos de animación en las demás escenas (evita 45% de lentitud).
                    SetPrivateObjectArray(sequence, "slideEffects", new GameObject[0]);
                    SetPrivate(sequence, "effectsSlideIndex", -1);
                }

                // Evento del botón.
                EnsureEventSystem();
                UnityEventTools.AddPersistentListener(nextButton.onClick, sequence.NextSlide);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, scenePath);

                Debug.Log("[Ashes] Escena cinematográfica creada: " + sceneName + " (" + scenePath + ")");
            }

            // Eliminar la escena única antigua si existe.
            string oldPath = "Assets/Scenes/3_scene_cinematic.unity";
            if (System.IO.File.Exists(oldPath) || AssetDatabase.LoadAssetAtPath<SceneAsset>(oldPath) != null)
            {
                AssetDatabase.DeleteAsset(oldPath);
                Debug.Log("[Ashes] Escena antigua 3_scene_cinematic.unity eliminada.");
            }
        }

        private static GameObject[] CreateS001SlideEffects(Transform parent)
        {
            Sprite[] frames = LoadSpritesAtPath(S001SpriteSheetPath);
            if (frames.Length < 2)
            {
                Debug.LogWarning("[Ashes] No se encontraron los sprites de animación en " + S001SpriteSheetPath);
                return new GameObject[0];
            }

            float[] xPositions = { -305f, 387f };
            float[] yPositions = { -10f, -192f };
            var effects = new GameObject[xPositions.Length];
            for (int i = 0; i < effects.Length; i++)
            {
                Image image = CreateImage(parent, "Destello" + (i + 1), frames[0]);
                image.raycastTarget = false;
                image.preserveAspect = true;

                RectTransform rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(xPositions[i], yPositions[i]);
                rect.sizeDelta = new Vector2(80f, 80f);

                FogAnimator animator = image.gameObject.AddComponent<FogAnimator>();
                SetPrivate(animator, "opacity", 1f);
                SetPrivate(animator, "framesPerSecond", S001AnimationFramesPerSecond);
                SetPrivateObjectArray(animator, "customFrames", frames);
                effects[i] = image.gameObject;
            }

            return effects;
        }

        // --------------------------------------------------------------- Créditos

        /// <summary>
        /// Construye la escena 5_scene_credits: imagen de fondo + texto de créditos
        /// mostrado por partes, con flechas laterales para pasar de parte y flecha
        /// para volver al menú anterior.
        /// </summary>
        private static void BuildCreditsScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(Color.black);
            EnsureEventSystem();

            Canvas canvas = CreateCanvas();

            Image background = CreateImage(canvas.transform, "Background", null);
            Stretch(background.rectTransform);
            background.color = Color.black;
            background.raycastTarget = false;

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CreditsImagePath);
            if (sprite == null)
                Debug.LogWarning("[Ashes] Falta la imagen de créditos: " + CreditsImagePath +
                                 " (¿no está o no se importó como Sprite?)");

            Image creditsBg = CreateImage(canvas.transform, "CreditsImage", sprite);
            creditsBg.raycastTarget = false;
            creditsBg.type = Image.Type.Simple;
            creditsBg.preserveAspect = false;

            RectTransform bgRt = creditsBg.rectTransform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.anchoredPosition = Vector2.zero;
            bgRt.pivot = new Vector2(0.5f, 0.5f);

            CoverBackground cover = creditsBg.gameObject.AddComponent<CoverBackground>();
            SetPrivate(cover, "verticalAlign", 0.5f);
            SetPrivate(cover, "offsetY", 0f);
            cover.Apply();

            RectTransform viewport = CreateCreditsViewport(canvas.transform);
            Text creditsText = CreateCreditsText(viewport);
            Text pageLabel = CreatePageLabel(viewport);
            Button previousArrow = CreatePageArrow(viewport, "PreviousButton", "\u2190", true);
            Button nextArrow = CreatePageArrow(viewport, "NextButton", "\u2192", false);

            Button backButton = CreateBackButton(canvas.transform);

            GameObject mgr = new GameObject("CreditsManager");
            mgr.transform.SetParent(canvas.transform, false);
            CreditsScreen screen = mgr.AddComponent<CreditsScreen>();
            SetPrivate(screen, "backSceneName", MainMenuSceneName);
            SetPrivate(screen, "backButton", backButton);
            SetPrivate(screen, "creditsText", creditsText);
            SetPrivate(screen, "nextButton", nextArrow);
            SetPrivate(screen, "previousButton", previousArrow);
            SetPrivate(screen, "pageLabel", pageLabel);
            SetPrivate(screen, "fadeTime", 0.18f);
            SetStringArray(screen, "pages", CreditsPages);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, CreditsPath);
        }

        // ------------------------------------------------------- Confirmar salida

        /// <summary>
        /// Construye la escena 6_scene_confirmation: imagen de fondo (menu_confirmacion)
        /// + botones Sí y No en el centro. Sí sale del juego, No vuelve al menú.
        /// Los sprites vienen de si_no.psd: 0 = sí, 1 = sí seleccionado,
        /// 2 = no, 3 = no seleccionado.
        /// </summary>
        private static void BuildConfirmationScene()
        {
            // Conserva la colocación hecha a mano en el editor: si la escena ya
            // existe, se leen la posición del panel de botones y sus ajustes para
            // que al regenerar no se muevan de sitio.
            Vector2 panelPosition = new Vector2(1f, -98.9f);   // por defecto: la colocación elegida a mano
            Vector2 panelSize = new Vector2(700f, 110f);
            float panelSpacing = 60f;
            TextAnchor panelAlignment = TextAnchor.MiddleCenter;

            if (System.IO.File.Exists(ConfirmationPath))
            {
                EditorSceneManager.OpenScene(ConfirmationPath, OpenSceneMode.Single);

                var oldPanel = GameObject.Find("ConfirmButtons");
                if (oldPanel != null)
                {
                    RectTransform oldRect = oldPanel.GetComponent<RectTransform>();
                    if (oldRect != null)
                    {
                        panelPosition = oldRect.anchoredPosition;
                        panelSize = oldRect.sizeDelta;
                    }

                    HorizontalLayoutGroup oldLayout = oldPanel.GetComponent<HorizontalLayoutGroup>();
                    if (oldLayout != null)
                    {
                        panelSpacing = oldLayout.spacing;
                        panelAlignment = oldLayout.childAlignment;
                    }
                }
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(Color.black);
            EnsureEventSystem();

            Canvas canvas = CreateCanvas();

            Image background = CreateImage(canvas.transform, "Background", null);
            Stretch(background.rectTransform);
            background.color = Color.black;
            background.raycastTarget = false;

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ConfirmationImagePath);
            if (sprite == null)
                Debug.LogWarning("[Ashes] Falta la imagen de confirmación: " + ConfirmationImagePath +
                                 " (¿no está o no se importó como Sprite?)");

            Image confirmBg = CreateImage(canvas.transform, "ConfirmImage", sprite);
            confirmBg.raycastTarget = false;
            confirmBg.type = Image.Type.Simple;
            confirmBg.preserveAspect = false;

            RectTransform bgRt = confirmBg.rectTransform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.anchoredPosition = Vector2.zero;
            bgRt.pivot = new Vector2(0.5f, 0.5f);

            CoverBackground cover = confirmBg.gameObject.AddComponent<CoverBackground>();
            SetPrivate(cover, "verticalAlign", 0.5f);
            cover.Apply();

            ConfirmationScreen screen = canvas.gameObject.AddComponent<ConfirmationScreen>();
            SetPrivate(screen, "menuSceneName", MainMenuSceneName);

            Sprite[] yesNo = LoadYesNoSprites();
            if (yesNo == null || yesNo.Length < 4)
            {
                Debug.LogError("[Ashes] No se pudieron cargar los 4 sprites (si_no_0..3) de " +
                               YesNoSheetPath + ". Encontrados: " + (yesNo == null ? 0 : yesNo.Length));
            }
            else
            {
                // Panel de botones (respeta la colocación leída de la escena actual).
                var panel = new GameObject("ConfirmButtons", typeof(RectTransform));
                panel.transform.SetParent(canvas.transform, false);
                RectTransform prt = panel.GetComponent<RectTransform>();
                prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
                prt.pivot = new Vector2(0.5f, 0.5f);
                prt.anchoredPosition = panelPosition;
                prt.sizeDelta = panelSize;

                HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = panelSpacing;
                layout.childAlignment = panelAlignment;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                // Misma altura para Sí y No, para que queden alineados.
                float height = ButtonWidth * yesNo[0].rect.height / yesNo[0].rect.width;
                CreateButton(panel.transform, "SiButton", yesNo[0], yesNo[1], height);
                CreateButton(panel.transform, "NoButton", yesNo[2], yesNo[3], height);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ConfirmationPath);
        }

        /// <summary>Carga los sprites si_no_0..3 del spritesheet de confirmación.</summary>
        private static Sprite[] LoadYesNoSprites()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(YesNoSheetPath);
            var sprites = new List<Sprite>();
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite)
                    sprites.Add(sprite);
            }

            return sprites.OrderBy(SpriteIndex).ToArray();
        }

        private static Text CreateNarrationText(Transform parent)
        {
            var go = new GameObject("Narration", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            Text text = go.AddComponent<Text>();
            text.font = GetBuiltinFont();
            text.fontStyle = FontStyle.Italic;
            text.fontSize = 38;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = string.Empty;

            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(2f, -2f);

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, NarrationBottom);
            rect.anchorMax = new Vector2(1f, NarrationTop);
            rect.offsetMin = new Vector2(140f, 0f);
            rect.offsetMax = new Vector2(-140f, 0f);

            return text;
        }

        /// <summary>
        /// Botón "Siguiente" de la cinemática: esquina inferior derecha, dentro de la franja negra.
        /// Sin fondo (transparente), solo se ve el texto. El clic se conecta a
        /// CinematicSequence.NextSlide (y también responde a Enter).
        /// </summary>
        private static Button CreateNextButton(Transform parent)
        {
            var go = new GameObject("NextButton", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            // Fondo transparente: solo se ve el texto. Recibe el clic igualmente.
            Image image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;

            Text label = CreateLabel(go.transform, "Siguiente  >");

            Button button = go.AddComponent<Button>();
            button.targetGraphic = label;                 // el fundido al pasar el ratón se aplica al texto
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1f, 0.85f, 0.45f, 1f),
                pressedColor = new Color(0.9f, 0.72f, 0.3f, 1f),
                selectedColor = new Color(1f, 0.85f, 0.45f, 1f),
                disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-6f, 0f);    // esquina inferior derecha, dentro de la franja (0..0.11)
            rect.sizeDelta = new Vector2(180f, 46f);

            return button;
        }

        private static Text CreateLabel(Transform parent, string value)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            Text text = go.AddComponent<Text>();
            text.font = GetBuiltinFont();
            text.fontSize = 24;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = value;

            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(2f, -2f);

            RectTransform rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return text;
        }

        private static Font GetBuiltinFont()
        {
            Font font = null;
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { /* no existe en esta versión */ }

            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch { /* tampoco */ }
            }

            return font;
        }

        private static RectTransform CreateCreditsViewport(Transform parent)
        {
            var go = new GameObject("CreditsViewport", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(410f, 70f);
            rt.offsetMax = new Vector2(-410f, -110f);
            return rt;
        }

        private static Text CreateCreditsText(RectTransform viewport)
        {
            var go = new GameObject("CreditsText", typeof(RectTransform));
            go.transform.SetParent(viewport, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            // Ocupa la ventana menos un margen a cada lado (ahí quedan las flechas);
            // cada parte se escribe centrada dentro de él.
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(150f, 0f);
            rt.offsetMax = new Vector2(-150f, 0f);

            Text text = go.AddComponent<Text>();
            text.font = GetBuiltinFont();
            text.fontSize = 34;
            text.lineSpacing = 1.2f;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = string.Empty;

            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(2f, -2f);

            // Permite fundir el texto (y su sombra) al cambiar de parte.
            CanvasGroup group = go.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            return text;
        }

        private static Text CreatePageLabel(RectTransform viewport)
        {
            var go = new GameObject("PageLabel", typeof(RectTransform));
            go.transform.SetParent(viewport, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 4f);
            rt.sizeDelta = new Vector2(200f, 40f);

            Text text = go.AddComponent<Text>();
            text.font = GetBuiltinFont();
            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 1f, 1f, 0.75f);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = string.Empty;
            return text;
        }

        /// <summary>Flecha lateral (← previous / → next) para pasar de parte de créditos.</summary>
        private static Button CreatePageArrow(Transform parent, string name, string glyph, bool left)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            Vector2 side = left ? Vector2.zero : Vector2.one; // anclada a la izquierda o a la derecha
            rt.anchorMin = new Vector2(side.x, 0.5f);
            rt.anchorMax = new Vector2(side.x, 0.5f);
            rt.pivot = new Vector2(side.x, 0.5f);
            rt.anchoredPosition = new Vector2(left ? 40f : -40f, 0f);
            rt.sizeDelta = new Vector2(90f, 90f);

            Image image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;

            Text label = CreateLabel(go.transform, glyph);
            label.fontSize = 64;
            label.fontStyle = FontStyle.Bold;
            label.color = Color.white;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1f, 0.85f, 0.45f, 1f),
                pressedColor = new Color(0.9f, 0.72f, 0.3f, 1f),
                selectedColor = Color.white,
                disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.35f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            return button;
        }

        private static Button CreateBackButton(Transform parent)
        {
            var go = new GameObject("BackButton", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(40f, -40f);
            rt.sizeDelta = new Vector2(80f, 80f);

            Image image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;

            Text label = CreateLabel(go.transform, "\u2190");
            label.fontSize = 56;
            label.fontStyle = FontStyle.Bold;
            label.color = Color.white;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = label;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1f, 0.85f, 0.45f, 1f),
                pressedColor = new Color(0.9f, 0.72f, 0.3f, 1f),
                selectedColor = Color.white,
                disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            return button;
        }

        private static void SetCinematicSlides(CinematicSequence sequence, Sprite[] images)
        {
            var so = new SerializedObject(sequence);
            SerializedProperty slides = so.FindProperty("slides");
            slides.arraySize = images.Length;

            for (int i = 0; i < images.Length; i++)
            {
                SerializedProperty element = slides.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("image").objectReferenceValue = images[i];
                element.FindPropertyRelative("text").stringValue = CinematicTexts[i];
                element.FindPropertyRelative("duration").floatValue = CinematicDurations[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }


        // ------------------------------------------------------------------ Utils

        /// <summary>
        /// Quita el filtrado de la textura para que los bordes de los sprites se vean nítidos.
        /// </summary>
        private static void ConfigureSpriteSheet()
        {
            if (AssetImporter.GetAtPath(SpriteSheetPath) is not TextureImporter importer)
                return;

            bool changed = false;

            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
                Debug.Log("[Ashes] Sprite sheet: filtro = Point, mipmaps = off.");
            }
        }

        private static Sprite[] LoadSprites()
        {
            return LoadSpritesAtPath(SpriteSheetPath);
        }

        private static Sprite[] LoadSpritesAtPath(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprites = new List<Sprite>();
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite)
                    sprites.Add(sprite);
            }

            return sprites.OrderBy(SpriteIndex).ToArray();
        }

        private static int SpriteIndex(Sprite sprite)
        {
            int underscore = sprite.name.LastIndexOf('_');
            if (underscore >= 0 && int.TryParse(sprite.name.Substring(underscore + 1), out int index))
                return index;
            return int.MaxValue;
        }

        private static void UpdateBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(SplashPath, true),
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(CinematicPath, true)
            };

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameScenePath) != null)
                scenes.Add(new EditorBuildSettingsScene(GameScenePath, true));

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CreditsPath) != null)
                scenes.Add(new EditorBuildSettingsScene(CreditsPath, true));

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ConfirmationPath) != null)
                scenes.Add(new EditorBuildSettingsScene(ConfirmationPath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void CreateCamera(Color background)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";

            Camera camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            go.AddComponent<AudioListener>();
            go.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(RectTransform));
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            if (name == "FadeOverlay")
            {
                image.color = Color.black;
                image.raycastTarget = false;
            }
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetCenteredSize(Image image, float width)
        {
            RectTransform rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, width * image.sprite.rect.height / image.sprite.rect.width);
        }

        private static void DestroyChild(GameObject parent, string name)
        {
            Transform child = parent.transform.Find(name);
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }

        private static void SetPrivate(Object target, string property, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(property).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetPrivate(Object target, string property, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(property).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetPrivate(Object target, string property, string value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(property).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetPrivate(Object target, string property, bool value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(property).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetStringArray(Object target, string property, string[] values)
        {
            var so = new SerializedObject(target);
            SerializedProperty array = so.FindProperty(property);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).stringValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetPrivateObjectArray(Object target, string property, Object[] values)
        {
            var so = new SerializedObject(target);
            SerializedProperty array = so.FindProperty(property);
            array.arraySize = values.Length;

            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
                return;

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }
}
#endif
