using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A small public garden: a lawn behind a stone kerb, a mix of umbrella pines and cypresses, and benches.
    /// Nothing tall blocks the street grid, so units can cut across it. The same plot always gets the same trees.
    /// </summary>
    internal sealed class ParkDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float LawnHeightMetres = 0.06f;
        private const float LawnMarginMetres = 0.25f;
        private const float KerbHeightMetres = 0.15f;
        private const float KerbThicknessMetres = 0.25f;
        private const float TreeSpacingMetres = 4f;
        private const float TreeJitterMetres = 0.8f;
        private const int CypressEvery = 3;
        private const int SeedXPrime = 73856093;
        private const int SeedYPrime = 19349663;
        private static readonly Vector3 BenchSize = new Vector3(1.6f, 0.45f, 0.5f);

        private readonly LandmarkMaterials _materials;
        private readonly TreeProps _trees;

        public ParkDressing(LandmarkMaterials materials)
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
            var lawnSize = new Vector3(frame.Width - (LawnMarginMetres * 2f), LawnHeightMetres, frame.Depth - (LawnMarginMetres * 2f));
            PropFactory.IgnoredByNavigation(props.Box(centre + (Vector3.up * (LawnHeightMetres * Half)), lawnSize, _materials.Grass));
            AddKerb(props, frame);
            AddTrees(props, frame);
            PropFactory.IgnoredByNavigation(props.Box(centre + new Vector3(0f, BenchSize.y * Half, -TreeSpacingMetres * Half), BenchSize, _materials.Wood));
            PropFactory.IgnoredByNavigation(props.Box(centre + new Vector3(0f, BenchSize.y * Half, TreeSpacingMetres * Half), BenchSize, _materials.Wood));
        }

        private void AddKerb(PropFactory props, PlotFrame frame)
        {
            float y = KerbHeightMetres * Half;
            float east = frame.West + frame.Width;
            float north = frame.South + frame.Depth;
            float midX = frame.West + (frame.Width * Half);
            float midZ = frame.South + (frame.Depth * Half);
            var alongX = new Vector3(frame.Width, KerbHeightMetres, KerbThicknessMetres);
            var alongZ = new Vector3(KerbThicknessMetres, KerbHeightMetres, frame.Depth);
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(midX, y, frame.South), alongX, _materials.Stone));
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(midX, y, north), alongX, _materials.Stone));
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(frame.West, y, midZ), alongZ, _materials.Stone));
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(east, y, midZ), alongZ, _materials.Stone));
        }

        private void AddTrees(PropFactory props, PlotFrame frame)
        {
            var random = new System.Random((frame.Plot.Area.Min.X * SeedXPrime) ^ (frame.Plot.Area.Min.Y * SeedYPrime));
            int columns = Math.Max(1, (int)(frame.Width / TreeSpacingMetres));
            int rows = Math.Max(1, (int)(frame.Depth / TreeSpacingMetres));
            int index = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    float x = frame.West + (frame.Width * (column + Half) / columns) + Jitter(random);
                    float z = frame.South + (frame.Depth * (row + Half) / rows) + Jitter(random);
                    AddTree(props, new Vector3(x, 0f, z), index % CypressEvery == CypressEvery - 1);
                    index++;
                }
            }
        }

        private void AddTree(PropFactory props, Vector3 foot, bool isCypress)
        {
            if (isCypress)
            {
                _trees.AddCypress(props, foot);
                return;
            }

            _trees.AddPine(props, foot);
        }

        private static float Jitter(System.Random random)
        {
            return ((float)random.NextDouble() - Half) * 2f * TreeJitterMetres;
        }
    }
}
