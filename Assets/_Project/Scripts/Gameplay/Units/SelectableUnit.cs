using System;
using Stilargo.Core.Orders;
using Stilargo.Core.Selection;
using UnityEngine;
using UnityEngine.AI;

namespace Stilargo.Gameplay.Units
{
    /// <summary>
    /// A selectable unit moved by a NavMeshAgent. Its orders live in Core's <see cref="UnitOrders"/>;
    /// this component only reports arrivals and shows selection.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class SelectableUnit : MonoBehaviour, ISelectable
    {
        // Slack on top of stoppingDistance so float noise near the target still counts as arrived.
        private const float ArrivalTolerance = 0.1f;
        // Covers a pivot at the body's centre, about a metre above the NavMesh, plus some slack.
        private const float NavMeshSampleRadius = 2f;

        [SerializeField] private GameObject _selectionMarker;
        [SerializeField] private WaypointPathView _pathView;

        private NavMeshAgent _agent;
        private UnitOrders _orders;
        private int _lastOrderFrame = -1;
        private bool _isSubscribed;

        public System.Numerics.Vector3 Position => transform.position.ToNumerics();

        public IUnitOrders Orders => _orders;

        public bool IsSelected { get; private set; }

        private bool IsConstructed => _orders != null;

        private void Awake()
        {
            CacheComponents();
            SetMarkerActive(false);
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>Call after the NavMesh is baked.</summary>
        public void Construct()
        {
            if (IsConstructed)
            {
                throw new InvalidOperationException($"{name} is already constructed.");
            }

            // The bootstrapper constructs units before their own Awake and OnEnable have run.
            CacheComponents();
            PlaceAgentOnNavMesh();
            _orders = new UnitOrders(new NavMeshNavigationAgent(_agent));
            Subscribe();

            if (_pathView != null)
            {
                _pathView.Construct(_orders);
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
            if (!IsConstructed || _orders.IsIdle)
            {
                return;
            }

            if (HasArrived())
            {
                _orders.NotifyWaypointReached();
            }
        }

        private bool HasArrived()
        {
            // A destination set this frame can report remainingDistance 0 before its path exists.
            if (_lastOrderFrame == Time.frameCount || _agent.pathPending)
            {
                return false;
            }

            return _agent.remainingDistance <= _agent.stoppingDistance + ArrivalTolerance;
        }

        private void OnWaypointsChanged()
        {
            _lastOrderFrame = Time.frameCount;
        }

        private void CacheComponents()
        {
            if (_agent == null)
            {
                _agent = GetComponent<NavMeshAgent>();
            }
        }

        private void Subscribe()
        {
            if (!IsConstructed || _isSubscribed)
            {
                return;
            }

            _orders.WaypointsChanged += OnWaypointsChanged;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed)
            {
                return;
            }

            _orders.WaypointsChanged -= OnWaypointsChanged;
            _isSubscribed = false;
        }

        private void PlaceAgentOnNavMesh()
        {
            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, NavMeshSampleRadius, NavMesh.AllAreas))
            {
                throw new InvalidOperationException($"{name} at {transform.position} is not on the NavMesh.");
            }

            _agent.enabled = true;
            _agent.Warp(hit.position);
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
