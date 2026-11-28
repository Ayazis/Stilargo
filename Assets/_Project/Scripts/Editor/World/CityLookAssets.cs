using System;
using Stilargo.Editor.Rendering;
using Stilargo.Gameplay.City;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.World
{
    /// <summary>
    /// Builds the placeholder materials the generated city's ground and open spaces are dressed in and wires them
    /// into a <see cref="CitySpawner"/>. Buildings, quays and bridges carry their own materials
    /// (<see cref="CanalTownModelAssets"/>). Materials are rewritten on every run so colour changes here reach the
    /// existing assets.
    /// </summary>
    public static class CityLookAssets
    {
        private const string Root = "Assets/_Project/World";
        private const string PlasterTextures = "white_plaster_rough_01";
        private const string CobbleTextures = "cobblestone_pavement";
        private const float PlasterMetresPerTile = 3f;
        private const float CobbleMetresPerTile = 3f;
        private const float PaintSmoothness = 0.4f;

        private static readonly Color GroundTint = new Color(0.95f, 0.9f, 0.82f);
        private static readonly Color WoodColor = new Color(0.36f, 0.23f, 0.14f);

        // Open spaces: weathered limestone, Mediterranean greens and dark water.
        private const float WaterSmoothness = 0.95f;
        private static readonly Color StoneColor = new Color(0.74f, 0.7f, 0.62f);
        private static readonly Color GrassColor = new Color(0.3f, 0.42f, 0.19f);
        private static readonly Color WaterColor = new Color(0.07f, 0.17f, 0.19f);

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

            AssignLandmarkMaterials(spawner, SurfaceTextures.Load(PlasterTextures));
        }

        /// <summary>Placeholder landmark materials; each can be swapped for an authored PBR material on the prefab.</summary>
        private static void AssignLandmarkMaterials(CitySpawner spawner, SurfaceTextures plaster)
        {
            const string Field = "_landmarkMaterials.";
            SerializedFields.Assign(spawner, Field + "_stone", PlasteredMaterial("LandmarkStone", StoneColor, plaster));
            SerializedFields.Assign(spawner, Field + "_grass", LoadOrCreateMaterial("LandmarkGrass", GrassColor));
            SerializedFields.Assign(spawner, Field + "_water", KitMaterial("Water", WaterColor, WaterSmoothness));
            SerializedFields.Assign(spawner, Field + "_wood", KitMaterial("Wood", WoodColor, PaintSmoothness));
        }

        private static Material PlasteredMaterial(string name, Color color, SurfaceTextures plaster)
        {
            Material material = LoadOrCreateMaterial(name, color);
            HdrpMaterials.ApplyTriplanarTextures(material, plaster, PlasterMetresPerTile);
            return material;
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

    }
}
