using System;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class CityConfigTests
    {
        [Test]
        public void Validate_Defaults_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new CityConfig().Validate());
        }

        [Test]
        public void Validate_NonPositiveBlocks_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CityConfig { BlocksX = 0 }.Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new CityConfig { BlocksY = -1 }.Validate());
        }

        [Test]
        public void Validate_NonPositiveCellSize_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CityConfig { CellSize = 0f }.Validate());
        }

        [Test]
        public void Validate_MaxPlotBelowTwiceMin_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CityConfig { MinPlotSize = 4, MaxPlotSize = 7 }.Validate());
        }

        [Test]
        public void Validate_BlockSmallerThanMinPlot_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CityConfig { BlockSize = 2, MinPlotSize = 3 }.Validate());
        }

        [Test]
        public void Validate_MaxStoreysBelowMin_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CityConfig { MinStoreys = 3, MaxStoreys = 2 }.Validate());
        }
    }
}
