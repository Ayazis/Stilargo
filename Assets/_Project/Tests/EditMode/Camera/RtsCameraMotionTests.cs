using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Camera;

namespace Stilargo.Tests.EditMode.Camera
{
    public sealed class RtsCameraMotionTests
    {
        private const float SpeedPerHeight = 1f;
        private const float EdgeMargin = 10f;
        private const float HeightPerZoomStep = 2f;
        private const float MinHeight = 10f;
        private const float MaxHeight = 50f;
        private const float BoundsExtent = 100f;
        private const float ScreenWidth = 1000f;
        private const float ScreenHeight = 800f;
        private const float StartHeight = 20f;
        private const float OneSecond = 1f;
        private const float Tolerance = 0.0001f;

        // With SpeedPerHeight 1 and one second elapsed, a full pan moves exactly one camera height.
        private const float FullPanDistance = StartHeight * SpeedPerHeight * OneSecond;

        private static readonly Vector2 ScreenCenter = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f);

        private RtsCameraMotion _motion;
        private FakePlayerInput _input;
        private CameraFrame _frame;
        private CameraState _start;

        [SetUp]
        public void SetUp()
        {
            var settings = new CameraMotionSettings(
                new PanSettings(SpeedPerHeight, EdgeMargin),
                new ZoomSettings(HeightPerZoomStep, MinHeight, MaxHeight),
                new CameraBounds(new Vector2(-BoundsExtent), new Vector2(BoundsExtent)));
            _motion = new RtsCameraMotion(settings);
            _input = new FakePlayerInput { PointerScreenPosition = ScreenCenter };
            _frame = new CameraFrame(new ScreenSize(ScreenWidth, ScreenHeight), OneSecond);
            _start = new CameraState(Vector2.Zero, StartHeight);
        }

        [Test]
        public void KeyboardPanRight_MovesFocusAlongX()
        {
            _input.PanAxis = Vector2.UnitX;

            CameraState next = _motion.Step(_start, _input, _frame);

            AssertFocus(new Vector2(FullPanDistance, 0f), next);
        }

        [Test]
        public void KeyboardPanForward_MovesFocusAlongWorldZ()
        {
            _input.PanAxis = Vector2.UnitY;

            CameraState next = _motion.Step(_start, _input, _frame);

            AssertFocus(new Vector2(0f, FullPanDistance), next);
        }

        [Test]
        public void PointerAtLeftEdge_PansLeft()
        {
            _input.PointerScreenPosition = new Vector2(EdgeMargin / 2f, ScreenCenter.Y);

            CameraState next = _motion.Step(_start, _input, _frame);

            AssertFocus(new Vector2(-FullPanDistance, 0f), next);
        }

        [Test]
        public void PointerAtRightEdge_PansRight()
        {
            _input.PointerScreenPosition = new Vector2(ScreenWidth - (EdgeMargin / 2f), ScreenCenter.Y);

            CameraState next = _motion.Step(_start, _input, _frame);

            AssertFocus(new Vector2(FullPanDistance, 0f), next);
        }

        [Test]
        public void PointerAtBottomEdge_PansBack()
        {
            _input.PointerScreenPosition = new Vector2(ScreenCenter.X, EdgeMargin / 2f);

            CameraState next = _motion.Step(_start, _input, _frame);

            AssertFocus(new Vector2(0f, -FullPanDistance), next);
        }

        [Test]
        public void PointerAtTopEdge_PansForward()
        {
            _input.PointerScreenPosition = new Vector2(ScreenCenter.X, ScreenHeight - (EdgeMargin / 2f));

            CameraState next = _motion.Step(_start, _input, _frame);

            AssertFocus(new Vector2(0f, FullPanDistance), next);
        }

        [Test]
        public void PointerInMiddle_DoesNotMove()
        {
            CameraState next = _motion.Step(_start, _input, _frame);

            AssertFocus(Vector2.Zero, next);
        }

        [TestCase(-50f, 400f)]
        [TestCase(1050f, 400f)]
        [TestCase(500f, -50f)]
        [TestCase(500f, 900f)]
        public void PointerOutsideScreen_DoesNotEdgeScroll(float x, float y)
        {
            _input.PointerScreenPosition = new Vector2(x, y);

            CameraState next = _motion.Step(_start, _input, _frame);

            AssertFocus(Vector2.Zero, next);
        }

        [Test]
        public void ZeroSizedScreen_DoesNotEdgeScroll()
        {
            _input.PointerScreenPosition = Vector2.Zero;
            var minimized = new CameraFrame(new ScreenSize(0f, 0f), OneSecond);

            CameraState next = _motion.Step(_start, _input, minimized);

            AssertFocus(Vector2.Zero, next);
        }

