using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>Designer-facing data for <see cref="CityConfig"/>. Validation lives in Core.</summary>
    [CreateAssetMenu(fileName = "CityConfig", menuName = "Stilargo/City Config")]
    public sealed class CityConfigAsset : ScriptableObject
    {
        [SerializeField] private int _blocksX = 5;
        [SerializeField] private int _blocksY = 5;
        [SerializeField] private int _blockSize = 12;
        [SerializeField] private int _roadWidth = 2;
        [SerializeField] private int _minPlotSize = 3;
        [SerializeField] private int _maxPlotSize = 6;
        [SerializeField] private int _minStoreys = 1;
        [SerializeField] private int _maxStoreys = 2;
        [SerializeField] private float _cellSize = 2f;

        public CityConfig ToConfig()
        {
            CityConfig config = new CityConfig
            {
                BlocksX = _blocksX,
                BlocksY = _blocksY,
                BlockSize = _blockSize,
                RoadWidth = _roadWidth,
                MinPlotSize = _minPlotSize,
                MaxPlotSize = _maxPlotSize,
                MinStoreys = _minStoreys,
                MaxStoreys = _maxStoreys,
                CellSize = _cellSize,
            };
            config.Validate();
            return config;
        }
    }
}
