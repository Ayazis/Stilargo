using System;
using NUnit.Framework;
using Stilargo.Core.Hill;

namespace Stilargo.Tests.EditMode.Hill
{
    public sealed class HillSettingsTests
    {
        [Test]
        public void Default_UsesTheDefaultConstants()
        {
            HillSettings settings = HillSettings.Default;

            Assert.AreEqual(HillSettings.DefaultCenterXMetres, settings.CenterXMetres);
            Assert.AreEqual(HillSettings.DefaultCenterZMetres, settings.CenterZMetres);
            Assert.AreEqual(HillSettings.DefaultRadiusMetres, settings.RadiusMetres);
            Assert.AreEqual(HillSettings.DefaultHoldDurationSeconds, settings.HoldDurationSeconds);
        }

        [TestCase(0f)]
        [TestCase(float.NaN)]
        public void Constructor_InvalidRadius_Throws(float radius)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HillSettings(0f, 0f, radius));
        }

        [TestCase(float.NaN)]
        [TestCase(float.NegativeInfinity)]
        public void Constructor_NonFiniteCenter_Throws(float coordinate)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HillSettings(coordinate, 0f, 1f));
        }

        [TestCase(0f)]
        [TestCase(float.PositiveInfinity)]
        public void WithHoldDuration_Invalid_Throws(float seconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HillSettings(0f, 0f, 1f).WithHoldDuration(seconds));
        }
    }
}
