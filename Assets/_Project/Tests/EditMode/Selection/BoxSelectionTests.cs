using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Selection;

namespace Stilargo.Tests.EditMode.Selection
{
    public sealed class BoxSelectionTests
    {
        private static readonly ScreenRect Box = ScreenRect.FromCorners(new Vector2(0f, 0f), new Vector2(100f, 100f));
        private static readonly Vector2 InsideBox = new Vector2(50f, 50f);
        private static readonly Vector2 OnBoxEdge = new Vector2(100f, 30f);
        private static readonly Vector2 OutsideBox = new Vector2(150f, 50f);

        private FakeScreenProjector _projector;

        [SetUp]
        public void SetUp()
        {
            _projector = new FakeScreenProjector();
        }

        [Test]
        public void Within_ReturnsOnlyUnitsInsideOrOnEdge()
        {
            FakeSelectable inside = PlacedAt(new Vector3(1f, 0f, 0f), InsideBox);
            FakeSelectable onEdge = PlacedAt(new Vector3(2f, 0f, 0f), OnBoxEdge);
            FakeSelectable outside = PlacedAt(new Vector3(3f, 0f, 0f), OutsideBox);

            var result = BoxSelection.Within(Box, new[] { inside, onEdge, outside }, _projector);

            Assert.That(result, Is.EqualTo(new[] { inside, onEdge }));
        }

        [Test]
        public void Within_ExcludesUnitsBehindCamera()
        {
            var behindCamera = new FakeSelectable(new Vector3(4f, 0f, 0f));

            var result = BoxSelection.Within(Box, new[] { behindCamera }, _projector);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Within_NoCandidates_ReturnsEmpty()
        {
            var result = BoxSelection.Within(Box, Array.Empty<ISelectable>(), _projector);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Within_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => BoxSelection.Within(Box, null, _projector));
            Assert.Throws<ArgumentNullException>(
                () => BoxSelection.Within(Box, Array.Empty<ISelectable>(), null));
        }

        [Test]
        public void Within_NullCandidate_Throws()
        {
            Assert.Throws<ArgumentException>(() => BoxSelection.Within(Box, new ISelectable[] { null }, _projector));
        }

        private FakeSelectable PlacedAt(Vector3 world, Vector2 screen)
        {
            _projector.Place(world, screen);
            return new FakeSelectable(world);
        }
    }
}
