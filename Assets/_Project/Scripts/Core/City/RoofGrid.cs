using System;
using System.Collections.Generic;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Heights of a model's top surface on a square grid in the model's own frame. Cells the model doesn't cover hold
    /// <see cref="float.NaN"/>.
    /// </summary>
    public sealed class RoofGrid
    {
        private readonly float _minX;
        private readonly float _minZ;
        private readonly float _cellSize;
        private readonly int _columns;
        private readonly int _rows;
        private readonly float[] _heights;

        /// <param name="heights">Row by row, starting at the min-Z row; <paramref name="columns"/> per row.</param>
        public RoofGrid(float minX, float minZ, float cellSize, int columns, IReadOnlyList<float> heights)
        {
            if (!(cellSize > 0f) || columns <= 0 || heights == null || heights.Count == 0 || heights.Count % columns != 0)
            {
                throw new ArgumentException($"A roof grid needs a positive cell size ({cellSize}) and whole rows of {columns} heights.");
            }

            _minX = minX;
            _minZ = minZ;
            _cellSize = cellSize;
            _columns = columns;
            _rows = heights.Count / columns;
            _heights = new float[heights.Count];
            for (int index = 0; index < heights.Count; index++)
            {
                _heights[index] = heights[index];
            }
        }

        public bool TryGetHeight(float x, float z, out float height)
        {
            int column = (int)Math.Floor((x - _minX) / _cellSize);
            int row = (int)Math.Floor((z - _minZ) / _cellSize);
            height = 0f;
            if (column < 0 || row < 0 || column >= _columns || row >= _rows)
            {
                return false;
            }

            height = _heights[(row * _columns) + column];
            return !float.IsNaN(height);
        }
    }
}
