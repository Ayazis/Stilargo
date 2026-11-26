using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.World
{
    /// <summary>
    /// The Blender-built canal-town models (buildings, quays, bridge, pond pieces, boat, fountain, well, square, market, green-space, countryside and street pieces; <c>Tools/Art/build_*.py</c> on the
    /// shared <c>canal_kit.py</c>, see <c>docs/CanalTownArtList.md</c>) as prefabs. All of them use the kit's material slot
    /// names, remapped here to HDRP materials; keep this map in step with <c>MATERIALS</c> in <c>canal_kit.py</c>.
    /// </summary>
    public static class CanalTownModelAssets
    {
        // Poly Haven CC0 scans; tile sizes are the real-world sizes Poly Haven lists for them.
        private const string BrickTextures = "red_brick";
        private const float BrickMetresPerTile = 1.4f;
        private const string StoneTextures = "beige_wall_001";
        private const float StoneMetresPerTile = 2f;
        private const float RubbleMetresPerTile = 1.2f;
        private const string CobbleTextures = "cobblestone_pavement";
        private const float CobbleMetresPerTile = 2f;

        // Every model shares the kit's slot names, so one set of materials serves them all.
        private const string MaterialFolder = "CanalTownMaterials";

        /// <summary>Model name and the Blender script that builds it.</summary>
        private static readonly (string Name, string Script)[] Models =
        {
            ("RowHouse", "Tools/Art/build_row_house.py"),
            ("Townhouse", "Tools/Art/build_townhouse.py"),
            ("DetachedHome", "Tools/Art/build_detached_home.py"),
            ("Farmhouse", "Tools/Art/build_farmhouse.py"),
            ("Workshop", "Tools/Art/build_workshop.py"),
            ("Church", "Tools/Art/build_church.py"),
            ("School", "Tools/Art/build_school.py"),
            ("Tavern", "Tools/Art/build_tavern.py"),
            ("QuayWall", "Tools/Art/build_quay.py"),
            ("QuaySteps", "Tools/Art/build_quay.py"),
            ("Bridge", "Tools/Art/build_bridge.py"),
            ("PondEdge", "Tools/Art/build_pond.py"),
            ("PondCorner", "Tools/Art/build_pond.py"),
            ("Reeds", "Tools/Art/build_pond.py"),
            ("LilyPads", "Tools/Art/build_pond.py"),
            ("RowingBoat", "Tools/Art/build_rowing_boat.py"),
            ("Fountain", "Tools/Art/build_fountain.py"),
            ("Well", "Tools/Art/build_fountain.py"),
            ("SquareInlay", "Tools/Art/build_square_props.py"),
            ("ChurchForecourt", "Tools/Art/build_square_props.py"),
            ("StoneBench", "Tools/Art/build_square_props.py"),
            ("Planter", "Tools/Art/build_square_props.py"),
            ("CafeTerrace", "Tools/Art/build_square_props.py"),
            ("MarketStall", "Tools/Art/build_market.py"),
            ("MarketHall", "Tools/Art/build_market.py"),
            ("PlaneTree", "Tools/Art/build_trees.py"),
            ("UmbrellaPine", "Tools/Art/build_trees.py"),
            ("Willow", "Tools/Art/build_trees.py"),
            ("FruitTreeA", "Tools/Art/build_trees.py"),
            ("FruitTreeB", "Tools/Art/build_trees.py"),
            ("FruitTreeC", "Tools/Art/build_trees.py"),
            ("Shrub", "Tools/Art/build_trees.py"),
            ("Gazebo", "Tools/Art/build_garden_props.py"),
            ("Monument", "Tools/Art/build_garden_props.py"),
            ("HedgeSegment", "Tools/Art/build_garden_props.py"),
            ("GardenWall", "Tools/Art/build_garden_props.py"),
            ("VegetableBed", "Tools/Art/build_garden_props.py"),
            ("WashingLine", "Tools/Art/build_garden_props.py"),
            ("FarmCart", "Tools/Art/build_garden_props.py"),
            ("HayStack", "Tools/Art/build_garden_props.py"),
            ("WoodPile", "Tools/Art/build_garden_props.py"),
            ("WheatPatch", "Tools/Art/build_countryside.py"),
            ("VegetableRows", "Tools/Art/build_countryside.py"),
            ("WildflowerClump", "Tools/Art/build_countryside.py"),
            ("RockA", "Tools/Art/build_countryside.py"),
            ("RockB", "Tools/Art/build_countryside.py"),
            ("Sheep", "Tools/Art/build_countryside.py"),
            ("FenceSegment", "Tools/Art/build_countryside.py"),
            ("Milestone", "Tools/Art/build_countryside.py"),
            ("WaysideShrine", "Tools/Art/build_countryside.py"),
            ("TownSign", "Tools/Art/build_countryside.py"),
            ("StreetLamp", "Tools/Art/build_street_props.py"),
            ("ParkBench", "Tools/Art/build_street_props.py"),
            ("CrateStack", "Tools/Art/build_street_props.py"),
            ("BarrelGroup", "Tools/Art/build_street_props.py"),
            ("StreetPlanter", "Tools/Art/build_street_props.py"),
            ("Handcart", "Tools/Art/build_street_props.py"),
        };

        private static readonly Dictionary<string, HouseMaterialLook> Looks = new Dictionary<string, HouseMaterialLook>
        {
            { "PlasterOchre", HouseMaterialLook.Plastered(new Color(0.72f, 0.52f, 0.3f), 0.15f) },
            { "PlasterCream", HouseMaterialLook.Plastered(new Color(0.78f, 0.7f, 0.56f), 0.15f) },
            { "PlasterRose", HouseMaterialLook.Plastered(new Color(0.74f, 0.5f, 0.42f), 0.15f) },
            { "PlasterTerracotta", HouseMaterialLook.Plastered(new Color(0.66f, 0.36f, 0.22f), 0.15f) },
            { "PlasterSand", HouseMaterialLook.Plastered(new Color(0.76f, 0.64f, 0.46f), 0.15f) },
            { "Ashlar", HouseMaterialLook.Scanned(new Color(0.9f, 0.86f, 0.78f), StoneTextures, StoneMetresPerTile) },
            { "Rubble", HouseMaterialLook.Scanned(new Color(0.7f, 0.64f, 0.55f), StoneTextures, RubbleMetresPerTile) },
            { "Brick", HouseMaterialLook.Scanned(new Color(0.9f, 0.88f, 0.86f), BrickTextures, BrickMetresPerTile) },
            { "Mortar", HouseMaterialLook.Plastered(new Color(0.6f, 0.55f, 0.47f), 0.1f) },
            { "Stone", HouseMaterialLook.Scanned(new Color(0.95f, 0.93f, 0.88f), StoneTextures, StoneMetresPerTile) },
            { "StoneDark", HouseMaterialLook.Scanned(new Color(0.84f, 0.8f, 0.72f), StoneTextures, StoneMetresPerTile) },
            { "StoneGrime", HouseMaterialLook.Scanned(new Color(0.7f, 0.65f, 0.56f), StoneTextures, StoneMetresPerTile) },
            { "Moss", HouseMaterialLook.Plastered(new Color(0.3f, 0.32f, 0.17f), 0.1f) },
            { "RoofTileA", HouseMaterialLook.Weathered(new Color(0.6f, 0.24f, 0.12f), 0.25f) },
            { "RoofTileB", HouseMaterialLook.Weathered(new Color(0.68f, 0.32f, 0.17f), 0.25f) },
            { "RoofTileC", HouseMaterialLook.Weathered(new Color(0.5f, 0.2f, 0.11f), 0.25f) },
            { "TileDark", HouseMaterialLook.Weathered(new Color(0.36f, 0.14f, 0.08f), 0.2f) },
            { "TileLichen", HouseMaterialLook.Weathered(new Color(0.52f, 0.45f, 0.33f), 0.15f) },
            { "RoofUnder", HouseMaterialLook.Painted(new Color(0.12f, 0.08f, 0.06f), 0.1f) },
            { "Zinc", HouseMaterialLook.Weathered(new Color(0.55f, 0.57f, 0.58f), 0.55f) },
            { "Glass", HouseMaterialLook.Painted(new Color(0.04f, 0.05f, 0.06f), 0.92f) },
            { "Frame", HouseMaterialLook.Weathered(new Color(0.78f, 0.75f, 0.68f), 0.35f) },
            { "Shutter", HouseMaterialLook.Weathered(new Color(0.27f, 0.3f, 0.14f), 0.35f) },
            { "ShutterFaded", HouseMaterialLook.Weathered(new Color(0.38f, 0.4f, 0.24f), 0.25f) },
            { "ShutterBlue", HouseMaterialLook.Weathered(new Color(0.2f, 0.3f, 0.36f), 0.3f) },
            { "Curtain", HouseMaterialLook.Painted(new Color(0.75f, 0.72f, 0.64f), 0.1f) },
            { "Wood", HouseMaterialLook.Weathered(new Color(0.16f, 0.09f, 0.05f), 0.3f) },
            { "Timber", HouseMaterialLook.Weathered(new Color(0.32f, 0.22f, 0.13f), 0.2f) },
            { "Iron", HouseMaterialLook.Painted(new Color(0.05f, 0.05f, 0.05f), 0.45f) },
            { "Bronze", HouseMaterialLook.Weathered(new Color(0.36f, 0.26f, 0.12f), 0.5f) },
            { "Terracotta", HouseMaterialLook.Weathered(new Color(0.55f, 0.32f, 0.18f), 0.2f) },
            { "CanvasA", HouseMaterialLook.Painted(new Color(0.7f, 0.66f, 0.56f), 0.1f) },
            { "CanvasB", HouseMaterialLook.Painted(new Color(0.5f, 0.16f, 0.12f), 0.1f) },
            { "Shade", HouseMaterialLook.Painted(new Color(0.06f, 0.05f, 0.04f), 0.05f) },
            { "FlowerRed", HouseMaterialLook.Painted(new Color(0.7f, 0.04f, 0.04f), 0.3f) },
            { "FlowerPink", HouseMaterialLook.Painted(new Color(0.8f, 0.35f, 0.45f), 0.3f) },
            { "FlowerWhite", HouseMaterialLook.Painted(new Color(0.8f, 0.8f, 0.76f), 0.3f) },
            { "Leaf", HouseMaterialLook.Painted(new Color(0.12f, 0.25f, 0.07f), 0.3f) },
            { "Plaque", HouseMaterialLook.Painted(new Color(0.8f, 0.79f, 0.75f), 0.4f) },
            { "LampGlow", HouseMaterialLook.Painted(new Color(0.9f, 0.7f, 0.4f), 0.8f) },
            { "Straw", HouseMaterialLook.Weathered(new Color(0.62f, 0.52f, 0.3f), 0.1f) },
            { "Cobble", HouseMaterialLook.Scanned(new Color(0.85f, 0.82f, 0.76f), CobbleTextures, CobbleMetresPerTile) },
            { "Rope", HouseMaterialLook.Painted(new Color(0.55f, 0.45f, 0.3f), 0.1f) },
            { "BoatPaint", HouseMaterialLook.Weathered(new Color(0.2f, 0.32f, 0.3f), 0.35f) },
            { "Reed", HouseMaterialLook.Painted(new Color(0.35f, 0.4f, 0.18f), 0.2f) },
            { "ReedHead", HouseMaterialLook.Painted(new Color(0.3f, 0.18f, 0.08f), 0.1f) },
            { "LilyPad", HouseMaterialLook.Painted(new Color(0.15f, 0.3f, 0.1f), 0.5f) },
            { "Water", HouseMaterialLook.Painted(new Color(0.08f, 0.12f, 0.1f), 0.95f) },
            { "Olive", HouseMaterialLook.Painted(new Color(0.3f, 0.36f, 0.22f), 0.25f) },
            { "Apple", HouseMaterialLook.Painted(new Color(0.6f, 0.08f, 0.05f), 0.5f) },
            { "Orange", HouseMaterialLook.Painted(new Color(0.8f, 0.4f, 0.05f), 0.4f) },
            { "Greens", HouseMaterialLook.Painted(new Color(0.2f, 0.4f, 0.1f), 0.3f) },
            { "Bark", HouseMaterialLook.Weathered(new Color(0.4f, 0.37f, 0.3f), 0.15f) },
            { "BarkDark", HouseMaterialLook.Weathered(new Color(0.22f, 0.18f, 0.14f), 0.15f) },
            { "Foliage", HouseMaterialLook.Painted(new Color(0.18f, 0.3f, 0.08f), 0.25f) },
            { "FoliageLight", HouseMaterialLook.Painted(new Color(0.3f, 0.4f, 0.12f), 0.25f) },
            { "FoliageAutumn", HouseMaterialLook.Painted(new Color(0.55f, 0.42f, 0.1f), 0.25f) },
            { "PineNeedles", HouseMaterialLook.Painted(new Color(0.12f, 0.22f, 0.08f), 0.2f) },
            { "Hedge", HouseMaterialLook.Painted(new Color(0.1f, 0.22f, 0.06f), 0.2f) },
            { "Wheat", HouseMaterialLook.Painted(new Color(0.72f, 0.6f, 0.3f), 0.2f) },
            { "Hay", HouseMaterialLook.Weathered(new Color(0.66f, 0.56f, 0.3f), 0.1f) },
            { "Wool", HouseMaterialLook.Plastered(new Color(0.8f, 0.77f, 0.7f), 0.05f) },
            { "Linen", HouseMaterialLook.Painted(new Color(0.82f, 0.8f, 0.74f), 0.1f) },
            { "LinenBlue", HouseMaterialLook.Painted(new Color(0.3f, 0.4f, 0.55f), 0.1f) },
            { "Soil", HouseMaterialLook.Weathered(new Color(0.25f, 0.18f, 0.12f), 0.05f) },
            { "Grass", HouseMaterialLook.Painted(new Color(0.2f, 0.32f, 0.1f), 0.2f) },
            { "Gravel", HouseMaterialLook.Weathered(new Color(0.62f, 0.58f, 0.5f), 0.1f) },
        };

        [MenuItem("Stilargo/Build Canal Town Prefabs")]
        public static void BuildAll()
        {
            foreach ((string name, string script) in Models)
            {
                GameObject prefab = new BlenderHouseAssets(name, script, Looks, MaterialFolder).LoadOrCreatePrefab();
                Debug.Log($"Built canal-town prefab {AssetDatabase.GetAssetPath(prefab)}");
            }

            AssetDatabase.SaveAssets();
        }
    }
}
