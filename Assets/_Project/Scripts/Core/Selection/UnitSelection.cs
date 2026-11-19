using System;
using System.Collections.Generic;

namespace Stilargo.Core.Selection
{
    /// <summary>
    /// The player's current selection. Keeps each unit's own selected state in sync with membership.
    /// </summary>
    public sealed class UnitSelection
    {
        private readonly List<ISelectable> _selected = new List<ISelectable>();

        /// <summary>Raised only when a unit actually joins or leaves the selection.</summary>
        public event Action Changed;

        /// <summary>Selected units in the order they were added.</summary>
        public IReadOnlyList<ISelectable> Selected => _selected;

        public bool Contains(ISelectable unit)
        {
            return _selected.Contains(unit);
        }

        /// <summary>Makes the given units the whole selection.</summary>
        public void Replace(IEnumerable<ISelectable> units)
        {
            List<ISelectable> incoming = DistinctUnits(units);
            int removedCount = DeselectAllExcept(incoming);
            int addedCount = SelectMissing(incoming);

            if (removedCount + addedCount > 0)
            {
                Changed?.Invoke();
            }
        }

        /// <summary>Adds the given units to the selection, keeping what is already selected.</summary>
        public void Add(IEnumerable<ISelectable> units)
        {
            List<ISelectable> incoming = DistinctUnits(units);

            if (SelectMissing(incoming) > 0)
            {
                Changed?.Invoke();
            }
        }

        /// <summary>Drops a single unit, for example one that died or was destroyed.</summary>
        public void Remove(ISelectable unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            if (!_selected.Remove(unit))
            {
                return;
            }

            unit.Deselect();
            Changed?.Invoke();
        }

        public void Clear()
        {
            if (DeselectAllExcept(new List<ISelectable>()) > 0)
            {
                Changed?.Invoke();
            }
        }

        private static List<ISelectable> DistinctUnits(IEnumerable<ISelectable> units)
        {
            if (units == null)
            {
                throw new ArgumentNullException(nameof(units));
            }

            var distinct = new List<ISelectable>();
            foreach (ISelectable unit in units)
            {
                if (unit == null)
                {
                    throw new ArgumentException("Units must not contain null.", nameof(units));
                }

                if (!distinct.Contains(unit))
                {
                    distinct.Add(unit);
                }
            }

            return distinct;
        }

        private int DeselectAllExcept(List<ISelectable> keep)
        {
            List<ISelectable> leaving = _selected.FindAll(unit => !keep.Contains(unit));
            foreach (ISelectable unit in leaving)
            {
                _selected.Remove(unit);
                unit.Deselect();
            }

            return leaving.Count;
        }

        private int SelectMissing(List<ISelectable> incoming)
        {
            int addedCount = 0;
            foreach (ISelectable unit in incoming)
            {
                if (_selected.Contains(unit))
                {
                    continue;
                }

                _selected.Add(unit);
                unit.Select();
                addedCount++;
            }

            return addedCount;
        }
    }
}
