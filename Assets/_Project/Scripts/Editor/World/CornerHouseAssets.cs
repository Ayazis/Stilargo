using System.Collections.Generic;
using UnityEngine;

namespace Stilargo.Editor.World
{
    /// <summary>
    /// The Blender-built corner house (<c>Tools/Art/build_corner_house.py</c>) as a prefab, with each of the script's
    /// material slots remapped to an HDRP material.
    /// </summary>
    public static class CornerHouseAssets
    {
        private static readonly Dictionary<string, HouseMaterialLook> Looks = new Dictionary<string, HouseMaterialLook>
        {
            { "Plaster", HouseMaterialLook.Plastered(new Color(0.66f, 0.48f, 0.27f), 0.2f) },
            { "Mortar", HouseMaterialLook.Plastered(new Color(0.55f, 0.5f, 0.42f), 0.1f) },
            { "BrickA", HouseMaterialLook.Painted(new Color(0.46f, 0.22f, 0.14f), 0.15f) },
            { "BrickB", HouseMaterialLook.Painted(new Color(0.54f, 0.28f, 0.17f), 0.15f) },
            { "BrickC", HouseMaterialLook.Painted(new Color(0.38f, 0.19f, 0.14f), 0.15f) },
            { "Stone", HouseMaterialLook.Plastered(new Color(0.72f, 0.67f, 0.57f), 0.2f) },
            { "RoofTileA", HouseMaterialLook.Painted(new Color(0.52f, 0.22f, 0.12f), 0.25f) },
            { "RoofTileB", HouseMaterialLook.Painted(new Color(0.6f, 0.28f, 0.15f), 0.25f) },
            { "RoofTileC", HouseMaterialLook.Painted(new Color(0.43f, 0.2f, 0.12f), 0.25f) },
            { "RoofUnder", HouseMaterialLook.Painted(new Color(0.12f, 0.08f, 0.06f), 0.1f) },
            { "Zinc", HouseMaterialLook.Painted(new Color(0.55f, 0.57f, 0.58f), 0.55f) },
            { "Glass", HouseMaterialLook.Painted(new Color(0.05f, 0.06f, 0.07f), 0.92f) },
            { "Frame", HouseMaterialLook.Painted(new Color(0.78f, 0.75f, 0.68f), 0.35f) },
            { "Shutter", HouseMaterialLook.Painted(new Color(0.12f, 0.25f, 0.16f), 0.35f) },
            { "Curtain", HouseMaterialLook.Painted(new Color(0.62f, 0.36f, 0.16f), 0.1f) },
            { "Wood", HouseMaterialLook.Painted(new Color(0.3f, 0.19f, 0.11f), 0.3f) },
            { "Iron", HouseMaterialLook.Painted(new Color(0.07f, 0.07f, 0.07f), 0.45f) },
            { "Terracotta", HouseMaterialLook.Painted(new Color(0.56f, 0.28f, 0.16f), 0.2f) },
            { "Flower", HouseMaterialLook.Painted(new Color(0.68f, 0.1f, 0.1f), 0.3f) },
            { "Leaf", HouseMaterialLook.Painted(new Color(0.14f, 0.27f, 0.09f), 0.3f) },
        };

        public static GameObject LoadOrCreatePrefab()
        {
            return new BlenderHouseAssets("CornerHouse", "Tools/Art/build_corner_house.py", Looks).LoadOrCreatePrefab();
        }
    }
}
