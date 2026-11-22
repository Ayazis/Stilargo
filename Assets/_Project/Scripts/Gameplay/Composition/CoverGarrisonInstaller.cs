using System;
using System.Collections.Generic;
using Stilargo.Core.Cover;
using Stilargo.Core.Garrison;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Cover;
using Stilargo.Gameplay.Garrison;
using Stilargo.Gameplay.GarrisonViews;
using Stilargo.Gameplay.Orders;
using Stilargo.Gameplay.Units;
using Stilargo.Gameplay.World;
using UnityEngine;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Derives cover points and garrison slots from the city and hands them to the units:
    /// every unit gets cover, player units can garrison buildings by right-clicking them.
    /// List it after <see cref="WorldInstaller"/> (city, fog) and <see cref="CombatInstaller"/> (units, orders).
    /// </summary>
    public sealed class CoverGarrisonInstaller : Installer
    {
        [SerializeField] private WorldInstaller _world;
        [SerializeField] private MoveOrderController _moveOrderController;
        [SerializeField] private SelectableUnit[] _units;
        [SerializeField] private CombatUnit[] _enemies;
        [SerializeField] private CoverPointView _coverPointView;
        [SerializeField] private GarrisonSlotMarkerView _slotMarkerView;

        private GarrisonExitOnMove _exitOnMove;

        public override void Install(GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ValidateReferences();
            InstallCover();
            InstallGarrisons();
        }

        private void OnDestroy()
        {
            if (_exitOnMove != null)
            {
                _exitOnMove.Dispose();
            }
        }

        private void InstallCover()
        {
            CoverSettings settings = CoverSettings.Default;
            IReadOnlyList<CoverPoint> points = new CoverPointGenerator(settings).Generate(_world.CityLayout);
            var map = new CoverMap(points, settings);
            _coverPointView.Construct(points);

            foreach (SelectableUnit unit in _units)
            {
                CombatUnit combatUnit = unit.GetComponent<CombatUnit>();
                combatUnit.AddBonusSource(new CoverBonusSource(map, combatUnit.transform));
            }

            foreach (CombatUnit enemy in _enemies)
            {
                enemy.AddBonusSource(new CoverBonusSource(map, enemy.transform));
            }
        }

        private void InstallGarrisons()
        {
            IReadOnlyList<GarrisonBuilding> buildings = new GarrisonSlotGenerator(new GarrisonLayoutSettings()).Generate(_world.CityLayout);
            var directory = new GarrisonDirectory(buildings, _world.CityLayout.CellSize);
            var bonuses = new GarrisonBonuses();
            var registry = new GarrisonOccupantRegistry();

            foreach (SelectableUnit unit in _units)
            {
                AddOccupant(unit, new GarrisonOccupantSetup(bonuses, unit.Orders, _world.VisibilityGrid), registry);
            }

            _moveOrderController.AddHandler(new GarrisonCommandHandler(directory, registry));
            _exitOnMove = new GarrisonExitOnMove(_moveOrderController, registry);
            _slotMarkerView.Construct(buildings);
        }

        private static void AddOccupant(SelectableUnit unit, GarrisonOccupantSetup setup, GarrisonOccupantRegistry registry)
        {
            GarrisonOccupant occupant = unit.GetComponent<GarrisonOccupant>();
            CombatUnit combatUnit = unit.GetComponent<CombatUnit>();
            if (occupant == null || combatUnit == null)
            {
                throw new InvalidOperationException($"{unit.name} needs a {nameof(GarrisonOccupant)} and a {nameof(CombatUnit)}.");
            }

            occupant.Construct(setup);
            registry.Add(unit, occupant);
            combatUnit.AddBonusSource(occupant);
            combatUnit.Died += () => registry.Remove(unit);
        }

        private void ValidateReferences()
        {
            if (_world == null || _moveOrderController == null || _coverPointView == null || _slotMarkerView == null)
            {
                throw new InvalidOperationException($"{nameof(CoverGarrisonInstaller)} on '{name}' needs a world, move orders and both views assigned.");
            }

            if (_world.CityLayout == null)
            {
                throw new InvalidOperationException($"{nameof(WorldInstaller)} has not installed yet; list it before {nameof(CoverGarrisonInstaller)}.");
            }

            RequireNoEmptySlots(_units, "unit");
            RequireNoEmptySlots(_enemies, "enemy");
        }

        private void RequireNoEmptySlots(UnityEngine.Object[] slots, string slotName)
        {
            if (slots == null)
            {
                throw new InvalidOperationException($"{nameof(CoverGarrisonInstaller)} on '{name}' has no {slotName} list.");
            }

            for (int index = 0; index < slots.Length; index++)
            {
                if (slots[index] == null)
                {
                    throw new InvalidOperationException($"{nameof(CoverGarrisonInstaller)} on '{name}' has an empty {slotName} slot at index {index}.");
                }
            }
        }
    }
}
