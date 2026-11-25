using System;
using NUnit.Framework;
using Stilargo.Core.Economy;

namespace Stilargo.Tests.EditMode.Economy
{
    public sealed class EconomySettingsTests
    {
        [Test]
        public void Default_StartsWithDefaultMoney()
        {
            Assert.AreEqual(EconomySettings.DefaultStartingMoney, new EconomySettings().StartingMoney);
        }

        [TestCase(0)]
        [TestCase(250)]
        public void ValidStartingMoney_IsKept(int amount)
        {
            Assert.AreEqual(amount, new EconomySettings(amount).StartingMoney);
        }

        [Test]
        public void NegativeStartingMoney_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EconomySettings(-1));
        }
    }
}
