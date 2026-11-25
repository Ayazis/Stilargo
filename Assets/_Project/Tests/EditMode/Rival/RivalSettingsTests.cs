using System;
using NUnit.Framework;
using Stilargo.Core.Rival;

namespace Stilargo.Tests.EditMode.Rival
{
    public sealed class RivalSettingsTests
    {
        [Test]
        public void Defaults_MatchTheConstants()
        {
            var settings = new RivalSettings();

            Assert.AreEqual(RivalSettings.DefaultSquadSize, settings.SquadSize);
            Assert.AreEqual(RivalSettings.DefaultScavengeSeconds, settings.ScavengeSeconds);
            Assert.AreEqual(RivalSettings.DefaultContestSeconds, settings.ContestSeconds);
            Assert.AreEqual(RivalSettings.DefaultRaidSeconds, settings.RaidSeconds);
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void WithSquadSize_Invalid_Throws(int units)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RivalSettings().WithSquadSize(units));
        }

        [TestCase(0f)]
        [TestCase(float.NaN)]
        public void WithMoveSpeed_Invalid_Throws(float speed)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RivalSettings().WithMoveSpeed(speed));
        }

        [Test]
        public void WithStrategyTimes_AnyInvalid_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RivalSettings().WithStrategyTimes(0f, 1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RivalSettings().WithStrategyTimes(1f, -1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RivalSettings().WithStrategyTimes(1f, 1f, float.PositiveInfinity));
        }

        [Test]
        public void WithHillSpread_Zero_IsAllowed()
        {
            Assert.AreEqual(0f, new RivalSettings().WithHillSpread(0f).HillSpreadMetres);
        }

        [TestCase(-0.1f)]
        [TestCase(float.NaN)]
        public void WithHillSpread_Invalid_Throws(float metres)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RivalSettings().WithHillSpread(metres));
        }

        [Test]
        public void WithReachAndTolerance_Invalid_Throw()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RivalSettings().WithScavengeReach(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RivalSettings().WithScavengeRadius(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RivalSettings().WithArrivalTolerance(-1f));
        }
    }
}
