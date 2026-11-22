using System;
using System.Collections.Generic;
using Stilargo.Core.Combat;
using Stilargo.Core.Orders;
using Stilargo.Gameplay.Units;
using UnityEngine;
using UnityEngine.AI;

namespace Stilargo.Gameplay.Combat
{
    /// <summary>
    /// Feeds a unit's world state into Core's <see cref="UnitCombat"/> each frame and carries out the
    /// resulting command on the NavMeshAgent. Shared by player units and enemies.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class CombatUnit : MonoBehaviour, ICombatTarget
    {
        // Covers a pivot at the body's centre, about a metre above the NavMesh, plus some slack.
        private const float NavMeshSampleRadius = 2f;
        // A chased target moving less than this does not trigger a new path request.
        private const float RepathThresholdMetres = 0.5f;

        private NavMeshAgent _agent;
        private UnitCombat _combat;
        private Health _health;
        private IUnitOrders _orders;
        private IReadOnlyList<ICombatTarget> _enemies;
        private readonly List<IPositionBonusSource> _bonusSources = new List<IPositionBonusSource>();
        private System.Numerics.Vector3 _anchor;
        private Vector3 _combatDestination;
        private bool _isMovingForCombat;
        private bool _wereOrdersBusy;
        private bool _isSubscribed;

        public event Action Died;

        public UnitCombat Combat => _combat;

        public System.Numerics.Vector3 Position => transform.position.ToNumerics();

        public float HealthFraction => _health.Fraction;

        public CombatProtection Protection => IsConstructed ? _combat.Protection : CombatProtection.None;

        public bool IsAlive => IsConstructed && _health.IsAlive;

        private bool IsConstructed => _combat != null;

        private bool AreOrdersBusy => _orders != null && !_orders.IsIdle;

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

        /// <summary>Call after the NavMesh is baked and, for player units, after SelectableUnit.Construct.</summary>
        public void Construct(CombatUnitSetup setup)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            if (IsConstructed)
            {
                throw new InvalidOperationException($"{name} is already constructed.");
            }

            // The bootstrapper constructs units before their own Awake and OnEnable have run.
            CacheComponents();
            EnsureAgentOnNavMesh();
            _combat = setup.Combat;
            _health = setup.Health;
            _orders = setup.Orders;
            _enemies = setup.Enemies;
            _anchor = Position;
            _wereOrdersBusy = AreOrdersBusy;
            Subscribe();
        }

        /// <summary>Adds a source (cover, garrison) whose bonus is summed into this unit's combat every frame.</summary>
        public void AddBonusSource(IPositionBonusSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            _bonusSources.Add(source);
        }

        public void TakeDamage(float amount)
        {
            _health.TakeDamage(amount);
        }

        private void Update()
        {
            if (!IsAlive)
            {
                return;
            }

            UpdateAnchorWhenOrdersFinish();
            _combat.SetPositionBonus(CurrentBonus());
            UnitCombatPose pose = new UnitCombatPose(Position, _anchor);
            CombatSituation situation = new CombatSituation(pose, AreOrdersBusy, _enemies);
            Execute(_combat.Tick(Time.deltaTime, situation));
        }

        private PositionBonus CurrentBonus()
        {
            PositionBonus bonus = PositionBonus.None;
            foreach (IPositionBonusSource source in _bonusSources)
            {
                bonus = bonus.Combine(source.CurrentBonus);
            }

            return bonus;
        }

        private void UpdateAnchorWhenOrdersFinish()
        {
            bool areOrdersBusy = AreOrdersBusy;

            if (_wereOrdersBusy && !areOrdersBusy)
            {
                _anchor = Position;
            }

            _wereOrdersBusy = areOrdersBusy;
        }

        private void Execute(CombatCommand command)
        {
            // A garrisoned unit's agent is off: it holds its slot and may only shoot.
            if (!_agent.enabled)
            {
                _isMovingForCombat = false;
                return;
            }

            switch (command.Action)
            {
                case CombatCommandAction.MoveTo:
                    MoveForCombat(command.Destination.ToUnity());
                    break;
                case CombatCommandAction.FollowOrders:
                    // Orders own the agent now; forget any combat path so we never reset theirs.
                    _isMovingForCombat = false;
                    break;
                default:
                    StopCombatMovement();
                    break;
            }
        }

        private void MoveForCombat(Vector3 destination)
        {
            if (_isMovingForCombat && Vector3.Distance(destination, _combatDestination) < RepathThresholdMetres)
            {
                return;
            }

            _agent.SetDestination(destination);
            _combatDestination = destination;
            _isMovingForCombat = true;
        }

        private void StopCombatMovement()
        {
            if (!_isMovingForCombat || AreOrdersBusy)
            {
                return;
            }

            _agent.ResetPath();
            _isMovingForCombat = false;
        }

        private void OnDestinationReached()
        {
            _anchor = Position;
        }

        private void OnHealthDepleted()
        {
            // Listeners (selection, enemy lists) must drop this unit before it is destroyed.
            Died?.Invoke();
            enabled = false;
            Destroy(gameObject);
        }

        private void CacheComponents()
        {
            if (_agent == null)
            {
                _agent = GetComponent<NavMeshAgent>();
            }
        }

        private void EnsureAgentOnNavMesh()
        {
            if (_agent.enabled && _agent.isOnNavMesh)
            {
                return;
            }

            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, NavMeshSampleRadius, NavMesh.AllAreas))
            {
                throw new InvalidOperationException($"{name} at {transform.position} is not on the NavMesh.");
            }

            _agent.enabled = true;
            _agent.Warp(hit.position);
        }

        private void Subscribe()
        {
            if (!IsConstructed || _isSubscribed)
            {
                return;
            }

            _health.Died += OnHealthDepleted;

            if (_orders != null)
            {
                _orders.DestinationReached += OnDestinationReached;
            }

            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed)
            {
                return;
            }

            _health.Died -= OnHealthDepleted;

            if (_orders != null)
            {
                _orders.DestinationReached -= OnDestinationReached;
            }

            _isSubscribed = false;
        }
    }
}
