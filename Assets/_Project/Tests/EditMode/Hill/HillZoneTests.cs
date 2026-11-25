using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Hill;

namespace Stilargo.Tests.EditMode.Hill
{
    public sealed class HillZoneTests
    {
        [Test]
        public void Contains_PointOnTheEdge_IsInside()
        {
            var zone = new HillZone(new Vector3(10f, 0f, 10f), 5f);

            Assert.IsTrue(zone.Contains(new Vector3(15f, 0f, 10f)));
        }

        [Test]
        public void Contains_IgnoresHeight()
        {
            var zone = new HillZone(Vector3.Zero, 2f);

            Assert.IsTrue(zone.Contains(new Vector3(1f, 30f, 1f)));
        }

        [Test]
        public void Contains_PointBeyondTheRadius_IsOutside()
        {
            var zone = new HillZone(Vector3.Zero, 2f);

            Assert.IsFalse(zone.Contains(new Vector3(2f, 0f, 0.5f)));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_InvalidRadius_Throws(float radius)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HillZone(Vector3.Zero, radius));
        }
    }
}
