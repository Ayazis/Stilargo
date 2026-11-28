using System.Collections.Generic;
using UnityEngine;

namespace Stilargo.Editor.World
{
    /// <summary>
    /// The Blender-built House 1 (<c>Tools/Art/build_house_1.py</c>, from <c>ConceptArt/CanalTown/House_1.png</c>) as a
    /// prefab, with each of the script's material slots remapped to an HDRP material.
    /// </summary>
    public static class House1Assets
    {
        // Poly Haven CC0 scans; tile sizes are the real-world sizes Poly Haven lists for them, except brick_4: it is
        // listed at 0.5 m, but a tile holds ~14 courses, so 1.05 m gives real 7.5 cm courses.
        private const string BrickTextures = "brick_4";
        private const float BrickMetresPerTile = 1.05f;
        private const string StoneTextures = "beige_wall_001";
        private const float StoneMetresPerTile = 2f;
        private const string RoofTextures = "clay_roof_tiles_02";
        private const float RoofMetresPerTile = 2.5f;
        private const float RoofTileDepthCentimetres = 4f;
        private const string PlantTextures = "periwinkle_plant";
        private const string RedPlantTextures = "periwinkle_plant_red";
        private const string ShutterTextures = "green_rough_planks";
        private const float ShutterMetresPerTile = 1.5f;
        private const string DoorTextures = "weathered_planks";
        private const float DoorMetresPerTile = 2f;
        private const string BoxTextures = "worn_planks";
        private const float BoxMetresPerTile = 1.4f;

        private static readonly Dictionary<string, HouseMaterialLook> Looks = new Dictionary<string, HouseMaterialLook>
        {
            { "Brick", HouseMaterialLook.Scanned(new Color(1f, 0.97f, 0.94f), BrickTextures, BrickMetresPerTile).Worn() },
            { "Mortar", HouseMaterialLook.Plastered(new Color(0.6f, 0.55f, 0.47f), 0.1f) },
            { "Stone", HouseMaterialLook.Scanned(new Color(0.95f, 0.93f, 0.88f), StoneTextures, StoneMetresPerTile).Worn() },
            { "StoneDark", HouseMaterialLook.Scanned(new Color(0.84f, 0.8f, 0.72f), StoneTextures, StoneMetresPerTile).Worn() },
            { "Moss", HouseMaterialLook.Plastered(new Color(0.3f, 0.32f, 0.17f), 0.1f) },
            { "RoofTileC", HouseMaterialLook.Weathered(new Color(0.5f, 0.2f, 0.11f), 0.25f) },
            { "RoofUnder", HouseMaterialLook.Painted(new Color(0.12f, 0.08f, 0.06f), 0.1f) },
            { "Zinc", HouseMaterialLook.Weathered(new Color(0.55f, 0.57f, 0.58f), 0.55f) },
            { "Glass", HouseMaterialLook.Painted(new Color(0.04f, 0.05f, 0.06f), 0.92f) },
            { "Frame", HouseMaterialLook.Weathered(new Color(0.78f, 0.75f, 0.68f), 0.35f) },
            { "Shutter", HouseMaterialLook.Scanned(new Color(1f, 0.92f, 0.55f), ShutterTextures, ShutterMetresPerTile).Worn() },
            { "Curtain", HouseMaterialLook.Painted(new Color(0.75f, 0.72f, 0.64f), 0.1f) },
            { "Wood", HouseMaterialLook.Scanned(new Color(0.75f, 0.7f, 0.65f), DoorTextures, DoorMetresPerTile).Worn() },
            { "Iron", HouseMaterialLook.Painted(new Color(0.05f, 0.05f, 0.05f), 0.45f) },
            { "Terracotta", HouseMaterialLook.Weathered(new Color(0.55f, 0.32f, 0.18f), 0.2f) },
            { "Plaque", HouseMaterialLook.Painted(new Color(0.8f, 0.79f, 0.75f), 0.4f) },
            { "LampGlow", HouseMaterialLook.Painted(new Color(0.9f, 0.7f, 0.4f), 0.8f) },
            { "StoneGrime", HouseMaterialLook.Scanned(new Color(0.7f, 0.65f, 0.56f), StoneTextures, StoneMetresPerTile).Worn() },
            { "ShutterFaded", HouseMaterialLook.Scanned(new Color(1f, 1f, 0.72f), ShutterTextures, ShutterMetresPerTile).Worn() },
            { "BoxWood", HouseMaterialLook.Scanned(new Color(0.9f, 0.82f, 0.72f), BoxTextures, BoxMetresPerTile).Worn() },
            { "PlantPink", HouseMaterialLook.ScannedFoliage(PlantTextures) },
            { "PlantRed", HouseMaterialLook.ScannedFoliage(RedPlantTextures) },
            { "RoofTiles", HouseMaterialLook.ScannedRelief(new Color(1f, 0.95f, 0.9f), RoofTextures, RoofMetresPerTile, RoofTileDepthCentimetres) },
        };

        public static GameObject LoadOrCreatePrefab()
        {
            return new BlenderHouseAssets("House1", "Tools/Art/build_house_1.py", Looks).LoadOrCreatePrefab();
        }
    }
}
