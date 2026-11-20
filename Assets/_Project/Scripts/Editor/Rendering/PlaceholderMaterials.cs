using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.Rendering
{
    /// <summary>
    /// Loads a generated placeholder material, or creates it through <see cref="HdrpMaterials"/> on first use.
    /// An existing asset is never overwritten, so authored PBR settings put into it survive rebuilds and its GUID stays put.
    /// </summary>
    public static class PlaceholderMaterials
    {
        public static Material LoadOrCreateLit(string path, Color albedo)
        {
            return LoadOrCreate(path, () => HdrpMaterials.CreateLit(albedo));
        }

        public static Material LoadOrCreateUnlit(string path, Color color)
        {
            return LoadOrCreate(path, () => HdrpMaterials.CreateUnlit(color));
        }

        public static Material LoadOrCreateTransparentUnlit(string path)
        {
            return LoadOrCreate(path, HdrpMaterials.CreateTransparentUnlit);
        }

        private static Material LoadOrCreate(string path, Func<Material> create)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Material material = create();
            AssetFolders.Ensure(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
