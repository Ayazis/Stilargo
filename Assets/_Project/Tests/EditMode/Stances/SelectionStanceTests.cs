using System;
using System.Collections.Generic;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Combat.Stances;
using Stilargo.Core.Combat.Targeting;
using Stilargo.Core.Selection;
using Stilargo.Tests.EditMode.Selection;

namespace Stilargo.Tests.EditMode.Stances
{
    public sealed class SelectionStanceTests
    {
        private const float Damage = 10f;
        private const float WeaponRange = 20f;
        private const float CooldownSeconds = 1f;
        private const float SightRange = 30f;
        private const float LeashRadius = 15f;

        private readonly AggressiveStance _aggressive = new AggressiveStance();
        private readonly DefensiveStance _defensive = new DefensiveStance(LeashRadius);
        private readonly HoldStance _hold = new HoldStance();

        private Dictionary<ISelectable, UnitCombat> _combatByUnit;
        private UnitSelection _selection;
        private SelectionStance _selectionStance;

        [SetUp]
        public void SetUp()
        {
            _combatByUnit = new Dictionary<ISelectable, UnitCombat>();
            _selection = new UnitSelection();
            _selectionStance = new SelectionStance(new StanceSet(_aggressive, _defensive, _hold), _combatByUnit);
        }

        private static UnitCombat NewCombat()
        {
            var weapon = new Weapon(new WeaponSettings(Damage, WeaponRange, CooldownSeconds));
            return new UnitCombat(weapon, new NearestTargetPriority(), new CombatAwareness(SightRange));
        }

        private FakeSelectable AddUnit(IStance stance)
        {
            var unit = new FakeSelectable();
            UnitCombat combat = NewCombat();
            combat.SetStance(stance);
            _combatByUnit.Add(unit, combat);
            return unit;
        }

        [Test]
        public void Apply_SetsStanceOnEverySelectedUnit()
        {
            FakeSelectable first = AddUnit(_aggressive);
            FakeSelectable second = AddUnit(_defensive);
            _selection.Replace(new ISelectable[] { first, second });

            _selectionStance.Apply(_selection, _hold);

            Assert.AreSame(_hold, _combatByUnit[first].Stance);
            Assert.AreSame(_hold, _combatByUnit[second].Stance);
        }

        [Test]
        public void Apply_LeavesUnselectedUnitsAlone()
        {
            FakeSelectable selected = AddUnit(_aggressive);
            FakeSelectable unselected = AddUnit(_aggressive);
            _selection.Replace(new ISelectable[] { selected });

            _selectionStance.Apply(_selection, _hold);

            Assert.AreSame(_aggressive, _combatByUnit[unselected].Stance);
        }

        [Test]
        public void Apply_SkipsSelectedUnitsMissingFromLookup()
        {
            FakeSelectable alive = AddUnit(_aggressive);
            _selection.Replace(new ISelectable[] { new FakeSelectable(), alive });

            _selectionStance.Apply(_selection, _hold);

            Assert.AreSame(_hold, _combatByUnit[alive].Stance);
        }

        [Test]
        public void Apply_NullStance_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _selectionStance.Apply(_selection, null));
        }

        [Test]
        public void CurrentStanceName_NothingSelected_IsPlaceholder()
        {
            AddUnit(_aggressive);

            Assert.AreEqual(SelectionStance.NothingSelectedName, _selectionStance.CurrentStanceName(_selection));
        }

        [Test]
        public void CurrentStanceName_SharedStance_IsItsName()
        {
            FakeSelectable first = AddUnit(_defensive);
            FakeSelectable second = AddUnit(_defensive);
            _selection.Replace(new ISelectable[] { first, second });

            Assert.AreEqual("Defensive", _selectionStance.CurrentStanceName(_selection));
        }

        [Test]
        public void CurrentStanceName_DifferentStances_IsMixed()
        {
            FakeSelectable first = AddUnit(_defensive);
            FakeSelectable second = AddUnit(_hold);
            _selection.Replace(new ISelectable[] { first, second });

            Assert.AreEqual(SelectionStance.MixedName, _selectionStance.CurrentStanceName(_selection));
        }

        [Test]
        public void CurrentStanceName_IgnoresUnitsMissingFromLookup()
        {
            FakeSelectable alive = AddUnit(_hold);
            _selection.Replace(new ISelectable[] { new FakeSelectable(), alive });

            Assert.AreEqual("Hold", _selectionStance.CurrentStanceName(_selection));
        }

        [Test]
        public void CurrentStanceName_OnlyMissingUnitsSelected_IsPlaceholder()
        {
            _selection.Replace(new ISelectable[] { new FakeSelectable() });

            Assert.AreEqual(SelectionStance.NothingSelectedName, _selectionStance.CurrentStanceName(_selection));
        }

        [Test]
        public void CurrentStanceName_AfterApply_IsAppliedStance()
        {
            FakeSelectable first = AddUnit(_aggressive);
            FakeSelectable second = AddUnit(_hold);
            _selection.Replace(new ISelectable[] { first, second });

            _selectionStance.Apply(_selection, _aggressive);

            Assert.AreEqual("Aggressive", _selectionStance.CurrentStanceName(_selection));
        }

        [Test]
        public void Constructor_NullLookup_Throws()
        {
            var stances = new StanceSet(_aggressive, _defensive, _hold);

            Assert.Throws<ArgumentNullException>(() => new SelectionStance(stances, null));
        }

        [Test]
        public void StanceSet_NullStance_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new StanceSet(_aggressive, null, _hold));
        }
    }
}
