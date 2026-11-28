using System;
using Stilargo.Core.City.CanalTown;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The <c>canalTown</c> section of <c>Town.json</c>. Defaults match a new <see cref="CanalTownSettings"/>.</summary>
    [Serializable]
    public sealed class CanalTownEntry
    {
        private static readonly CanalTownSettings Defaults = new CanalTownSettings();

        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private int mapWidthCells = Defaults.WidthCells;
        [SerializeField] private int mapHeightCells = Defaults.HeightCells;
        [SerializeField] private float cellSizeMetres = Defaults.CellSizeMetres;
        [SerializeField] private int countrysideCells = Defaults.CountrysideCells;
        [SerializeField] private int minParcelCells = Defaults.MinParcelCells;
        [SerializeField] private int maxParcelCells = Defaults.MaxParcelCells;
        [SerializeField] private int farmhouseCells = Defaults.FarmhouseCells;
        [SerializeField] private int farmyardCells = Defaults.FarmyardCells;
        [SerializeField] private int streetWidthCells = Defaults.StreetWidthCells;
        [SerializeField] private int highRoadWidthCells = Defaults.HighRoadWidthCells;
        [SerializeField] private int canalWidthCells = Defaults.CanalWidthCells;
        [SerializeField] private float canalPositionShare = Defaults.CanalPositionShare;
        [SerializeField] private int bridgeCount = Defaults.BridgeCount;
        [SerializeField] private int minBlockCells = Defaults.MinBlockCells;
        [SerializeField] private int maxBlockCells = Defaults.MaxBlockCells;
        [SerializeField] private int houseDepthCells = Defaults.HouseDepthCells;
        [SerializeField] private int minFrontageCells = Defaults.MinFrontageCells;
        [SerializeField] private int maxFrontageCells = Defaults.MaxFrontageCells;
        [SerializeField] private int minStoreys = Defaults.MinStoreys;
        [SerializeField] private int maxStoreys = Defaults.MaxStoreys;
        [SerializeField] private int prominentExtraStoreys = Defaults.ProminentExtraStoreys;
        [SerializeField] private int parkCount = Defaults.ParkCount;
        [SerializeField] private float detachedEdgeShare = Defaults.DetachedEdgeShare;
        [SerializeField] private int detachedLotCells = Defaults.DetachedLotCells;

        public CanalTownSettings ToSettings()
        {
            return new CanalTownSettings
            {
                WidthCells = mapWidthCells,
                HeightCells = mapHeightCells,
                CellSizeMetres = cellSizeMetres,
                CountrysideCells = countrysideCells,
                MinParcelCells = minParcelCells,
                MaxParcelCells = maxParcelCells,
                FarmhouseCells = farmhouseCells,
                FarmyardCells = farmyardCells,
                StreetWidthCells = streetWidthCells,
                HighRoadWidthCells = highRoadWidthCells,
                CanalWidthCells = canalWidthCells,
                CanalPositionShare = canalPositionShare,
                BridgeCount = bridgeCount,
                MinBlockCells = minBlockCells,
                MaxBlockCells = maxBlockCells,
                HouseDepthCells = houseDepthCells,
                MinFrontageCells = minFrontageCells,
                MaxFrontageCells = maxFrontageCells,
                MinStoreys = minStoreys,
                MaxStoreys = maxStoreys,
                ProminentExtraStoreys = prominentExtraStoreys,
                ParkCount = parkCount,
                DetachedEdgeShare = detachedEdgeShare,
                DetachedLotCells = detachedLotCells,
            };
        }
    }
}
