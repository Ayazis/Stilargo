using System;
using System.Collections.Generic;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Immutable result of city generation: a grid of cells split into roads and blocks,
    /// with blocks subdivided into building plots. Cell (0, 0) is the south-west corner.
    /// </summary>
    public sealed class CityLayout
    {
        private readonly bool[] _roadCells;

        public CityLayout(CityGrid grid, IReadOnlyList<GridRect> blocks, IReadOnlyList<Plot> plots)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            Blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
            Plots = plots ?? throw new ArgumentNullException(nameof(plots));
            Width = grid.Width;
            Height = grid.Height;
            CellSize = grid.CellSize;
            _roadCells = grid.CopyRoadCells();
        }

        /// <summary>Grid width in cells.</summary>
        public int Width { get; }

        /// <summary>Grid height in cells.</summary>
        public int Height { get; }

        /// <summary>World size of one cell edge, in metres.</summary>
        public float CellSize { get; }

        public IReadOnlyList<GridRect> Blocks { get; }

        public IReadOnlyList<Plot> Plots { get; }

        public bool IsInside(GridPosition position)
        {
            return position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
        }

        /// <summary>True when the cell is road. Cells outside the grid are never road.</summary>
        public bool IsRoad(GridPosition position)
        {
            if (!IsInside(position))
            {
                return false;
            }

            return _roadCells[(position.Y * Width) + position.X];
        }

        /// <summary>True when the world point (metres, city corner at the origin) lies on a road cell.</summary>
        public bool IsRoadAt(float worldX, float worldZ)
        {
            GridPosition cell = new GridPosition(
                (int)Math.Floor(worldX / CellSize),
                (int)Math.Floor(worldZ / CellSize));
            return IsRoad(cell);
        }

        public IEnumerable<GridPosition> RoadCells()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    if (IsRoad(position))
                    {
                        yield return position;
                    }
                }
            }
        }
    }
}
