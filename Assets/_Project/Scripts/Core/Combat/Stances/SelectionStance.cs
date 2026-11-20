using System;
using System.Collections.Generic;
using Stilargo.Core.Selection;

namespace Stilargo.Core.Combat.Stances
{
    /// <summary>
    /// Sets stances on the selected units and summarises which stance they share.
    /// Selected units missing from the lookup (for example ones that died) are skipped.
    /// </summary>
    public sealed class SelectionStance
    {
        public const string NothingSelectedName = "-";
        public const string MixedName = "Mixed";

        private readonly IReadOnlyDictionary<ISelectable, UnitCombat> _combatByUnit;

        public SelectionStance(StanceSet stances, IReadOnlyDictionary<ISelectable, UnitCombat> combatByUnit)
        {
            Stances = stances ?? throw new ArgumentNullException(nameof(stances));
            _combatByUnit = combatByUnit ?? throw new ArgumentNullException(nameof(combatByUnit));
        }

        public StanceSet Stances { get; }

        public void Apply(UnitSelection selection, IStance stance)
        {
            if (selection == null)
            {
                throw new ArgumentNullException(nameof(selection));
            }

            if (stance == null)
            {
                throw new ArgumentNullException(nameof(stance));
            }

            foreach (ISelectable unit in selection.Selected)
            {
                if (_combatByUnit.TryGetValue(unit, out UnitCombat combat))
                {
                    combat.SetStance(stance);
                }
            }
        }

        /// <summary>The shared stance name, <see cref="MixedName"/>, or <see cref="NothingSelectedName"/>.</summary>
        public string CurrentStanceName(UnitSelection selection)
        {
            if (selection == null)
            {
                throw new ArgumentNullException(nameof(selection));
            }

            IStance shared = null;
            int counted = 0;

            foreach (ISelectable unit in selection.Selected)
            {
                if (!_combatByUnit.TryGetValue(unit, out UnitCombat combat))
                {
                    continue;
                }

                if (counted > 0 && !ReferenceEquals(shared, combat.Stance))
                {
                    return MixedName;
                }

                shared = combat.Stance;
                counted++;
            }

            if (shared == null)
            {
                return NothingSelectedName;
            }

            return shared.Name;
        }
    }
}
