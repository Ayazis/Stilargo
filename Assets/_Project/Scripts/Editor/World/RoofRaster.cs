using UnityEngine;

namespace Stilargo.Editor.World
{
    /// <summary>A grid of heights seen from above that triangles are drawn into, keeping the highest per cell centre.</summary>
    internal sealed class RoofRaster
    {
        private const float Half = 0.5f;

        // Walls seen edge-on from above cover no area; skipping them avoids dividing by zero.
        private const float MinimumFootprintArea = 1e-6f;

        private readonly float _minX;
        private readonly float _minZ;
        private readonly float _cellSize;
        private readonly int _columns;
        private readonly int _rows;

        public RoofRaster(float minX, float minZ, float cellSize, int columns, int rows)
        {
            _minX = minX;
            _minZ = minZ;
            _cellSize = cellSize;
            _columns = columns;
            _rows = rows;
            Heights = new float[columns * rows];
            for (int index = 0; index < Heights.Length; index++)
            {
                Heights[index] = float.NaN;
            }
        }

        /// <summary>Row by row from the min-Z row; NaN where no triangle covers a cell's centre.</summary>
        public float[] Heights { get; }

        /// <summary>Lifts every cell whose centre the triangle covers to the triangle's height there.</summary>
        public void Raise(Vector3 a, Vector3 b, Vector3 c)
        {
            float area = Cross(a, b, c);
            if (Mathf.Abs(area) < MinimumFootprintArea)
            {
                return;
            }

            int firstColumn = Mathf.Max(0, Cell(Mathf.Min(a.x, b.x, c.x), _minX));
            int lastColumn = Mathf.Min(_columns - 1, Cell(Mathf.Max(a.x, b.x, c.x), _minX));
            int firstRow = Mathf.Max(0, Cell(Mathf.Min(a.z, b.z, c.z), _minZ));
            int lastRow = Mathf.Min(_rows - 1, Cell(Mathf.Max(a.z, b.z, c.z), _minZ));
            var triangle = new[] { a, b, c };
            for (int row = firstRow; row <= lastRow; row++)
            {
                for (int column = firstColumn; column <= lastColumn; column++)
                {
                    var centre = new Vector3(_minX + ((column + Half) * _cellSize), 0f, _minZ + ((row + Half) * _cellSize));
                    RaiseCell((row * _columns) + column, centre, triangle, area);
                }
            }
        }

        private void RaiseCell(int cell, Vector3 centre, Vector3[] triangle, float area)
        {
            float weightA = Cross(triangle[1], triangle[2], centre) / area;
            float weightB = Cross(triangle[2], triangle[0], centre) / area;
            float weightC = 1f - weightA - weightB;
            if (weightA < 0f || weightB < 0f || weightC < 0f)
            {
                return;
            }

            float height = (weightA * triangle[0].y) + (weightB * triangle[1].y) + (weightC * triangle[2].y);
            if (float.IsNaN(Heights[cell]) || height > Heights[cell])
            {
                Heights[cell] = height;
            }
        }

        private int Cell(float coordinate, float min)
        {
            return Mathf.FloorToInt((coordinate - min) / _cellSize);
        }

        // Twice the signed area of the triangle (p, q, r) seen from above.
        private static float Cross(Vector3 p, Vector3 q, Vector3 r)
        {
            return ((q.x - p.x) * (r.z - p.z)) - ((q.z - p.z) * (r.x - p.x));
        }
    }
}
