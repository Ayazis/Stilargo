using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Fortification;

namespace Stilargo.Tests.EditMode.Fortification
{
    public sealed class TurretAimTests
    {
        private const float YawSpeed = 90f;
        private const float PitchSpeed = 30f;
        private const float MinPitch = -10f;
        private const float MaxPitch = 45f;
        private const float Tolerance = 0.01f;

        private TurretAim _aim;

        [SetUp]
        public void SetUp()
        {
            _aim = new TurretAim(new TurretAimSettings(YawSpeed, PitchSpeed, MinPitch, MaxPitch));
        }

        [Test]
        public void TurnToward_ReachesTargetWithinOneStep_WhenFastEnough()
        {
            _aim.TurnToward(new Vector3(1f, 0f, 1f), 1f);

            Assert.AreEqual(45f, _aim.YawDegrees, Tolerance);
            Assert.AreEqual(0f, _aim.PitchDegrees, Tolerance);
        }

        [Test]
        public void TurnToward_LimitsYawBySpeed()
        {
            _aim.TurnToward(new Vector3(1f, 0f, 0f), 0.5f);

            Assert.AreEqual(YawSpeed * 0.5f, _aim.YawDegrees, Tolerance);
        }

        [Test]
        public void TurnToward_TurnsTheShortWayRound()
        {
            Vector3 behindLeft = new Vector3(-0.01f, 0f, -1f);
            _aim.TurnToward(behindLeft, 1f);
            _aim.TurnToward(behindLeft, 1f);

            Assert.Less(_aim.YawDegrees, -170f);
            _aim.TurnToward(new Vector3(0.01f, 0f, -1f), 0.1f);

            Assert.Greater(_aim.YawDegrees, 170f);
        }

        [Test]
        public void TurnToward_ClampsPitchToLimits()
        {
            _aim.TurnToward(new Vector3(0f, 10f, 1f), 10f);
            Assert.AreEqual(MaxPitch, _aim.PitchDegrees, Tolerance);

            _aim.TurnToward(new Vector3(0f, -10f, 1f), 10f);
            Assert.AreEqual(MinPitch, _aim.PitchDegrees, Tolerance);
        }

        [Test]
        public void TurnToward_ZeroDirection_KeepsAim()
        {
            _aim.TurnToward(new Vector3(1f, 0f, 1f), 1f);

            _aim.TurnToward(Vector3.Zero, 1f);

            Assert.AreEqual(45f, _aim.YawDegrees, Tolerance);
        }

        [Test]
        public void TurnToward_NegativeTime_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _aim.TurnToward(Vector3.UnitZ, -1f));
        }

        [TestCase(0f, 30f, -10f, 45f)]
        [TestCase(90f, 0f, -10f, 45f)]
        [TestCase(90f, 30f, 45f, 45f)]
        [TestCase(90f, 30f, -100f, 45f)]
        public void Settings_InvalidValues_Throw(float yawSpeed, float pitchSpeed, float minPitch, float maxPitch)
        {
            Assert.Throws(Is.InstanceOf<ArgumentException>(), () => new TurretAimSettings(yawSpeed, pitchSpeed, minPitch, maxPitch));
        }
    }
}
