using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>Plants the city's placeholder trees: wide umbrella pines and narrow cypresses.</summary>
    internal sealed class TreeProps
    {
        private const float Half = 0.5f;
        private const float TrunkDiameterMetres = 0.3f;
        private const float PineTrunkMetres = 3.2f;
        private const float CypressTrunkMetres = 0.8f;
        private static readonly Vector3 PineCanopySize = new Vector3(3.4f, 1.6f, 3.4f);
        private static readonly Vector3 CypressCanopySize = new Vector3(1.1f, 5.5f, 1.1f);

        private readonly LandmarkMaterials _materials;

        public TreeProps(LandmarkMaterials materials)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
        }

        public void AddPine(PropFactory props, Vector3 foot)
        {
            AddTree(props, foot, PineTrunkMetres, PineCanopySize);
        }

        public void AddCypress(PropFactory props, Vector3 foot)
        {
            AddTree(props, foot, CypressTrunkMetres, CypressCanopySize);
        }

        private void AddTree(PropFactory props, Vector3 foot, float trunk, Vector3 canopySize)
        {
            props.Cylinder(foot, TrunkDiameterMetres, trunk, _materials.Bark);

            // No collider on the canopy: clicks must reach the units standing under it.
            GameObject canopy = props.Sphere(foot + (Vector3.up * (trunk + (canopySize.y * Half))), canopySize, _materials.Foliage);
            UnityEngine.Object.Destroy(canopy.GetComponent<Collider>());
            PropFactory.IgnoredByNavigation(canopy);
        }
    }
}
