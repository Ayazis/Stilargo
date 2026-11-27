using System;
using Stilargo.Core.Combat;
using Stilargo.Core.Garrison;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Fog;
using Stilargo.Gameplay.Units;
using UnityEngine;
using UnityEngine.AI;

namespace Stilargo.Gameplay.Garrison
{
    /// <summary>
    /// Lets a unit walk into a building, stand in a garrison slot and leave again.
    /// While garrisoned the NavMeshAgent is off, so the unit stays put and only fires.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class GarrisonOccupant : MonoBehaviour, IPositionBonusSource
    {
        private const float DefaultArrivalRadiusMetres = 1.5f;
        private const float DefaultHalfHeightMetres = 1f;

        [SerializeField] private float _arrivalRadiusMetres = DefaultArrivalRadiusMetres;
        [Tooltip("Distance from feet to pivot; capsule units have their pivot at the centre.")]
        [SerializeField] private float _halfHeightMetres = DefaultHalfHeightMetres;

        private GarrisonOccupantSetup _setup;
        private NavMeshAgent _agent;
        private VisionSourceComponent _visionSource;
        private Transform _transform;
        private GarrisonBuilding _pendingBuilding;
        private GarrisonBuilding _occupiedBuilding;
        private RooftopVision _rooftopVision;

        public bool IsGarrisoned => CurrentSlot != null;

        public GarrisonSlot CurrentSlot { get; private set; }

        public PositionBonus CurrentBonus => IsGarrisoned ? _setup.Bonuses.For(CurrentSlot) : PositionBonus.None;

        public void Construct(GarrisonOccupantSetup setup)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            if (_setup != null)
            {
                throw new InvalidOperationException($"{nameof(GarrisonOccupant)} is already constructed.");
            }

            _setup = setup;
            CacheComponents();
        }

        /// <summary>Walks to the entrance and takes a free slot; stays at the entrance when the building is full.</summary>
        public void Enter(GarrisonBuilding building)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            RequireConstructed();
            if (building == _occupiedBuilding)
            {
                return;
            }

            Leave();
            _pendingBuilding = building;
            _setup.Orders.MoveTo(building.Entrance);
        }

        /// <summary>Leaves the current building, or cancels walking to one.</summary>
        public void Leave()
        {
            _pendingBuilding = null;
            if (_occupiedBuilding == null)
            {
                return;
            }

            GarrisonBuilding building = _occupiedBuilding;
            UnregisterRooftopVision();
            building.Release(this);
            _occupiedBuilding = null;
            CurrentSlot = null;

            Vector3 entrance = building.Entrance.ToUnity();
            _transform.position = StandingPivot(entrance);
            _agent.enabled = true;
            _agent.Warp(entrance);
        }

        private void Awake()
        {
            CacheComponents();
        }

        // Construct can run before Awake when the bootstrapper wires scene objects, so both paths cache.
        private void CacheComponents()
        {
            if (_transform != null)
            {
                return;
            }

            _transform = transform;
            _agent = GetComponent<NavMeshAgent>();
            _visionSource = GetComponent<VisionSourceComponent>();
        }

        private void Update()
        {
            if (_pendingBuilding == null || !HasReachedEntrance())
            {
                return;
            }

            GarrisonBuilding building = _pendingBuilding;
            _pendingBuilding = null;
            if (building.TryOccupy(this, out GarrisonSlot slot))
            {
                Occupy(building, slot);
            }
        }

        // A dying or deactivated unit must free its slot and rooftop vision.
        private void OnDisable()
        {
            if (_setup == null)
            {
                return;
            }

            Leave();
        }

        private bool HasReachedEntrance()
        {
            Vector3 entrance = _pendingBuilding.Entrance.ToUnity();
            Vector3 position = _transform.position;
            Vector2 offset = new Vector2(entrance.x - position.x, entrance.z - position.z);
            return offset.sqrMagnitude <= _arrivalRadiusMetres * _arrivalRadiusMetres;
        }

        private void Occupy(GarrisonBuilding building, GarrisonSlot slot)
        {
            _occupiedBuilding = building;
            CurrentSlot = slot;

            // Orders must be idle before the agent goes off, or they keep querying a disabled agent.
            _setup.Orders.Stop();
            _agent.enabled = false;
            _transform.position = StandingPivot(slot.Position.ToUnity());
            _transform.rotation = Quaternion.LookRotation(slot.Facing.ToUnity(), Vector3.up);
            RegisterRooftopVision(slot);
        }

        private void RegisterRooftopVision(GarrisonSlot slot)
        {
            if (slot.Kind != GarrisonSlotKind.Roof || _visionSource == null)
            {
                return;
            }

            float bonus = _setup.Bonuses.For(slot).SightMetres;
            _rooftopVision = new RooftopVision(_visionSource, bonus);
            _setup.VisibilityGrid.Register(_rooftopVision);
        }

        private void UnregisterRooftopVision()
        {
            if (_rooftopVision == null)
            {
                return;
            }

            _setup.VisibilityGrid.Unregister(_rooftopVision);
            _rooftopVision = null;
        }

        private Vector3 StandingPivot(Vector3 feet)
        {
            return feet + (Vector3.up * _halfHeightMetres);
        }

        private void RequireConstructed()
        {
            if (_setup == null)
            {
                throw new InvalidOperationException($"{nameof(GarrisonOccupant)} must be constructed before use.");
            }
        }
    }
}
