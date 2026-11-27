using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Input;

namespace Stilargo.Tests.EditMode.Input
{
    public sealed class MouseGestureTests
    {
        private const float Threshold = 6f;

        [Test]
        public void PressAndReleaseInPlace_IsAClick()
        {
            var button = new DragOrClick(Threshold);

            button.Update(true, Vector2.Zero);
            button.Update(true, new Vector2(2f, 1f));
            button.Update(false, new Vector2(2f, 1f));

            Assert.IsTrue(button.WasClicked);
            Assert.IsFalse(button.IsDragging);
        }

        [Test]
        public void MovingPastTheThreshold_IsADragAndNoClick()
        {
            var button = new DragOrClick(Threshold);

            button.Update(true, Vector2.Zero);
            button.Update(true, new Vector2(20f, 0f));
            Assert.IsTrue(button.IsDragging);

            button.Update(false, new Vector2(20f, 0f));
            Assert.IsFalse(button.WasClicked);
            Assert.IsFalse(button.IsDragging);
        }

        [Test]
        public void Click_LastsOneUpdate()
        {
            var button = new DragOrClick(Threshold);
            button.Update(true, Vector2.Zero);
            button.Update(false, Vector2.Zero);

            button.Update(false, Vector2.Zero);

            Assert.IsFalse(button.WasClicked);
        }

        [Test]
        public void DoubleClick_SameTargetInTime_Counts()
        {
            var doubleClick = new DoubleClick(0.4f);
            var building = new object();

            Assert.IsFalse(doubleClick.Register(building, 1f));
            Assert.IsTrue(doubleClick.Register(building, 1.3f));
        }

        [Test]
        public void DoubleClick_TooSlow_DoesNotCount()
        {
            var doubleClick = new DoubleClick(0.4f);
            var building = new object();

            doubleClick.Register(building, 1f);

            Assert.IsFalse(doubleClick.Register(building, 1.5f));
        }

        [Test]
        public void DoubleClick_OtherTarget_DoesNotCount()
        {
            var doubleClick = new DoubleClick(0.4f);

            doubleClick.Register(new object(), 1f);

            Assert.IsFalse(doubleClick.Register(new object(), 1.1f));
        }

        [Test]
        public void DoubleClick_ThirdClick_StartsOver()
        {
            var doubleClick = new DoubleClick(0.4f);
            var building = new object();
            doubleClick.Register(building, 1f);
            doubleClick.Register(building, 1.1f);

            Assert.IsFalse(doubleClick.Register(building, 1.2f));
        }
    }
}
