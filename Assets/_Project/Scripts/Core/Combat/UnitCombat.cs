using System;
using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.Combat.Stances;
using Stilargo.Core.Combat.Targeting;

namespace Stilargo.Core.Combat
{
    /// <summary>
    /// One unit's combat decisions: picks a target, consults its stance and fires its weapon.
    /// </summary>
    public sealed class UnitCombat
    {
        private readonly Weapon _weapon;
        private readonly ITargetPriority _priority;
        private readonly CombatAwareness _awareness;
        private readonly List<ICombatTarget> _visibleEnemies = new List<ICombatTarget>();

        public UnitCombat(Weapon weapon, ITargetPriority priority, CombatAwareness awareness)
        {
            _weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
            _priority = priority ?? throw new ArgumentNullException(nameof(priority));
            _awareness = awareness ?? throw new ArgumentNullException(nameof(awareness));

            if (weapon.Settings.Range > awareness.SightRange)
            {
                throw new ArgumentException("Weapon range must not exceed sight range.", nameof(weapon));
            }
        }

        public event Action<IStance> StanceChanged;

        /// <summary>Raised when the weapon actually hit a target.</summary>
        public event Action<ICombatTarget> Fired;

        /// <summary>Null until <see cref="SetStance"/> is called.</summary>
        public IStance Stance { get; private set; }

        public void SetStance(IStance stance)
        {
            Stance = stance ?? throw new ArgumentNullException(nameof(stance));
            StanceChanged?.Invoke(stance);
        }

        public CombatCommand Tick(float deltaSeconds, CombatSituation situation)
        {
            if (Stance == null)
            {
                throw new InvalidOperationException("Set a stance before ticking combat.");
            }

            _weapon.Tick(deltaSeconds);
            ICombatTarget target = ChooseVisibleTarget(situation);

            if (situation.IsFollowingMoveOrder)
            {
                TryFireAt(situation.UnitPosition, target);
                return CombatCommand.FollowOrders;
            }

            return ApplyStance(situation, target);
        }

        private CombatCommand ApplyStance(CombatSituation situation, ICombatTarget target)
        {
            StanceTarget stanceTarget = ToStanceTarget(situation.UnitPosition, target);
            StanceContext context = new StanceContext(situation.UnitPosition, situation.Anchor, stanceTarget);

            switch (Stance.Decide(context))
            {
                case StanceAction.Fire:
                    return FireCommand(situation.UnitPosition, target);
                case StanceAction.Chase:
                    return ChaseCommand(target);
                case StanceAction.ReturnToAnchor:
                    return CombatCommand.MoveTo(situation.Anchor);
                default:
                    return CombatCommand.Idle;
            }
        }

        private CombatCommand FireCommand(Vector3 unitPosition, ICombatTarget target)
        {
            if (target == null)
            {
                return CombatCommand.Idle;
            }

            TryFireAt(unitPosition, target);
            return CombatCommand.Fire;
        }

        private static CombatCommand ChaseCommand(ICombatTarget target)
        {
            if (target == null)
            {
                return CombatCommand.Idle;
            }

            return CombatCommand.MoveTo(target.Position);
        }

        private void TryFireAt(Vector3 unitPosition, ICombatTarget target)
        {
            if (target == null)
            {
                return;
            }

            if (_weapon.TryFire(unitPosition, target))
            {
                Fired?.Invoke(target);
            }
        }

        private StanceTarget ToStanceTarget(Vector3 unitPosition, ICombatTarget target)
        {
            if (target == null)
            {
                return StanceTarget.None;
            }

            return StanceTarget.At(target.Position, _weapon.IsInRange(unitPosition, target.Position));
        }

        private ICombatTarget ChooseVisibleTarget(CombatSituation situation)
        {
            _visibleEnemies.Clear();

            foreach (ICombatTarget enemy in situation.Enemies)
            {
                if (IsVisibleAndAlive(situation.UnitPosition, enemy))
                {
                    _visibleEnemies.Add(enemy);
                }
            }

            return _priority.Choose(situation.UnitPosition, _visibleEnemies);
        }

        private bool IsVisibleAndAlive(Vector3 unitPosition, ICombatTarget enemy)
        {
            if (enemy == null || !enemy.IsAlive)
            {
                return false;
            }

            return GroundDistance.Between(unitPosition, enemy.Position) <= _awareness.SightRange;
        }
    }
}
