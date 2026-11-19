using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.Formations
{
    /// <summary>
    /// Arranges a group of units around a destination. Positions are world space, Y up.
    /// </summary>
    public interface IFormation
    {
        /// <summary>
        /// Returns exactly <paramref name="count"/> slot positions centred on <paramref name="center"/>,
        /// with the front of the formation facing along <paramref name="facing"/> (ground plane, Y ignored).
        /// Every slot has the centre's height.
        /// </summary>
        IReadOnlyList<Vector3> SlotsFor(int count, Vector3 center, Vector3 facing);
    }
}
