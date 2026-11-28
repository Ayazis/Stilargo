using System;
using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.Combat;
using Stilargo.Core.Combat.Targeting;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// A built turret: fires at the nearest living enemy it can see within its weapon's range. It has no
    /// stance and cannot move, so it skips the unit combat brain.
    /// </summary>
    public sealed class Turret
    {
        private readonly Weapon _weapon;
        private readonly IVisibilityQuery _visibility;
        private readonly ITargetPriority _priority = new NearestTargetPriority();
        private readonly List<ICombatTarget> _candidates = new List<ICombatTarget>();

        public Turret(Weapon weapon, Vector3 position, IVisibilityQuery visibility)
        {
            _weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
            _visibility = visibility ?? throw new ArgumentNullException(nameof(visibility));
            Position = position;
        }

        public Vector3 Position { get; }

        /// <summary>The enemy fired at on the last tick, or null; views draw tracers from it.</summary>
        public ICombatTarget LastTarget { get; private set; }

        /// <summary>The enemy chosen on the last tick, whether fired at or still cooling down; null when none. Views aim at it.</summary>
        public ICombatTarget AimTarget { get; private set; }

        public FireOutcome Tick(float deltaSeconds, IReadOnlyList<ICombatTarget> enemies)
        {
            if (enemies == null)
            {
                throw new ArgumentNullException(nameof(enemies));
            }

            _weapon.Tick(deltaSeconds);
            LastTarget = null;
            ICombatTarget target = _priority.Choose(Position, CandidatesFrom(enemies));
            AimTarget = target;
            if (target == null)
            {
                return FireOutcome.NotFired;
            }

            FireOutcome outcome = _weapon.TryFire(Position, target);
            if (outcome != FireOutcome.NotFired)
            {
                LastTarget = target;
            }

            return outcome;
        }

        // Reuses one list so a turret ticking every frame does not allocate.
        private IReadOnlyList<ICombatTarget> CandidatesFrom(IReadOnlyList<ICombatTarget> enemies)
        {
            _candidates.Clear();
            foreach (ICombatTarget enemy in enemies)
            {
                if (enemy != null && enemy.IsAlive && _weapon.IsInRange(Position, enemy.Position) && _visibility.IsVisible(enemy.Position))
                {
                    _candidates.Add(enemy);
                }
            }

            return _candidates;
        }
    }
}
