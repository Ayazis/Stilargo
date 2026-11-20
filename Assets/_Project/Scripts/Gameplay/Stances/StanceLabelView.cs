using System;
using Stilargo.Core.Combat.Stances;
using Stilargo.Core.Selection;
using UnityEngine;

namespace Stilargo.Gameplay.Stances
{
    /// <summary>
    /// Shows the selected units' stance just below the formation label.
    /// </summary>
    public sealed class StanceLabelView : MonoBehaviour
    {
        private const string LabelFormat = "Stance: {0} (Z aggressive, X defensive, C hold)";

        [SerializeField] private Rect _area = new Rect(10f, 34f, 420f, 24f);

        private UnitSelection _selection;
        private SelectionStance _selectionStance;
        private string _shownName;
        private string _label;

        public void Construct(UnitSelection selection, SelectionStance selectionStance)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _selectionStance = selectionStance ?? throw new ArgumentNullException(nameof(selectionStance));
        }

        private void OnGUI()
        {
            if (_selectionStance == null)
            {
                return;
            }

            GUI.Label(_area, CurrentLabel());
        }

        // Formats only when the name changes, so OnGUI does not allocate a string every call.
        private string CurrentLabel()
        {
            string stanceName = _selectionStance.CurrentStanceName(_selection);
            if (!string.Equals(stanceName, _shownName, StringComparison.Ordinal))
            {
                _shownName = stanceName;
                _label = string.Format(LabelFormat, stanceName);
            }

            return _label;
        }
    }
}
