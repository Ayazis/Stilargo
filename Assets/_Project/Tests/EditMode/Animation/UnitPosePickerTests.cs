using System;
using NUnit.Framework;
using Stilargo.Core.Animation;

namespace Stilargo.Tests.EditMode.Animation
{
    public sealed class UnitPosePickerTests
    {
        private const float RunThreshold = 0.5f;
        private const float AimHold = 2f;
        private const float LongAgo = 100f;

        private readonly UnitPosePicker _picker = new UnitPosePicker(RunThreshold, AimHold);

        [Test]
        public void Pick_StandingStillAndQuiet_IsIdle()
        {
            Assert.That(_picker.Pick(new UnitPoseInput(0f, LongAgo, false)), Is.EqualTo(UnitPose.Idle));
        }

        [Test]
        public void Pick_Moving_IsRunEvenInCoverAndShooting()
        {
            Assert.That(_picker.Pick(new UnitPoseInput(RunThreshold, 0f, true)), Is.EqualTo(UnitPose.Run));
        }

        [Test]
        public void Pick_StillInCover_IsCrouch()
        {
            Assert.That(_picker.Pick(new UnitPoseInput(0f, 0f, true)), Is.EqualTo(UnitPose.Crouch));
        }

        [Test]
        public void Pick_RecentlyShot_IsAim()
        {
            Assert.That(_picker.Pick(new UnitPoseInput(0f, AimHold, false)), Is.EqualTo(UnitPose.Aim));
        }

        [Test]
        public void Pick_ShotLongerAgoThanHold_IsIdle()
        {
            Assert.That(_picker.Pick(new UnitPoseInput(0f, AimHold + 0.1f, false)), Is.EqualTo(UnitPose.Idle));
        }

        [Test]
        public void Constructor_NonPositiveRunThreshold_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new UnitPosePicker(0f, AimHold));
        }
    }
}
