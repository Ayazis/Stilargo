using System;
using NUnit.Framework;
using Stilargo.Core.Hud;

namespace Stilargo.Tests.EditMode.Hud
{
    public sealed class HudTextTests
    {
        [Test]
        public void Money_ShowsBalance()
        {
            Assert.AreEqual("Money: 125", HudText.Money(125));
        }

        [TestCase(0f, "Hold the hill: 00:00")]
        [TestCase(59.2f, "Hold the hill: 01:00")]
        [TestCase(60f, "Hold the hill: 01:00")]
        [TestCase(61f, "Hold the hill: 01:01")]
        [TestCase(600f, "Hold the hill: 10:00")]
        public void HillTimer_RoundsUpToWholeSeconds(float seconds, string expected)
        {
            Assert.AreEqual(expected, HudText.HillTimer(seconds));
        }

        [Test]
        public void HillTimer_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => HudText.HillTimer(-1f));
        }

        [Test]
        public void HouseHealth_ShowsWholeNumbers()
        {
            Assert.AreEqual("House: 80 / 120 HP", HudText.HouseHealth(79.6f, 120f));
        }

        [Test]
        public void NoHillStatus_IsInactive()
        {
            Assert.IsFalse(new NoHillStatus().IsActive);
        }
    }
}
