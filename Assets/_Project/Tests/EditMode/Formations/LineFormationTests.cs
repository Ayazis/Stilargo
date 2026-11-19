using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Formations;

namespace Stilargo.Tests.EditMode.Formations
{
    public sealed class LineFormationTests
    {
        private const float Spacing = 2f;
        private const int UnitCount = 5;
        private const float Tolerance = SlotGeometry.Tolerance;

        private static readonly Vector3 Center = new Vector3(10f, 3f, -4f);

        private LineFormation _formation;

        [SetUp]
        public void SetUp()
        {
            _formation = new LineFormation(Spacing);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_WithBadSpacing_Throws(float spacing)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LineFormation(spacing));
        }

        [Test]
        public void SlotsFor_NegativeCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _formation.SlotsFor(-1, Center, Vector3.UnitZ));
        }

        [Test]
        public void SlotsFor_ZeroCount_ReturnsEmpty()
        {
            Assert.That(_formation.SlotsFor(0, Center, Vector3.UnitZ), Is.Empty);
        }

        [Test]
        public void SlotsFor_ReturnsRequestedCount()
        {
            Assert.That(_formation.SlotsFor(UnitCount, Center, Vector3.UnitZ).Count, Is.EqualTo(UnitCount));
        }

        [Test]
        public void SlotsFor_OneUnit_StandsOnCenter()
        {
            SlotGeometry.AssertClose(Center, _formation.SlotsFor(1, Center, Vector3.UnitZ)[0]);
        }

        [Test]
        public void SlotsFor_NeighboursAreSpacingApart()
        {
            IReadOnlyList<Vector3> slots = _formation.SlotsFor(UnitCount, Center, Vector3.UnitZ);

            for (int i = 1; i < slots.Count; i++)
            {
                Assert.That(Vector3.Distance(slots[i - 1], slots[i]), Is.EqualTo(Spacing).Within(Tolerance));
            }
        }

        [Test]
        public void SlotsFor_CentroidIsCenter()
        {
            SlotGeometry.AssertClose(Center, SlotGeometry.Centroid(_formation.SlotsFor(UnitCount, Center, Vector3.UnitZ)));
        }

        [Test]
        public void SlotsFor_KeepCenterHeight()
        {
            foreach (Vector3 slot in _formation.SlotsFor(UnitCount, Center, new Vector3(1f, 5f, 1f)))
            {
                Assert.That(slot.Y, Is.EqualTo(Center.Y).Within(Tolerance));
            }
        }

        [TestCase(1f, 0f)]
        [TestCase(0f, 1f)]
        [TestCase(-3f, 4f)]
        public void SlotsFor_LineIsPerpendicularToFacing(float facingX, float facingZ)
        {
            var facing = new Vector3(facingX, 0f, facingZ);
            Vector3 forward = Vector3.Normalize(facing);

            foreach (Vector3 slot in _formation.SlotsFor(UnitCount, Center, facing))
            {
                Assert.That(Vector3.Dot(slot - Center, forward), Is.EqualTo(0f).Within(Tolerance));
            }
        }

        [Test]
        public void SlotsFor_FacingPlusZ_FirstSlotIsOnTheLeft()
        {
            IReadOnlyList<Vector3> slots = _formation.SlotsFor(UnitCount, Center, Vector3.UnitZ);

            Assert.That(slots[0].X, Is.LessThan(slots[slots.Count - 1].X));
        }

        [Test]
        public void SlotsFor_FacingWithoutGroundDirection_UsesDefaultForward()
        {
            IReadOnlyList<Vector3> expected = _formation.SlotsFor(UnitCount, Center, GroundPlane.DefaultForward);

            IReadOnlyList<Vector3> fromZero = _formation.SlotsFor(UnitCount, Center, Vector3.Zero);
            IReadOnlyList<Vector3> fromUp = _formation.SlotsFor(UnitCount, Center, Vector3.UnitY);

            for (int i = 0; i < expected.Count; i++)
            {
                SlotGeometry.AssertClose(expected[i], fromZero[i]);
                SlotGeometry.AssertClose(expected[i], fromUp[i]);
            }
        }
    }
}
