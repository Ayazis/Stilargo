using System;
using Stilargo.Core.Combat;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// One kind of fortification the player can buy, e.g. a barricade or a turret. Immutable; optional parts
    /// are added with the With... methods. Health bonus strengthens the house; a turret weapon makes it shoot.
    /// </summary>
    public sealed class FortificationDefinition
    {
        public FortificationDefinition(string id, FortificationSlotKind slotKind, int costMoney)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id must not be empty.", nameof(id));
            }

            if (costMoney <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(costMoney), costMoney, $"Cost of '{id}' must be positive.");
            }

            Id = id;
            DisplayName = id;
            SlotKind = slotKind;
            CostMoney = costMoney;
        }

        public string Id { get; }

        public string DisplayName { get; private set; }

        public FortificationSlotKind SlotKind { get; }

        public int CostMoney { get; }

        /// <summary>Hit points added to the house when this is built.</summary>
        public float HealthBonusPoints { get; private set; }

        /// <summary>The turret's gun, or null when this fortification does not shoot.</summary>
        public WeaponSettings TurretWeapon { get; private set; }

        public bool IsTurret => TurretWeapon != null;

        public FortificationDefinition WithDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException($"Display name of '{Id}' must not be empty.", nameof(displayName));
            }

            FortificationDefinition copy = Copy();
            copy.DisplayName = displayName;
            return copy;
        }

        public FortificationDefinition WithHealthBonus(float healthBonusPoints)
        {
            if (!(healthBonusPoints >= 0f) || float.IsInfinity(healthBonusPoints))
            {
                throw new ArgumentOutOfRangeException(nameof(healthBonusPoints), healthBonusPoints, $"Health bonus of '{Id}' must be non-negative and finite.");
            }

            FortificationDefinition copy = Copy();
            copy.HealthBonusPoints = healthBonusPoints;
            return copy;
        }

        public FortificationDefinition WithTurret(WeaponSettings turretWeapon)
        {
            FortificationDefinition copy = Copy();
            copy.TurretWeapon = turretWeapon ?? throw new ArgumentNullException(nameof(turretWeapon));
            return copy;
        }

        private FortificationDefinition Copy()
        {
            return (FortificationDefinition)MemberwiseClone();
        }
    }
}
