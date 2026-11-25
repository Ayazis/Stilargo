using System;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class HipRoofBuilderTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Build_PeaksAtRiseAndCoversTheBase()
        {
            RoofGeometry roof = HipRoofBuilder.Build(10f, 6f, 2f);

            Assert.AreEqual(2f, roof.Vertices.Max(v => v.Y), Tolerance);
            Assert.AreEqual(5f, roof.Vertices.Max(v => v.X), Tolerance);
            Assert.AreEqual(-3f, roof.Vertices.Min(v => v.Z), Tolerance);
        }

        [Test]
        public void Build_UvsRunUpTheSlopeInMetres()
        {
            RoofGeometry roof = HipRoofBuilder.Build(10f, 6f, 4f);

            Assert.AreEqual(5f, roof.Uvs.Max(uv => uv.Y), Tolerance);
            Assert.AreEqual(roof.Vertices.Count, roof.Uvs.Count);
            Assert.That(roof.Triangles.All(index => index < roof.Vertices.Count));
        }

        [Test]
        public void Build_WiderThanLong_Throws()
        {
            Assert.Throws<ArgumentException>(() => HipRoofBuilder.Build(4f, 6f, 1f));
        }

        [Test]
        public void Build_NonPositiveRise_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => HipRoofBuilder.Build(6f, 4f, 0f));
        }
    }
}
