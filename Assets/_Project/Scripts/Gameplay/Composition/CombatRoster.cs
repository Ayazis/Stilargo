using System;
using System.Collections.Generic;
using Stilargo.Core.Combat;
using Stilargo.Core.Combat.Stances;
using Stilargo.Core.Combat.Targeting;
using Stilargo.Core.Selection;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Units;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Builds each side's combat and keeps the live lists that units target, so a dead unit leaves
    /// the enemy lists, the selection candidates, the stance lookup and the selection in one place.
    /// </summary>
    internal sealed class CombatRoster
    {
        private readonly List<ICombatTarget> _playerTargets = new List<ICombatTarget>();
        private readonly List<ICombatTarget> _enemyTargets = new List<ICombatTarget>();
        private readonly List<ISelectable> _selectableUnits = new List<ISelectable>();
        private readonly Dictionary<ISelectable, UnitCombat> _combatByUnit = new Dictionary<ISelectable, UnitCombat>();
        private readonly CombatRosterSettings _settings;
        private readonly ITargetPriority _priority = new NearestTargetPriority();
        private UnitSelection _selection;

        public CombatRoster(CombatRosterSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>Live list of player units the selection can pick from; dead units leave it.</summary>
        public IReadOnlyList<ISelectable> SelectableUnits => _selectableUnits;

        public IReadOnlyDictionary<ISelectable, UnitCombat> CombatByUnit => _combatByUnit;

        /// <summary>Call once the selection exists, so dead units can leave it.</summary>
        public void AttachSelection(UnitSelection selection)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        /// <summary>Call after <see cref="SelectableUnit.Construct"/>, which places the agent and creates its orders.</summary>
        public void AddPlayerUnit(SelectableUnit unit, CombatUnit combatUnit, HitEffectView hitEffect)
        {
            UnitCombat combat = CreateCombat(_settings.PlayerSide, _settings.Stances.Defensive);
            var setup = new CombatUnitSetup(combat, new Health(_settings.PlayerSide.MaxHealth), _enemyTargets).WithOrders(unit.Orders);
            Construct(combatUnit, setup, hitEffect);

            _playerTargets.Add(combatUnit);
            _selectableUnits.Add(unit);
            _combatByUnit.Add(unit, combat);
            combatUnit.Died += () => RemovePlayerUnit(unit, combatUnit);
        }

        public void AddEnemyUnit(CombatUnit combatUnit, HitEffectView hitEffect)
        {
            UnitCombat combat = CreateCombat(_settings.EnemySide, _settings.Stances.Defensive);
            Construct(combatUnit, new CombatUnitSetup(combat, new Health(_settings.EnemySide.MaxHealth), _playerTargets), hitEffect);

            _enemyTargets.Add(combatUnit);
            combatUnit.Died += () => _enemyTargets.Remove(combatUnit);
        }

        private static void Construct(CombatUnit combatUnit, CombatUnitSetup setup, HitEffectView hitEffect)
        {
            combatUnit.Construct(setup);
            hitEffect.Construct(combatUnit.Combat);
        }

        private UnitCombat CreateCombat(CombatSide side, IStance stance)
        {
            var combat = new UnitCombat(new Weapon(side.Weapon.ToSettings()), _priority, new CombatAwareness(side.SightRange));
            combat.SetStance(stance);
            return combat;
        }

        private void RemovePlayerUnit(SelectableUnit unit, CombatUnit combatUnit)
        {
            _playerTargets.Remove(combatUnit);
            _selectableUnits.Remove(unit);
            _combatByUnit.Remove(unit);

            if (_selection != null)
            {
                _selection.Remove(unit);
            }
        }
    }
}
