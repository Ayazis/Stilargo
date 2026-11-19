using System;
using Stilargo.Core.City;
using Unity.AI.Navigation;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>Turns a <see cref="CityLayout"/> into greybox geometry and bakes the NavMesh over it.</summary>
    [RequireComponent(typeof(NavMeshSurface))]
    public sealed class CitySpawner : MonoBehaviour
    {
        private const float Half = 0.5f;

        [SerializeField] private GameObject _groundPrefab;
        [SerializeField] private GameObject _buildingPrefab;
        [SerializeField] private float _storeyHeight = 3f;
        [SerializeField] private float _groundThickness = 0.2f;

        private NavMeshSurface _navMeshSurface;

        private void Awake()
        {
            CacheComponents();
        }

        public void Construct(CityLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            if (_groundPrefab == null || _buildingPrefab == null)
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} needs a ground and a building prefab.");
            }

            CacheComponents();
            SpawnGround(layout);
            foreach (Plot plot in layout.Plots)
            {
                SpawnBuilding(plot, layout.CellSize);
            }

            _navMeshSurface.BuildNavMesh();
        }

        // Construct can run before Awake when the bootstrapper wires scene objects, so both paths cache.
        private void CacheComponents()
        {
            if (_navMeshSurface == null)
            {
                _navMeshSurface = GetComponent<NavMeshSurface>();
            }
        }

        private void SpawnGround(CityLayout layout)
        {
            float width = layout.Width * layout.CellSize;
            float depth = layout.Height * layout.CellSize;
            Vector3 position = new Vector3(width * Half, -_groundThickness * Half, depth * Half);
            GameObject ground = Instantiate(_groundPrefab, position, Quaternion.identity, transform);
            ground.transform.localScale = new Vector3(width, _groundThickness, depth);
        }

        private void SpawnBuilding(Plot plot, float cellSize)
        {
            GridRect area = plot.Area;
            float height = plot.Storeys * _storeyHeight;
            Vector3 position = new Vector3(
                (area.Min.X + (area.Width * Half)) * cellSize,
                height * Half,
                (area.Min.Y + (area.Height * Half)) * cellSize);
            GameObject building = Instantiate(_buildingPrefab, position, Quaternion.identity, transform);
            building.transform.localScale = new Vector3(area.Width * cellSize, height, area.Height * cellSize);
        }
    }
}
