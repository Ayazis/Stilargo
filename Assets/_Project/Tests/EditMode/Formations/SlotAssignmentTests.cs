using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Formations;

namespace Stilargo.Tests.EditMode.Formations
{
    public sealed class SlotAssignmentTests
    {
        private const float SlotRowZ = 10f;

        private SlotAssignment _assignment;

        [SetUp]
        public void SetUp()
        {
            _assignment = new SlotAssignment();
        }

        [Test]
        public void Assign_PicksNearestSlotPerUnit()
        {
            Vector3[] slots =
            {
                new Vector3(-5f, 0f, SlotRowZ),
                new Vector3(0f, 0f, SlotRowZ),
                new Vector3(5f, 0f, SlotRowZ),
            };
            Vector3[] positions =
            {
                new Vector3(5f, 0f, 0f),
                new Vector3(-5f, 0f, 0f),
                new Vector3(0f, 0f, 0f),
            };

            IReadOnlyList<int> assigned = _assignment.Assign(positions, slots);

            Assert.That(assigned, Is.EqualTo(new[] { 2, 0, 1 }));
        }

        [Test]
        public void Assign_WhenUnitsShareNearestSlot_UsesEachSlotOnce()
        {
            Vector3[] slots = { new Vector3(0f, 0f, SlotRowZ), new Vector3(1f, 0f, SlotRowZ) };
            Vector3[] positions = { new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1f) };

            IReadOnlyList<int> assigned = _assignment.Assign(positions, slots);

            Assert.That(assigned.Distinct().Count(), Is.EqualTo(positions.Length));
        }

        [Test]
        public void Assign_IgnoresHeight()
        {
            Vector3[] slots = { new Vector3(0f, 100f, 0f), new Vector3(3f, 0f, 0f) };
            Vector3[] positions = { new Vector3(0f, 0f, 0f) };

            Assert.That(_assignment.Assign(positions, slots), Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void Assign_WithNoUnits_ReturnsEmpty()
        {
            Assert.That(_assignment.Assign(Array.Empty<Vector3>(), Array.Empty<Vector3>()), Is.Empty);
        }

        [Test]
        public void Assign_WithTooFewSlots_Throws()
        {
            Vector3[] positions = { Vector3.Zero, Vector3.UnitX };

            Assert.Throws<ArgumentException>(() => _assignment.Assign(positions, new[] { Vector3.Zero }));
        }

        [Test]
        public void Assign_WithNullArguments_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _assignment.Assign(null, Array.Empty<Vector3>()));
            Assert.Throws<ArgumentNullException>(() => _assignment.Assign(Array.Empty<Vector3>(), null));
        }
    }
}
