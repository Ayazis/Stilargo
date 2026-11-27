using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A small neighbourhood plaza: paving, an old well in the middle, a ring of trees with benches beneath them.
    /// Quieter than the market square, so the two never look alike.
    /// </summary>
    internal sealed class PlazaDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float PavingHeightMetres = 0.04f;
        private const float WellDiameterMetres = 1.6f;
        private const float WellHeightMetres = 0.9f;
        private const float WellPostHeightMetres = 2.2f;
        private const float WellPostDiameterMetres = 0.15f;
        private const float RingFraction = 0.32f;
        private const int RingTrees = 6;
        private const float FullTurnRadians = Mathf.PI * 2f;
        private const float BenchInwardMetres = 1.4f;
        private static readonly Vector3 WellRoofSize = new Vector3(2f, 0.15f, 1.4f);
        private static readonly Vector3 BenchSize = new Vector3(1.6f, 0.45f, 0.5f);

        private readonly LandmarkMaterials _materials;
        private readonly TreeProps _trees;

        public PlazaDressing(LandmarkMaterials materials)
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
            var paving = new Vector3(frame.Width, PavingHeightMetres, frame.Depth);
            PropFactory.IgnoredByNavigation(props.Box(centre + (Vector3.up * (PavingHeightMetres * Half)), paving, _materials.Stone));
            AddWell(props, centre);

            float radius = Math.Min(frame.Width, frame.Depth) * RingFraction;
            for (int index = 0; index < RingTrees; index++)
            {
                float angle = FullTurnRadians * index / RingTrees;
                var outward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                _trees.AddPine(props, centre + (outward * radius));
                GameObject bench = props.Box(centre + (outward * (radius - BenchInwardMetres)) + (Vector3.up * (BenchSize.y * Half)), BenchSize, _materials.Wood);
                bench.transform.rotation = Quaternion.LookRotation(outward);
                PropFactory.IgnoredByNavigation(bench);
            }
        }

        private void AddWell(PropFactory props, Vector3 centre)
        {
            props.Cylinder(centre, WellDiameterMetres, WellHeightMetres, _materials.Stone);
            float postX = WellDiameterMetres * Half;
            PropFactory.Decorative(props.Cylinder(centre + new Vector3(-postX, 0f, 0f), WellPostDiameterMetres, WellPostHeightMetres, _materials.Wood));
            PropFactory.Decorative(props.Cylinder(centre + new Vector3(postX, 0f, 0f), WellPostDiameterMetres, WellPostHeightMetres, _materials.Wood));
            PropFactory.Decorative(props.Box(centre + (Vector3.up * WellPostHeightMetres), WellRoofSize, _materials.Wood));
        }
    }
}
