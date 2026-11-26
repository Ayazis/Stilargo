using System;
using Stilargo.Core.Garrison;
using Stilargo.Core.Selection;
using UnityEngine;

namespace Stilargo.Gameplay.Selection
{
    /// <summary>
    /// A click on a building selects it; a click anywhere else, or picking units, clears it. Units and a building
    /// are never selected at once, so the panel always describes what the player last clicked.
    /// </summary>
    public sealed class BuildingClickSelection : IDisposable
    {
        private readonly SelectionController _controller;
        private readonly UnitSelection _units;
        private readonly GarrisonDirectory _buildings;
        private readonly BuildingSelection _selection;

        public BuildingClickSelection(SelectionController controller, GarrisonDirectory buildings, BuildingSelection selection)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            _controller = controller;
            _units = controller.Selection;
            _buildings = buildings ?? throw new ArgumentNullException(nameof(buildings));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _controller.WorldClicked += OnWorldClicked;
            _units.Changed += OnUnitsChanged;
        }

        public void Dispose()
        {
            _controller.WorldClicked -= OnWorldClicked;
            _units.Changed -= OnUnitsChanged;
        }

        private void OnWorldClicked(Vector3 point)
        {
            if (_buildings.TryFindBuildingAt(point.x, point.z, out GarrisonBuilding building))
            {
                _selection.Select(building);
                return;
            }

            _selection.Clear();
        }

        private void OnUnitsChanged()
        {
            if (_units.Selected.Count > 0)
            {
                _selection.Clear();
            }
        }
    }
}
