using System;
using System.Collections.Generic;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Combat.Stances;
using Stilargo.Core.Combat.Targeting;
using Stilargo.Core.Hud;
using Stilargo.Core.Selection;
using Stilargo.Tests.EditMode.Selection;

namespace Stilargo.Tests.EditMode.Hud
{
    public sealed class StanceMenuTests
    {
        private const float LeashRadius = 10f;
        private const float Damage = 5f;
        private const float WeaponRange = 8f;
        private const float CooldownSeconds = 1f;
        private const float SightRange = 20f;

        private readonly AggressiveStance _aggressive = new AggressiveStance();
        private readonly DefensiveStance _defensive = new DefensiveStance(LeashRadius);
        private readonly HoldStance _hold = new HoldStance();

        private Dictionary<ISelectable, UnitCombat> _combatByUnit;
        private UnitSelection _selection;
        private StanceMenu _menu;

        [SetUp]
        public void SetUp()
        {
            _combatByUnit = new Dictionary<ISelectable, UnitCombat>();
            _selection = new UnitSelection();
            var selectionStance = new SelectionStance(new StanceSet(_aggressive, _defensive, _hold), _combatByUnit);
            _menu = new StanceMenu(_selection, selectionStance);
        }

        [Test]
        public void Options_ListAllThreeStances()
        {
            CollectionAssert.AreEqual(new IStance[] { _aggressive, _defensive, _hold }, _menu.Options);
        }

        [Test]
        public void HasSelection_FollowsTheSelection()
        {
            Assert.IsFalse(_menu.HasSelection);

            _selection.Replace(new[] { AddUnit(_aggressive) });

            Assert.IsTrue(_menu.HasSelection);
        }

        [Test]
        public void Choose_SetsStanceOnSelectedUnits()
        {
            FakeSelectable unit = AddUnit(_aggressive);
            _selection.Replace(new[] { unit });

            _menu.Choose(_hold);

            Assert.AreSame(_hold, _combatByUnit[unit].Stance);
        }

        [Test]
        public void IsActive_OnlyForTheSharedStance()
        {
            _selection.Replace(new[] { AddUnit(_defensive), AddUnit(_defensive) });

            Assert.IsTrue(_menu.IsActive(_defensive));
            Assert.IsFalse(_menu.IsActive(_hold));
        }

        [Test]
        public void IsActive_MixedStances_IsNeverActive()
        {
            _selection.Replace(new[] { AddUnit(_defensive), AddUnit(_hold) });

            Assert.IsFalse(_menu.IsActive(_defensive));
            Assert.IsFalse(_menu.IsActive(_hold));
        }

        [Test]
        public void IsActive_NullStance_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _menu.IsActive(null));
        }

        private FakeSelectable AddUnit(IStance stance)
        {
            var unit = new FakeSelectable();
            var weapon = new Weapon(new WeaponSettings(Damage, WeaponRange, CooldownSeconds));
            var combat = new UnitCombat(weapon, new NearestTargetPriority(), new CombatAwareness(SightRange));
            combat.SetStance(stance);
            _combatByUnit.Add(unit, combat);
            return unit;
        }
    }
}
