using System;
using System.Collections.Generic;
using Stilargo.Editor.Rendering;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.World
{
    /// <summary>
    /// Turns a Blender-built house FBX into a prefab: each material slot the Blender script names is remapped to an
    /// HDRP material in the house's own folder. Textured looks get their texture set projected in world space so
    /// surfaces are not flat colour; everything else is a plain placeholder that can be swapped for an authored material.
    /// </summary>
    public sealed class BlenderHouseAssets
    {
        private const string WorldRoot = "Assets/_Project/World";

        private readonly string _name;
        private readonly string _modelPath;
        private readonly string _blenderScript;
        private readonly IReadOnlyDictionary<string, HouseMaterialLook> _looks;
        private readonly string _materialFolder;

        /// <param name="materialFolder">
        /// Folder under <c>World</c> for the materials, shared by models that use the same slot names; null keeps them
        /// next to the prefab.
        /// </param>
        public BlenderHouseAssets(string name, string blenderScript, IReadOnlyDictionary<string, HouseMaterialLook> looks,
            string materialFolder = null)
        {
            _name = name;
            _materialFolder = materialFolder;
            _modelPath = $"Assets/_Project/Art/Models/{name}/{name}.fbx";
            _blenderScript = blenderScript;
            _looks = looks;
        }

        private string Root => $"{WorldRoot}/{_name}";

        private string MaterialRoot => _materialFolder == null ? Root : $"{WorldRoot}/{_materialFolder}";

        public GameObject LoadOrCreatePrefab()
        {
            EnsureFolder();
            RemapMaterials();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(_modelPath);
            if (model == null)
            {
                throw MissingModel();
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.name = _name;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, $"{Root}/{_name}.prefab");
            UnityEngine.Object.DestroyImmediate(instance);
            return prefab;
        }

        private void RemapMaterials()
        {
            if (!(AssetImporter.GetAtPath(_modelPath) is ModelImporter importer))
            {
                throw MissingModel();
            }

            var textureSets = new Dictionary<string, SurfaceTextures>();
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.addCollider = false;
            foreach (KeyValuePair<string, HouseMaterialLook> look in _looks)
            {
                Material material = LoadOrCreateMaterial(look.Key, look.Value);
                if (look.Value.IsTextured)
                {
                    HdrpMaterials.ApplyTriplanarTextures(material, LoadCached(textureSets, look.Value.TextureSet), look.Value.MetresPerTile);
                }

                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), look.Key), material);
            }

            importer.SaveAndReimport();
        }

        private static SurfaceTextures LoadCached(Dictionary<string, SurfaceTextures> loaded, string name)
        {
            if (!loaded.TryGetValue(name, out SurfaceTextures textures))
            {
                textures = SurfaceTextures.Load(name);
                loaded.Add(name, textures);
            }

            return textures;
        }

        private Material LoadOrCreateMaterial(string name, HouseMaterialLook look)
        {
            string path = $"{MaterialRoot}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = HdrpMaterials.CreateLit(look.Color);
                AssetDatabase.CreateAsset(material, path);
            }

            HdrpMaterials.SetLitColor(material, look.Color);
            HdrpMaterials.SetSmoothness(material, look.Smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private void EnsureFolder()
        {
            EnsureWorldFolder(_name);
            if (_materialFolder != null)
            {
                EnsureWorldFolder(_materialFolder);
            }
        }

        private static void EnsureWorldFolder(string folder)
        {
            if (!AssetDatabase.IsValidFolder($"{WorldRoot}/{folder}"))
            {
                AssetDatabase.CreateFolder(WorldRoot, folder);
            }
        }

        private InvalidOperationException MissingModel()
        {
            return new InvalidOperationException($"No model at {_modelPath}. Run {_blenderScript} in Blender.");
        }
    }
}
