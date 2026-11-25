using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Camera;

namespace Stilargo.Tests.EditMode.Camera
{
    public sealed class CameraOrbitTests
    {
        private const float TurnSpeed = 90f;
        private const float DragPerPixel = 0.5f;
        private const float MinPitch = 20f;
        private const float MaxPitch = 80f;
        private const float StartPitch = 50f;
        private const float OneSecond = 1f;
        private const float Tolerance = 0.0001f;

        private CameraOrbit _orbit;
        private FakePlayerInput _input;
        private CameraAngles _start;

        [SetUp]
        public void SetUp()
        {
            _orbit = new CameraOrbit(new OrbitSettings(TurnSpeed, DragPerPixel, new PitchLimits(MinPitch, MaxPitch)));
            _input = new FakePlayerInput();
            _start = new CameraAngles(0f, StartPitch);
        }

        [Test]
        public void TurnRight_AddsYawBySpeedTimesTime()
        {
            _input.TurnAxis = 1f;

            CameraAngles next = _orbit.Step(_start, _input, OneSecond);

            Assert.That(next.YawDegrees, Is.EqualTo(TurnSpeed).Within(Tolerance));
        }

        [Test]
        public void TurnLeftFromZero_WrapsIntoPositiveRange()
        {
            _input.TurnAxis = -1f;

            CameraAngles next = _orbit.Step(_start, _input, OneSecond);

            Assert.That(next.YawDegrees, Is.EqualTo(360f - TurnSpeed).Within(Tolerance));
        }

        [Test]
        public void Drag_TurnsAndTilts()
        {
            _input.IsOrbitDragHeld = true;
            _input.PointerDelta = new Vector2(10f, 10f);

            CameraAngles next = _orbit.Step(_start, _input, OneSecond);

            Assert.That(next.YawDegrees, Is.EqualTo(10f * DragPerPixel).Within(Tolerance));
            Assert.That(next.PitchDegrees, Is.EqualTo(StartPitch - (10f * DragPerPixel)).Within(Tolerance));
        }

        [Test]
        public void PointerMovementWithoutDrag_DoesNothing()
        {
            _input.PointerDelta = new Vector2(10f, 10f);

            CameraAngles next = _orbit.Step(_start, _input, OneSecond);

            Assert.That(next.YawDegrees, Is.EqualTo(0f));
            Assert.That(next.PitchDegrees, Is.EqualTo(StartPitch));
        }

        [TestCase(1000f, MinPitch)]
        [TestCase(-1000f, MaxPitch)]
        public void TiltPastLimits_Clamps(float pointerDeltaY, float expectedPitch)
        {
            _input.IsOrbitDragHeld = true;
            _input.PointerDelta = new Vector2(0f, pointerDeltaY);

            CameraAngles next = _orbit.Step(_start, _input, OneSecond);

            Assert.That(next.PitchDegrees, Is.EqualTo(expectedPitch));
        }

        [TestCase(5f, 50f)]
        [TestCase(50f, 30f)]
        [TestCase(20f, 95f)]
        public void InvalidPitchLimits_Throw(float min, float max)
        {
            Assert.Throws(Is.InstanceOf<ArgumentException>(), () => new PitchLimits(min, max));
        }

        [Test]
        public void Step_WithNullInput_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _orbit.Step(_start, null, OneSecond));
        }
    }
}
