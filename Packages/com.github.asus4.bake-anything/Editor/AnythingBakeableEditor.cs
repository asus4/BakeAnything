using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace BakeAnything
{
    /// <summary>
    /// Add a "Bake" button to the end of the inspector.
    /// </summary>
    [CustomEditor(typeof(AnythingBakeable), true)]
    public class AnythingBakeableEditor : Editor
    {
        private SerializedProperty bakeOptions;

        private void OnEnable()
        {
            bakeOptions = serializedObject.FindProperty("bakeOptions");
            Assert.IsNotNull(bakeOptions);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            // base.OnInspectorGUI();
            // Draw default inspector without bakeOptions
            DrawPropertiesExcluding(serializedObject, "bakeOptions");

            var target = this.target as AnythingBakeable;
            // Statistics
            EditorGUILayout.Space();
            GUILayout.Label("Texture Statistics:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox($"Width: {target.Width}\nHeight: {target.Height}", MessageType.None);

            // Draw Options
            EditorGUILayout.PropertyField(bakeOptions);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            // Buttons
            if (GUILayout.Button("Bake to Asset"))
            {
                BakeToAsset();
            }
            if (GUILayout.Button("Bake to EXR"))
            {
                BakeToEXR();
            }
        }

        protected virtual void BakeToAsset()
        {
            var target = this.target as AnythingBakeable;
            if (TryGetSavePath(target, "asset", out string path))
            {
                BakeAnythingCore.BakeToAsset(target, path, target.BakeOptions);
            }
        }

        protected virtual void BakeToEXR()
        {
            var target = this.target as AnythingBakeable;
            if (TryGetSavePath(target, "exr", out string path))
            {
                BakeAnythingCore.ExportToEXR(target, path, target.BakeOptions);
            }
        }

        protected static bool TryGetSavePath(UnityEngine.Object target, string extension, out string savePath)
        {
            string assetPath = AssetDatabase.GetAssetPath(target);
            savePath = EditorUtility.SaveFilePanelInProject(
                title: $"Bake to {extension}",
                defaultName: $"{Path.GetFileNameWithoutExtension(assetPath)}-baked",
                extension: extension,
                message: $"Bake data as {extension}",
                path: Path.GetDirectoryName(assetPath));
            return !string.IsNullOrEmpty(savePath);
        }
    }
}
