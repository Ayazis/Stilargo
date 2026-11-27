using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>An ochre school with a flag flying from a pole at its corner.</summary>
    internal sealed class SchoolDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float PoleOffsetMetres = 0.5f;
        private const float PoleDiameterMetres = 0.12f;
        private const float PoleAboveRoofMetres = 4f;
        private static readonly Vector3 FlagSizeMetres = new Vector3(0.03f, 0.9f, 1.5f);

        private readonly LandmarkMaterials _materials;

        public SchoolDressing(LandmarkMaterials materials)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
        }

        public bool TryGetWallMaterial(out Material material)
        {
            material = _materials.SchoolWall;
            return true;
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            float poleHeight = frame.BuildingHeight + PoleAboveRoofMetres;
            var poleBase = new Vector3(frame.West - PoleOffsetMetres, 0f, frame.South - PoleOffsetMetres);
            PropFactory.IgnoredByNavigation(props.Cylinder(poleBase, PoleDiameterMetres, poleHeight, _materials.Stone));

            // The flag hangs off the pole towards the building, so it never pokes into the street.
            float flagCentreHeight = poleHeight - (FlagSizeMetres.y * Half);
            var flagCentre = new Vector3(poleBase.x, flagCentreHeight, poleBase.z + (FlagSizeMetres.z * Half));
            PropFactory.IgnoredByNavigation(props.Box(flagCentre, FlagSizeMetres, _materials.Cloth));
        }
    }
}
