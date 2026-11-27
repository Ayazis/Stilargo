using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A workshop yard: packed gravel with stacked crates, barrels, a woodpile and a cart. It is cover-ish clutter,
    /// but all decorative, so units can still cross it. The same plot always gets the same clutter.
    /// </summary>
    internal sealed class YardDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float GravelHeightMetres = 0.03f;
        private const float CrateEdgeMetres = 0.7f;
        private const float BarrelDiameterMetres = 0.6f;
        private const float BarrelHeightMetres = 0.9f;
        private const float ClutterMarginMetres = 1f;
        private const int ClutterPerTenSquareMetres = 1;
        private const float SquareMetresPerClutter = 10f;
        private const int Kinds = 3;
        private const int SeedXPrime = 83492791;
        private const int SeedYPrime = 2654435;
        private static readonly Vector3 WoodpileSize = new Vector3(2f, 0.8f, 0.9f);

        private readonly LandmarkMaterials _materials;

        public YardDressing(LandmarkMaterials materials)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
        }

        public bool TryGetWallMaterial(out Material material)
        {
            material = null;
            return false;
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            var gravel = new Vector3(frame.Width, GravelHeightMetres, frame.Depth);
            PropFactory.IgnoredByNavigation(props.Box(frame.GroundCentre + (Vector3.up * (GravelHeightMetres * Half)), gravel, _materials.Stone));

            var random = new System.Random((frame.Plot.Area.Min.X * SeedXPrime) ^ (frame.Plot.Area.Min.Y * SeedYPrime));
            int count = Math.Max(Kinds, (int)(frame.Width * frame.Depth / SquareMetresPerClutter) * ClutterPerTenSquareMetres);
            for (int index = 0; index < count; index++)
            {
                AddClutter(props, RandomFoot(frame, random), index % Kinds);
            }
        }

        private void AddClutter(PropFactory props, Vector3 foot, int kind)
        {
            if (kind == 0)
            {
                PropFactory.Decorative(props.Box(foot + (Vector3.up * (CrateEdgeMetres * Half)), Vector3.one * CrateEdgeMetres, _materials.Wood));
                return;
            }

            if (kind == 1)
            {
                PropFactory.Decorative(props.Cylinder(foot, BarrelDiameterMetres, BarrelHeightMetres, _materials.Wood));
                return;
            }

            PropFactory.Decorative(props.Box(foot + (Vector3.up * (WoodpileSize.y * Half)), WoodpileSize, _materials.Bark));
        }

        private static Vector3 RandomFoot(PlotFrame frame, System.Random random)
        {
            float spanX = Math.Max(0f, frame.Width - (ClutterMarginMetres * 2f));
            float spanZ = Math.Max(0f, frame.Depth - (ClutterMarginMetres * 2f));
            float x = frame.West + ClutterMarginMetres + ((float)random.NextDouble() * spanX);
            float z = frame.South + ClutterMarginMetres + ((float)random.NextDouble() * spanZ);
            return new Vector3(x, 0f, z);
        }
    }
}
