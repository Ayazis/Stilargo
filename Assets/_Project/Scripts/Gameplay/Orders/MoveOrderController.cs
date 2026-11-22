using System;
using System.Collections.Generic;
using Stilargo.Core.Formations;
using Stilargo.Core.Input;
using Stilargo.Core.Orders;
using Stilargo.Core.Selection;
using Stilargo.Gameplay.Selection;
using UnityEngine;
using UnityEngine.AI;

namespace Stilargo.Gameplay.Orders
{
    /// <summary>
    /// Turns right clicks on the ground into formation moves for the selected units,
    /// queued while the queue modifier is held. The cycle key switches formation.
    /// </summary>
    public sealed class MoveOrderController : MonoBehaviour
    {
        private const float DefaultFormationSpacing = 2.5f;

        // How far from the clicked point we look for walkable NavMesh before ignoring the click.
        private const float MaxNavMeshSnapDistance = 2f;

        private const string FormationTypeSuffix = "Formation";

        [SerializeField] private float _formationSpacing = DefaultFormationSpacing;

        private IPlayerInput _input;
        private UnityEngine.Camera _camera;
        private UnitSelection _selection;
        private FormationCycle _cycle;
        private GroupMoveOrder _order;
        private readonly List<IPointerCommandHandler> _handlers = new List<IPointerCommandHandler>();

        /// <summary>Raised with the destination each time a move order is given.</summary>
        public event Action<Vector3> MoveOrdered;

        /// <summary>Raised with the ordered units just before a move order (new or queued) is given to them.</summary>
        public event Action<IReadOnlyList<ISelectable>> UnitsMoveOrdered;

        /// <summary>Display name of the current formation, e.g. "Line".</summary>
        public string CurrentFormationName
        {
            get
            {
                if (_cycle == null)
                {
                    throw new InvalidOperationException($"{nameof(MoveOrderController)} used before {nameof(Construct)}.");
                }

                string typeName = _cycle.Current.GetType().Name;
                if (!typeName.EndsWith(FormationTypeSuffix, StringComparison.Ordinal))
                {
                    return typeName;
                }

                return typeName.Substring(0, typeName.Length - FormationTypeSuffix.Length);
            }
        }

        public void Construct(IPlayerInput input, UnityEngine.Camera camera, UnitSelection selection)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            _input = input ?? throw new ArgumentNullException(nameof(input));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _camera = camera;
            _cycle = new FormationCycle(new IFormation[]
            {
                new LineFormation(_formationSpacing),
                new BoxFormation(_formationSpacing),
            });
            _order = new GroupMoveOrder(_cycle.Current);
        }

        /// <summary>Handlers are asked in the order they were added; the first that handles a click wins.</summary>
        public void AddHandler(IPointerCommandHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handlers.Add(handler);
        }

        private void Update()
        {
            if (_input == null)
            {
                return;
            }

            if (_input.WasCycleFormationPressedThisFrame)
            {
                _order.Formation = _cycle.Next();
            }

            if (_input.WasCommandPressedThisFrame && _selection.Selected.Count > 0)
            {
                TryOrderCommand();
            }
        }

        private void TryOrderCommand()
        {
            Ray ray = _camera.ScreenPointToRay(_input.PointerScreenPosition.ToScreenPoint());
            foreach (IPointerCommandHandler handler in _handlers)
            {
                if (handler.TryHandle(ray, _selection.Selected))
                {
                    return;
                }
            }

            TryOrderMove(ray);
        }

        private void TryOrderMove(Ray ray)
        {
            if (!TryFindGroundPoint(ray, out Vector3 destination))
            {
                return;
            }

            UnitsMoveOrdered?.Invoke(_selection.Selected);
            if (_input.IsQueueModifierHeld)
            {
                _order.QueueMove(_selection.Selected, destination.ToNumerics());
            }
            else
            {
                _order.Move(_selection.Selected, destination.ToNumerics());
            }

            MoveOrdered?.Invoke(destination);
        }

        private bool TryFindGroundPoint(Ray ray, out Vector3 point)
        {
            point = default;
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                return false;
            }

            if (!NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, MaxNavMeshSnapDistance, NavMesh.AllAreas))
            {
                return false;
            }

            point = navHit.position;
            return true;
        }
    }
}
