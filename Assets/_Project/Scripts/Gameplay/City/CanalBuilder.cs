using System;
using System.Linq;
using Stilargo.Core.City;
using Stilargo.Core.City.CanalTown;
using Unity.AI.Navigation;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// Builds the sunken canal across the whole map: a bed below the street, the water surface, a quay wall along each
    /// bank (with steps down now and then) and a stone bridge where each bridge street crosses. The ground is left open
    /// over the canal's band, so <see cref="CitySpawner"/> lays it on either side.
    /// </summary>
    internal sealed class CanalBuilder
    {
        private const string CanalName = "Canal";
        private const int NotWalkableArea = 1;
        private const float Half = 0.5f;
        private const float WaterThicknessMetres = 0.05f;
        private const float BedThicknessMetres = 0.2f;
        private const float HalfTurn = 180f;
        private const int SegmentsPerBoat = 3;
        private const float BoatLengthMetres = 4.2f;
        private const float BoatOffsetMetres = 1.4f;

        // The quay's coping and paving sit at street level; lifting them clear of the ground keeps them from z-fighting it.
        private const float QuayLiftMetres = 0.02f;

        private readonly Canal _canal;
        private readonly CanalModels _models;
        private readonly LandmarkMaterials _materials;
        private readonly float _cellSize;

        public CanalBuilder(Canal canal, CanalModels models, LandmarkMaterials materials, float cellSize)
        {
            _canal = canal ?? throw new ArgumentNullException(nameof(canal));
            _models = models ?? throw new ArgumentNullException(nameof(models));
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _cellSize = cellSize;
        }

        /// <summary>South bank of the canal in world metres (z).</summary>
        public float SouthBank => _canal.Water.Min.Y * _cellSize;

        /// <summary>North bank of the canal in world metres (z).</summary>
        public float NorthBank => _canal.Water.YMax * _cellSize;

        /// <summary>True for the water plots that make up the canal, which the canal draws itself.</summary>
        public bool IsCanal(Plot plot)
        {
            return plot.Use == PlotUse.Water && plot.Area.Min.Y == _canal.Water.Min.Y && plot.Area.YMax == _canal.Water.YMax;
        }

        /// <summary>Builds the canal; every few quay segments a <paramref name="boat"/> is moored below the wall.</summary>
        public void Build(Transform parent, float mapWidthMetres, GameObject boat)
        {
            var root = new GameObject(CanalName);
            root.transform.SetParent(parent, false);
            BuildBedAndWater(root.transform, mapWidthMetres);
            BuildBank(root.transform, mapWidthMetres, SouthBank, Quaternion.identity);
            BuildBank(root.transform, mapWidthMetres, NorthBank, Quaternion.Euler(0f, HalfTurn, 0f));
            MoorBoats(root.transform, mapWidthMetres, boat);
            foreach (int bridgeX in _canal.Bridges)
            {
                float x = (bridgeX + (_canal.BridgeWidthCells * Half)) * _cellSize;
                UnityEngine.Object.Instantiate(_models.Bridge, new Vector3(x, 0f, (SouthBank + NorthBank) * Half), Quaternion.identity, root.transform);
            }
        }

        private void BuildBedAndWater(Transform root, float mapWidthMetres)
        {
            var props = new PropFactory(root);
            float width = NorthBank - SouthBank;
            var centre = new Vector3(mapWidthMetres * Half, 0f, (SouthBank + NorthBank) * Half);
            GameObject bed = props.Box(centre + (Vector3.down * (_models.BedDepthMetres + (BedThicknessMetres * Half))),
                new Vector3(mapWidthMetres, BedThicknessMetres, width), _materials.Stone);
            var modifier = bed.AddComponent<NavMeshModifier>();
            modifier.overrideArea = true;
            modifier.area = NotWalkableArea;
            GameObject water = props.Box(centre + (Vector3.down * _models.WaterDepthMetres), new Vector3(mapWidthMetres, WaterThicknessMetres, width),
                _materials.Water);
            PropFactory.IgnoredByNavigation(water);
        }

        // Quay segments along one bank, left out where a bridge street meets the water.
        private void BuildBank(Transform root, float mapWidthMetres, float bankZ, Quaternion facingWater)
        {
            float length = _models.SegmentLengthMetres;
            int count = Mathf.FloorToInt(mapWidthMetres / length);
            for (int index = 0; index < count; index++)
            {
                float x = (index + Half) * length;
                if (UnderBridge(x, length))
                {
                    continue;
                }

                GameObject piece = index % _models.SegmentsPerSteps == _models.SegmentsPerSteps / 2 ? _models.QuaySteps : _models.QuayWall;
                // The ground beside the canal is what units walk on; the quay is only seen.
                PropFactory.IgnoredByNavigation(UnityEngine.Object.Instantiate(piece, new Vector3(x, QuayLiftMetres, bankZ), facingWater, root));
            }
        }

        private void MoorBoats(Transform root, float mapWidthMetres, GameObject boat)
        {
            var props = new PropFactory(root);
            float spacing = _models.SegmentLengthMetres * SegmentsPerBoat;
            for (float x = spacing * Half; x < mapWidthMetres; x += spacing)
            {
                bool south = Mathf.RoundToInt(x / spacing) % 2 == 0;
                if (UnderBridge(x, BoatLengthMetres))
                {
                    continue;
                }

                float z = south ? SouthBank + BoatOffsetMetres : NorthBank - BoatOffsetMetres;
                props.Model(boat, new Vector3(x, -_models.WaterDepthMetres, z), Quaternion.identity);
            }
        }

        private bool UnderBridge(float segmentCentre, float segmentLength)
        {
            float half = segmentLength * Half;
            return _canal.Bridges.Any(bridgeX =>
            {
                float west = bridgeX * _cellSize;
                float east = (bridgeX + _canal.BridgeWidthCells) * _cellSize;
                return segmentCentre + half > west && segmentCentre - half < east;
            });
        }
    }
}
