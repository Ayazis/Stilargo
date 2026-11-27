using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.Combat;
using Stilargo.Core.Fog;
using Stilargo.Core.Fortification;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Units;
using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>
    /// Runs every turret built on a claimed house. Turrets only fire while their house stands and belongs to the
    /// player, so a fallen or confiscated house's guns go quiet. Each turret also reveals the fog within its range,
    /// because a gun on the roof should see what it can hit.
    /// </summary>
    public sealed class TurretBattery : MonoBehaviour
    {
        private readonly List<MountedTurret> _turrets = new List<MountedTurret>();
        private HouseLedger _ledger;
        private IReadOnlyList<ICombatTarget> _enemies;
        private VisibilityGrid _grid;

        /// <summary>Raised after a turret fires, hit or miss, with where it stood and what it aimed at.</summary>
        public event Action<Vector3, ICombatTarget> Fired;

        public void Construct(HouseLedger ledger, IReadOnlyList<ICombatTarget> enemies, VisibilityGrid grid)
        {
            if (_ledger != null)
            {
                throw new InvalidOperationException($"{nameof(TurretBattery)} is already constructed.");
            }

            _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _ledger.HouseCreated += HandleHouseCreated;
        }

        private void OnDestroy()
        {
            if (_ledger == null)
            {
                return;
            }

            _ledger.HouseCreated -= HandleHouseCreated;
            foreach (ClaimedHouse house in _ledger.Houses)
            {
                house.FortificationBuilt -= HandleBuilt;
            }
        }

        private void HandleHouseCreated(ClaimedHouse house)
        {
            house.FortificationBuilt += HandleBuilt;
        }

        private void HandleBuilt(FortificationSlot slot)
        {
            if (!slot.Fortification.IsTurret)
            {
                return;
            }

            ClaimedHouse house = _ledger.Houses.First(candidate => candidate.Slots.Contains(slot));

            WeaponSettings weapon = slot.Fortification.TurretWeapon;
            var turret = new Turret(new Weapon(weapon), slot.Position, new VisibilityGridQuery(_grid));
            _turrets.Add(new MountedTurret(house, turret, Team.Blue));
            // The gun sits on the roof, so it looks over the lower buildings around it.
            float eyeHeight = slot.Position.Y + VisionSettings.DefaultEyeHeightMetres;
            _grid.Register(new StaticVisionSource(new WorldPoint(slot.Position.X, slot.Position.Z), weapon.Range, eyeHeight));
        }

        private void Update()
        {
            foreach (MountedTurret mounted in _turrets)
            {
                if (!mounted.IsManned)
                {
                    continue;
                }

                Turret turret = mounted.Turret;
                if (turret.Tick(Time.deltaTime, _enemies) != FireOutcome.NotFired)
                {
                    Fired?.Invoke(turret.Position.ToUnity(), turret.LastTarget);
                }
            }
        }
    }
}
