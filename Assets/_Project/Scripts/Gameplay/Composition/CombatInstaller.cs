using System;
using Stilargo.Core.Selection;
using Stilargo.Gameplay.CameraControl;
using Stilargo.Gameplay.Orders;
using Stilargo.Gameplay.Selection;
using Stilargo.Gameplay.Units;
using UnityEngine;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Wires Ci's side of the game: the RTS camera, the player's units, selection and move orders.
    /// Runs after the installer that bakes the NavMesh, because units need it to be placed.
    /// </summary>
    public sealed class CombatInstaller : Installer
    {
        [SerializeField] private RtsCameraRig _cameraRig;
        [SerializeField] private CameraSettings _cameraSettings;
        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private SelectableUnit[] _units;
        [SerializeField] private SelectionController _selectionController;
        [SerializeField] private SelectionBoxView _selectionBoxView;
        [SerializeField] private MoveOrderController _moveOrderController;
        [SerializeField] private FormationLabelView _formationLabelView;

        public override void Install(GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ValidateReferences();

            _cameraRig.Construct(context.PlayerInput, _cameraSettings);
            ISelectable[] units = ConstructUnits();

            _selectionController.Construct(context.PlayerInput, _camera, units);
            _selectionBoxView.Construct(_selectionController.Drag);
            _moveOrderController.Construct(context.PlayerInput, _camera, _selectionController.Selection);
            _formationLabelView.Construct(_moveOrderController);
        }

        private ISelectable[] ConstructUnits()
        {
            var units = new ISelectable[_units.Length];
            for (int index = 0; index < _units.Length; index++)
            {
                _units[index].Construct();
                units[index] = _units[index];
            }

            return units;
        }

        private void ValidateReferences()
        {
            RequireAssigned(_cameraRig, nameof(RtsCameraRig));
            RequireAssigned(_cameraSettings, nameof(CameraSettings));
            RequireAssigned(_camera, nameof(UnityEngine.Camera));
            RequireAssigned(_selectionController, nameof(SelectionController));
            RequireAssigned(_selectionBoxView, nameof(SelectionBoxView));
            RequireAssigned(_moveOrderController, nameof(MoveOrderController));
            RequireAssigned(_formationLabelView, nameof(FormationLabelView));

            if (_units == null)
            {
                throw new InvalidOperationException($"{nameof(CombatInstaller)} on '{name}' has no unit list.");
            }

            for (int index = 0; index < _units.Length; index++)
            {
                if (_units[index] == null)
                {
                    throw new InvalidOperationException($"{nameof(CombatInstaller)} on '{name}' has an empty unit slot at index {index}.");
                }
            }
        }

        private void RequireAssigned(UnityEngine.Object reference, string typeName)
        {
            if (reference == null)
            {
                throw new InvalidOperationException($"{nameof(CombatInstaller)} on '{name}' has no {typeName} assigned.");
            }
        }
    }
}
