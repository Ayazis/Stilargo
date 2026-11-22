using System;
using Stilargo.Core.Combat.Stances;
using Stilargo.Gameplay.CameraControl;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Orders;
using Stilargo.Gameplay.Selection;
using Stilargo.Gameplay.Stances;
using Stilargo.Gameplay.Units;
using Stilargo.Gameplay.World;
using UnityEngine;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Wires Ci's side of the game: the RTS camera, the player's units, selection, move orders,
    /// stances and combat against the enemy units.
    /// Runs after the installer that bakes the NavMesh, because units need it to be placed.
    /// </summary>
    public sealed class CombatInstaller : Installer
    {
        private const float DefaultLeashRadiusMetres = 10f;

        [SerializeField] private WorldInstaller _world;
        [SerializeField] private RtsCameraRig _cameraRig;
        [SerializeField] private CameraSettings _cameraSettings;
        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private SelectableUnit[] _units;
        [SerializeField] private CombatUnit[] _enemies;
        [SerializeField] private CombatSide _playerSide = new CombatSide();
        [SerializeField] private CombatSide _enemySide = new CombatSide();
        [SerializeField, Min(0.1f)] private float _defensiveLeashRadius = DefaultLeashRadiusMetres;
        [SerializeField] private SelectionController _selectionController;
        [SerializeField] private SelectionBoxView _selectionBoxView;
        [SerializeField] private MoveOrderController _moveOrderController;
        [SerializeField] private FormationLabelView _formationLabelView;
        [SerializeField] private StanceController _stanceController;
        [SerializeField] private StanceLabelView _stanceLabelView;

        public override void Install(GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ValidateReferences();

            _cameraRig.Construct(context.PlayerInput, _cameraSettings);

            var stances = new StanceSet(new AggressiveStance(), new DefensiveStance(_defensiveLeashRadius), new HoldStance());
            var roster = new CombatRoster(new CombatRosterSettings(stances, _playerSide, _enemySide), new VisibilityGridQuery(_world.VisibilityGrid));
            AddPlayerUnits(roster);
            AddEnemies(roster);

            _selectionController.Construct(context.PlayerInput, _camera, roster.SelectableUnits);
            roster.AttachSelection(_selectionController.Selection);
            _selectionBoxView.Construct(_selectionController.Drag);
            _moveOrderController.Construct(context.PlayerInput, _camera, _selectionController.Selection);
            _formationLabelView.Construct(_moveOrderController);

            var selectionStance = new SelectionStance(stances, roster.CombatByUnit);
            _stanceController.Construct(context.PlayerInput, _selectionController.Selection, selectionStance);
            _stanceLabelView.Construct(_selectionController.Selection, selectionStance);
        }

        private void AddPlayerUnits(CombatRoster roster)
        {
            foreach (SelectableUnit unit in _units)
            {
                unit.Construct();
                roster.AddPlayerUnit(unit, ComponentOf<CombatUnit>(unit), ChildComponentOf<HitEffectView>(unit));
            }
        }

        private void AddEnemies(CombatRoster roster)
        {
            foreach (CombatUnit enemy in _enemies)
            {
                roster.AddEnemyUnit(enemy, ChildComponentOf<HitEffectView>(enemy));
            }
        }

        private T ComponentOf<T>(Component owner) where T : Component
        {
            T component = owner.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"{owner.name} has no {typeof(T).Name}.");
            }

            return component;
        }

        private T ChildComponentOf<T>(Component owner) where T : Component
        {
            T component = owner.GetComponentInChildren<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"{owner.name} has no {typeof(T).Name} in its children.");
            }

            return component;
        }

        private void ValidateReferences()
        {
            RequireAssigned(_world, nameof(WorldInstaller));
            RequireAssigned(_cameraRig, nameof(RtsCameraRig));
            RequireAssigned(_cameraSettings, nameof(CameraSettings));
            RequireAssigned(_camera, nameof(UnityEngine.Camera));
            RequireAssigned(_selectionController, nameof(SelectionController));
            RequireAssigned(_selectionBoxView, nameof(SelectionBoxView));
            RequireAssigned(_moveOrderController, nameof(MoveOrderController));
            RequireAssigned(_formationLabelView, nameof(FormationLabelView));
            RequireAssigned(_stanceController, nameof(StanceController));
            RequireAssigned(_stanceLabelView, nameof(StanceLabelView));
            RequireAssigned(_playerSide.Weapon, "player " + nameof(WeaponDefinition));
            RequireAssigned(_enemySide.Weapon, "enemy " + nameof(WeaponDefinition));
            RequireNoEmptySlots(_units, "unit");
            RequireNoEmptySlots(_enemies, "enemy");
        }

        private void RequireNoEmptySlots(UnityEngine.Object[] slots, string slotName)
        {
            if (slots == null)
            {
                throw new InvalidOperationException($"{nameof(CombatInstaller)} on '{name}' has no {slotName} list.");
            }

            for (int index = 0; index < slots.Length; index++)
            {
                if (slots[index] == null)
                {
                    throw new InvalidOperationException($"{nameof(CombatInstaller)} on '{name}' has an empty {slotName} slot at index {index}.");
                }
            }
        }

        private void RequireAssigned(UnityEngine.Object reference, string typeName)
        {
            if (reference == null)
            {
                throw new InvalidOperationException($"{nameof(CombatInstaller)} on '{name}' has no {typeName} assigned.");
            }
        }
    }
}
