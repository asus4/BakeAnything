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
            BakeHelper.BakeToAsset(target);
        }

        protected virtual void BakeToEXR()
        {
            string assetPath = AssetDatabase.GetAssetPath(target);
            // Rename path to {original}-baked.exr
            string fileName = $"{assetPath[..assetPath.LastIndexOf('.')]}-baked.exr";
            string path = EditorUtility.SaveFilePanelInProject(
                "Export to EXR",
                Path.GetFileNameWithoutExtension(fileName),
                "exr",
                "Save baked data as EXR",
                Path.GetDirectoryName(assetPath));
            BakeHelper.ExportToEXR(target as IBakable, path);
        }
    }
}
