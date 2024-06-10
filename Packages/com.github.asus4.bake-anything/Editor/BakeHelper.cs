using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

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

        public static void Bake(IBakable bakable, string path)
        {
            Debug.Log($"Bake {path}");

            int width = bakable.Width;
            int height = bakable.Height;
            ReadOnlySpan<Color> colors = bakable.Bake();
            if (colors.Length > width * height)
            {
                throw new Exception($"Baked data is too long: {colors.Length} > {width} * {height}");
            }

            var texture = new Texture2D(
                width, height,
                textureFormat: TextureFormat.RGBAHalf,
                mipChain: false,
                linear: true);
            var data = new Color[width * height];
            colors.CopyTo(data);
            texture.SetPixels(data);

            // clean up
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}
