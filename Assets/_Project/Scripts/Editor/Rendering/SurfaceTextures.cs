using System;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.Rendering
{
    /// <summary>
    /// A PBR texture set under <c>Assets/_Project/Art/Textures/&lt;name&gt;/</c>: <c>&lt;name&gt;_albedo.jpg</c>,
    /// <c>&lt;name&gt;_normal.jpg</c> (OpenGL convention) and <c>&lt;name&gt;_mask.png</c> (HDRP mask map, see
    /// <c>Tools/Art/pack_mask_maps.py</c>), plus an optional <c>&lt;name&gt;_height.jpg</c> for parallax. Loading fixes the
    /// import settings HDRP needs.
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

        private SurfaceTextures(SurfaceTextures maps, Texture2D height)
            : this(maps.Albedo, maps.Normal, maps.Mask)
        {
            Height = height;
        }

        public Texture2D Albedo { get; }

        public Texture2D Normal { get; }

        public Texture2D Mask { get; }

        /// <summary>Height map for parallax, or null when the set has none.</summary>
        public Texture2D Height { get; }

        public bool HasHeight => Height != null;

        public static SurfaceTextures Load(string name)
        {
            string folder = $"{Root}/{name}";
            Texture2D albedo = LoadColour($"{folder}/{name}_albedo.jpg");
            Texture2D normal = LoadNormal($"{folder}/{name}_normal.jpg");
            Texture2D mask = LoadLinear($"{folder}/{name}_mask.png");
            var maps = new SurfaceTextures(albedo, normal, mask);
            string heightPath = $"{folder}/{name}_height.jpg";
            if (AssetImporter.GetAtPath(heightPath) == null)
            {
                return maps;
            }

            return new SurfaceTextures(maps, LoadLinear(heightPath));
        }

        /// <summary>Loads an HDRP detail map, <c>&lt;name&gt;/&lt;name&gt;.png</c> under the texture root, as linear data.</summary>
        public static Texture2D LoadDetailMap(string name)
        {
            return LoadLinear($"{Root}/{name}/{name}.png");
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
