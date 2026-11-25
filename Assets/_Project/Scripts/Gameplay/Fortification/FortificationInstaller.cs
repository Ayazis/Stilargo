using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.Combat;
using Stilargo.Core.Fortification;
using Stilargo.Core.Hud;
using Stilargo.Core.Income;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.World;
using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>
    /// Wires claim, fortify and house income: the player claims buildings the squad is garrisoned in, spends money on
    /// fortifications from <c>Fortification.json</c> and earns income per house from <c>Income.json</c>. List it after <see cref="WorldInstaller"/> (wallet, drop-off,
    /// fog) and <see cref="CoverGarrisonInstaller"/> (buildings).
    /// </summary>
    public sealed class FortificationInstaller : Installer
    {
        [SerializeField] private WorldInstaller _world;
        [SerializeField] private CoverGarrisonInstaller _garrisons;
        [SerializeField] private CombatUnit[] _enemies;
        [SerializeField] private TurretBattery _turretBattery;
        [SerializeField] private TurretTracerView _tracerView;
        [SerializeField] private FortificationView _fortificationView;

        /// <summary>The player's claim; null until <see cref="Install"/> has run. Step 10 reads the house from it.</summary>
        public HouseClaim Claim { get; private set; }

        /// <summary>What the HUD shows and does for claiming and buying; null until <see cref="Install"/> has run.</summary>
        public FortifyMenu FortifyMenu { get; private set; }

        public override void Install(GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ValidateReferences();
            FortificationSettings settings = context.Config.Fortifications;
            var layout = new FortificationSlotLayout(_world.CityLayout.CellSize);
            Claim = new HouseClaim(settings, layout, _world.ScavengingArea.DropOff);
            var shop = new FortificationShop(_world.Wallet, settings.Definitions);

            IReadOnlyList<ICombatTarget> enemies = _enemies.Cast<ICombatTarget>().ToList();
            _turretBattery.Construct(Claim, enemies, _world.VisibilityGrid);
            _tracerView.Construct(_turretBattery);
            _fortificationView.Construct(Claim);

            var target = new ClaimTarget(Claim, new ClaimCandidatePicker(_garrisons.Garrisons.Buildings));
            FortifyMenu = new FortifyMenu(target, shop, CreateUpkeepMenu(target, context.Config.HouseIncome));
        }

        // The ticker is added here rather than in the scene, so the scene needs no regenerating for it.
        private UpkeepMenu CreateUpkeepMenu(ClaimTarget target, HouseIncomeSettings incomeSettings)
        {
            var income = new HouseIncome(Claim, incomeSettings, _world.Wallet);
            gameObject.AddComponent<HouseIncomeTicker>().Construct(income);
            return new UpkeepMenu(target, income, new HouseRepair(incomeSettings, _world.Wallet));
        }

        private void ValidateReferences()
        {
            if (_world == null || _garrisons == null || _turretBattery == null || _tracerView == null
                || _fortificationView == null)
            {
                throw new InvalidOperationException($"{nameof(FortificationInstaller)} on '{name}' needs a world, garrisons, turrets and all views assigned.");
            }

            if (_world.ScavengingArea == null || _garrisons.Garrisons == null)
            {
                throw new InvalidOperationException(
                    $"List {nameof(WorldInstaller)} and {nameof(CoverGarrisonInstaller)} before {nameof(FortificationInstaller)}.");
            }

            if (_enemies == null || _enemies.Any(enemy => enemy == null))
            {
                throw new InvalidOperationException($"{nameof(FortificationInstaller)} on '{name}' has a missing or empty enemy slot.");
            }
        }
    }
}
