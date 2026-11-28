using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.City;
using Stilargo.Core.City.CanalTown;
using Unity.AI.Navigation;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// Turns a <see cref="CityLayout"/> into buildings (the Blender-built model for each house style and landmark,
    /// over an invisible box that units garrison and the camera picks), the sunken canal with its quays and bridges,
    /// parks, plazas, gardens, yards and street life (lamps, trees, clutter), then bakes the NavMesh over it.
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
        [SerializeField] private LandmarkMaterials _landmarkMaterials = new LandmarkMaterials();
        [SerializeField] private BuildingModels _buildingModels = new BuildingModels();
        [SerializeField] private CanalModels _canalModels = new CanalModels();
        [SerializeField] private PropModels _propModels = new PropModels();
        [SerializeField] private float _entryMarkerOffsetCells = 3f;

        private NavMeshSurface _navMeshSurface;
        private Dictionary<PlotUse, IPlotDressing> _dressings;
        private PropFactory _props;
        private CanalBuilder _canal;

        /// <summary>Height of one storey in metres; garrison slots and the fog's skyline use the same value.</summary>
        public float StoreyHeightMetres => _storeyHeight;

        private void Awake()
        {
            CacheComponents();
        }

        /// <summary>Builds the planned <paramref name="layout"/> of <paramref name="town"/>, whose canal and entries it also draws.</summary>
        public void Construct(CityLayout layout, IReadOnlyList<StreetProp> streetProps, CanalTown town)
        {
            if (layout == null || streetProps == null || town == null)
            {
                throw new ArgumentNullException(layout == null ? nameof(layout) : streetProps == null ? nameof(streetProps) : nameof(town));
            }

            RequireAssets();
            CacheComponents();
            _dressings = CreateDressings();
            _props = new PropFactory(transform);
            _canal = new CanalBuilder(town.Canal, _canalModels, _landmarkMaterials, layout.CellSize);

            SpawnGround(layout);
            _canal.Build(transform, layout.Width * layout.CellSize, _propModels[PropKind.RowingBoat]);
            ShowcaseSite showcase = ShowcaseHouse.Place(layout);
            foreach (Plot plot in layout.Plots.Where(plot => !_canal.IsCanal(plot) && plot != showcase.Replaced))
            {
                SpawnPlot(plot, layout.CellSize);
            }

            SpawnStreetProps(streetProps, layout.CellSize);
            SpawnEntryMarkers(town, layout.CellSize);
            SpawnShowcaseHouse(showcase.House, layout.CellSize);
            _navMeshSurface.BuildNavMesh();

            // BuildNavMesh only registers the data once the surface is enabled. The bootstrapper runs before
            // that, and installers after it place units right away, so register now. AddData is idempotent.
            _navMeshSurface.AddData();
        }

        private Dictionary<PlotUse, IPlotDressing> CreateDressings()
        {
            return new Dictionary<PlotUse, IPlotDressing>
            {
                { PlotUse.Park, new ParkDressing(_landmarkMaterials, _propModels) },
                { PlotUse.Water, new WaterDressing(_landmarkMaterials, _propModels) },
                { PlotUse.Market, new MarketDressing(_landmarkMaterials, _propModels) },
                { PlotUse.Plaza, new PlazaDressing(_landmarkMaterials, _propModels) },
                { PlotUse.Garden, new GardenDressing(_landmarkMaterials, _propModels) },
                { PlotUse.Yard, new YardDressing(_landmarkMaterials, _propModels) },
                { PlotUse.Fountain, new FountainDressing(_propModels) },
                { PlotUse.Meadow, new MeadowDressing(_landmarkMaterials, _propModels) },
                { PlotUse.Field, new FieldDressing(_landmarkMaterials, _propModels) },
                { PlotUse.Orchard, new OrchardDressing(_landmarkMaterials, _propModels) },
            };
        }

        private void RequireAssets()
        {
            if (_groundPrefab == null || _buildingPrefab == null)
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} needs a ground and a building prefab.");
            }

            if (_showcaseHousePrefab == null)
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} needs the showcase house prefab. Run Stilargo → Build World Prefab.");
            }

            if (_buildingModels == null || !_buildingModels.IsComplete || _canalModels == null || !_canalModels.IsComplete
                || _propModels == null || !_propModels.IsComplete)
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} is missing building, canal or prop models. Run Stilargo → Build Canal Town Prefabs, then Stilargo → Build World Prefab.");
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

        // Ground south and north of the canal; the canal's band is left open down to its bed.
        private void SpawnGround(CityLayout layout)
        {
            float width = layout.Width * layout.CellSize;
            float depth = layout.Height * layout.CellSize;
            SpawnGroundStrip(width, 0f, _canal.SouthBank);
            SpawnGroundStrip(width, _canal.NorthBank, depth);
        }

        private void SpawnGroundStrip(float width, float southZ, float northZ)
        {
            var position = new Vector3(width * Half, -_groundThickness * Half, (southZ + northZ) * Half);
            GameObject ground = Instantiate(_groundPrefab, position, Quaternion.identity, transform);
            ground.transform.localScale = new Vector3(width, _groundThickness, northZ - southZ);
        }

        private void SpawnStreetProps(IReadOnlyList<StreetProp> streetProps, float cellSize)
        {
            var builder = new StreetPropBuilder(_propModels);
            foreach (StreetProp prop in streetProps)
            {
                builder.Build(prop, cellSize, _props);
            }
        }

        // A town sign and a milestone flank each high road where the soldiers come in, with a wayside shrine on the
        // player's side. Each faces the road.
        private void SpawnEntryMarkers(CanalTown town, float cellSize)
        {
            SpawnEntryMarker(town.PlayerEntry, cellSize, PropKind.WaysideShrine);
            SpawnEntryMarker(town.RivalEntry, cellSize, PropKind.Milestone);
        }

        private void SpawnEntryMarker(GridPosition entry, float cellSize, PropKind second)
        {
            bool fromWest = entry.X == 0;
            float x = (entry.X + Half + (fromWest ? _entryMarkerOffsetCells : -_entryMarkerOffsetCells)) * cellSize;
            float roadZ = (entry.Y + Half) * cellSize;
            float offset = _entryMarkerOffsetCells * cellSize;
            _props.Model(_propModels[PropKind.TownSign], new Vector3(x, 0f, roadZ + offset), FacingRotation.Of(Facing.South));
            _props.Model(_propModels[second], new Vector3(x, 0f, roadZ - offset), FacingRotation.Of(Facing.North));
        }

        // The concept-art showcase house (built in Blender by a script under Tools/Art) stands in the countryside on its own patch of cobbles.
        private void SpawnShowcaseHouse(Plot plot, float cellSize)
        {
            var frame = new PlotFrame(plot, cellSize, _storeyHeight);
            Vector3 position = frame.GroundCentre + (Vector3.down * (_groundThickness * Half));
            GameObject pad = Instantiate(_groundPrefab, position, Quaternion.identity, transform);
            float margin = _showcasePadMarginMetres + _showcasePadMarginMetres;
            pad.transform.localScale = new Vector3(frame.Width + margin, _groundThickness, frame.Depth + margin);
            PropFactory.IgnoredByNavigation(Instantiate(_showcaseHousePrefab, frame.GroundCentre, Quaternion.Euler(0f, _showcaseFacingDegrees, 0f), transform));
        }

        // A building's model carries everything its old dressing added (doors, towers, flags), so only open ground is dressed.
        private void SpawnPlot(Plot plot, float cellSize)
        {
            var frame = new PlotFrame(plot, cellSize, _storeyHeight);
            if (plot.HasBuilding)
            {
                SpawnBuilding(frame);
                return;
            }

            DressingFor(plot).Dress(frame, _props);
        }

        private IPlotDressing DressingFor(Plot plot)
        {
            if (!_dressings.TryGetValue(plot.Use, out IPlotDressing dressing))
            {
                throw new InvalidOperationException($"{nameof(CitySpawner)} has no look for {plot.Use} plots.");
            }

            return dressing;
        }

        // The box stays as the building's collider, selection volume and NavMesh obstacle; the model is only seen.
        private void SpawnBuilding(PlotFrame frame)
        {
            float height = frame.BuildingHeight;
            Vector3 position = frame.GroundCentre + (Vector3.up * (height * Half));
            GameObject building = Instantiate(_buildingPrefab, position, Quaternion.identity, transform);
            building.transform.localScale = new Vector3(frame.Width, height, frame.Depth);
            building.GetComponent<Renderer>().enabled = false;
            PropFactory.IgnoredByNavigation(Instantiate(_buildingModels.For(frame.Plot), frame.GroundCentre, FacingRotation.Of(frame.Plot.Front), transform));
        }

    }
}