        [Test]
        public void KeyboardDiagonal_IsNotFasterThanStraight()
        {
            _input.PanAxis = Vector2.One;

            CameraState next = _motion.Step(_start, _input, _frame);

            Assert.That(next.Focus.Length(), Is.EqualTo(FullPanDistance).Within(Tolerance));
        }

        [Test]
        public void EdgeScrollCorner_IsNotFasterThanStraight()
        {
            _input.PointerScreenPosition = new Vector2(EdgeMargin / 2f, EdgeMargin / 2f);

            CameraState next = _motion.Step(_start, _input, _frame);

            Assert.That(next.Focus.Length(), Is.EqualTo(FullPanDistance).Within(Tolerance));
        }

        [Test]
        public void KeyboardAndEdgeScrollTogether_AreNotFasterThanStraight()
        {
            _input.PanAxis = Vector2.UnitX;
            _input.PointerScreenPosition = new Vector2(ScreenWidth - (EdgeMargin / 2f), ScreenCenter.Y);

            CameraState next = _motion.Step(_start, _input, _frame);

            AssertFocus(new Vector2(FullPanDistance, 0f), next);
        }

        [Test]
        public void PanSpeed_ScalesWithHeight()
        {
            const float doubleHeight = StartHeight * 2f;
            _input.PanAxis = Vector2.UnitX;

            CameraState next = _motion.Step(new CameraState(Vector2.Zero, doubleHeight), _input, _frame);

            AssertFocus(new Vector2(FullPanDistance * 2f, 0f), next);
        }

        [Test]
        public void PanDistance_ScalesWithDeltaTime()
        {
            const float halfSecond = OneSecond / 2f;
            _input.PanAxis = Vector2.UnitX;
            var halfFrame = new CameraFrame(new ScreenSize(ScreenWidth, ScreenHeight), halfSecond);

            CameraState next = _motion.Step(_start, _input, halfFrame);

            AssertFocus(new Vector2(FullPanDistance / 2f, 0f), next);
        }

        [Test]
        public void ZoomIn_LowersHeight()
        {
            _input.ZoomDelta = 1f;

            CameraState next = _motion.Step(_start, _input, _frame);

            Assert.That(next.Height, Is.EqualTo(StartHeight - HeightPerZoomStep).Within(Tolerance));
        }

        [Test]
        public void ZoomOut_RaisesHeight()
        {
            _input.ZoomDelta = -1f;

            CameraState next = _motion.Step(_start, _input, _frame);

            Assert.That(next.Height, Is.EqualTo(StartHeight + HeightPerZoomStep).Within(Tolerance));
        }

        [Test]
        public void ZoomInPastMin_ClampsToMinHeight()
        {
            _input.ZoomDelta = MaxHeight;

            CameraState next = _motion.Step(_start, _input, _frame);

            Assert.That(next.Height, Is.EqualTo(MinHeight));
        }

        [Test]
        public void ZoomOutPastMax_ClampsToMaxHeight()
        {
            _input.ZoomDelta = -MaxHeight;

            CameraState next = _motion.Step(_start, _input, _frame);

            Assert.That(next.Height, Is.EqualTo(MaxHeight));
        }

        [Test]
        public void PanPastBounds_ClampsFocus()
        {
            _input.PanAxis = Vector2.One;
            var nearCorner = new CameraState(new Vector2(BoundsExtent - 1f), StartHeight);

            CameraState next = _motion.Step(nearCorner, _input, _frame);

            AssertFocus(new Vector2(BoundsExtent), next);
        }

        [Test]
        public void Constrain_PullsOutOfRangeStateInside()
        {
            var outside = new CameraState(new Vector2(-BoundsExtent * 2f, BoundsExtent * 2f), MaxHeight * 2f);

            CameraState constrained = _motion.Constrain(outside);

            AssertFocus(new Vector2(-BoundsExtent, BoundsExtent), constrained);
            Assert.That(constrained.Height, Is.EqualTo(MaxHeight));
        }

        [Test]
        public void Step_WithNullInput_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _motion.Step(_start, null, _frame));
        }

        private static void AssertFocus(Vector2 expected, CameraState actual)
        {
            Assert.That(actual.Focus.X, Is.EqualTo(expected.X).Within(Tolerance), "Focus X");
            Assert.That(actual.Focus.Y, Is.EqualTo(expected.Y).Within(Tolerance), "Focus Y");
        }
    }
}
