using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Formations;

namespace Stilargo.Tests.EditMode.Formations
{
    public sealed class BoxFormationTests
    {
        private const float Spacing = 2f;
        private const int FullGridCount = 9;
        private const int GridSide = 3;
        private const int PartialGridCount = 5;
        private const float Tolerance = SlotGeometry.Tolerance;

        private static readonly Vector3 Center = new Vector3(10f, 3f, -4f);
        private static readonly Vector3 Facing = Vector3.UnitZ;

        private BoxFormation _formation;

        [SetUp]
        public void SetUp()
        {
            _formation = new BoxFormation(Spacing);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void Constructor_WithBadSpacing_Throws(float spacing)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoxFormation(spacing));
        }

        [Test]
        public void SlotsFor_NegativeCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _formation.SlotsFor(-1, Center, Facing));
        }

        [Test]
        public void SlotsFor_ZeroCount_ReturnsEmpty()
        {
            Assert.That(_formation.SlotsFor(0, Center, Facing), Is.Empty);
        }

        [TestCase(1)]
        [TestCase(PartialGridCount)]
        [TestCase(FullGridCount)]
        [TestCase(10)]
        public void SlotsFor_ReturnsRequestedCountOfDistinctSlots(int count)
        {
            IReadOnlyList<Vector3> slots = _formation.SlotsFor(count, Center, Facing);

            Assert.That(slots.Count, Is.EqualTo(count));
            Assert.That(slots.Distinct().Count(), Is.EqualTo(count));
        }

        [Test]
        public void SlotsFor_FullGrid_CentroidIsCenter()
        {
            SlotGeometry.AssertClose(Center, SlotGeometry.Centroid(_formation.SlotsFor(FullGridCount, Center, Facing)));
        }

        [Test]
        public void SlotsFor_FullGrid_IsSquare()
        {
            IReadOnlyList<Vector3> slots = _formation.SlotsFor(FullGridCount, Center, Facing);

            Assert.That(slots.Select(slot => MathF.Round(slot.X, 2)).Distinct().Count(), Is.EqualTo(GridSide));
            Assert.That(slots.Select(slot => MathF.Round(slot.Z, 2)).Distinct().Count(), Is.EqualTo(GridSide));
        }

        [Test]
        public void SlotsFor_NeighboursInRowAndColumnAreSpacingApart()
        {
            IReadOnlyList<Vector3> slots = _formation.SlotsFor(FullGridCount, Center, Facing);

            Assert.That(Vector3.Distance(slots[0], slots[1]), Is.EqualTo(Spacing).Within(Tolerance));
            Assert.That(Vector3.Distance(slots[0], slots[GridSide]), Is.EqualTo(Spacing).Within(Tolerance));
        }

        [TestCase(1f, 0f)]
        [TestCase(0f, 1f)]
        [TestCase(-3f, -4f)]
        public void SlotsFor_FrontRowIsAheadOfBackRowAlongFacing(float facingX, float facingZ)
        {
            var facing = new Vector3(facingX, 0f, facingZ);
            Vector3 forward = Vector3.Normalize(facing);
            IReadOnlyList<Vector3> slots = _formation.SlotsFor(FullGridCount, Center, facing);

            float front = Vector3.Dot(slots[0] - Center, forward);
            float back = Vector3.Dot(slots[FullGridCount - 1] - Center, forward);

            Assert.That(front, Is.EqualTo(Spacing).Within(Tolerance));
            Assert.That(back, Is.EqualTo(-Spacing).Within(Tolerance));
        }

        [Test]
        public void SlotsFor_PartialLastRowIsCentred()
        {
            IReadOnlyList<Vector3> slots = _formation.SlotsFor(PartialGridCount, Center, Facing);
            float backRowZ = slots.Min(slot => slot.Z);

            Vector3[] backRow = slots.Where(slot => MathF.Abs(slot.Z - backRowZ) < Tolerance).ToArray();

            Assert.That(backRow.Length, Is.EqualTo(PartialGridCount - GridSide));
            Assert.That(backRow.Average(slot => slot.X), Is.EqualTo(Center.X).Within(Tolerance));
        }

        [Test]
        public void SlotsFor_KeepCenterHeight()
        {
            foreach (Vector3 slot in _formation.SlotsFor(FullGridCount, Center, new Vector3(1f, -5f, 2f)))
            {
                Assert.That(slot.Y, Is.EqualTo(Center.Y).Within(Tolerance));
            }
        }

        [Test]
        public void SlotsFor_ZeroFacing_UsesDefaultForward()
        {
            IReadOnlyList<Vector3> expected = _formation.SlotsFor(FullGridCount, Center, GroundPlane.DefaultForward);
            IReadOnlyList<Vector3> actual = _formation.SlotsFor(FullGridCount, Center, Vector3.Zero);

            for (int i = 0; i < expected.Count; i++)
            {
                SlotGeometry.AssertClose(expected[i], actual[i]);
            }
        }
    }
}
