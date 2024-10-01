using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Assertions;

namespace BakeAnything
{
    /// <summary>
    /// Core methods to bake texture.
    /// </summary>
    public static class BakeAnythingCore
    {
        /// <summary>
        /// Bakes data into a texture.
        /// </summary>
        /// <param name="bakable">An IBakable instance</param>
        /// <param name="texture">A texture to be baked. When null, creates new texture</param>
        /// <param name="options">Options</param>
        /// <returns>The Baked Texture</returns>
        public static Texture2D BakeToTexture(
            IBakable bakable,
            Texture2D texture = null,
            BakeOptions options = null)
        {
            options ??= BakeOptions.Default;
            int width = bakable.Width;
            int height = bakable.Height;
            ReadOnlySpan<Color> colors = bakable.Bake();

            // Create asset if it doesn't exist
            if (texture == null)
            {
                texture = new Texture2D(width, height, options.Format, false, true)
                {
                    alphaIsTransparency = false,
                };
            }
            else
            {
                texture.Reinitialize(width, height, options.Format, false);
            }

            texture.wrapMode = options.WrapMode;
            texture.filterMode = options.FilterMode;
            var data = new Color[width * height];
            colors.CopyTo(data);
            texture.SetPixels(data);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Bakes and saves data into a texture asset.
        /// </summary>
        /// <param name="bakable">An IBakable instance</param>
        /// <param name="path">A path inside project</param>
        /// <param name="options">Options</param>
        /// <returns>The Baked Texture</returns>
        public static Texture2D BakeToAsset(
            IBakable bakable,
            string path,
            BakeOptions options = null)
        {
            if (!IsProjectPath(path))
            {
                throw new ArgumentException("Path must be inside of the project", nameof(path));
            }

            string textureName = Path.GetFileNameWithoutExtension(path);
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null)
            {
                // Create new asset at the path
                var texture = BakeToTexture(bakable, null, options);
                AssetDatabase.CreateAsset(texture, path);
                texture.name = textureName;
                return texture;
            }
            else if (asset is Texture2D texture)
            {
                // Replace texture if it already exists
                BakeToTexture(bakable, texture, options);
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

        /// <summary>
        /// Exports data into an EXR file.
        /// </summary>
        /// <param name="bakable">An IBakable instance</param>
        /// <param name="path">An export path</param>
        /// <param name="options">Options</param>
        public static void ExportToEXR(
            IBakable bakable,
            string path,
            BakeOptions options = null)
        {
            var texture = BakeToTexture(bakable);
            byte[] bytes = texture.EncodeToEXR();
            File.WriteAllBytes(path, bytes);

            // Override texture importer settings if it's a inside of the project
            if (IsProjectPath(path))
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.IsNotNull(importer);
                int maxSize = Mathf.NextPowerOfTwo(Mathf.Max(texture.width, texture.height));
                ModifyImporterSetting(importer, maxSize, options);
            }
        }

        public static void ModifyImporterSetting(
            TextureImporter importer,
            int maxTextureSize = 8192,
            BakeOptions options = null)
        {
            if (maxTextureSize > 16384)
            {
                throw new ArgumentOutOfRangeException(nameof(maxTextureSize),
                    "Max texture size must be less than or equal to 16384");
            }

            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
            importer.sRGBTexture = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.isReadable = true;

            importer.wrapMode = options.WrapMode;
            importer.filterMode = options.FilterMode;
            importer.anisoLevel = 0;

            var defaultSettings = importer.GetDefaultPlatformTextureSettings();
            defaultSettings.overridden = true;
            defaultSettings.allowsAlphaSplitting = false;
            defaultSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality16Bit;
            defaultSettings.maxTextureSize = maxTextureSize;
            defaultSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
            defaultSettings.format = options.Format.ToImporterFormat();
            defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
            defaultSettings.compressionQuality = 100;
            importer.SetPlatformTextureSettings(defaultSettings);

            // List of all platforms
            // https://docs.unity3d.com/ScriptReference/Build.NamedBuildTarget.html
            // https://docs.unity3d.com/ScriptReference/TextureImporter.GetPlatformTextureSettings.html
            NamedBuildTarget[] buildTargets = {
                NamedBuildTarget.Standalone,
                NamedBuildTarget.iOS,
                NamedBuildTarget.Android,
                NamedBuildTarget.WebGL,
                NamedBuildTarget.WindowsStoreApps,
                NamedBuildTarget.PS4,
                NamedBuildTarget.XboxOne,
                NamedBuildTarget.tvOS,
                NamedBuildTarget.VisionOS,
                NamedBuildTarget.NintendoSwitch,
            };
            var platforms = buildTargets.Select(target => target.TargetName);
            foreach (var platformName in platforms)
            {
                var settings = importer.GetPlatformTextureSettings(platformName);
                // Copy default settings
                defaultSettings.CopyTo(settings);
                settings.name = platformName;
                importer.SetPlatformTextureSettings(settings);
                // Debug.Log($"Set {platformName} settings");
            }

            importer.SaveAndReimport();
        }

        private static TextureImporterFormat ToImporterFormat(this TextureFormat format)
        {
            // Covers common formats, fallback to automatic
            return format switch
            {
                TextureFormat.Alpha8 => TextureImporterFormat.Alpha8,
                TextureFormat.ARGB4444 => TextureImporterFormat.ARGB16,
                TextureFormat.ARGB32 => TextureImporterFormat.ARGB32,
                TextureFormat.RGB24 => TextureImporterFormat.RGB24,
                TextureFormat.RGB565 => TextureImporterFormat.RGB16,
                TextureFormat.RG16 => TextureImporterFormat.RG16,
                TextureFormat.R8 => TextureImporterFormat.R8,
                TextureFormat.RGBA32 => TextureImporterFormat.RGBA32,
                TextureFormat.RGBA64 => TextureImporterFormat.RGBA64,
                TextureFormat.RGBA4444 => TextureImporterFormat.RGBA16,
                _ => TextureImporterFormat.Automatic,
            };
        }

        private static bool IsProjectPath(string path)
        {
            return Path.GetFullPath(path).StartsWith(Application.dataPath);
        }
    }
}
