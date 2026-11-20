using System.Numerics;

namespace Stilargo.Core.Combat
{
    /// <summary>
    /// Combat distances ignore height so units on slopes or stairs are not treated as out of range.
    /// </summary>
    public static class GroundDistance
    {
        public static float Between(Vector3 from, Vector3 to)
        {
            Vector2 offset = new Vector2(to.X - from.X, to.Z - from.Z);
            return offset.Length();
        }
    }
}
