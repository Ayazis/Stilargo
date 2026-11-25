using System;
using System.Collections.Generic;
using Stilargo.Core.Combat;
using Stilargo.Core.Fog;
using Stilargo.Core.Fortification;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Units;
using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>
    /// Runs every turret built on the claimed house. Each turret also reveals the fog within its range,
    /// because a gun on the roof should see what it can hit.
    /// </summary>
    public sealed class TurretBattery : MonoBehaviour
    {
        private readonly List<Turret> _turrets = new List<Turret>();
        private HouseClaim _claim;
        private IReadOnlyList<ICombatTarget> _enemies;
        private VisibilityGrid _grid;

        /// <summary>Raised after a turret fires, hit or miss, with where it stood and what it aimed at.</summary>
        public event Action<Vector3, ICombatTarget> Fired;

        public void Construct(HouseClaim claim, IReadOnlyList<ICombatTarget> enemies, VisibilityGrid grid)
        {
            if (_claim != null)
            {
                throw new InvalidOperationException($"{nameof(TurretBattery)} is already constructed.");
            }

            _claim = claim ?? throw new ArgumentNullException(nameof(claim));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _claim.Claimed += HandleClaimed;
        }

        private void OnDestroy()
        {
            if (_claim == null)
            {
                return;
            }

            _claim.Claimed -= HandleClaimed;
            if (_claim.IsClaimed)
            {
                _claim.House.FortificationBuilt -= HandleBuilt;
            }
        }

        private void HandleClaimed(ClaimedHouse house)
        {
            house.FortificationBuilt += HandleBuilt;
        }

        private void HandleBuilt(FortificationSlot slot)
        {
            if (!slot.Fortification.IsTurret)
            {
                return;
            }

            WeaponSettings weapon = slot.Fortification.TurretWeapon;
            _turrets.Add(new Turret(new Weapon(weapon), slot.Position, new VisibilityGridQuery(_grid)));
            _grid.Register(new StaticVisionSource(new WorldPoint(slot.Position.X, slot.Position.Z), weapon.Range));
        }

        private void Update()
        {
            foreach (Turret turret in _turrets)
            {
                if (turret.Tick(Time.deltaTime, _enemies) != FireOutcome.NotFired)
                {
                    Fired?.Invoke(turret.Position.ToUnity(), turret.LastTarget);
                }
            }
        }
    }
}
