using System;
using Stilargo.Editor.Rendering;
using Stilargo.Gameplay.City;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.World
{
    /// <summary>
    /// Builds the assets that give the generated city its Mediterranean look and wires them into a
    /// <see cref="CitySpawner"/>: CC0 textured plaster, terracotta and cobble materials, the roof prefab and
    /// the facade kit made by <c>Tools/Art/build_facade_kit.py</c>. Materials are rewritten on every run so
    /// palette changes here reach the existing assets.
    /// </summary>
    public static class CityLookAssets
    {
        private const string Root = "Assets/_Project/World";
        private const string KitRoot = "Assets/_Project/Art/Models/FacadeKit";

        private const string PlasterTextures = "white_plaster_rough_01";
        private const string RoofTextures = "clay_roof_tiles";
        private const string CobbleTextures = "cobblestone_pavement";
        private const float PlasterMetresPerTile = 3f;
        private const float RoofMetresPerTile = 2.5f;
        private const float CobbleMetresPerTile = 3f;

        private const float TrimSmoothness = 0.3f;
        private const float GlassSmoothness = 0.92f;
        private const float PaintSmoothness = 0.4f;

        // Lime plaster in earth pigments. The plaster albedo is near white, so these tint it directly.
        private static readonly Color[] WallColors =
        {
            new Color(0.82f, 0.64f, 0.4f),
            new Color(0.88f, 0.78f, 0.52f),
            new Color(0.7f, 0.36f, 0.26f),
            new Color(0.84f, 0.58f, 0.48f),
            new Color(0.88f, 0.83f, 0.7f),
            new Color(0.6f, 0.65f, 0.68f),
        };

        // The tile texture is already terracotta; these only shift each roof's age and tone.
        private static readonly Color[] RoofTints =
        {
            new Color(1f, 0.95f, 0.9f),
            new Color(0.88f, 0.8f, 0.75f),
            new Color(1f, 0.88f, 0.75f),
        };

        private static readonly Color GroundTint = new Color(0.95f, 0.9f, 0.82f);
        private static readonly Color TrimColor = new Color(0.8f, 0.76f, 0.67f);
        private static readonly Color GlassColor = new Color(0.06f, 0.08f, 0.09f);
        private static readonly Color ShutterColor = new Color(0.25f, 0.42f, 0.33f);
        private static readonly Color WoodColor = new Color(0.36f, 0.23f, 0.14f);

        // Landmarks: weathered limestone, an ochre school, a terracotta tavern, Mediterranean greens and a dark pond.
        private const float WaterSmoothness = 0.95f;
        private static readonly Color StoneColor = new Color(0.74f, 0.7f, 0.62f);
        private static readonly Color SchoolWallColor = new Color(0.86f, 0.66f, 0.32f);
        private static readonly Color TavernWallColor = new Color(0.64f, 0.33f, 0.24f);
        private static readonly Color GrassColor = new Color(0.3f, 0.42f, 0.19f);
        private static readonly Color FoliageColor = new Color(0.19f, 0.31f, 0.15f);
        private static readonly Color BarkColor = new Color(0.3f, 0.22f, 0.16f);
        private static readonly Color WaterColor = new Color(0.07f, 0.17f, 0.19f);
        private static readonly Color ClothColor = new Color(0.62f, 0.2f, 0.15f);
        private static readonly Color ShadowColor = new Color(0.05f, 0.05f, 0.055f);

        public static Material GroundMaterial()
        {
            Material material = LoadOrCreateMaterial("Ground", GroundTint);
            HdrpMaterials.ApplyTriplanarTextures(material, SurfaceTextures.Load(CobbleTextures), CobbleMetresPerTile);
            return material;
        }

        public static void Dress(CitySpawner spawner)
        {
            if (spawner == null)
            {
                throw new ArgumentNullException(nameof(spawner));
            }

            SurfaceTextures plaster = SurfaceTextures.Load(PlasterTextures);
            SurfaceTextures tiles = SurfaceTextures.Load(RoofTextures);
            SerializedFields.AssignArray(spawner, "_wallMaterials", Palette("Wall", WallColors, m => HdrpMaterials.ApplyTriplanarTextures(m, plaster, PlasterMetresPerTile)));
            SerializedFields.AssignArray(spawner, "_roofMaterials", Palette("Roof", RoofTints, m => HdrpMaterials.ApplyMetricUvTextures(m, tiles, RoofMetresPerTile)));
            SerializedFields.Assign(spawner, "_roofPrefab", LoadOrCreateRoofPrefab());

            RemapKitMaterials();
            SerializedFields.Assign(spawner, "_windowPrefab", LoadOrCreateKitPrefab("Window"));
            SerializedFields.Assign(spawner, "_doorPrefab", LoadOrCreateKitPrefab("Door"));
            SerializedFields.Assign(spawner, "_cornicePrefab", LoadOrCreateKitPrefab("Cornice"));
            AssignLandmarkMaterials(spawner, plaster);
        }

        /// <summary>Placeholder landmark materials; each can be swapped for an authored PBR material on the prefab.</summary>
        private static void AssignLandmarkMaterials(CitySpawner spawner, SurfaceTextures plaster)
        {
            const string Field = "_landmarkMaterials.";
            SerializedFields.Assign(spawner, Field + "_stone", PlasteredMaterial("LandmarkStone", StoneColor, plaster));
            SerializedFields.Assign(spawner, Field + "_schoolWall", PlasteredMaterial("LandmarkSchoolWall", SchoolWallColor, plaster));
            SerializedFields.Assign(spawner, Field + "_tavernWall", PlasteredMaterial("LandmarkTavernWall", TavernWallColor, plaster));
            SerializedFields.Assign(spawner, Field + "_grass", LoadOrCreateMaterial("LandmarkGrass", GrassColor));
            SerializedFields.Assign(spawner, Field + "_foliage", LoadOrCreateMaterial("LandmarkFoliage", FoliageColor));
            SerializedFields.Assign(spawner, Field + "_bark", LoadOrCreateMaterial("LandmarkBark", BarkColor));
            SerializedFields.Assign(spawner, Field + "_water", KitMaterial("Water", WaterColor, WaterSmoothness));
            SerializedFields.Assign(spawner, Field + "_wood", KitMaterial("Wood", WoodColor, PaintSmoothness));
            SerializedFields.Assign(spawner, Field + "_cloth", LoadOrCreateMaterial("LandmarkCloth", ClothColor));
            SerializedFields.Assign(spawner, Field + "_shadow", LoadOrCreateMaterial("LandmarkShadow", ShadowColor));
        }

        private static Material PlasteredMaterial(string name, Color color, SurfaceTextures plaster)
        {
            Material material = LoadOrCreateMaterial(name, color);
            HdrpMaterials.ApplyTriplanarTextures(material, plaster, PlasterMetresPerTile);
            return material;
        }

        private static Material[] Palette(string name, Color[] colors, Action<Material> applyTextures)
        {
            var materials = new Material[colors.Length];
            for (int index = 0; index < colors.Length; index++)
            {
                materials[index] = LoadOrCreateMaterial($"{name}{index}", colors[index]);
                applyTextures(materials[index]);
            }

            return materials;
        }

        private static Material LoadOrCreateMaterial(string name, Color color)
        {
            string path = $"{Root}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = HdrpMaterials.CreateLit(color);
                AssetDatabase.CreateAsset(material, path);
            }

            HdrpMaterials.SetLitColor(material, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material KitMaterial(string name, Color color, float smoothness)
        {
            Material material = LoadOrCreateMaterial($"Kit{name}", color);
            HdrpMaterials.SetSmoothness(material, smoothness);
            return material;
        }

        private static GameObject LoadOrCreateRoofPrefab()
        {
            string path = $"{Root}/Roof.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            // The spawner builds each roof's mesh to fit its building, so the prefab carries no mesh.
            var roof = new GameObject("Roof", typeof(MeshFilter), typeof(MeshRenderer));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(roof, path);
            UnityEngine.Object.DestroyImmediate(roof);
            return prefab;
        }

        private static void RemapKitMaterials()
        {
            Material trim = KitMaterial("Trim", TrimColor, TrimSmoothness);
            Material glass = KitMaterial("Glass", GlassColor, GlassSmoothness);
            Material shutter = KitMaterial("Shutter", ShutterColor, PaintSmoothness);
            Material wood = KitMaterial("Wood", WoodColor, PaintSmoothness);
            foreach (string piece in new[] { "Window", "Door", "Cornice" })
            {
                ModelImporter importer = RequireModelImporter($"{KitRoot}/{piece}.fbx");
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                importer.addCollider = false;
                Remap(importer, "Trim", trim);
                Remap(importer, "Glass", glass);
                Remap(importer, "Shutter", shutter);
                Remap(importer, "Wood", wood);
                importer.SaveAndReimport();
            }
        }

        private static void Remap(ModelImporter importer, string sourceName, Material material)
        {
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), sourceName), material);
        }

        private static GameObject LoadOrCreateKitPrefab(string piece)
        {
            string path = $"{Root}/Kit{piece}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{KitRoot}/{piece}.fbx");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.name = $"Kit{piece}";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            UnityEngine.Object.DestroyImmediate(instance);
            return prefab;
        }

        private static ModelImporter RequireModelImporter(string path)
        {
            if (AssetImporter.GetAtPath(path) is ModelImporter importer)
            {
                return importer;
            }

            throw new InvalidOperationException($"No model at {path}. Run Tools/Art/build_facade_kit.py in Blender.");
        }
    }
}
