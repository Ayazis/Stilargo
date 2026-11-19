using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Selection;

namespace Stilargo.Tests.EditMode.Selection
{
    public sealed class SelectionDragTests
    {
        private const float Threshold = 5f;
        private const float JustOverThreshold = 5.01f;

        private static readonly Vector2 Start = new Vector2(100f, 100f);

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_InvalidThreshold_Throws(float threshold)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SelectionDrag(threshold));
        }

        [Test]
        public void Constructor_ZeroThreshold_IsAllowed()
        {
            Assert.DoesNotThrow(() => new SelectionDrag(0f));
        }

        [Test]
        public void New_IsNotActive()
        {
            var drag = new SelectionDrag(Threshold);

            Assert.That(drag.IsActive, Is.False);
        }

        [Test]
        public void MoveTo_WithoutBegin_Throws()
        {
            var drag = new SelectionDrag(Threshold);

            Assert.Throws<InvalidOperationException>(() => drag.MoveTo(Start));
        }

        [Test]
        public void End_WithoutBegin_Throws()
        {
            var drag = new SelectionDrag(Threshold);

            Assert.Throws<InvalidOperationException>(() => drag.End());
        }

        [Test]
        public void End_Twice_Throws()
        {
            var drag = new SelectionDrag(Threshold);
            drag.Begin(Start);
            drag.End();

            Assert.Throws<InvalidOperationException>(() => drag.End());
        }

        [Test]
        public void Begin_SetsActiveAndBothPoints()
        {
            var drag = new SelectionDrag(Threshold);

            drag.Begin(Start);

            Assert.That(drag.IsActive, Is.True);
            Assert.That(drag.IsBox, Is.False);
            Assert.That(drag.StartPoint, Is.EqualTo(Start));
            Assert.That(drag.CurrentPoint, Is.EqualTo(Start));
        }

        [Test]
        public void MoveTo_ExactlyThreshold_IsClick()
        {
            var drag = new SelectionDrag(Threshold);
            drag.Begin(Start);

            drag.MoveTo(Start + new Vector2(Threshold, 0f));

            Assert.That(drag.IsBox, Is.False);
        }

        [Test]
        public void MoveTo_JustOverThreshold_IsBox()
        {
            var drag = new SelectionDrag(Threshold);
            drag.Begin(Start);

            drag.MoveTo(Start + new Vector2(0f, JustOverThreshold));

            Assert.That(drag.IsBox, Is.True);
        }

        [Test]
        public void MoveTo_BackToStartAfterBox_StaysBox()
        {
            var drag = new SelectionDrag(Threshold);
            drag.Begin(Start);
            drag.MoveTo(Start + new Vector2(JustOverThreshold, 0f));

            drag.MoveTo(Start);

            Assert.That(drag.IsBox, Is.True);
        }

        [Test]
        public void ZeroThreshold_AnyMovement_IsBox()
        {
            var drag = new SelectionDrag(0f);
            drag.Begin(Start);

            drag.MoveTo(Start + new Vector2(1f, 0f));

            Assert.That(drag.IsBox, Is.True);
        }

        [Test]
        public void Rect_SpansStartToCurrent()
        {
            var drag = new SelectionDrag(Threshold);
            var end = new Vector2(40f, 160f);
            drag.Begin(Start);

            drag.MoveTo(end);

            Assert.That(drag.Rect.Min, Is.EqualTo(new Vector2(end.X, Start.Y)));
            Assert.That(drag.Rect.Max, Is.EqualTo(new Vector2(Start.X, end.Y)));
        }

        [Test]
        public void End_KeepsLastGestureReadable()
        {
            var drag = new SelectionDrag(Threshold);
            var end = Start + new Vector2(JustOverThreshold, 0f);
            drag.Begin(Start);
            drag.MoveTo(end);

            drag.End();

            Assert.That(drag.IsActive, Is.False);
            Assert.That(drag.IsBox, Is.True);
            Assert.That(drag.CurrentPoint, Is.EqualTo(end));
        }

        [Test]
        public void Begin_AfterBox_ResetsToClick()
        {
            var drag = new SelectionDrag(Threshold);
            drag.Begin(Start);
            drag.MoveTo(Start + new Vector2(JustOverThreshold, 0f));
            drag.End();

            drag.Begin(Start);

            Assert.That(drag.IsBox, Is.False);
            Assert.That(drag.IsActive, Is.True);
        }
    }
}
