using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Orders;

namespace Stilargo.Tests.EditMode.Orders
{
    public sealed class UnitOrdersTests
    {
        private static readonly Vector3 First = new Vector3(1f, 0f, 2f);
        private static readonly Vector3 Second = new Vector3(3f, 0f, 4f);
        private static readonly Vector3 Third = new Vector3(5f, 0f, 6f);

        private FakeNavigationAgent _agent;
        private UnitOrders _orders;
        private int _reachedCount;
        private int _changedCount;

        [SetUp]
        public void SetUp()
        {
            _agent = new FakeNavigationAgent();
            _orders = new UnitOrders(_agent);
            _reachedCount = 0;
            _changedCount = 0;
            _orders.DestinationReached += () => _reachedCount++;
            _orders.WaypointsChanged += () => _changedCount++;
        }

        [Test]
        public void Constructor_WithoutAgent_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new UnitOrders(null));
        }

        [Test]
        public void NewOrders_AreIdle()
        {
            Assert.That(_orders.IsIdle, Is.True);
            Assert.That(_orders.Waypoints, Is.Empty);
        }

        [Test]
        public void MoveTo_SendsAgentAndIsNotIdle()
        {
            _orders.MoveTo(First);

            Assert.That(_agent.Destinations, Is.EqualTo(new[] { First }));
            Assert.That(_orders.IsIdle, Is.False);
            Assert.That(_changedCount, Is.EqualTo(1));
        }

        [Test]
        public void MoveTo_ReplacesQueuedWaypoints()
        {
            _orders.MoveTo(First);
            _orders.QueueMoveTo(Second);

            _orders.MoveTo(Third);

            Assert.That(_orders.Waypoints, Is.EqualTo(new[] { Third }));
            Assert.That(_agent.Destinations, Is.EqualTo(new[] { First, Third }));
        }

        [Test]
        public void QueueMoveTo_WhenIdle_StartsMoving()
        {
            _orders.QueueMoveTo(First);

            Assert.That(_agent.Destinations, Is.EqualTo(new[] { First }));
            Assert.That(_orders.IsIdle, Is.False);
        }

        [Test]
        public void QueueMoveTo_WhenMoving_AppendsWithoutRedirecting()
        {
            _orders.MoveTo(First);

            _orders.QueueMoveTo(Second);

            Assert.That(_agent.Destinations, Is.EqualTo(new[] { First }));
            Assert.That(_orders.Waypoints, Is.EqualTo(new[] { First, Second }));
        }

        [Test]
        public void NotifyWaypointReached_SendsAgentToNextWaypoint()
        {
            _orders.MoveTo(First);
            _orders.QueueMoveTo(Second);

            _orders.NotifyWaypointReached();

            Assert.That(_agent.Destinations, Is.EqualTo(new[] { First, Second }));
            Assert.That(_orders.Waypoints, Is.EqualTo(new[] { Second }));
            Assert.That(_reachedCount, Is.EqualTo(0));
        }

        [Test]
        public void DestinationReached_RaisedOnceAtEndOfQueue()
        {
            _orders.MoveTo(First);
            _orders.QueueMoveTo(Second);
            _orders.QueueMoveTo(Third);

            _orders.NotifyWaypointReached();
            _orders.NotifyWaypointReached();
            Assert.That(_reachedCount, Is.EqualTo(0));

            _orders.NotifyWaypointReached();
            Assert.That(_reachedCount, Is.EqualTo(1));
            Assert.That(_orders.IsIdle, Is.True);
        }

        [Test]
        public void DestinationReached_UnitIsAlreadyIdleWhenRaised()
        {
            bool wasIdleWhenRaised = false;
            _orders.DestinationReached += () => wasIdleWhenRaised = _orders.IsIdle;
            _orders.MoveTo(First);

            _orders.NotifyWaypointReached();

            Assert.That(wasIdleWhenRaised, Is.True);
        }

        [Test]
        public void NotifyWaypointReached_WhenIdle_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _orders.NotifyWaypointReached());
        }

        [Test]
        public void Stop_ClearsHaltsAndDoesNotRaiseReached()
        {
            _orders.MoveTo(First);
            _orders.QueueMoveTo(Second);

            _orders.Stop();

            Assert.That(_orders.IsIdle, Is.True);
            Assert.That(_orders.Waypoints, Is.Empty);
            Assert.That(_agent.HaltCount, Is.EqualTo(1));
            Assert.That(_reachedCount, Is.EqualTo(0));
        }
    }
}
