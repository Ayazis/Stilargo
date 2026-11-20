using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Combat.Targeting;

namespace Stilargo.Tests.EditMode.Combat
{
    public sealed class TargetPriorityTests
    {
        private const float MaxHealth = 100f;

        private static FakeCombatTarget TargetAt(float x, float z)
        {
            return new FakeCombatTarget(new Vector3(x, 0f, z), MaxHealth);
        }

        [Test]
        public void Nearest_EmptyList_ReturnsNull()
        {
            ICombatTarget chosen = new NearestTargetPriority().Choose(Vector3.Zero, new List<ICombatTarget>());

            Assert.That(chosen, Is.Null);
        }

        [Test]
        public void Nearest_AllDead_ReturnsNull()
        {
            FakeCombatTarget dead = TargetAt(1f, 0f);
            dead.TakeDamage(MaxHealth);

            ICombatTarget chosen = new NearestTargetPriority().Choose(Vector3.Zero, new List<ICombatTarget> { dead });

            Assert.That(chosen, Is.Null);
        }

        [Test]
        public void Nearest_PicksClosestLivingOnGroundPlane()
        {
            FakeCombatTarget far = TargetAt(10f, 0f);
            FakeCombatTarget nearButHigh = new FakeCombatTarget(new Vector3(2f, 30f, 0f), MaxHealth);
            FakeCombatTarget deadClosest = TargetAt(1f, 0f);
            deadClosest.TakeDamage(MaxHealth);

            ICombatTarget chosen = new NearestTargetPriority().Choose(Vector3.Zero, new List<ICombatTarget> { far, nearButHigh, deadClosest });

            Assert.That(chosen, Is.SameAs(nearButHigh));
        }

        [Test]
        public void Nearest_Tie_KeepsFirst()
        {
            FakeCombatTarget first = TargetAt(3f, 0f);
            FakeCombatTarget second = TargetAt(0f, 3f);

            ICombatTarget chosen = new NearestTargetPriority().Choose(Vector3.Zero, new List<ICombatTarget> { first, second });

            Assert.That(chosen, Is.SameAs(first));
        }

        [Test]
        public void Weakest_EmptyList_ReturnsNull()
        {
            ICombatTarget chosen = new WeakestTargetPriority().Choose(Vector3.Zero, new List<ICombatTarget>());

            Assert.That(chosen, Is.Null);
        }

        [Test]
        public void Weakest_AllDead_ReturnsNull()
        {
            FakeCombatTarget dead = TargetAt(1f, 0f);
            dead.TakeDamage(MaxHealth);

            ICombatTarget chosen = new WeakestTargetPriority().Choose(Vector3.Zero, new List<ICombatTarget> { dead });

            Assert.That(chosen, Is.Null);
        }

        [Test]
        public void Weakest_PicksLowestHealthFraction()
        {
            FakeCombatTarget healthyNear = TargetAt(1f, 0f);
            FakeCombatTarget woundedFar = TargetAt(20f, 0f);
            woundedFar.TakeDamage(60f);

            ICombatTarget chosen = new WeakestTargetPriority().Choose(Vector3.Zero, new List<ICombatTarget> { healthyNear, woundedFar });

            Assert.That(chosen, Is.SameAs(woundedFar));
        }

        [Test]
        public void Weakest_Tie_PicksCloser()
        {
            FakeCombatTarget far = TargetAt(9f, 0f);
            FakeCombatTarget near = TargetAt(4f, 0f);
            far.TakeDamage(50f);
            near.TakeDamage(50f);

            ICombatTarget chosen = new WeakestTargetPriority().Choose(Vector3.Zero, new List<ICombatTarget> { far, near });

            Assert.That(chosen, Is.SameAs(near));
        }
    }
}
