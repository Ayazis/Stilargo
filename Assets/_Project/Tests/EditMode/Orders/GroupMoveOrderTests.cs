using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Formations;
using Stilargo.Core.Orders;
using Stilargo.Core.Selection;

namespace Stilargo.Tests.EditMode.Orders
{
    public sealed class GroupMoveOrderTests
    {
        private const float Spacing = 2f;
        private const float Tolerance = 0.0001f;
        private const float DestinationDistance = 20f;

        private static readonly Vector3 Destination = new Vector3(0f, 0f, DestinationDistance);

        private GroupMoveOrder _order;
        private FakeSelectable[] _fakes;
        private IReadOnlyList<ISelectable> _units;

        [SetUp]
        public void SetUp()
        {
            _order = new GroupMoveOrder(new LineFormation(Spacing));
            _fakes = new[]
            {
                new FakeSelectable(new Vector3(-Spacing, 0f, 0f)),
                new FakeSelectable(Vector3.Zero),
                new FakeSelectable(new Vector3(Spacing, 0f, 0f)),
            };
            _units = _fakes;
        }

        [Test]
        public void Constructor_WithoutFormation_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupMoveOrder(null));
        }

        [Test]
        public void Formation_SetToNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _order.Formation = null);
        }

        [Test]
        public void Move_GivesEachUnitOneDistinctSlot()
        {
            _order.Move(_units, Destination);

            Vector3[] targets = _fakes.Select(unit => unit.FakeOrders.MoveTargets.Single()).ToArray();
            Assert.That(targets.Distinct().Count(), Is.EqualTo(_fakes.Length));
            Assert.That(_fakes.All(unit => unit.FakeOrders.QueuedTargets.Count == 0), Is.True);
        }

        [Test]
        public void Move_AssignsNearestSlotSoPathsDoNotCross()
        {
            _order.Move(_units, Destination);

            for (int i = 0; i < _fakes.Length; i++)
            {
                Vector3 target = _fakes[i].FakeOrders.MoveTargets.Single();
                Assert.That(target.X, Is.EqualTo(_fakes[i].Position.X).Within(Tolerance));
                Assert.That(target.Z, Is.EqualTo(DestinationDistance).Within(Tolerance));
            }
        }

        [Test]
        public void QueueMove_UsesQueueMoveTo()
        {
            _order.QueueMove(_units, Destination);

            Assert.That(_fakes.All(unit => unit.FakeOrders.QueuedTargets.Count == 1), Is.True);
            Assert.That(_fakes.All(unit => unit.FakeOrders.MoveTargets.Count == 0), Is.True);
        }

        [Test]
        public void Move_UsesCurrentFormation()
        {
            var recording = new RecordingFormation(new LineFormation(Spacing));
            _order.Formation = recording;

            _order.Move(_units, Destination);

            Assert.That(recording.LastCount, Is.EqualTo(_fakes.Length));
            Assert.That(recording.LastCenter, Is.EqualTo(Destination));
            Assert.That(recording.LastFacing.Z, Is.GreaterThan(0f));
        }

        [Test]
        public void Move_WithNoUnits_DoesNothing()
        {
            Assert.DoesNotThrow(() => _order.Move(Array.Empty<ISelectable>(), Destination));
        }

        [Test]
        public void Move_WithNullUnits_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _order.Move(null, Destination));
        }
    }
}
