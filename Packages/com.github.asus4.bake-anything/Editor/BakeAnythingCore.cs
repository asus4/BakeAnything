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

        public static Texture2D BakeToAsset(
            IBakable bakable,
            string path,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            FilterMode filterMode = FilterMode.Point)
        {
            string textureName = Path.GetFileNameWithoutExtension(path);
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null)
            {
                // Create new asset at the path
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

        public static void ExportToEXR(IBakable bakable, string path)
        {
            var texture = BakeToTexture(bakable);
            byte[] bytes = texture.EncodeToEXR();
            File.WriteAllBytes(path, bytes);

            // Override texture importer settings if it's a inside of the project
            bool isProjectPath = Path.GetFullPath(path).StartsWith(Application.dataPath);
            if (isProjectPath)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.IsNotNull(importer);
                int maxSize = Mathf.NextPowerOfTwo(Mathf.Max(texture.width, texture.height));
                ModifyImporterSetting(importer, maxSize);
            }
        }

        public static void ModifyImporterSetting(
            TextureImporter importer,
            int maxTextureSize = 8192,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            FilterMode filterMode = FilterMode.Point)
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

            importer.wrapMode = wrapMode;
            importer.filterMode = filterMode;
            importer.anisoLevel = 0;

            var defaultSettings = importer.GetDefaultPlatformTextureSettings();
            defaultSettings.overridden = true;
            defaultSettings.allowsAlphaSplitting = false;
            defaultSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality16Bit;
            defaultSettings.maxTextureSize = maxTextureSize;
            defaultSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
            defaultSettings.format = TextureImporterFormat.RGBAHalf;
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
    }
}
