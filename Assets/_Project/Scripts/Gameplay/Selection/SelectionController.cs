using System;
using System.Collections.Generic;
using Stilargo.Core.Hud;
using Stilargo.Core.Input;
using Stilargo.Core.Selection;
using UnityEngine;

namespace Stilargo.Gameplay.Selection
{
    /// <summary>
    /// Turns left-mouse clicks and drags into selection changes. A click picks the unit under the pointer,
    /// a drag picks every unit inside the box; the queue modifier adds instead of replacing.
    /// </summary>
    public sealed class SelectionController : MonoBehaviour
    {
        private const float DefaultDragThresholdPixels = 6f;

        [SerializeField] private float _dragThresholdPixels = DefaultDragThresholdPixels;

        private IPlayerInput _input;
        private UnityEngine.Camera _camera;
        private IReadOnlyList<ISelectable> _units;
        private IScreenProjector _projector;
        private UnitSelection _selection;
        private IUiPointerBlocker _uiPointerBlocker = new NoUiPointerBlocker();

        public UnitSelection Selection
        {
            get
            {
                if (_selection == null)
                {
                    throw new InvalidOperationException($"{nameof(SelectionController)} used before {nameof(Construct)}.");
                }

                return _selection;
            }
        }

        public SelectionDrag Drag { get; private set; }

        public void Construct(IPlayerInput input, UnityEngine.Camera camera, IReadOnlyList<ISelectable> units)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            _input = input ?? throw new ArgumentNullException(nameof(input));
            _units = units ?? throw new ArgumentNullException(nameof(units));
            _camera = camera;
            _projector = new CameraScreenProjector(camera);
            _selection = new UnitSelection();
            Drag = new SelectionDrag(_dragThresholdPixels);
        }

        /// <summary>Makes presses on the HUD start no selection drag. Without it the HUD never blocks clicks.</summary>
        public void UseUiPointerBlocker(IUiPointerBlocker uiPointerBlocker)
        {
            _uiPointerBlocker = uiPointerBlocker ?? throw new ArgumentNullException(nameof(uiPointerBlocker));
        }

        private void Update()
        {
            if (_input == null)
            {
                return;
            }

            System.Numerics.Vector2 pointer = _input.PointerScreenPosition;

            if (_input.WasSelectPressedThisFrame)
            {
                if (!_uiPointerBlocker.IsPointerOverUi)
                {
                    Drag.Begin(pointer);
                }

                return;
            }

            if (!Drag.IsActive)
            {
                return;
            }

            Drag.MoveTo(pointer);

            if (_input.WasSelectReleasedThisFrame || !_input.IsSelectHeld)
            {
                Drag.End();
                ApplySelection(PickedUnits());
            }
        }

        private IReadOnlyList<ISelectable> PickedUnits()
        {
            if (Drag.IsBox)
            {
                return BoxSelection.Within(Drag.Rect, _units, _projector);
            }

            ISelectable clicked = UnitUnderPointer();
            if (clicked == null)
            {
                return Array.Empty<ISelectable>();
            }

            return new[] { clicked };
        }

        private ISelectable UnitUnderPointer()
        {
            Ray ray = _camera.ScreenPointToRay(Drag.CurrentPoint.ToScreenPoint());

            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                return null;
            }

            return hit.collider.GetComponentInParent<ISelectable>();
        }

        private void ApplySelection(IReadOnlyList<ISelectable> picked)
        {
            if (_input.IsQueueModifierHeld)
            {
                _selection.Add(picked);
                return;
            }

            _selection.Replace(picked);
        }
    }
}
