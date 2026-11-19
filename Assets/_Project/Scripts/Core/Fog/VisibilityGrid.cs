using System;
using System.Collections.Generic;

namespace Stilargo.Core.Fog
{
    /// <summary>
    /// Fog of war state per cell. Cell (0, 0) starts at the world origin; cell x spans world X
    /// [x * CellSize, (x + 1) * CellSize) and cell y spans world Z the same way.
    /// </summary>
    public sealed class VisibilityGrid
    {
        private const float HalfCell = 0.5f;

        private readonly VisibilityState[] _states;
        private readonly List<IVisionSource> _sources = new List<IVisionSource>();

        public VisibilityGrid(int width, int height, float cellSize)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive.");
            }

            if (!(cellSize > 0f) || float.IsInfinity(cellSize))
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "Cell size must be a positive finite number.");
            }

            Width = width;
            Height = height;
            CellSize = cellSize;
            _states = new VisibilityState[width * height];
        }

        public event Action Refreshed;

        public int Width { get; }

        public int Height { get; }

        public float CellSize { get; }

        public void Register(IVisionSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (_sources.Contains(source))
            {
                throw new InvalidOperationException("Vision source is already registered.");
            }

            _sources.Add(source);
        }

        public bool Unregister(IVisionSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return _sources.Remove(source);
        }

        public void Refresh()
        {
            DemoteVisibleCells();
            foreach (IVisionSource source in _sources)
            {
                RevealAround(source);
            }

            Refreshed?.Invoke();
        }

        public VisibilityState GetState(int x, int y)
        {
            if (!IsInside(x, y))
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"Cell ({x}, {y}) is outside the {Width}x{Height} grid.");
            }

            return _states[ToIndex(x, y)];
        }

        /// <summary>Positions outside the grid count as not visible, so off-map objects stay hidden.</summary>
        public bool IsVisible(WorldPoint position)
        {
            int x = ToCell(position.X);
            int y = ToCell(position.Z);
            return IsInside(x, y) && _states[ToIndex(x, y)] == VisibilityState.Visible;
        }

        private void DemoteVisibleCells()
        {
            for (int i = 0; i < _states.Length; i++)
            {
                if (_states[i] == VisibilityState.Visible)
                {
                    _states[i] = VisibilityState.Explored;
                }
            }
        }

        private void RevealAround(IVisionSource source)
        {
            float radius = source.VisionRadius;
            if (!(radius > 0f))
            {
                return;
            }

            WorldPoint center = source.Position;
            int minX = Math.Max(0, ToCell(center.X - radius));
            int maxX = Math.Min(Width - 1, ToCell(center.X + radius));
            int minY = Math.Max(0, ToCell(center.Z - radius));
            int maxY = Math.Min(Height - 1, ToCell(center.Z + radius));
            float radiusSquared = radius * radius;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (DistanceSquaredToCellCenter(center, x, y) <= radiusSquared)
                    {
                        _states[ToIndex(x, y)] = VisibilityState.Visible;
                    }
                }
            }
        }

        private float DistanceSquaredToCellCenter(WorldPoint point, int x, int y)
        {
            float dx = ((x + HalfCell) * CellSize) - point.X;
            float dz = ((y + HalfCell) * CellSize) - point.Z;
            return (dx * dx) + (dz * dz);
        }

        private int ToCell(float worldCoordinate)
        {
            return (int)Math.Floor(worldCoordinate / CellSize);
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        private int ToIndex(int x, int y)
        {
            return (y * Width) + x;
        }
    }
}
