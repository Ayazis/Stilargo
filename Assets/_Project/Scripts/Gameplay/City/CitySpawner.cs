using System;
using System.Collections.Generic;
using Stilargo.Core.City;
using Unity.AI.Navigation;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// Turns a <see cref="CityLayout"/> into buildings (plaster walls, tiled hip roofs, facade kit pieces) and the
    /// city's landmarks (church, school, taverns, parks, a fountain square), plazas, gardens, yards and street
    /// life (lamps, trees, clutter), then bakes the NavMesh over it.
    /// </summary>
    [RequireComponent(typeof(NavMeshSurface))]
    public sealed class CitySpawner : MonoBehaviour
    {
        private const float Half = 0.5f;

        [SerializeField] private GameObject _groundPrefab;
        [SerializeField] private GameObject _buildingPrefab;
        [SerializeField] private float _storeyHeight = 3f;
        [SerializeField] private float _groundThickness = 0.2f;
        [SerializeField] private float _showcasePadMarginMetres = 4f;
        [SerializeField] private GameObject _showcaseHousePrefab;
        [SerializeField] private float _showcaseFacingDegrees = 180f;
        [SerializeField] private GameObject _roofPrefab;
        [SerializeField] private Material[] _wallMaterials = Array.Empty<Material>();
        [SerializeField] private Material[] _roofMaterials = Array.Empty<Material>();
        [SerializeField] private float _roofOverhangMetres = 0.4f;
        [SerializeField] private float _roofRisePerHalfSpan = 0.5f;
        [SerializeField] private GameObject _windowPrefab;
        [SerializeField] private GameObject _doorPrefab;
        [SerializeField] private GameObject _cornicePrefab;
        [SerializeField] private LandmarkMaterials _landmarkMaterials = new LandmarkMaterials();

        private NavMeshSurface _navMeshSurface;
        private Dictionary<FacadePieceKind, GameObject> _facadePrefabs;
        private Dictionary<PlotUse, IPlotDressing> _dressings;
        private RoofMaker _roofs;
        private PropFactory _props;

        /// <summary>Height of one storey in metres; garrison slots and the fog's skyline use the same value.</summary>
        public float StoreyHeightMetres => _storeyHeight;

        private void Awake()
        {
            CacheComponents();
        }

        private void OnDestroy()
        {
            if (_roofs == null)
            {
                return;
            }

            foreach (Mesh mesh in _roofs.Meshes)
            {
                Destroy(mesh);
            }
        }

        public void Construct(CityLayout layout, IReadOnlyList<StreetProp> streetProps)
        {
            if (layout == null || streetProps == null)
            {
                throw new ArgumentNullException(layout == null ? nameof(layout) : nameof(streetProps));
            }

            RequireAssets();
            CacheComponents();
            _facadePrefabs = new Dictionary<FacadePieceKind, GameObject>
            {
                { FacadePieceKind.Window, _windowPrefab },
                { FacadePieceKind.Door, _doorPrefab },
                { FacadePieceKind.Cornice, _cornicePrefab },
            };
            _dressings = CreateDressings();
            _roofs = new RoofMaker(_roofPrefab, new RoofShape(_roofOverhangMetres, _roofRisePerHalfSpan), transform);
            _props = new PropFactory(transform);

            SpawnGround(layout);
            foreach (Plot plot in layout.Plots)
            {
                SpawnPlot(plot, DressingFor(plot), layout.CellSize);
            }

            SpawnStreetProps(streetProps, layout.CellSize);
            SpawnShowcaseHouse(layout);
            _navMeshSurface.BuildNavMesh();

            // BuildNavMesh only registers the data once the surface is enabled. The bootstrapper runs before
            // that, and installers after it place units right away, so register now. AddData is idempotent.
            _navMeshSurface.AddData();
        }

        private Dictionary<PlotUse, IPlotDressing> CreateDressings()
        {
            return new Dictionary<PlotUse, IPlotDressing>
            {
                { PlotUse.House, new HouseDressing() },
                { PlotUse.Church, new ChurchDressing(_landmarkMaterials) },
                { PlotUse.School, new SchoolDressing(_landmarkMaterials) },
                { PlotUse.Tavern, new TavernDressing(_landmarkMaterials) },
                { PlotUse.Park, new ParkDressing(_landmarkMaterials) },
                { PlotUse.Water, new WaterDressing(_landmarkMaterials) },
                { PlotUse.Market, new MarketDressing(_landmarkMaterials) },
                { PlotUse.Plaza, new PlazaDressing(_landmarkMaterials) },
                { PlotUse.Garden, new GardenDressing(_landmarkMaterials) },
                { PlotUse.Yard, new YardDressing(_landmarkMaterials) },
            };
        }

        private void RequireAssets()
        {
            if (_groundPrefab == null || _buildingPrefab == null || _roofPrefab == null)
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} needs a ground, a building and a roof prefab.");
            }

            if (_windowPrefab == null || _doorPrefab == null || _cornicePrefab == null)
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} needs window, door and cornice prefabs. Run Stilargo → Build World Prefab.");
            }

            if (_showcaseHousePrefab == null)
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} needs the showcase house prefab. Run Stilargo → Build World Prefab.");
            }

            if (_landmarkMaterials == null || !_landmarkMaterials.IsComplete)
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} is missing landmark materials. Run Stilargo → Build World Prefab.");
            }
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

        private void SpawnStreetProps(IReadOnlyList<StreetProp> streetProps, float cellSize)
        {
            var builder = new StreetPropBuilder(_landmarkMaterials);
            foreach (StreetProp prop in streetProps)
            {
                builder.Build(prop, cellSize, _props);
            }
        }

        // The concept-art showcase house (built in Blender by a script under Tools/Art) stands outside the city on its own patch of cobbles.
        private void SpawnShowcaseHouse(CityLayout layout)
        {
            Plot plot = ShowcaseHouse.BesideCity(layout);
            var frame = new PlotFrame(plot, layout.CellSize, _storeyHeight);
            Vector3 position = frame.GroundCentre + (Vector3.down * (_groundThickness * Half));
            GameObject pad = Instantiate(_groundPrefab, position, Quaternion.identity, transform);
            float margin = _showcasePadMarginMetres + _showcasePadMarginMetres;
            pad.transform.localScale = new Vector3(frame.Width + margin, _groundThickness, frame.Depth + margin);
            Instantiate(_showcaseHousePrefab, frame.GroundCentre, Quaternion.Euler(0f, _showcaseFacingDegrees, 0f), transform);
        }

        private void SpawnPlot(Plot plot, IPlotDressing dressing, float cellSize)
        {
            var frame = new PlotFrame(plot, cellSize, _storeyHeight);
            if (plot.HasBuilding)
            {
                SpawnBuilding(frame, dressing);
            }

            dressing.Dress(frame, _props);
        }

        private IPlotDressing DressingFor(Plot plot)
        {
            if (!_dressings.TryGetValue(plot.Use, out IPlotDressing dressing))
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} has no look for {plot.Use} plots.");
            }

            return dressing;
        }

        private void SpawnBuilding(PlotFrame frame, IPlotDressing dressing)
        {
            float height = frame.BuildingHeight;
            Vector3 position = frame.GroundCentre + (Vector3.up * (height * Half));
            GameObject building = Instantiate(_buildingPrefab, position, Quaternion.identity, transform);
            building.transform.localScale = new Vector3(frame.Width, height, frame.Depth);
            Renderer walls = building.GetComponent<Renderer>();
            if (dressing.TryGetWallMaterial(out Material wall))
            {
                walls.sharedMaterial = wall;
            }
            else
            {
                ApplyVariant(walls, _wallMaterials, frame.Plot);
            }

            GameObject roof = _roofs.Build(new Vector3(position.x, height, position.z), new Vector2(frame.Width, frame.Depth));
            ApplyVariant(roof.GetComponent<Renderer>(), _roofMaterials, frame.Plot);
            SpawnFacade(frame);
        }

        private void SpawnFacade(PlotFrame frame)
        {
            foreach (FacadePiece piece in FacadeLayout.For(frame.Plot, frame.CellSize, _storeyHeight))
            {
                Vector3 position = new Vector3(piece.Position.X, piece.Position.Y, piece.Position.Z);
                Quaternion rotation = Quaternion.Euler(0f, piece.FacingDegrees, 0f);
                Instantiate(_facadePrefabs[piece.Kind], position, rotation, transform);
            }
        }

        private static void ApplyVariant(Renderer renderer, Material[] variants, Plot plot)
        {
            if (variants.Length == 0)
            {
                return;
            }

            renderer.sharedMaterial = variants[PlotVariation.Pick(plot, variants.Length)];
        }
    }
}
