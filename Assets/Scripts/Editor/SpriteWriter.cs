using System.IO;
using UnityEditor;
using UnityEngine;

namespace Runner.EditorTools
{
    /// <summary>
    /// Gets a generated canvas onto disk as a pixel-perfect single sprite, with the import
    /// settings that 2D pixel art actually needs (point filtering, no compression, shared PPU).
    /// </summary>
    internal static class SpriteWriter
    {
        public const string ArtRoot = "Assets/Art";
        public const string SpriteRoot = ArtRoot + "/Sprites";
        public const float PixelsPerUnit = 32f;

        public static Sprite Save(string relativeFolder, string name, PixelCanvas canvas, Vector2 pivot)
        {
            return Save(relativeFolder, name, canvas, pivot, Vector4.zero);
        }

        public static Sprite Save(string relativeFolder, string name, PixelCanvas canvas, Vector2 pivot, Vector4 border)
        {
            var folder = $"{SpriteRoot}/{relativeFolder}";
            EnsureFolder(folder);

            var path = $"{folder}/{name}.png";
            var texture = canvas.ToTexture();

            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            ApplySettings(path, pivot, border);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        public static void ApplySettings(string path, Vector2 pivot, Vector4 border)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.spritePivot = pivot;
            importer.spriteBorder = border;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.sRGBTexture = true;
            importer.maxTextureSize = 2048;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var leaf = Path.GetFileName(path);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            if (string.IsNullOrEmpty(parent)) parent = "Assets";

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
