using System;
using System.IO;
using UnityEngine;

namespace BakeAnything.Editor
{
    /// <summary>
    /// Core methods to bake anything.
    /// </summary>
    public static class Baker
    {
        public static void Bake(string path, ReadOnlySpan<float> data)
        {
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
