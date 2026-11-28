using System;

namespace Stilargo.Core.Fog
{
    /// <summary>
    /// Lets a fog grid finer than the city ask a city-cell blocker: each coarse cell is split into
    /// <c>fogCellsPerCityCell</c> x <c>fogCellsPerCityCell</c> fog cells that share its obstacle height.
    /// </summary>
    public sealed class SubdividedSightBlocker : ISightBlocker
    {
        private readonly ISightBlocker _cityBlocker;
        private readonly int _fogCellsPerCityCell;

        public SubdividedSightBlocker(ISightBlocker cityBlocker, int fogCellsPerCityCell)
        {
            if (fogCellsPerCityCell < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(fogCellsPerCityCell), fogCellsPerCityCell, "Must be at least 1.");
            }

            _cityBlocker = cityBlocker ?? throw new ArgumentNullException(nameof(cityBlocker));
            _fogCellsPerCityCell = fogCellsPerCityCell;
        }

        public float ObstacleHeightAt(int cellX, int cellY)
        {
            return _cityBlocker.ObstacleHeightAt(ToCityCell(cellX), ToCityCell(cellY));
        }

        // Floor division, so fog cells left of or below the map still map to negative (outside) city cells.
        private int ToCityCell(int fogCell)
        {
            return (int)Math.Floor((double)fogCell / _fogCellsPerCityCell);
        }
    }
}
