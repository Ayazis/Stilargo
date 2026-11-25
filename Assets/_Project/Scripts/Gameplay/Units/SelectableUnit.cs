using System;
using Stilargo.Core.Orders;
using Stilargo.Core.Selection;
using UnityEngine;
using UnityEngine.AI;

namespace Stilargo.Gameplay.Units
{
    /// <summary>
    /// A selectable unit moved by a NavMeshAgent. Its orders live in Core's <see cref="UnitOrders"/>, run by a
    /// <see cref="NavMeshOrderDriver"/>; this component only shows selection.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class SelectableUnit : MonoBehaviour, ISelectable
    {
        [SerializeField] private GameObject _selectionMarker;
        [SerializeField] private WaypointPathView _pathView;

        private NavMeshOrderDriver _driver;

        public System.Numerics.Vector3 Position => transform.position.ToNumerics();

        public IUnitOrders Orders => _driver == null ? null : _driver.Orders;

        public bool IsSelected { get; private set; }

        private bool IsConstructed => _driver != null;

        private void Awake()
        {
            SetMarkerActive(false);
        }

        /// <summary>Call after the NavMesh is baked.</summary>
        public void Construct()
        {
            if (IsConstructed)
            {
                throw new InvalidOperationException($"{name} is already constructed.");
            }

            // The bootstrapper constructs units before their own Awake has run, so fetch the agent here.
            _driver = new NavMeshOrderDriver(GetComponent<NavMeshAgent>());

            if (_pathView != null)
            {
                _pathView.Construct(_driver.Orders);
            }
        }

        public void Select()
        {
            IsSelected = true;
            SetMarkerActive(true);

            if (_pathView != null)
            {
                _pathView.Show();
            }
        }

        public void Deselect()
        {
            IsSelected = false;
            SetMarkerActive(false);

            if (_pathView != null)
            {
                _pathView.Hide();
            }
        }

        private void Update()
        {
            if (!IsConstructed)
            {
                return;
            }

            _driver.ReportArrival();
        }

        private void SetMarkerActive(bool isActive)
        {
            if (_selectionMarker != null)
            {
                _selectionMarker.SetActive(isActive);
            }
        }
    }
}
