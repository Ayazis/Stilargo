using System;
using Stilargo.Core.City;
using Stilargo.Core.Fog;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A building model's top surface, baked from its mesh in the editor (the meshes aren't readable in the player),
    /// so turret decks can rest on the roof's real tiles.
    /// </summary>
    public sealed class RoofProfile : MonoBehaviour
    {
        [SerializeField] private float _minX;
        [SerializeField] private float _minZ;
        [SerializeField] private float _cellSizeMetres;
        [SerializeField] private int _columns;
        [SerializeField] private float[] _heights = Array.Empty<float>();

        public void Bake(RoofProfileData data)
        {
            _minX = data.MinX;
            _minZ = data.MinZ;
            _cellSizeMetres = data.CellSizeMetres;
            _columns = data.Columns;
            _heights = data.Heights;
        }

        /// <summary>The surface of this model standing at <paramref name="ground"/>, turned to <paramref name="facing"/>.</summary>
        public IRoofSurface PlacedAt(Vector3 ground, Facing facing)
        {
            var grid = new RoofGrid(_minX, _minZ, _cellSizeMetres, _columns, _heights);
            return new RoofHeightField(grid, new WorldPoint(ground.x, ground.z), ground.y, facing);
        }
    }
}
