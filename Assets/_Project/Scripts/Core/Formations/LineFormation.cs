using System;
using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.Formations
{
    /// <summary>
    /// Units side by side in one row, perpendicular to the facing and centred on the centre.
    /// A facing without ground direction falls back to <see cref="GroundPlane.DefaultForward"/>.
    /// </summary>
    public sealed class LineFormation : IFormation
    {
        private readonly float _spacing;

        public LineFormation(float spacing)
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

            Vector3 right = GroundPlane.RightOf(GroundPlane.ForwardOf(facing));
            float firstOffset = -(count - 1) * _spacing / 2f;

            var slots = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                slots[i] = center + (right * (firstOffset + (i * _spacing)));
            }

            return slots;
        }
    }
}
