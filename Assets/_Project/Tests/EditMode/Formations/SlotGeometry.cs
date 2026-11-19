using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;

namespace Stilargo.Tests.EditMode.Formations
{
    /// <summary>
    /// Measurements on slot lists shared by the formation tests.
    /// </summary>
    public static class SlotGeometry
    {
        public const float Tolerance = 0.0001f;

        public static Vector3 Centroid(IReadOnlyList<Vector3> slots)
        {
            Vector3 sum = Vector3.Zero;
            foreach (Vector3 slot in slots)
            {
                sum += slot;
            }

            return sum / slots.Count;
        }

        public static void AssertClose(Vector3 expected, Vector3 actual)
        {
            Assert.That(Vector3.Distance(expected, actual), Is.LessThan(Tolerance),
                $"Expected {expected} but was {actual}.");
        }
    }
}
