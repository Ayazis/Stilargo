using System;
using System.Linq;
using Stilargo.Core.Combat;
using Stilargo.Core.Fortification;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>One fortification in <c>Fortification.json</c>. Turret keys are read only when <c>hasTurret</c> is true.</summary>
    [Serializable]
    public sealed class FortificationEntry
    {
        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private string slotKind = string.Empty;
        [SerializeField] private int costMoney;
        [SerializeField] private float healthBonusPoints;
        [SerializeField] private bool hasTurret;
        [SerializeField] private float turretDamagePerShot;
        [SerializeField] private float turretRangeMetres;
        [SerializeField] private float turretCooldownSeconds;
        [SerializeField] private float turretHitChance;

        public static FortificationEntry[] Defaults()
        {
            return FortificationSettings.Default.Definitions.Select(From).ToArray();
        }

        public FortificationDefinition ToDefinition()
        {
            if (!Enum.TryParse(slotKind, false, out FortificationSlotKind kind) || !Enum.IsDefined(typeof(FortificationSlotKind), kind))
            {
                string allowed = string.Join(", ", Enum.GetNames(typeof(FortificationSlotKind)));
                throw new ArgumentException($"'{id}' has slotKind '{slotKind}'; use one of {allowed}.");
            }

            FortificationDefinition definition = new FortificationDefinition(id, kind, costMoney)
                .WithDisplayName(displayName)
                .WithHealthBonus(healthBonusPoints);
            if (!hasTurret)
            {
                return definition;
            }

            return definition.WithTurret(ToTurretWeapon());
        }

        private WeaponSettings ToTurretWeapon()
        {
            try
            {
                return new WeaponSettings(turretDamagePerShot, turretRangeMetres, turretCooldownSeconds).WithHitChance(turretHitChance);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException($"Turret of '{id}': {exception.Message}", exception);
            }
        }

        private static FortificationEntry From(FortificationDefinition definition)
        {
            FortificationEntry entry = new FortificationEntry
            {
                id = definition.Id,
                displayName = definition.DisplayName,
                slotKind = definition.SlotKind.ToString(),
                costMoney = definition.CostMoney,
                healthBonusPoints = definition.HealthBonusPoints,
                hasTurret = definition.IsTurret,
            };

            if (definition.IsTurret)
            {
                entry.turretDamagePerShot = definition.TurretWeapon.Damage;
                entry.turretRangeMetres = definition.TurretWeapon.Range;
                entry.turretCooldownSeconds = definition.TurretWeapon.CooldownSeconds;
                entry.turretHitChance = definition.TurretWeapon.HitChance;
            }

            return entry;
        }
    }
}
