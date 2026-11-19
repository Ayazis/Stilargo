using System;
using NUnit.Framework;
using Stilargo.Core.Formations;

namespace Stilargo.Tests.EditMode.Formations
{
    public sealed class FormationCycleTests
    {
        private const float Spacing = 2f;

        private readonly IFormation _line = new LineFormation(Spacing);
        private readonly IFormation _box = new BoxFormation(Spacing);

        [Test]
        public void Constructor_WithNoFormations_Throws()
        {
            Assert.Throws<ArgumentException>(() => new FormationCycle(Array.Empty<IFormation>()));
        }

        [Test]
        public void Constructor_WithNullList_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FormationCycle(null));
        }

        [Test]
        public void Constructor_WithNullFormation_Throws()
        {
            Assert.Throws<ArgumentException>(() => new FormationCycle(new[] { _line, null }));
        }

        [Test]
        public void Current_StartsAtFirst()
        {
            var cycle = new FormationCycle(new[] { _line, _box });

            Assert.That(cycle.Current, Is.SameAs(_line));
        }

        [Test]
        public void Next_AdvancesAndWrapsAround()
        {
            var cycle = new FormationCycle(new[] { _line, _box });

            Assert.That(cycle.Next(), Is.SameAs(_box));
            Assert.That(cycle.Current, Is.SameAs(_box));
            Assert.That(cycle.Next(), Is.SameAs(_line));
        }

        [Test]
        public void Next_WithSingleFormation_StaysOnIt()
        {
            var cycle = new FormationCycle(new[] { _box });

            Assert.That(cycle.Next(), Is.SameAs(_box));
        }
    }
}
