using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BakeAnything.Editor
{
    /// <summary>
    /// Editor tool to bake anything.
    /// </summary>
    public sealed class TimelineBakerWindow : EditorWindow
    {
        [MenuItem("Window/Bake Anything/Open Timeline Baker")]
        public static void OpenWindow()
        {
            var window = GetWindow<TimelineBakerWindow>();
            window.titleContent = new GUIContent("Bake Anything");
            window.Show();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            PropertyField propertyField = new();
            root.Add(propertyField);

            // Create toggle
            Toggle toggle = new()
            {
                name = "toggle",
                label = "Toggle"
            };
            root.Add(toggle);

            // Bake Button
            root.Add(new Button(OnClickBake)
            {
                name = "bake-button",
                text = "Bake",
            });
        }

        private void OnClickBake()
        {
            Debug.Log("TODO: Bake");
        }
    }
}
