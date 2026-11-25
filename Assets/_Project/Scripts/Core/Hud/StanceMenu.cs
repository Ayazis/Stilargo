using System;
using System.Collections.Generic;
using Stilargo.Core.Combat.Stances;
using Stilargo.Core.Selection;

namespace Stilargo.Core.Hud
{
    /// <summary>What the stance buttons show and do: one button per stance, applied to the selected units.</summary>
    public sealed class StanceMenu
    {
        private readonly UnitSelection _selection;
        private readonly SelectionStance _selectionStance;

        public StanceMenu(UnitSelection selection, SelectionStance selectionStance)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _selectionStance = selectionStance ?? throw new ArgumentNullException(nameof(selectionStance));
            Options = new IStance[] { selectionStance.Stances.Aggressive, selectionStance.Stances.Defensive, selectionStance.Stances.Hold };
        }

        public IReadOnlyList<IStance> Options { get; }

        public bool HasSelection => _selection.Selected.Count > 0;

        /// <summary>True when every selected unit has this stance, so its button can show as pressed.</summary>
        public bool IsActive(IStance stance)
        {
            if (stance == null)
            {
                throw new ArgumentNullException(nameof(stance));
            }

            return string.Equals(_selectionStance.CurrentStanceName(_selection), stance.Name, StringComparison.Ordinal);
        }

        public void Choose(IStance stance)
        {
            _selectionStance.Apply(_selection, stance);
        }
    }
}
