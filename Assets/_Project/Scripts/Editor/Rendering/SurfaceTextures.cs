using System;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.Rendering
{
    /// <summary>
    /// A PBR texture set under <c>Assets/_Project/Art/Textures/&lt;name&gt;/</c>: <c>&lt;name&gt;_albedo.jpg</c>,
    /// <c>&lt;name&gt;_normal.jpg</c> (OpenGL convention) and <c>&lt;name&gt;_mask.png</c> (HDRP mask map, see
    /// <c>Tools/Art/pack_mask_maps.py</c>). Loading fixes the import settings HDRP needs.
    /// </summary>
    public sealed class SurfaceTextures
    {
        private const string Root = "Assets/_Project/Art/Textures";

        private SurfaceTextures(Texture2D albedo, Texture2D normal, Texture2D mask)
        {
            Albedo = albedo;
            Normal = normal;
            Mask = mask;
        }

        public Texture2D Albedo { get; }

        public Texture2D Normal { get; }

        public Texture2D Mask { get; }

        public static SurfaceTextures Load(string name)
        {
            string folder = $"{Root}/{name}";
            Texture2D albedo = LoadColour($"{folder}/{name}_albedo.jpg");
            Texture2D normal = LoadNormal($"{folder}/{name}_normal.jpg");
            Texture2D mask = LoadLinear($"{folder}/{name}_mask.png");
            return new SurfaceTextures(albedo, normal, mask);
        }

        /// <summary>Loads <c>&lt;name&gt;_albedo.png</c>, <c>_normal.png</c> and <c>_mask.png</c> from any folder, such as a character's textures.</summary>
        public static SurfaceTextures LoadPng(string folder, string name)
        {
            Texture2D albedo = LoadColour($"{folder}/{name}_albedo.png");
            Texture2D normal = LoadNormal($"{folder}/{name}_normal.png");
            Texture2D mask = LoadLinear($"{folder}/{name}_mask.png");
            return new SurfaceTextures(albedo, normal, mask);
        }

        private static Texture2D LoadColour(string path)
        {
            TextureImporter importer = RequireImporter(path);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            return Reimport(importer, path);
        }

        private static Texture2D LoadNormal(string path)
        {
            TextureImporter importer = RequireImporter(path);
            importer.textureType = TextureImporterType.NormalMap;
            return Reimport(importer, path);
        }

        private static Texture2D LoadLinear(string path)
        {
            TextureImporter importer = RequireImporter(path);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            return Reimport(importer, path);
        }

        private static TextureImporter RequireImporter(string path)
        {
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                return importer;
            }

            throw new InvalidOperationException($"No texture at {path}. Download it and run Tools/Art/pack_mask_maps.py.");
        }

        private static Texture2D Reimport(TextureImporter importer, string path)
        {
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
