using System;
using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.Formations
{
    /// <summary>
    /// Units in rows behind each other, roughly square (columns = ceil(sqrt(count))).
    /// Slots are filled front row first (toward the facing); a partial last row is centred.
    /// The grid's rows and columns are centred on the centre, so the slot centroid equals the centre
    /// whenever the grid is full. A facing without ground direction falls back to
    /// <see cref="GroundPlane.DefaultForward"/>.
    /// </summary>
    public sealed class BoxFormation : IFormation
    {
        private readonly float _spacing;

        public BoxFormation(float spacing)
        {
            _spacing = FormationGuard.PositiveSpacing(spacing);
        }

        public IReadOnlyList<Vector3> SlotsFor(int count, Vector3 center, Vector3 facing)
        {
            FormationGuard.NonNegativeCount(count);

            if (count == 0)
            {
                return Array.Empty<Vector3>();
            }

            Vector3 forward = GroundPlane.ForwardOf(facing);
            Vector3 right = GroundPlane.RightOf(forward);
            int columns = (int)Math.Ceiling(Math.Sqrt(count));
            int rows = (count + columns - 1) / columns;

            var slots = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                int row = i / columns;
                int column = i % columns;
                int unitsInRow = Math.Min(columns, count - (row * columns));

                float ahead = CenteredOffset(rows - 1 - row, rows);
                float across = CenteredOffset(column, unitsInRow);
                slots[i] = center + (forward * ahead) + (right * across);
            }

            return slots;
        }

        /// <summary>Offset of position <paramref name="index"/> in a run of <paramref name="length"/> centred on zero.</summary>
        private float CenteredOffset(int index, int length)
        {
            return (index - ((length - 1) / 2f)) * _spacing;
        }
    }
}
