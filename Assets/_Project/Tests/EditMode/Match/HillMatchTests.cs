using System;
using System.Collections.Generic;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Hill;
using Stilargo.Core.Match;
using Stilargo.Core.Rival;
using Stilargo.Tests.EditMode.Combat;
using Stilargo.Tests.EditMode.Rival;

namespace Stilargo.Tests.EditMode.Match
{
    public sealed class HillMatchTests
    {
        private static readonly IReadOnlyList<ICombatTarget> Nobody = Array.Empty<ICombatTarget>();

        private RivalTestWorld _test;
        private RivalBrain _brain;
        private MatchReferee _referee;
        private HillMatch _match;

        [SetUp]
        public void SetUp()
        {
            _test = new RivalTestWorld();
            _brain = new RivalBrain(_test.World, _test.Strategies);
            _brain.Start();
            _referee = new MatchReferee(_test.Hill, _test.Claim);
            _match = new HillMatch(_test.Hill, _brain, _referee);
        }

        [Test]
        public void Tick_RunsTheHillAndTheRival()
        {
            _match.Tick(RivalTestWorld.ScavengeSeconds, PlayerOnHill(), Nobody);

            Assert.AreEqual(HillState.HeldByPlayer, _test.Hill.State);
            Assert.AreSame(_test.Strategies.ContestHill, _brain.Current);
        }

        [Test]
        public void Tick_AfterTheMatchIsDecided_StopsEverything()
        {
            _match.Tick(_test.Hill.Settings.HoldDurationSeconds, PlayerOnHill(), Nobody);
            Assert.AreEqual(MatchOutcome.Won, _referee.Outcome);
            IRivalStrategy before = _brain.Current;
            float held = _test.Hill.HeldSeconds;

            _match.Tick(RivalTestWorld.ScavengeSeconds, Nobody, Nobody);

            Assert.AreSame(before, _brain.Current);
            Assert.AreEqual(held, _test.Hill.HeldSeconds);
            Assert.AreEqual(HillState.HeldByPlayer, _test.Hill.State);
        }

        private static IReadOnlyList<ICombatTarget> PlayerOnHill()
        {
            return new ICombatTarget[] { new FakeCombatTarget(RivalTestWorld.HillCenter, 1f) };
        }
    }
}
