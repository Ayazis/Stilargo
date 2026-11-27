using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// The market square in the middle of town, after the church-square concept art: open stone paving with a
    /// fountain in the centre, a few trees and benches round it, and rows of market stalls along two sides.
    /// Units cross the paving freely; only the fountain and the stall tables are in the way.
    /// </summary>
    internal sealed class MarketDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float PavingHeightMetres = 0.04f;
        private const float BasinDiameterMetres = 3.6f;
        private const float BasinHeightMetres = 0.55f;
        private const float WaterDiameterMetres = 3.1f;
        private const float WaterHeightMetres = 0.5f;
        private const float WaterThicknessMetres = 0.04f;
        private const float ColumnDiameterMetres = 0.4f;
        private const float ColumnHeightMetres = 1.8f;
        private const float BowlDiameterMetres = 1.2f;
        private const float BowlHeightMetres = 0.15f;
        private const float TreeInsetFraction = 0.25f;
        private const float BenchOffsetMetres = 3.4f;
        private const float QuarterTurnDegrees = 90f;
        private const float StallInsetMetres = 2.2f;
        private const float StallSpacingMetres = 3.6f;
        private const float StallPostHeightMetres = 2.3f;
        private const float StallPostDiameterMetres = 0.1f;
        private const float StallRowFraction = 0.6f;
        private static readonly Vector3 BenchSize = new Vector3(1.6f, 0.45f, 0.5f);
        private static readonly Vector3 StallTableSize = new Vector3(2f, 0.9f, 0.9f);
        private static readonly Vector3 AwningSize = new Vector3(2.4f, 0.08f, 1.5f);

        private readonly LandmarkMaterials _materials;
        private readonly TreeProps _trees;

        public MarketDressing(LandmarkMaterials materials)
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
            var pavingSize = new Vector3(frame.Width, PavingHeightMetres, frame.Depth);
            PropFactory.IgnoredByNavigation(props.Box(centre + (Vector3.up * (PavingHeightMetres * Half)), pavingSize, _materials.Stone));
            AddFountain(props, centre);
            AddTreesAndBenches(props, frame);
            AddStallRow(props, frame, frame.South + StallInsetMetres);
            AddStallRow(props, frame, frame.South + frame.Depth - StallInsetMetres);
        }

        private void AddFountain(PropFactory props, Vector3 centre)
        {
            props.Cylinder(centre, BasinDiameterMetres, BasinHeightMetres, _materials.Stone);
            PropFactory.IgnoredByNavigation(props.Cylinder(centre + (Vector3.up * WaterHeightMetres), WaterDiameterMetres, WaterThicknessMetres, _materials.Water));
            props.Cylinder(centre, ColumnDiameterMetres, ColumnHeightMetres, _materials.Stone);
            props.Cylinder(centre + (Vector3.up * ColumnHeightMetres), BowlDiameterMetres, BowlHeightMetres, _materials.Stone);
        }

        // One tree in each quarter of the square, a bench facing the fountain on each side.
        private void AddTreesAndBenches(PropFactory props, PlotFrame frame)
        {
            Vector3 centre = frame.GroundCentre;
            float insetX = frame.Width * TreeInsetFraction;
            float insetZ = frame.Depth * TreeInsetFraction;
            _trees.AddPine(props, centre + new Vector3(-insetX, 0f, -insetZ));
            _trees.AddPine(props, centre + new Vector3(insetX, 0f, -insetZ));
            _trees.AddPine(props, centre + new Vector3(-insetX, 0f, insetZ));
            _trees.AddPine(props, centre + new Vector3(insetX, 0f, insetZ));

            float benchY = BenchSize.y * Half;
            Quaternion turned = Quaternion.Euler(0f, QuarterTurnDegrees, 0f);
            props.Box(centre + new Vector3(0f, benchY, -BenchOffsetMetres), BenchSize, _materials.Wood);
            props.Box(centre + new Vector3(0f, benchY, BenchOffsetMetres), BenchSize, _materials.Wood);
            props.Box(centre + new Vector3(-BenchOffsetMetres, benchY, 0f), BenchSize, _materials.Wood).transform.rotation = turned;
            props.Box(centre + new Vector3(BenchOffsetMetres, benchY, 0f), BenchSize, _materials.Wood).transform.rotation = turned;
        }

        private void AddStallRow(PropFactory props, PlotFrame frame, float z)
        {
            float rowLength = frame.Width * StallRowFraction;
            int stalls = Math.Max(1, (int)(rowLength / StallSpacingMetres));
            float firstX = frame.GroundCentre.x - ((stalls - 1) * StallSpacingMetres * Half);
            for (int index = 0; index < stalls; index++)
            {
                AddStall(props, new Vector3(firstX + (index * StallSpacingMetres), 0f, z));
            }
        }

        private void AddStall(PropFactory props, Vector3 foot)
        {
            props.Box(foot + (Vector3.up * (StallTableSize.y * Half)), StallTableSize, _materials.Wood);
            float postX = AwningSize.x * Half;
            float postZ = AwningSize.z * Half;
            PropFactory.IgnoredByNavigation(props.Cylinder(foot + new Vector3(-postX, 0f, -postZ), StallPostDiameterMetres, StallPostHeightMetres, _materials.Wood));
            PropFactory.IgnoredByNavigation(props.Cylinder(foot + new Vector3(postX, 0f, -postZ), StallPostDiameterMetres, StallPostHeightMetres, _materials.Wood));

            // No collider on the awning: clicks must reach the units standing under it.
            GameObject awning = props.Box(foot + (Vector3.up * StallPostHeightMetres), AwningSize, _materials.Cloth);
            UnityEngine.Object.Destroy(awning.GetComponent<Collider>());
            PropFactory.IgnoredByNavigation(awning);
        }
    }
}
