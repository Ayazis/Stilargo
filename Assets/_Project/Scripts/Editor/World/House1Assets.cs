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
        // Poly Haven CC0 scans; tile sizes are the real-world sizes Poly Haven lists for them.
        private const string BrickTextures = "red_brick";
        private const float BrickMetresPerTile = 1.4f;
        private const string StoneTextures = "beige_wall_001";
        private const float StoneMetresPerTile = 2f;

        private static readonly Dictionary<string, HouseMaterialLook> Looks = new Dictionary<string, HouseMaterialLook>
        {
            { "Brick", HouseMaterialLook.Scanned(new Color(0.9f, 0.88f, 0.86f), BrickTextures, BrickMetresPerTile) },
            { "Mortar", HouseMaterialLook.Plastered(new Color(0.6f, 0.55f, 0.47f), 0.1f) },
            { "Stone", HouseMaterialLook.Scanned(new Color(0.95f, 0.93f, 0.88f), StoneTextures, StoneMetresPerTile) },
            { "StoneDark", HouseMaterialLook.Scanned(new Color(0.84f, 0.8f, 0.72f), StoneTextures, StoneMetresPerTile) },
            { "Moss", HouseMaterialLook.Plastered(new Color(0.3f, 0.32f, 0.17f), 0.1f) },
            { "RoofTileA", HouseMaterialLook.Weathered(new Color(0.6f, 0.24f, 0.12f), 0.25f) },
            { "RoofTileB", HouseMaterialLook.Weathered(new Color(0.68f, 0.32f, 0.17f), 0.25f) },
            { "RoofTileC", HouseMaterialLook.Weathered(new Color(0.5f, 0.2f, 0.11f), 0.25f) },
            { "RoofUnder", HouseMaterialLook.Painted(new Color(0.12f, 0.08f, 0.06f), 0.1f) },
            { "Zinc", HouseMaterialLook.Weathered(new Color(0.55f, 0.57f, 0.58f), 0.55f) },
            { "Glass", HouseMaterialLook.Painted(new Color(0.04f, 0.05f, 0.06f), 0.92f) },
            { "Frame", HouseMaterialLook.Weathered(new Color(0.78f, 0.75f, 0.68f), 0.35f) },
            { "Shutter", HouseMaterialLook.Weathered(new Color(0.27f, 0.3f, 0.14f), 0.35f) },
            { "Curtain", HouseMaterialLook.Painted(new Color(0.75f, 0.72f, 0.64f), 0.1f) },
            { "Wood", HouseMaterialLook.Weathered(new Color(0.16f, 0.09f, 0.05f), 0.3f) },
            { "Iron", HouseMaterialLook.Painted(new Color(0.05f, 0.05f, 0.05f), 0.45f) },
            { "Terracotta", HouseMaterialLook.Weathered(new Color(0.55f, 0.32f, 0.18f), 0.2f) },
            { "FlowerRed", HouseMaterialLook.Painted(new Color(0.7f, 0.04f, 0.04f), 0.3f) },
            { "FlowerPink", HouseMaterialLook.Painted(new Color(0.8f, 0.35f, 0.45f), 0.3f) },
            { "FlowerWhite", HouseMaterialLook.Painted(new Color(0.8f, 0.8f, 0.76f), 0.3f) },
            { "Leaf", HouseMaterialLook.Painted(new Color(0.12f, 0.25f, 0.07f), 0.3f) },
            { "Plaque", HouseMaterialLook.Painted(new Color(0.8f, 0.79f, 0.75f), 0.4f) },
            { "LampGlow", HouseMaterialLook.Painted(new Color(0.9f, 0.7f, 0.4f), 0.8f) },
            { "StoneGrime", HouseMaterialLook.Scanned(new Color(0.7f, 0.65f, 0.56f), StoneTextures, StoneMetresPerTile) },
            { "TileLichen", HouseMaterialLook.Weathered(new Color(0.52f, 0.45f, 0.33f), 0.15f) },
            { "TileDark", HouseMaterialLook.Weathered(new Color(0.36f, 0.14f, 0.08f), 0.2f) },
            { "ShutterFaded", HouseMaterialLook.Weathered(new Color(0.38f, 0.4f, 0.24f), 0.25f) },
        };

        public static GameObject LoadOrCreatePrefab()
        {
            return new BlenderHouseAssets("House1", "Tools/Art/build_house_1.py", Looks).LoadOrCreatePrefab();
        }
    }
}
