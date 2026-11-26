using System;

namespace Stilargo.Core.Fortification
{
    /// <summary>A turret on a house's roof. It is only manned while the house stands and belongs to its crew's team.</summary>
    public sealed class MountedTurret
    {
        private readonly ClaimedHouse _house;
        private readonly Team _crew;

        public MountedTurret(ClaimedHouse house, Turret turret, Team crew)
        {
            _house = house ?? throw new ArgumentNullException(nameof(house));
            Turret = turret ?? throw new ArgumentNullException(nameof(turret));
            _crew = crew;
        }

        public Turret Turret { get; }

        public bool IsManned => _house.IsAlive && _house.IsOwnedBy(_crew);
    }
}
