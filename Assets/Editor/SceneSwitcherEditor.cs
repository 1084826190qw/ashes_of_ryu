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
                EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            }
        }

        [MenuItem("Scenes/Gameplay (Bosque Oscuro)")]
        public static void OpenGameplay()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            }
        }
    }
}