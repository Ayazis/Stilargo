using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.Combat;
using Stilargo.Core.Fortification;
using Stilargo.Core.Hud;
using Stilargo.Core.Income;
using Stilargo.Core.Selection;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Selection;
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
        [SerializeField] private CombatInstaller _combat;
        [SerializeField] private Material _selectionOutlineMaterial;
        [SerializeField] private CombatUnit[] _enemies;
        [SerializeField] private TurretBattery _turretBattery;
        [SerializeField] private TurretTracerView _tracerView;
        [SerializeField] private FortificationView _fortificationView;

        private const string OutlineName = "Selected Building Outline";

        // Kept so the drop-off keeps following the player's base for the whole match.
        private BaseDropOff _baseDropOff;
        private BuildingClickSelection _buildingClicks;

        /// <summary>The building the player clicked on; null until <see cref="Install"/> has run.</summary>
        public BuildingSelection BuildingSelection { get; private set; }

        /// <summary>The roof guns of the player's houses; they fire once fortifications are built.</summary>
        public TurretBattery Turrets => _turretBattery;

        /// <summary>Every house either team has claimed; null until <see cref="Install"/> has run.</summary>
        public HouseLedger Ledger { get; private set; }

        /// <summary>The player's (Blue) houses; null until <see cref="Install"/> has run. Step 10 reads the houses from it.</summary>
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
            Ledger = new HouseLedger(settings, layout);
            Claim = new HouseClaim(Team.Blue, Ledger);
            _baseDropOff = new BaseDropOff(Claim, _world.ScavengingArea.DropOff);
            var shop = new FortificationShop(_world.Wallet, settings.Definitions);

            IReadOnlyList<ICombatTarget> enemies = _enemies.Cast<ICombatTarget>().ToList();
            _turretBattery.Construct(Ledger, enemies, _world.VisibilityGrid);
            _tracerView.Construct(_turretBattery);
            _fortificationView.Construct(Ledger);

            BuildingSelection = new BuildingSelection();
            InstallBuildingSelection();
            var target = new ClaimTarget(Claim, new ClaimCandidatePicker(_garrisons.Garrisons.Buildings), BuildingSelection);
            FortifyMenu = new FortifyMenu(target, shop, CreateUpkeepMenu(target, context.Config.HouseIncome));
        }

        private void OnDestroy()
        {
            if (_buildingClicks != null)
            {
                _buildingClicks.Dispose();
            }
        }

        // The outline is added here rather than in the scene, like the income ticker below.
        private void InstallBuildingSelection()
        {
            _buildingClicks = new BuildingClickSelection(_combat.SelectionController, _garrisons.Garrisons, BuildingSelection);
            var outline = new GameObject(OutlineName, typeof(LineRenderer));
            outline.transform.SetParent(transform, false);
            outline.AddComponent<BuildingSelectionView>().Construct(BuildingSelection, _world.CityLayout.CellSize, _selectionOutlineMaterial);
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
            if (_world == null || _garrisons == null || _combat == null || _turretBattery == null || _tracerView == null
                || _fortificationView == null || _selectionOutlineMaterial == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(FortificationInstaller)} on '{name}' needs a world, garrisons, combat, turrets, all views and an outline material assigned.");
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
