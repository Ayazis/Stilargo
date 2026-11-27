using System;
using System.Collections.Generic;
using Stilargo.Core.Combat;
using Stilargo.Core.Combat.Stances;
using Stilargo.Core.Combat.Targeting;
using Stilargo.Core.Orders;
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
        private readonly List<HealthBarSource> _healthBars = new List<HealthBarSource>();
        private readonly Dictionary<ISelectable, UnitCombat> _combatByUnit = new Dictionary<ISelectable, UnitCombat>();
        private readonly CombatRosterSettings _settings;
        private readonly IVisibilityQuery _playerVisibility;
        private readonly IVisibilityQuery _enemyVisibility = new AlwaysVisible();
        private readonly ITargetPriority _priority = new NearestTargetPriority();
        private UnitSelection _selection;

        /// <param name="playerVisibility">The fog of war; player units only target enemies it shows. The AI side sees everything.</param>
        public CombatRoster(CombatRosterSettings settings, IVisibilityQuery playerVisibility)
        {
            _playerVisibility = playerVisibility ?? throw new ArgumentNullException(nameof(playerVisibility));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>Live list of player units the selection can pick from; dead units leave it.</summary>
        public IReadOnlyList<ISelectable> SelectableUnits => _selectableUnits;

        public IReadOnlyDictionary<ISelectable, UnitCombat> CombatByUnit => _combatByUnit;

        /// <summary>Live list of every unit's health bar, both sides; a unit's bar leaves when it dies.</summary>
        public IReadOnlyList<HealthBarSource> HealthBars => _healthBars;

        /// <summary>Call once the selection exists, so dead units can leave it.</summary>
        public void AttachSelection(UnitSelection selection)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        /// <summary>Call after <see cref="SelectableUnit.Construct"/>, which places the agent and creates its orders.</summary>
        public void AddPlayerUnit(SelectableUnit unit, CombatUnit combatUnit, HitEffectView hitEffect)
        {
            UnitCombat combat = CreateCombat(_settings.PlayerSide, _playerVisibility);
            var setup = new CombatUnitSetup(combat, new Health(_settings.PlayerSide.MaxHealth), _enemyTargets).WithOrders(unit.Orders);
            Construct(combatUnit, setup, hitEffect);

            _playerTargets.Add(combatUnit);
            _selectableUnits.Add(unit);
            _combatByUnit.Add(unit, combat);
            AddHealthBar(combatUnit, new HealthBarSource(combatUnit, _enemyVisibility, false));
            combatUnit.Died += () => RemovePlayerUnit(unit, combatUnit);
        }

        /// <param name="orders">The AI's move orders for this unit.</param>
        public void AddEnemyUnit(CombatUnit combatUnit, HitEffectView hitEffect, IUnitOrders orders)
        {
            UnitCombat combat = CreateCombat(_settings.EnemySide, _enemyVisibility);
            var setup = new CombatUnitSetup(combat, new Health(_settings.EnemySide.MaxHealth), _playerTargets).WithOrders(orders);
            Construct(combatUnit, setup, hitEffect);

            _enemyTargets.Add(combatUnit);
            AddHealthBar(combatUnit, new HealthBarSource(combatUnit, _playerVisibility, true));
            combatUnit.Died += () => _enemyTargets.Remove(combatUnit);
        }

        /// <summary>Adds something enemies may shoot that is not a unit, such as the claimed house. It leaves when it dies.</summary>
        public void AddPlayerTarget(ICombatTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            _playerTargets.Add(target);
            target.Died += () => _playerTargets.Remove(target);
        }

        /// <summary>Adds something the player's units may shoot that is not a unit, such as a rival house. It leaves when it dies.</summary>
        public void AddEnemyTarget(ICombatTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            _enemyTargets.Add(target);
            target.Died += () => _enemyTargets.Remove(target);
        }

        private void AddHealthBar(CombatUnit combatUnit, HealthBarSource bar)
        {
            _healthBars.Add(bar);
            combatUnit.Died += () => _healthBars.Remove(bar);
        }

        private static void Construct(CombatUnit combatUnit, CombatUnitSetup setup, HitEffectView hitEffect)
        {
            combatUnit.Construct(setup);
            hitEffect.Construct(combatUnit.Combat);
        }

        private UnitCombat CreateCombat(UnitSideSettings side, IVisibilityQuery visibility)
        {
            var combat = new UnitCombat(new Weapon(side.Weapon), _priority, new CombatAwareness(side.SightRangeMetres, visibility));
            combat.SetStance(_settings.Stances.Defensive);
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
