using UnityEditor;
using UnityEditor.SceneManagement;

namespace AshesOfRyu.EditorTools
{
    public static class SceneSwitcherEditor
    {
        [MenuItem("Scenes/Main Menu (Menu Principal)")]
        public static void OpenMainMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene("Assets/Scenes/2_scene_menu.unity");
            }
        }

        [MenuItem("Scenes/Cinematic (Cinemática)")]
        public static void OpenCinematic()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene("Assets/Scenes/3_scene_cinematic.unity");
            }
        }

        [MenuItem("Scenes/Gameplay (Bosque Oscuro)")]
        public static void OpenGameplay()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene("Assets/Scenes/4_scene_game.unity");
            }
        }
    }
}