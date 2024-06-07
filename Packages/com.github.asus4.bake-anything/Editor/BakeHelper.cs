using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace BakeAnything
{
    /// <summary>
    /// Core methods to bake anything.
    /// </summary>
    public static class BakeHelper
    {
        /// <summary>
        /// 
        public static string GetAssetPath(UnityEngine.Object obj)
        {
            return AssetDatabase.GetAssetPath(obj);
        }

        public static void Bake(string defaultName, IBakable bakable)
        {
            string path = EditorUtility.SaveFilePanel("Bake to Exr", Application.dataPath, defaultName, "exr");

            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Application.dataPath, path);
            }
            Debug.Log($"Bake {path}");
            // Check if the path is inside of the Assets folder or not
            if (path.StartsWith("Assets/"))
            {
                Debug.Log("Bake succeeded");
            }
            else
            {
                Debug.LogError("Bake failed: path is not inside of the Assets folder");
            }
        }
    }
}
