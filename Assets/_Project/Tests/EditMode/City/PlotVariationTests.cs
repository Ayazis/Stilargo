using System;
using System.Collections.Generic;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class PlotVariationTests
    {
        private const int VariantCount = 6;

        [Test]
        public void Pick_SamePlot_ReturnsSameVariant()
        {
            Assert.AreEqual(PlotVariation.Pick(PlotAt(3, 4), VariantCount), PlotVariation.Pick(PlotAt(3, 4), VariantCount));
        }

        [Test]
        public void Pick_ManyPlots_StaysInRangeAndUsesSeveralVariants()
        {
            var used = new HashSet<int>();
            for (int x = 0; x < 20; x++)
            {
                int variant = PlotVariation.Pick(PlotAt(x * 5, x * 3), VariantCount);
                Assert.That(variant, Is.InRange(0, VariantCount - 1));
                used.Add(variant);
            }

            Assert.That(used.Count, Is.GreaterThan(2));
        }

        [Test]
        public void Pick_NoVariants_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PlotVariation.Pick(PlotAt(0, 0), 0));
        }

        private static Plot PlotAt(int x, int y)
        {
            return new Plot(new GridRect(new GridPosition(x, y), 3, 3), 2);
        }
    }
}
