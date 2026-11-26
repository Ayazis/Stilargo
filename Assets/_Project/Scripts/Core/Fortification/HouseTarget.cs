using System;
using System.Numerics;
using Stilargo.Core.Combat;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// A house as seen by the team that wants to shoot it: a valid target only while it stands and belongs to
    /// <see cref="Owner"/>. Houses change hands and can be repaired, so this target never leaves a roster for good.
    /// </summary>
    public sealed class HouseTarget : ICombatTarget
    {
        private readonly ClaimedHouse _house;

        public HouseTarget(ClaimedHouse house, Team owner)
        {
            _house = house ?? throw new ArgumentNullException(nameof(house));
            Owner = owner;
        }

        // Rosters drop a target when it dies; a house can come back, so this target never reports a death.
        public event Action Died
        {
            add { }
            remove { }
        }

        /// <summary>The team that must own the house for it to be a target.</summary>
        public Team Owner { get; }

        public bool IsAlive => _house.IsAlive && _house.IsOwnedBy(Owner);

        public Vector3 Position => _house.Position;

        public float HealthFraction => _house.HealthFraction;

        public CombatProtection Protection => _house.Protection;

        public void TakeDamage(float amount)
        {
            if (!IsAlive)
            {
                return;
            }

            _house.TakeDamage(amount);
        }
    }
}
