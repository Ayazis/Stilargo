using NUnit.Framework;
using Stilargo.Core.Fog;

namespace Stilargo.Tests.EditMode.Fog
{
    public sealed class VisionSettingsTests
    {
        [Test]
        public void RadiusAt_StreetLevel_IsTheUnitRadius()
        {
            var settings = new VisionSettings().WithUnitVision(20f, 1.7f).WithHeightBonus(1f, 60f);

            Assert.AreEqual(20f, settings.RadiusAt(0f, 0f), 0.001f);
        }

        [Test]
        public void RadiusAt_GrowsWithHeightAndBonus()
        {
            var settings = new VisionSettings().WithUnitVision(20f, 1.7f).WithHeightBonus(1.5f, 60f);

            Assert.AreEqual(20f + 8f + 15f, settings.RadiusAt(10f, 8f), 0.001f);
        }

        [Test]
        public void RadiusAt_NeverExceedsTheMaximum()
        {
            var settings = new VisionSettings().WithUnitVision(20f, 1.7f).WithHeightBonus(10f, 40f);

            Assert.AreEqual(40f, settings.RadiusAt(30f, 0f), 0.001f);
        }

        [Test]
        public void RadiusAt_BelowTheStreet_CountsAsStreetLevel()
        {
            var settings = new VisionSettings().WithUnitVision(20f, 1.7f).WithHeightBonus(2f, 60f);

            Assert.AreEqual(20f, settings.RadiusAt(-3f, 0f), 0.001f);
        }

        [Test]
        public void WithUnitVision_RejectsNonPositiveRadius()
        {
            Assert.That(() => new VisionSettings().WithUnitVision(0f, 1.7f), Throws.InstanceOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void WithFogResolution_BelowOne_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new VisionSettings().WithFogResolution(0));
        }

        [Test]
        public void WithFogResolution_StoresTheSplit()
        {
            Assert.AreEqual(3, new VisionSettings().WithFogResolution(3).FogCellsPerCityCell);
        }
    }
}
