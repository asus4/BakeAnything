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
        public static Texture2D BakeToTexture(
            IBakable bakable,
            Texture2D texture = null,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            FilterMode filterMode = FilterMode.Point)
        {
            int width = bakable.Width;
            int height = bakable.Height;
            ReadOnlySpan<Color> colors = bakable.Bake();

            // Create asset if it doesn't exist
            if (texture == null)
            {
                texture = new Texture2D(width, height, TextureFormat.RGBAHalf, false, true)
                {
                    alphaIsTransparency = false,
                };
            }
            else
            {
                texture.Reinitialize(width, height, TextureFormat.RGBAHalf, false);
            }

            texture.wrapMode = wrapMode;
            texture.filterMode = filterMode;
            var data = new Color[width * height];
            colors.CopyTo(data);
            texture.SetPixels(data);
            texture.Apply();
            return texture;
        }

        public static Texture2D BakeToAsset(IBakable bakable, string path,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            FilterMode filterMode = FilterMode.Point)
        {
            string textureName = Path.GetFileNameWithoutExtension(path);
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null)
            {
                // Create asset if it doesn't exist
                var texture = BakeToTexture(bakable, null, wrapMode, filterMode);
                AssetDatabase.CreateAsset(texture, path);
                texture.name = textureName;
                return texture;
            }
            else if (asset is Texture2D texture)
            {
                // Replace texture if it already exists
                BakeToTexture(bakable, texture, wrapMode, filterMode);
                texture.name = textureName;
                EditorUtility.SetDirty(texture);
                return texture;
            }
            else
            {
                // Don't create asset if it already exists with the other type
                throw new Exception($"Asset already exists at {path}");
            }
        }

        public static Texture2D BakeToAsset(UnityEngine.Object obj)
        {
            if (obj is not IBakable bakable)
            {
                throw new InvalidOperationException($"target is not IBakable");
            }

            string path = AssetDatabase.GetAssetPath(obj);
            // Rename path to {original}-baked.asset
            path = $"{path[..path.LastIndexOf('.')]}-baked.asset";

            path = EditorUtility.SaveFilePanelInProject(
                "Bake into Texture",
                Path.GetFileNameWithoutExtension(path),
                "asset",
                "Save baked data as texture",
                Path.GetDirectoryName(path));
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return BakeToAsset(bakable, path);
        }

        public static void ExportToEXR(IBakable bakable, string path)
        {
            var texture = BakeToTexture(bakable);
            byte[] bytes = texture.EncodeToEXR();
            File.WriteAllBytes(path, bytes);
        }
    }
}
