using System;
using System.Collections.Generic;

namespace Stilargo.Core.City
{
    /// <summary>An axis-aligned rectangle of cells. Min is inclusive, Min + Size is exclusive.</summary>
    public readonly struct GridRect : IEquatable<GridRect>
    {
        public GridRect(GridPosition min, int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive.");
            }

            Min = min;
            Width = width;
            Height = height;
        }

        public GridPosition Min { get; }

        public int Width { get; }

        public int Height { get; }

        public int XMax => Min.X + Width;

        public int YMax => Min.Y + Height;

        public bool Contains(GridPosition position)
        {
            return position.X >= Min.X && position.X < XMax && position.Y >= Min.Y && position.Y < YMax;
        }

        public bool Overlaps(GridRect other)
        {
            return Min.X < other.XMax && other.Min.X < XMax && Min.Y < other.YMax && other.Min.Y < YMax;
        }

        public IEnumerable<GridPosition> Cells()
        {
            for (int y = Min.Y; y < YMax; y++)
            {
                for (int x = Min.X; x < XMax; x++)
                {
                    yield return new GridPosition(x, y);
                }
            }
        }

        public bool Equals(GridRect other)
        {
            return Min == other.Min && Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is GridRect other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Min, Width, Height);
        }

        public override string ToString()
        {
            return $"[{Min} {Width}x{Height}]";
        }
    }
}
