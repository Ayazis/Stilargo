using System;
using System.Collections.Generic;
using Stilargo.Core.Combat;
using Stilargo.Core.Combat.Stances;
using Stilargo.Gameplay.Animation;
using Stilargo.Gameplay.CameraControl;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Orders;
using Stilargo.Gameplay.Selection;
using Stilargo.Gameplay.Stances;
using Stilargo.Gameplay.Units;
using Stilargo.Gameplay.World;
using UnityEngine;
using UnityEngine.AI;

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
        [SerializeField, Min(0.1f)] private float _defensiveLeashRadius = DefaultLeashRadiusMetres;
        [SerializeField] private SelectionController _selectionController;
        [SerializeField] private SelectionBoxView _selectionBoxView;
        [SerializeField] private MoveOrderController _moveOrderController;
        [SerializeField] private FormationLabelView _formationLabelView;
        [SerializeField] private StanceController _stanceController;

        public SelectionController SelectionController => _selectionController;

        public UnityEngine.Camera Camera => _camera;

        /// <summary>The player's soldiers as placed at the start, including ones that have since died.</summary>
        public IReadOnlyList<CombatUnit> PlayerUnits => _playerUnits;

        /// <summary>The rival's soldiers as placed at the start, including ones that have since died.</summary>
        public IReadOnlyList<CombatUnit> EnemyUnits => _enemies;

        /// <summary>Live list of every unit's health bar; null until <see cref="Install"/> has run.</summary>
        public IReadOnlyList<HealthBarSource> HealthBars => _roster == null ? null : _roster.HealthBars;

        /// <summary>Sets and reads the selected units' stance; null until <see cref="Install"/> has run.</summary>
        public SelectionStance SelectionStance { get; private set; }

        private readonly List<CombatUnit> _playerUnits = new List<CombatUnit>();
        private CombatRoster _roster;
        private float _corpseSeconds;

        /// <summary>Lets enemies shoot something that is not a unit, such as the claimed house. Call after <see cref="Install"/>.</summary>
        public void AddPlayerTarget(ICombatTarget target)
        {
            if (_roster == null)
            {
                throw new InvalidOperationException($"Run {nameof(CombatInstaller)}.{nameof(Install)} before adding targets.");
            }

            _roster.AddPlayerTarget(target);
        }

        /// <summary>Lets the player's units shoot something that is not a unit, such as a rival house. Call after <see cref="Install"/>.</summary>
        public void AddEnemyTarget(ICombatTarget target)
        {
            if (_roster == null)
            {
                throw new InvalidOperationException($"Run {nameof(CombatInstaller)}.{nameof(Install)} before adding targets.");
            }

            _roster.AddEnemyTarget(target);
        }

        public override void Install(GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ValidateReferences();

            _cameraRig.Construct(context.PlayerInput, _cameraSettings);

            var stances = new StanceSet(new AggressiveStance(), new DefensiveStance(_defensiveLeashRadius), new HoldStance());
            CombatSettings combat = context.Config.Combat;
            var roster = new CombatRoster(new CombatRosterSettings(stances, combat.Player, combat.Enemy), new VisibilityGridQuery(_world.VisibilityGrid));
            _corpseSeconds = combat.CorpseSeconds;
            AddPlayerUnits(roster, combat.PlayerMoveSpeedMetresPerSecond);
            AddEnemies(roster);
            _roster = roster;

            _selectionController.Construct(context.PlayerInput, _camera, roster.SelectableUnits);
            roster.AttachSelection(_selectionController.Selection);
            _selectionBoxView.Construct(_selectionController.Drag);
            _moveOrderController.Construct(context.PlayerInput, _camera, _selectionController.Selection);
            _formationLabelView.Construct(_moveOrderController);

            SelectionStance = new SelectionStance(stances, roster.CombatByUnit);
            _stanceController.Construct(context.PlayerInput, _selectionController.Selection, SelectionStance);
        }

        private void AddPlayerUnits(CombatRoster roster, float moveSpeedMetresPerSecond)
        {
            foreach (SelectableUnit unit in _units)
            {
                ComponentOf<NavMeshAgent>(unit).speed = moveSpeedMetresPerSecond;
                unit.Construct();
                CombatUnit combatUnit = ComponentOf<CombatUnit>(unit);
                roster.AddPlayerUnit(unit, combatUnit, ChildComponentOf<HitEffectView>(unit));
                Animate(combatUnit);
                _playerUnits.Add(combatUnit);
            }
        }

        private void AddEnemies(CombatRoster roster)
        {
            foreach (CombatUnit enemy in _enemies)
            {
                AutonomousUnit autonomous = ComponentOf<AutonomousUnit>(enemy);
                autonomous.Construct();
                roster.AddEnemyUnit(enemy, ChildComponentOf<HitEffectView>(enemy), autonomous.Orders);
                Animate(enemy);
            }
        }

        // The avatar outlives the unit for a few seconds as a fallen body.
        private void Animate(CombatUnit unit)
        {
            UnitAnimationView view = ComponentOf<UnitAnimationView>(unit);
            view.Construct(unit.Combat);
            unit.Died += () => FallenBody.Lay(view.Body, _corpseSeconds);
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
