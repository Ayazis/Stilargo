using System;
using Unity.AI.Navigation;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A park pond: water inside a stone kerb with rounded corners, reeds and lily pads, and a willow leaning over one
    /// corner. The whole pond is left out of the walkable NavMesh, so units go round it. (The canal's water plots
    /// are drawn by <see cref="CanalBuilder"/> instead.)
    /// </summary>
    internal sealed class WaterDressing : IPlotDressing
    {
        private const string PondName = "Pond";
        private const int NotWalkableArea = 1;
        private const float Half = 0.5f;
        private const float MarginMetres = 0.6f;

        // The kit's kerb and water sit below lawn level; the pond is raised so they show above flat ground.
        private const float LiftMetres = 0.3f;
        private const float WaterLevelMetres = 0.1f;
        private const float WaterThicknessMetres = 0.04f;
        private const float EdgeSegmentMetres = 2f;
        private const float CornerRadiusMetres = 1.2f;
        private const float QuarterTurnDegrees = 90f;
        private const int ReedClumps = 3;
        private const int LilyClusters = 2;
        private const float PlantInsetMetres = 1.2f;

        private readonly LandmarkMaterials _materials;
        private readonly PropModels _models;

        public WaterDressing(LandmarkMaterials materials, PropModels models)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            var pond = new GameObject(PondName);
            pond.transform.SetParent(props.Parent, false);
            var pondProps = new PropFactory(pond.transform);
            var rect = new Rect(frame.West + MarginMetres, frame.South + MarginMetres, frame.Width - (MarginMetres * 2f), frame.Depth - (MarginMetres * 2f));
            AddWater(pondProps, rect);
            AddKerb(pondProps, rect);
            AddPlants(pondProps, rect, PlotRandom.For(frame.Plot));
            props.Model(_models[PropKind.Willow], new Vector3(frame.West + MarginMetres, 0f, frame.South + MarginMetres), Quaternion.identity);
        }

        private void AddWater(PropFactory props, Rect rect)
        {
            var centre = new Vector3(rect.center.x, WaterLevelMetres, rect.center.y);
            GameObject water = props.Box(centre, new Vector3(rect.width, WaterThicknessMetres, rect.height), _materials.Water);
            var modifier = water.AddComponent<NavMeshModifier>();
            modifier.overrideArea = true;
            modifier.area = NotWalkableArea;
        }

        // Rounded corners first, then straight kerb segments stretched to fill each side between them.
        private void AddKerb(PropFactory props, Rect rect)
        {
            float y = LiftMetres;
            props.Model(_models[PropKind.PondCorner], new Vector3(rect.xMax, y, rect.yMax), Quaternion.identity);
            props.Model(_models[PropKind.PondCorner], new Vector3(rect.xMax, y, rect.yMin), Quaternion.Euler(0f, QuarterTurnDegrees, 0f));
            props.Model(_models[PropKind.PondCorner], new Vector3(rect.xMin, y, rect.yMin), Quaternion.Euler(0f, QuarterTurnDegrees * 2f, 0f));
            props.Model(_models[PropKind.PondCorner], new Vector3(rect.xMin, y, rect.yMax), Quaternion.Euler(0f, QuarterTurnDegrees * 3f, 0f));

            // An edge's water side faces +Z unrotated, so the south side needs no turn.
            AddSide(props, new Vector3(rect.xMin, y, rect.yMin), new Vector3(rect.xMax, y, rect.yMin), Quaternion.identity);
            AddSide(props, new Vector3(rect.xMin, y, rect.yMax), new Vector3(rect.xMax, y, rect.yMax), Quaternion.Euler(0f, QuarterTurnDegrees * 2f, 0f));
            AddSide(props, new Vector3(rect.xMin, y, rect.yMin), new Vector3(rect.xMin, y, rect.yMax), Quaternion.Euler(0f, QuarterTurnDegrees, 0f));
            AddSide(props, new Vector3(rect.xMax, y, rect.yMin), new Vector3(rect.xMax, y, rect.yMax), Quaternion.Euler(0f, QuarterTurnDegrees * 3f, 0f));
        }

        private void AddSide(PropFactory props, Vector3 from, Vector3 to, Quaternion facingWater)
        {
            Vector3 along = (to - from).normalized;
            Vector3 start = from + (along * CornerRadiusMetres);
            float length = Vector3.Distance(from, to) - (CornerRadiusMetres * 2f);
            if (length <= 0f)
            {
                return;
            }

            int count = Mathf.Max(1, Mathf.RoundToInt(length / EdgeSegmentMetres));
            float stretch = length / (count * EdgeSegmentMetres);
            for (int index = 0; index < count; index++)
            {
                Vector3 centre = start + (along * (length * (index + Half) / count));
                props.Model(_models[PropKind.PondEdge], centre, facingWater).transform.localScale = new Vector3(stretch, 1f, 1f);
            }
        }

        private void AddPlants(PropFactory props, Rect rect, System.Random random)
        {
            Rect inner = new Rect(rect.xMin + PlantInsetMetres, rect.yMin + PlantInsetMetres, rect.width - (PlantInsetMetres * 2f), rect.height - (PlantInsetMetres * 2f));
            if (inner.width <= 0f || inner.height <= 0f)
            {
                return;
            }

            for (int index = 0; index < ReedClumps + LilyClusters; index++)
            {
                var spot = new Vector3(inner.xMin + ((float)random.NextDouble() * inner.width), WaterLevelMetres, inner.yMin + ((float)random.NextDouble() * inner.height));
                props.Model(_models[index < ReedClumps ? PropKind.Reeds : PropKind.LilyPads], spot, PropScatter.AnyHeading(random));
            }
        }
    }
}
