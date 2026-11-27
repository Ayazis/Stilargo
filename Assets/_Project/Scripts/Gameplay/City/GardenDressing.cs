using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A private garden between villas or in a house block's courtyard: lawn inside a low hedge, a few trees and a
    /// bench. The same plot always gets the same trees.
    /// </summary>
    internal sealed class GardenDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float LawnHeightMetres = 0.05f;
        private const float HedgeHeightMetres = 0.8f;
        private const float HedgeThicknessMetres = 0.5f;
        private const float TreeEveryMetres = 5f;
        private const float TreeMarginMetres = 1.2f;
        private const int CypressChanceInThree = 1;
        private const int Three = 3;
        private const int SeedXPrime = 73856093;
        private const int SeedYPrime = 19349663;
        private static readonly Vector3 BenchSize = new Vector3(1.6f, 0.45f, 0.5f);

        private readonly LandmarkMaterials _materials;
        private readonly TreeProps _trees;

        public GardenDressing(LandmarkMaterials materials)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _trees = new TreeProps(materials);
        }

        public bool TryGetWallMaterial(out Material material)
        {
            material = null;
            return false;
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            Vector3 centre = frame.GroundCentre;
            var lawn = new Vector3(frame.Width, LawnHeightMetres, frame.Depth);
            PropFactory.IgnoredByNavigation(props.Box(centre + (Vector3.up * (LawnHeightMetres * Half)), lawn, _materials.Grass));
            AddHedge(props, frame);

            var random = new System.Random((frame.Plot.Area.Min.X * SeedXPrime) ^ (frame.Plot.Area.Min.Y * SeedYPrime));
            int trees = Math.Max(1, (int)((frame.Width * frame.Depth) / (TreeEveryMetres * TreeEveryMetres)));
            for (int index = 0; index < trees; index++)
            {
                AddTree(props, frame, random);
            }

            PropFactory.IgnoredByNavigation(props.Box(centre + (Vector3.up * (BenchSize.y * Half)), BenchSize, _materials.Wood));
        }

        private void AddHedge(PropFactory props, PlotFrame frame)
        {
            float y = HedgeHeightMetres * Half;
            float inset = HedgeThicknessMetres * Half;
            float midX = frame.West + (frame.Width * Half);
            float midZ = frame.South + (frame.Depth * Half);
            var alongX = new Vector3(frame.Width, HedgeHeightMetres, HedgeThicknessMetres);
            var alongZ = new Vector3(HedgeThicknessMetres, HedgeHeightMetres, frame.Depth);
            PropFactory.Decorative(props.Box(new Vector3(midX, y, frame.South + inset), alongX, _materials.Foliage));
            PropFactory.Decorative(props.Box(new Vector3(midX, y, frame.South + frame.Depth - inset), alongX, _materials.Foliage));
            PropFactory.Decorative(props.Box(new Vector3(frame.West + inset, y, midZ), alongZ, _materials.Foliage));
            PropFactory.Decorative(props.Box(new Vector3(frame.West + frame.Width - inset, y, midZ), alongZ, _materials.Foliage));
        }

        private void AddTree(PropFactory props, PlotFrame frame, System.Random random)
        {
            float x = frame.West + TreeMarginMetres + ((float)random.NextDouble() * Math.Max(0f, frame.Width - (TreeMarginMetres * 2f)));
            float z = frame.South + TreeMarginMetres + ((float)random.NextDouble() * Math.Max(0f, frame.Depth - (TreeMarginMetres * 2f)));
            var foot = new Vector3(x, 0f, z);
            if (random.Next(Three) < CypressChanceInThree)
            {
                _trees.AddCypress(props, foot);
                return;
            }

            _trees.AddPine(props, foot);
        }
    }
}
