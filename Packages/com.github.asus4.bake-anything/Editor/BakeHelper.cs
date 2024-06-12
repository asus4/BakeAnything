using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BakeAnything
{
    /// <summary>
    /// Core methods to bake anything.
    /// </summary>
    public static class BakeHelper
    {
        public static string GetDefaultPath(UnityEngine.Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            // replace path to .asset
            return $"{path[..path.LastIndexOf('.')]}-baked.asset";
        }

        public static void Bake(UnityEngine.Object obj)
        {
            if (obj is not IBakable bakable)
            {
                throw new InvalidOperationException($"target is not IBakable");
            }

            string path = GetDefaultPath(obj);
            path = EditorUtility.SaveFilePanelInProject(
                "Bake into Texture",
                Path.GetFileNameWithoutExtension(path),
                "asset",
                "Save baked data as texture",
                Path.GetDirectoryName(path));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            Bake(bakable, path);
        }

        public static void Bake(IBakable bakable, string path,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            FilterMode filterMode = FilterMode.Point)
        {
            int width = bakable.Width;
            int height = bakable.Height;
            ReadOnlySpan<Color> colors = bakable.Bake();
            if (colors.Length > width * height)
            {
                throw new Exception($"Baked data is too long: {colors.Length} > {width} * {height}");
            }

            var data = new Color[width * height];
            colors.CopyTo(data);

            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null)
            {
                // Create asset if it doesn't exist
                var texture = new Texture2D(
                width, height,
                textureFormat: TextureFormat.RGBAHalf,
                mipChain: false,
                linear: true)
                {
                    wrapMode = wrapMode,
                    filterMode = filterMode,
                    alphaIsTransparency = false,
                };
                texture.SetPixels(data);
                AssetDatabase.CreateAsset(texture, path);
            }
            else if (asset is Texture2D texture)
            {
                // Replace texture if it already exists
                texture.Reinitialize(width, height, TextureFormat.RGBAHalf, hasMipMap: false);
                texture.wrapMode = wrapMode;
                texture.filterMode = filterMode;
                texture.SetPixels(data);
                texture.Apply();
                EditorUtility.SetDirty(texture);
            }
            else
            {
                // Don't create asset if it already exists with the other type
                throw new Exception($"Asset already exists at {path}");
            }
        }
    }
}
