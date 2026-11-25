using System;
using System.Collections.Generic;
using Stilargo.Core.City;
using Unity.AI.Navigation;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// Turns a <see cref="CityLayout"/> into buildings (plaster walls, tiled hip roofs, facade kit pieces)
    /// and bakes the NavMesh over it.
    /// </summary>
    [RequireComponent(typeof(NavMeshSurface))]
    public sealed class CitySpawner : MonoBehaviour
    {
        private const float Half = 0.5f;
        private const float QuarterTurnDegrees = 90f;

        [SerializeField] private GameObject _groundPrefab;
        [SerializeField] private GameObject _buildingPrefab;
        [SerializeField] private float _storeyHeight = 3f;
        [SerializeField] private float _groundThickness = 0.2f;
        [SerializeField] private GameObject _roofPrefab;
        [SerializeField] private Material[] _wallMaterials = Array.Empty<Material>();
        [SerializeField] private Material[] _roofMaterials = Array.Empty<Material>();
        [SerializeField] private float _roofOverhangMetres = 0.4f;
        [SerializeField] private float _roofRisePerHalfSpan = 0.5f;
        [SerializeField] private GameObject _windowPrefab;
        [SerializeField] private GameObject _doorPrefab;
        [SerializeField] private GameObject _cornicePrefab;

        private readonly List<Mesh> _roofMeshes = new List<Mesh>();
        private NavMeshSurface _navMeshSurface;
        private Dictionary<FacadePieceKind, GameObject> _facadePrefabs;

        private void Awake()
        {
            CacheComponents();
        }

        private void OnDestroy()
        {
            foreach (Mesh mesh in _roofMeshes)
            {
                Destroy(mesh);
            }
        }

        public void Construct(CityLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            if (_groundPrefab == null || _buildingPrefab == null || _roofPrefab == null)
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} needs a ground, a building and a roof prefab.");
            }

            if (_windowPrefab == null || _doorPrefab == null || _cornicePrefab == null)
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} needs window, door and cornice prefabs. Run Stilargo → Build World Prefab.");
            }

            CacheComponents();
            _facadePrefabs = new Dictionary<FacadePieceKind, GameObject>
            {
                { FacadePieceKind.Window, _windowPrefab },
                { FacadePieceKind.Door, _doorPrefab },
                { FacadePieceKind.Cornice, _cornicePrefab },
            };
            SpawnGround(layout);
            foreach (Plot plot in layout.Plots)
            {
                SpawnBuilding(plot, layout.CellSize);
            }

            _navMeshSurface.BuildNavMesh();

            // BuildNavMesh only registers the data once the surface is enabled. The bootstrapper runs before
            // that, and installers after it place units right away, so register now. AddData is idempotent.
            _navMeshSurface.AddData();
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
            ApplyVariant(building, _wallMaterials, plot);
            SpawnRoof(plot, new Vector3(position.x, height, position.z), building.transform.localScale);
            SpawnFacade(plot, cellSize);
        }

        private void SpawnFacade(Plot plot, float cellSize)
        {
            foreach (FacadePiece piece in FacadeLayout.For(plot, cellSize, _storeyHeight))
            {
                Vector3 position = new Vector3(piece.Position.X, piece.Position.Y, piece.Position.Z);
                Quaternion rotation = Quaternion.Euler(0f, piece.FacingDegrees, 0f);
                Instantiate(_facadePrefabs[piece.Kind], position, rotation, transform);
            }
        }

        private void SpawnRoof(Plot plot, Vector3 eavesCentre, Vector3 buildingSize)
        {
            float width = buildingSize.x + (_roofOverhangMetres + _roofOverhangMetres);
            float depth = buildingSize.z + (_roofOverhangMetres + _roofOverhangMetres);
            float rise = Mathf.Min(width, depth) * Half * _roofRisePerHalfSpan;

            // The roof's ridge runs along its local X, so turn it to follow the building's longer side.
            bool isDeeperThanWide = depth > width;
            Quaternion rotation = isDeeperThanWide ? Quaternion.Euler(0f, QuarterTurnDegrees, 0f) : Quaternion.identity;
            RoofGeometry geometry = HipRoofBuilder.Build(Mathf.Max(width, depth), Mathf.Min(width, depth), rise);
            Mesh mesh = RoofMeshes.Create(geometry);
            _roofMeshes.Add(mesh);
            GameObject roof = Instantiate(_roofPrefab, eavesCentre, rotation, transform);
            roof.GetComponent<MeshFilter>().sharedMesh = mesh;
            ApplyVariant(roof, _roofMaterials, plot);
        }

        private static void ApplyVariant(GameObject instance, Material[] variants, Plot plot)
        {
            if (variants.Length == 0)
            {
                return;
            }

            instance.GetComponent<Renderer>().sharedMaterial = variants[PlotVariation.Pick(plot, variants.Length)];
        }
    }
}
