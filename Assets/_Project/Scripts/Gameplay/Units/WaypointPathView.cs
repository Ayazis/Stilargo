using System;
using System.Collections.Generic;
using Stilargo.Core.Orders;
using UnityEngine;

namespace Stilargo.Gameplay.Units
{
    /// <summary>
    /// Draws a line from the unit through its queued waypoints while the unit is selected.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class WaypointPathView : MonoBehaviour
    {
        [SerializeField] private float _heightAboveGround = 0.1f;

        private LineRenderer _line;
        private UnitOrders _orders;
        private bool _isShown;
        private bool _isSubscribed;

        private void Awake()
        {
            CacheComponents();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Construct(UnitOrders orders)
        {
            if (_orders != null)
            {
                throw new InvalidOperationException($"{name} path view is already constructed.");
            }

            _orders = orders ?? throw new ArgumentNullException(nameof(orders));

            // Constructed by the bootstrapper before this component's own Awake and OnEnable have run.
            CacheComponents();
            Subscribe();
        }

        private void CacheComponents()
        {
            if (_line != null)
            {
                return;
            }

            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.enabled = false;
        }

        private void Subscribe()
        {
            if (_orders == null || _isSubscribed)
            {
                return;
            }

            _orders.WaypointsChanged += Redraw;
            _isSubscribed = true;
            Redraw();
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed)
            {
                return;
            }

            _orders.WaypointsChanged -= Redraw;
            _isSubscribed = false;
        }

        public void Show()
        {
            _isShown = true;
            Redraw();
        }

        public void Hide()
        {
            _isShown = false;
            Redraw();
        }

        private void LateUpdate()
        {
            if (_line.enabled)
            {
                _line.SetPosition(0, Raised(transform.position));
            }
        }

        private void Redraw()
        {
            if (_orders == null || !_isShown || _orders.IsIdle)
            {
                _line.enabled = false;
                return;
            }

            IReadOnlyList<System.Numerics.Vector3> waypoints = _orders.Waypoints;
            _line.positionCount = waypoints.Count + 1;
            _line.SetPosition(0, Raised(transform.position));

            for (int i = 0; i < waypoints.Count; i++)
            {
                _line.SetPosition(i + 1, Raised(waypoints[i].ToUnity()));
            }

            _line.enabled = true;
        }

        private Vector3 Raised(Vector3 point)
        {
            return point + Vector3.up * _heightAboveGround;
        }
    }
}
