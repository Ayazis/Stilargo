using System;
using Stilargo.Core.Fog;

namespace Stilargo.Core.City
{
    /// <summary>
    /// The height of whatever stands on each city cell, so buildings block the fog of war's line of sight.
    /// Streets, parks and water are open; a bell tower is taller than the building it rises from.
    /// </summary>
    public sealed class CitySkyline : ISightBlocker
    {
        private readonly float[] _heights;
        private readonly int _width;
        private readonly int _height;

        public CitySkyline(CityLayout layout, float storeyHeightMetres)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            if (!(storeyHeightMetres > 0f) || float.IsInfinity(storeyHeightMetres))
            {
                throw new ArgumentOutOfRangeException(nameof(storeyHeightMetres), storeyHeightMetres, "Storey height must be a positive number.");
            }

            _width = layout.Width;
            _height = layout.Height;
            _heights = new float[_width * _height];
            foreach (Plot plot in layout.Plots)
            {
                Raise(plot, storeyHeightMetres);
            }
        }

        public float ObstacleHeightAt(int cellX, int cellY)
        {
            if (cellX < 0 || cellX >= _width || cellY < 0 || cellY >= _height)
            {
                return 0f;
            }

            return _heights[(cellY * _width) + cellX];
        }

        private void Raise(Plot plot, float storeyHeightMetres)
        {
            if (!plot.HasBuilding)
            {
                return;
            }

            Fill(plot.Area, plot.Storeys * storeyHeightMetres);
            if (plot.HasTower)
            {
                Fill(BellTower.AreaOf(plot), plot.TowerStoreys * storeyHeightMetres);
            }
        }

        private void Fill(GridRect area, float heightMetres)
        {
            foreach (GridPosition cell in area.Cells())
            {
                if (cell.X >= 0 && cell.X < _width && cell.Y >= 0 && cell.Y < _height)
                {
                    _heights[(cell.Y * _width) + cell.X] = heightMetres;
                }
            }
        }
    }
}
