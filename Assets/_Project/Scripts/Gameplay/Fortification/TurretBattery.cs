using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.City;
using Stilargo.Core.Combat;
using Stilargo.Core.Fog;
using Stilargo.Core.Fortification;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Units;
using Stilargo.Gameplay.World;
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
        private const string ModelNameFormat = "Turret ({0})";

        [SerializeField] private TurretRigView _modelPrefab;

        private readonly Dictionary<MountedTurret, TurretRigView> _turrets = new Dictionary<MountedTurret, TurretRigView>();
        private HouseLedger _ledger;
        private IReadOnlyList<ICombatTarget> _enemies;
        private VisibilityGrid _grid;
        private RoofHeights _roofs;
        private TurretDeck _deck;

        /// <summary>Raised after a turret fires, hit or miss, with its muzzle position and what it aimed at.</summary>
        public event Action<Vector3, ICombatTarget> Fired;

        /// <summary>The world supplies the fog grid each turret reveals and the roofs its deck stands on.</summary>
        public void Construct(HouseLedger ledger, IReadOnlyList<ICombatTarget> enemies, WorldInstaller world)
        {
            if (_ledger != null)
            {
                throw new InvalidOperationException($"{nameof(TurretBattery)} is already constructed.");
            }

            if (_modelPrefab == null)
            {
                throw new InvalidOperationException($"{nameof(TurretBattery)} on '{name}' has no turret model prefab.");
            }

            _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            _grid = world.VisibilityGrid ?? throw new InvalidOperationException("The world must be installed before the turrets.");
            _roofs = world.Roofs;
            _deck = _modelPrefab.GetComponent<TurretDeck>();
            if (_deck == null)
            {
                throw new InvalidOperationException($"Turret model '{_modelPrefab.name}' has no {nameof(TurretDeck)}.");
            }

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
            _turrets.Add(new MountedTurret(house, turret, Team.Blue), CreateModel(slot, turret));
            // The gun sits on the roof, so it looks over the lower buildings around it.
            float eyeHeight = slot.Position.Y + VisionSettings.DefaultEyeHeightMetres;
            _grid.Register(new StaticVisionSource(new WorldPoint(slot.Position.X, slot.Position.Z), weapon.Range, eyeHeight));
        }

        private TurretRigView CreateModel(FortificationSlot slot, Turret turret)
        {
            Quaternion facing = Quaternion.LookRotation(slot.Facing.ToUnity(), Vector3.up);
            var ground = new WorldPoint(slot.Position.X, slot.Position.Z);
            float deckTop = _roofs.HighestUnder(ground, _deck.HalfSizeMetres, slot.Position.Y);
            var position = new Vector3(slot.Position.X, deckTop, slot.Position.Z);
            TurretRigView model = Instantiate(_modelPrefab, position, facing, transform);
            model.name = string.Format(ModelNameFormat, slot.Fortification.DisplayName);
            model.Construct(turret, slot.Fortification.TurretAim);
            return model;
        }

        private void Update()
        {
            foreach (KeyValuePair<MountedTurret, TurretRigView> pair in _turrets)
            {
                MountedTurret mounted = pair.Key;
                if (!mounted.IsManned)
                {
                    continue;
                }

                Turret turret = mounted.Turret;
                if (turret.Tick(Time.deltaTime, _enemies) != FireOutcome.NotFired)
                {
                    pair.Value.ShotEffects.Play();
                    Fired?.Invoke(pair.Value.MuzzlePosition, turret.LastTarget);
                }
            }
        }
    }
}
