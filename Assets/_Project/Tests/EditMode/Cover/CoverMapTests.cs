using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Cover;

namespace Stilargo.Tests.EditMode.Cover
{
    public sealed class CoverMapTests
    {
        private const float Tolerance = 1e-4f;
        private const float InsideRadius = 1f;
        private const float OutsideRadius = 1.5f;
        private const float HighAbove = 10f;

        private static readonly Vector3 Facing = new Vector3(1f, 0f, 0f);
        private static readonly Vector3 NearPoint = new Vector3(10f, 0f, 10f);
        private static readonly Vector3 FarPoint = new Vector3(11.5f, 0f, 10f);

        [Test]
        public void TryFindCoverAt_InsideRadius_FindsPoint()
        {
            CoverMap map = CreateMap(NearPoint);

            Assert.IsTrue(map.TryFindCoverAt(NearPoint + new Vector3(InsideRadius, 0f, 0f), out CoverPoint point));
            Assert.AreSame(map.Points[0], point);
        }

        [Test]
        public void TryFindCoverAt_OutsideRadius_FindsNothing()
        {
            CoverMap map = CreateMap(NearPoint);

            Assert.IsFalse(map.TryFindCoverAt(NearPoint + new Vector3(0f, 0f, OutsideRadius), out CoverPoint point));
            Assert.IsNull(point);
        }

        [Test]
        public void TryFindCoverAt_IgnoresHeight()
        {
            CoverMap map = CreateMap(NearPoint);

            Assert.IsTrue(map.TryFindCoverAt(NearPoint + new Vector3(0f, HighAbove, 0f), out _));
        }

        [Test]
        public void TryFindCoverAt_TwoInRange_NearestWins()
        {
            CoverMap map = CreateMap(NearPoint, FarPoint);
            Vector3 closerToFar = new Vector3(11f, 0f, 10f);

            Assert.IsTrue(map.TryFindCoverAt(closerToFar, out CoverPoint point));
            Assert.AreSame(map.Points[1], point);
        }

        [Test]
        public void BonusAt_InCover_ReducesHitChanceOnly()
        {
            CoverMap map = CreateMap(NearPoint);

            PositionBonus bonus = map.BonusAt(NearPoint);

            Assert.Less(bonus.Protection.HitChanceMultiplier, 1f);
            Assert.AreEqual(1f, bonus.Protection.DamageMultiplier, Tolerance);
            Assert.AreEqual(0f, bonus.RangeMetres, Tolerance);
            Assert.AreEqual(0f, bonus.SightMetres, Tolerance);
        }

        [Test]
        public void BonusAt_OutOfCover_IsNone()
        {
            CoverMap map = CreateMap(NearPoint);

            PositionBonus bonus = map.BonusAt(NearPoint + new Vector3(OutsideRadius, 0f, 0f));

            Assert.AreEqual(1f, bonus.Protection.HitChanceMultiplier, Tolerance);
        }

        private static CoverMap CreateMap(params Vector3[] positions)
        {
            CoverSettings settings = CoverSettings.Default;
            List<CoverPoint> points = new List<CoverPoint>();
            foreach (Vector3 position in positions)
            {
                points.Add(new CoverPoint(position, Facing, settings.Protection));
            }

            return new CoverMap(points, settings);
        }
    }
}
