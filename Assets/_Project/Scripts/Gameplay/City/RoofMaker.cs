using System;
using System.Collections.Generic;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// Builds tiled hip roofs sized to a building. A square footprint gives a pyramid, which tops the church tower.
    /// Owns the meshes it makes, so the spawner can free them.
    /// </summary>
    internal sealed class RoofMaker
    {
        private const float Half = 0.5f;
        private const float QuarterTurnDegrees = 90f;

        private readonly GameObject _roofPrefab;
        private readonly RoofShape _shape;
        private readonly Transform _parent;
        private readonly List<Mesh> _meshes = new List<Mesh>();

        public RoofMaker(GameObject roofPrefab, RoofShape shape, Transform parent)
        {
            if (roofPrefab == null)
            {
                throw new ArgumentNullException(nameof(roofPrefab));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _roofPrefab = roofPrefab;
            _shape = shape;
            _parent = parent;
        }

        public IReadOnlyList<Mesh> Meshes => _meshes;

        /// <param name="eavesCentre">Centre of the top of the walls the roof sits on.</param>
        /// <param name="footprint">Width (X) and depth (Z) of those walls.</param>
        public GameObject Build(Vector3 eavesCentre, Vector2 footprint, float risePerHalfSpan)
        {
            float overhang = _shape.OverhangMetres + _shape.OverhangMetres;
            float width = footprint.x + overhang;
            float depth = footprint.y + overhang;
            float rise = Mathf.Min(width, depth) * Half * risePerHalfSpan;

            // The roof's ridge runs along its local X, so turn it to follow the building's longer side.
            bool isDeeperThanWide = depth > width;
            Quaternion rotation = isDeeperThanWide ? Quaternion.Euler(0f, QuarterTurnDegrees, 0f) : Quaternion.identity;
            RoofGeometry geometry = HipRoofBuilder.Build(Mathf.Max(width, depth), Mathf.Min(width, depth), rise);
            Mesh mesh = RoofMeshes.Create(geometry);
            _meshes.Add(mesh);
            GameObject roof = UnityEngine.Object.Instantiate(_roofPrefab, eavesCentre, rotation, _parent);
            roof.GetComponent<MeshFilter>().sharedMesh = mesh;
            return roof;
        }

        public GameObject Build(Vector3 eavesCentre, Vector2 footprint)
        {
            return Build(eavesCentre, footprint, _shape.RisePerHalfSpan);
        }
    }
}
