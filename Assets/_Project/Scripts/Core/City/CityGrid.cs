using System;

namespace Stilargo.Core.City
{
    /// <summary>Dimensions of the city grid plus a mutable road mask used while generating.</summary>
    public sealed class CityGrid
    {
        private readonly bool[] _roadCells;

        public CityGrid(int width, int height, float cellSize)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive.");
            }

            if (cellSize <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "Cell size must be positive.");
            }

            Width = width;
            Height = height;
            CellSize = cellSize;
            _roadCells = new bool[width * height];
        }

        public int Width { get; }

        public int Height { get; }

        public float CellSize { get; }

        public void MarkRoad(GridPosition position)
        {
            _roadCells[IndexOf(position)] = true;
        }

        /// <summary>Turns a road cell back into building ground, e.g. where two blocks are merged into one.</summary>
        public void ClearRoad(GridPosition position)
        {
            _roadCells[IndexOf(position)] = false;
        }

        private int IndexOf(GridPosition position)
        {
            if (position.X < 0 || position.X >= Width || position.Y < 0 || position.Y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "Position is outside the grid.");
            }

            return (position.Y * Width) + position.X;
        }

        internal bool[] CopyRoadCells()
        {
            return (bool[])_roadCells.Clone();
        }
    }
}
