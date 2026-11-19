using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Orders;

namespace Stilargo.Tests.EditMode.Orders
{
    public sealed class WaypointQueueTests
    {
        private static readonly Vector3 First = new Vector3(1f, 0f, 2f);
        private static readonly Vector3 Second = new Vector3(3f, 0f, 4f);
        private static readonly Vector3 Third = new Vector3(5f, 0f, 6f);

        private WaypointQueue _queue;

        [SetUp]
        public void SetUp()
        {
            _queue = new WaypointQueue();
        }

        [Test]
        public void NewQueue_IsEmpty()
        {
            Assert.That(_queue.HasCurrent, Is.False);
            Assert.That(_queue.Count, Is.EqualTo(0));
            Assert.That(_queue.Waypoints, Is.Empty);
        }

        [Test]
        public void Current_WhenEmpty_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _ = _queue.Current);
        }

        [Test]
        public void Advance_WhenEmpty_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _queue.Advance());
        }

        [Test]
        public void Enqueue_KeepsOrderWithCurrentFirst()
        {
            _queue.Enqueue(First);
            _queue.Enqueue(Second);

            Assert.That(_queue.Current, Is.EqualTo(First));
            Assert.That(_queue.Waypoints, Is.EqualTo(new[] { First, Second }));
            Assert.That(_queue.Count, Is.EqualTo(2));
        }

        [Test]
        public void Replace_DropsEverythingElse()
        {
            _queue.Enqueue(First);
            _queue.Enqueue(Second);

            _queue.Replace(Third);

            Assert.That(_queue.Waypoints, Is.EqualTo(new[] { Third }));
        }

        [Test]
        public void Advance_DropsCurrentAndReportsWhetherMoreRemain()
        {
            _queue.Enqueue(First);
            _queue.Enqueue(Second);

            Assert.That(_queue.Advance(), Is.True);
            Assert.That(_queue.Current, Is.EqualTo(Second));
            Assert.That(_queue.Advance(), Is.False);
            Assert.That(_queue.HasCurrent, Is.False);
        }

        [Test]
        public void Clear_EmptiesQueue()
        {
            _queue.Enqueue(First);

            _queue.Clear();

            Assert.That(_queue.HasCurrent, Is.False);
        }
    }
}
