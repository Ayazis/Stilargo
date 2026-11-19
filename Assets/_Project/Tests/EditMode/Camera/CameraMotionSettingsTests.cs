using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Camera;

namespace Stilargo.Tests.EditMode.Camera
{
    public sealed class CameraMotionSettingsTests
    {
        private const float ValidSpeed = 1f;
        private const float ValidMargin = 10f;
        private const float ValidZoomStep = 2f;
        private const float ValidMinHeight = 10f;
        private const float ValidMaxHeight = 50f;
        private const float BoundsExtent = 100f;

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void PanSpeed_NotPositiveOrNotFinite_Throws(float speed)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PanSettings(speed, ValidMargin));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void EdgeMargin_NegativeOrNaN_Throws(float margin)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PanSettings(ValidSpeed, margin));
        }

        [Test]
        public void EdgeMargin_Zero_IsAllowed()
        {
            Assert.DoesNotThrow(() => new PanSettings(ValidSpeed, 0f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void ZoomStep_NotPositive_Throws(float step)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ZoomSettings(step, ValidMinHeight, ValidMaxHeight));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void MinHeight_NotPositive_Throws(float minHeight)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ZoomSettings(ValidZoomStep, minHeight, ValidMaxHeight));
        }

        [Test]
        public void MaxHeight_BelowMinHeight_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ZoomSettings(ValidZoomStep, ValidMaxHeight, ValidMinHeight));
        }

        [Test]
        public void Bounds_MaxBelowMinOnX_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => new CameraBounds(new Vector2(BoundsExtent, 0f), new Vector2(-BoundsExtent, 0f)));
        }

        [Test]
        public void Bounds_MaxBelowMinOnY_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => new CameraBounds(new Vector2(0f, BoundsExtent), new Vector2(0f, -BoundsExtent)));
        }

        [Test]
        public void Bounds_NotFinite_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new CameraBounds(new Vector2(float.NaN), new Vector2(BoundsExtent)));
        }

        [Test]
        public void MotionSettings_WithMissingPart_Throws()
        {
            var pan = new PanSettings(ValidSpeed, ValidMargin);
            var zoom = new ZoomSettings(ValidZoomStep, ValidMinHeight, ValidMaxHeight);
            var bounds = new CameraBounds(new Vector2(-BoundsExtent), new Vector2(BoundsExtent));

            Assert.Throws<ArgumentNullException>(() => new CameraMotionSettings(null, zoom, bounds));
            Assert.Throws<ArgumentNullException>(() => new CameraMotionSettings(pan, null, bounds));
            Assert.Throws<ArgumentNullException>(() => new CameraMotionSettings(pan, zoom, null));
        }

        [Test]
        public void Motion_WithNullSettings_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new RtsCameraMotion(null));
        }

        [Test]
        public void Frame_WithNegativeDeltaTime_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CameraFrame(new ScreenSize(1f, 1f), -1f));
        }
    }
}
