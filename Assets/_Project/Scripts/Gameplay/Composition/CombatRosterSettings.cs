using System;
using Stilargo.Core.Combat.Stances;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>The stances and per-side tuning a <see cref="CombatRoster"/> builds units from.</summary>
    internal sealed class CombatRosterSettings
    {
        public CombatRosterSettings(StanceSet stances, CombatSide playerSide, CombatSide enemySide)
        {
            Stances = stances ?? throw new ArgumentNullException(nameof(stances));
            PlayerSide = playerSide ?? throw new ArgumentNullException(nameof(playerSide));
            EnemySide = enemySide ?? throw new ArgumentNullException(nameof(enemySide));
        }

        public StanceSet Stances { get; }

        public CombatSide PlayerSide { get; }

        public CombatSide EnemySide { get; }
    }
}
