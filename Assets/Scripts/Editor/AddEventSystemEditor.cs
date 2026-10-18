using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace AshesOfRyu.EditorTools
{
    public static class AddEventSystemEditor
    {
        [MenuItem("Tools/Gameplay/Add EventSystem To Scene")]
        public static void AddEventSystem()
        {
            var existing = Object.FindAnyObjectByType<EventSystem>();
            if (existing != null)
            {
                Debug.Log("EventSystem ya existe en la escena");
                return;
            }

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            Undo.RegisterCreatedObjectUndo(go, "Add EventSystem");
            Selection.activeGameObject = go;
            Debug.Log("EventSystem creado con EventSystem + InputSystemUIInputModule");
        }

        [MenuItem("Tools/Gameplay/Simulate PlayButton Click")]
        public static void SimulatePlayClick()
        {
            var buttons = Object.FindObjectsByType<Button>();
            foreach (var b in buttons)
            {
                if (b.gameObject.name == "PlayButton")
                {
                    int calls = b.onClick.GetPersistentEventCount();
                    Debug.Log($"[SimulateClick] {b.name} persistentCalls={calls}");
                    if (calls > 0)
                    {
                        var target = b.onClick.GetPersistentTarget(0);
                        Debug.Log($"[SimulateClick] target={(target == null ? "NULL" : target.name + " (" + target.GetType().Name + ")")} method={b.onClick.GetPersistentMethodName(0)}");
                    }
                    Debug.Log("[SimulateClick] Invocando onClick de " + b.name);
                    b.onClick.Invoke();
                    return;
                }
            }
            Debug.LogWarning("[SimulateClick] No se encontro un PlayButton con componente Button");
        }
    }
}