using System.IO;
using UnityEditor;
using UnityEngine;

namespace BakeAnything
{
    /// <summary>
    /// Add a "Bake" button to the end of the inspector.
    /// </summary>
    [CustomEditor(typeof(AnythingBakable), true)]
    public class AnythingBakableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

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
            if (TryGetSavePath(target, "asset", out string path))
            {
                BakeAnythingCore.BakeToAsset(target as IBakable, path);
            }
        }

        protected virtual void BakeToEXR()
        {
            if (TryGetSavePath(target, "exr", out string path))
            {
                BakeAnythingCore.ExportToEXR(target as IBakable, path);
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
