using System;
using Stilargo.Core.Audio;
using Stilargo.Core.Combat;
using Stilargo.Core.Fortification;
using Stilargo.Core.Match;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Fortification;
using Stilargo.Gameplay.Hill;
using Stilargo.Gameplay.Units;
using UnityEngine;

namespace Stilargo.Gameplay.Audio
{
    /// <summary>
    /// Gives the game its sound: every shot, hit and death, turret fire, claims, new fortifications, the end of the
    /// match. Volumes come from <c>Audio.json</c>. List it after the combat, fortification and
    /// hill installers, and before the HUD, which plays button clicks through <see cref="Sounds"/>.
    /// </summary>
    public sealed class AudioInstaller : Installer
    {
        [SerializeField] private CombatInstaller _combat;
        [SerializeField] private FortificationInstaller _fortification;
        [SerializeField] private HillRivalInstaller _hill;

        private SoundPlayer _player;

        /// <summary>Plays any sound; null until <see cref="Install"/> has run.</summary>
        public ISoundPlayer Sounds => _player;

        public override void Install(GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ValidateReferences();
            var bank = new SoundBank(new SoundLibrary(), UnityEngine.AudioSettings.outputSampleRate);
            _player = gameObject.AddComponent<SoundPlayer>();
            _player.Construct(bank, context.Config.Audio);

            foreach (CombatUnit unit in _combat.PlayerUnits)
            {
                ListenToUnit(unit, SoundCue.RifleShot);
            }

            foreach (CombatUnit unit in _combat.EnemyUnits)
            {
                ListenToUnit(unit, SoundCue.EnemyRifleShot);
            }

            ListenToFortifications();
            _hill.Referee.Decided += OnMatchDecided;
        }

        private void ListenToUnit(CombatUnit unit, SoundCue shot)
        {
            UnitCombat combat = unit.Combat;
            combat.Fired += target => OnShotHit(unit, shot, target);
            combat.Missed += target => _player.PlayAt(shot, unit.Position);
            unit.Died += () => _player.PlayAt(SoundCue.Death, unit.Position);
        }

        private void OnShotHit(CombatUnit shooter, SoundCue shot, ICombatTarget target)
        {
            _player.PlayAt(shot, shooter.Position);
            _player.PlayAt(SoundCue.BulletHit, target.Position);
        }

        private void ListenToFortifications()
        {
            _fortification.Turrets.Fired += (muzzle, target) => _player.PlayAt(SoundCue.TurretShot, muzzle.ToNumerics());
            _fortification.Claim.Claimed += house => _player.Play(SoundCue.Claim);
            _fortification.Ledger.HouseCreated += house => house.FortificationBuilt += slot => _player.PlayAt(SoundCue.Build, slot.Position);
        }

        private void OnMatchDecided(MatchOutcome outcome)
        {
            _player.Play(outcome == MatchOutcome.Won ? SoundCue.Victory : SoundCue.Defeat);
        }

        private void ValidateReferences()
        {
            if (_combat == null || _fortification == null || _hill == null)
            {
                throw new InvalidOperationException($"{nameof(AudioInstaller)} on '{name}' needs the combat, fortification and hill installers.");
            }

            if (_combat.PlayerUnits.Count == 0 || _fortification.Claim == null || _hill.Referee == null)
            {
                throw new InvalidOperationException(
                    $"List {nameof(CombatInstaller)}, {nameof(FortificationInstaller)} and {nameof(HillRivalInstaller)} before {nameof(AudioInstaller)}.");
            }
        }
    }
}
