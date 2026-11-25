using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.Combat;
using Stilargo.Core.Economy;
using Stilargo.Core.Fog;
using Stilargo.Core.Fortification;
using Stilargo.Core.Hill;
using Stilargo.Core.Match;
using Stilargo.Core.Rival;
using Stilargo.Core.Scavenging;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Fortification;
using Stilargo.Gameplay.Rival;
using Stilargo.Gameplay.Units;
using Stilargo.Gameplay.World;
using UnityEngine;
using UnityEngine.AI;

namespace Stilargo.Gameplay.Hill
{
    /// <summary>
    /// Wires step 10: the hill from <c>Hill.json</c>, the AI rival that commands the enemy units, and the win/lose
    /// referee. List it last: it needs the world (wallet, loot, roads), combat (enemy orders, targets) and the claim.
    /// </summary>
    public sealed class HillRivalInstaller : Installer
    {
        [SerializeField] private WorldInstaller _world;
        [SerializeField] private CombatInstaller _combat;
        [SerializeField] private FortificationInstaller _fortifications;
        [SerializeField] private CombatUnit[] _playerUnits;
        [SerializeField] private CombatUnit[] _enemies;
        [SerializeField] private HillZoneView _zoneView;
        [SerializeField] private HillHudView _hudView;
        [SerializeField] private MatchLoop _matchLoop;
        [SerializeField] private MatchEndPause _endPause;

        /// <summary>Null until <see cref="Install"/>. Step 11's UI reads hill state and hold time from it.</summary>
        public HillControl Hill { get; private set; }

        /// <summary>Null until <see cref="Install"/>. Raises <see cref="MatchReferee.Decided"/> on win or lose.</summary>
        public MatchReferee Referee { get; private set; }

        /// <summary>Null until <see cref="Install"/>. Its current strategy says what the rival is doing.</summary>
        public RivalBrain Rival { get; private set; }

        /// <summary>The rival's scavenged money; empty until <see cref="Install"/>.</summary>
        public Wallet RivalWallet { get; } = new Wallet();

        public override void Install(GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ValidateReferences();
            HillSettings hillSettings = context.Config.Hill;
            RivalSettings rivalSettings = context.Config.Rival;
            RequireHillOnRoad(hillSettings);
            RequireSquadFits(rivalSettings);

            Hill = new HillControl(new HillZone(CenterOf(hillSettings), hillSettings.RadiusMetres), hillSettings);
            HouseClaim claim = _fortifications.Claim;
            LetEnemiesTargetTheHouse(claim);
            Referee = new MatchReferee(Hill, claim);
            Rival = CreateRival(rivalSettings, claim);
            Rival.Start();
            _matchLoop.Construct(new HillMatch(Hill, Rival, Referee), _playerUnits.Cast<ICombatTarget>().ToList(), _enemies.Cast<ICombatTarget>().ToList());
            _zoneView.Construct(Hill);
            _hudView.Construct(Hill, Rival, Referee);
            _endPause.Construct(Referee);
        }

        private void LetEnemiesTargetTheHouse(HouseClaim claim)
        {
            claim.Claimed += house => _combat.AddPlayerTarget(house);
            if (claim.IsClaimed)
            {
                _combat.AddPlayerTarget(claim.House);
            }
        }

        private RivalBrain CreateRival(RivalSettings settings, HouseClaim claim)
        {
            List<IRivalUnit> units = _enemies.Take(settings.SquadSize).Select(enemy => CreateRivalUnit(enemy, settings)).ToList();
            System.Numerics.Vector3 basePosition = AverageGroundPosition(units);
            var dropOff = new DropOff(new WorldPoint(basePosition.X, basePosition.Z), RivalWallet);
            var scavenging = new ScavengingArea(_world.ScavengingArea.Caches, dropOff, settings.ScavengeReachMetres);
            var world = new RivalWorld(new RivalSquad(units, basePosition), new RivalSurroundings(Hill, claim, scavenging), settings);
            return new RivalBrain(world, new RivalStrategySet());
        }

        private static IRivalUnit CreateRivalUnit(CombatUnit enemy, RivalSettings settings)
        {
            enemy.GetComponent<NavMeshAgent>().speed = settings.MoveSpeedMetresPerSecond;
            AutonomousUnit autonomous = enemy.GetComponent<AutonomousUnit>();
            if (autonomous == null || autonomous.Orders == null)
            {
                throw new InvalidOperationException($"{enemy.name} needs a constructed {nameof(AutonomousUnit)}; list {nameof(CombatInstaller)} first.");
            }

            return new RivalUnitAdapter(enemy, autonomous.Orders);
        }

        private static System.Numerics.Vector3 AverageGroundPosition(IReadOnlyList<IRivalUnit> units)
        {
            float x = units.Average(unit => unit.Position.X);
            float z = units.Average(unit => unit.Position.Z);
            return new System.Numerics.Vector3(x, 0f, z);
        }

        private static System.Numerics.Vector3 CenterOf(HillSettings settings)
        {
            return new System.Numerics.Vector3(settings.CenterXMetres, 0f, settings.CenterZMetres);
        }

        private void RequireHillOnRoad(HillSettings settings)
        {
            if (!_world.CityLayout.IsRoadAt(settings.CenterXMetres, settings.CenterZMetres))
            {
                throw new InvalidOperationException(
                    $"Hill.json puts the hill centre at ({settings.CenterXMetres}, {settings.CenterZMetres}), which is not on a road. Move it onto a street.");
            }
        }

        private void RequireSquadFits(RivalSettings settings)
        {
            if (settings.SquadSize > _enemies.Length)
            {
                throw new InvalidOperationException(
                    $"Hill.json asks for a rival squad of {settings.SquadSize}, but the scene has only {_enemies.Length} enemy units.");
            }
        }

        private void ValidateReferences()
        {
            if (_world == null || _combat == null || _fortifications == null || _zoneView == null
                || _hudView == null || _matchLoop == null || _endPause == null)
            {
                throw new InvalidOperationException($"{nameof(HillRivalInstaller)} on '{name}' needs the world, combat, fortifications and all views assigned.");
            }

            if (_world.CityLayout == null || _world.ScavengingArea == null || _fortifications.Claim == null)
            {
                throw new InvalidOperationException(
                    $"List {nameof(WorldInstaller)}, {nameof(CombatInstaller)} and {nameof(FortificationInstaller)} before {nameof(HillRivalInstaller)}.");
            }

            RequireNoEmptySlots(_playerUnits, "player unit");
            RequireNoEmptySlots(_enemies, "enemy");
        }

        private void RequireNoEmptySlots(CombatUnit[] slots, string slotName)
        {
            if (slots == null || slots.Length == 0 || slots.Any(slot => slot == null))
            {
                throw new InvalidOperationException($"{nameof(HillRivalInstaller)} on '{name}' has a missing or empty {slotName} list.");
            }
        }
    }
}
