using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Selection;

namespace Stilargo.Tests.EditMode.Selection
{
    public sealed class ScreenRectTests
    {
        private const float Left = 10f;
        private const float Right = 50f;
        private const float Bottom = 20f;
        private const float Top = 80f;
        private const float Middle = 30f;
        private const float Outside = 100f;

        private static readonly Vector2 BottomLeft = new Vector2(Left, Bottom);
        private static readonly Vector2 BottomRight = new Vector2(Right, Bottom);
        private static readonly Vector2 TopLeft = new Vector2(Left, Top);
        private static readonly Vector2 TopRight = new Vector2(Right, Top);

        private static readonly object[] AllDragDirections =
        {
            new object[] { BottomLeft, TopRight },
            new object[] { TopRight, BottomLeft },
            new object[] { TopLeft, BottomRight },
            new object[] { BottomRight, TopLeft },
        };

        [TestCaseSource(nameof(AllDragDirections))]
        public void FromCorners_AnyDragDirection_NormalizesMinAndMax(Vector2 start, Vector2 end)
        {
            ScreenRect rect = ScreenRect.FromCorners(start, end);

            Assert.That(rect.Min, Is.EqualTo(BottomLeft));
            Assert.That(rect.Max, Is.EqualTo(TopRight));
        }

        [TestCaseSource(nameof(AllDragDirections))]
        public void FromCorners_AnyDragDirection_HasPositiveSize(Vector2 start, Vector2 end)
        {
            ScreenRect rect = ScreenRect.FromCorners(start, end);

            Assert.That(rect.Width, Is.EqualTo(Right - Left));
            Assert.That(rect.Height, Is.EqualTo(Top - Bottom));
        }

        [Test]
        public void FromCorners_SamePoint_HasZeroSize()
        {
            ScreenRect rect = ScreenRect.FromCorners(BottomLeft, BottomLeft);

            Assert.That(rect.Width, Is.Zero);
            Assert.That(rect.Height, Is.Zero);
            Assert.That(rect.Contains(BottomLeft), Is.True);
        }

        [TestCase(Middle, Middle)]
        [TestCase(Left, Middle)]
        [TestCase(Right, Middle)]
        [TestCase(Middle, Bottom)]
        [TestCase(Middle, Top)]
        [TestCase(Left, Bottom)]
        [TestCase(Right, Top)]
        public void Contains_InsideOrOnEdge_IsTrue(float x, float y)
        {
            ScreenRect rect = ScreenRect.FromCorners(BottomLeft, TopRight);

            Assert.That(rect.Contains(new Vector2(x, y)), Is.True);
        }

        [TestCase(Outside, Middle)]
        [TestCase(-Outside, Middle)]
        [TestCase(Middle, Outside)]
        [TestCase(Middle, -Outside)]
        public void Contains_Outside_IsFalse(float x, float y)
        {
            ScreenRect rect = ScreenRect.FromCorners(BottomLeft, TopRight);

            Assert.That(rect.Contains(new Vector2(x, y)), Is.False);
        }
    }
}
