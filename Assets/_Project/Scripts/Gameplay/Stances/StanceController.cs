using System;
using Stilargo.Core.Combat.Stances;
using Stilargo.Core.Input;
using Stilargo.Core.Selection;
using UnityEngine;

namespace Stilargo.Gameplay.Stances
{
    /// <summary>
    /// Applies the stance whose key was pressed this frame to the selected units.
    /// </summary>
    public sealed class StanceController : MonoBehaviour
    {
        private IPlayerInput _input;
        private UnitSelection _selection;
        private SelectionStance _selectionStance;

        public void Construct(IPlayerInput input, UnitSelection selection, SelectionStance selectionStance)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _selectionStance = selectionStance ?? throw new ArgumentNullException(nameof(selectionStance));
        }

        private void Update()
        {
            if (_input == null)
            {
                return;
            }

            IStance pressed = PressedStance();
            if (pressed == null)
            {
                return;
            }

            _selectionStance.Apply(_selection, pressed);
        }

        private IStance PressedStance()
        {
            StanceSet stances = _selectionStance.Stances;

            if (_input.WasAggressiveStancePressedThisFrame)
            {
                return stances.Aggressive;
            }

            if (_input.WasDefensiveStancePressedThisFrame)
            {
                return stances.Defensive;
            }

            if (_input.WasHoldStancePressedThisFrame)
            {
                return stances.Hold;
            }

            return null;
        }
    }
}
