using System;
using Unity.AI.Navigation;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A fountain square: a stone-rimmed pond with a fountain in the middle. Its whole footprint is left out of
    /// the walkable NavMesh, so units go round it.
    /// </summary>
    internal sealed class WaterDressing : IPlotDressing
    {
        private const string PondName = "Pond";
        private const int NotWalkableArea = 1;
        private const float Half = 0.5f;
        private const float RimMarginMetres = 0.6f;
        private const float RimHeightMetres = 0.5f;
        private const float RimThicknessMetres = 0.4f;
        private const float WaterHeightMetres = 0.32f;
        private const float WaterThicknessMetres = 0.04f;
        private const float BasinDiameterMetres = 1.8f;
        private const float BasinHeightMetres = 0.7f;
        private const float ColumnDiameterMetres = 0.3f;
        private const float ColumnHeightMetres = 1.6f;
        private const float BowlDiameterMetres = 1f;
        private const float BowlHeightMetres = 0.15f;

        private readonly LandmarkMaterials _materials;

        public WaterDressing(LandmarkMaterials materials)
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
            var pond = new GameObject(PondName);
            pond.transform.SetParent(props.Parent, false);
            var modifier = pond.AddComponent<NavMeshModifier>();
            modifier.overrideArea = true;
            modifier.area = NotWalkableArea;
            var pondProps = new PropFactory(pond.transform);

            float width = frame.Width - (RimMarginMetres * 2f);
            float depth = frame.Depth - (RimMarginMetres * 2f);
            Vector3 centre = frame.GroundCentre;
            AddRim(pondProps, centre, new Vector2(width, depth));
            var waterSize = new Vector3(width - RimThicknessMetres, WaterThicknessMetres, depth - RimThicknessMetres);
            pondProps.Box(centre + (Vector3.up * WaterHeightMetres), waterSize, _materials.Water);

            pondProps.Cylinder(centre, BasinDiameterMetres, BasinHeightMetres, _materials.Stone);
            pondProps.Cylinder(centre, ColumnDiameterMetres, ColumnHeightMetres, _materials.Stone);
            pondProps.Cylinder(centre + (Vector3.up * ColumnHeightMetres), BowlDiameterMetres, BowlHeightMetres, _materials.Stone);
        }

        private void AddRim(PropFactory props, Vector3 centre, Vector2 size)
        {
            float y = RimHeightMetres * Half;
            var alongX = new Vector3(size.x, RimHeightMetres, RimThicknessMetres);
            var alongZ = new Vector3(RimThicknessMetres, RimHeightMetres, size.y);
            props.Box(new Vector3(centre.x, y, centre.z - (size.y * Half)), alongX, _materials.Stone);
            props.Box(new Vector3(centre.x, y, centre.z + (size.y * Half)), alongX, _materials.Stone);
            props.Box(new Vector3(centre.x - (size.x * Half), y, centre.z), alongZ, _materials.Stone);
            props.Box(new Vector3(centre.x + (size.x * Half), y, centre.z), alongZ, _materials.Stone);
        }
    }
}
