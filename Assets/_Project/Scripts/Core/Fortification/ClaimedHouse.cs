using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Stilargo.Core.Combat;
using Stilargo.Core.Garrison;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// The player's base: a claimed building with fortification slots and its own hit points. Enemies can
    /// shoot it like a unit; losing it is the step-10 lose condition. Fortifications only add strength, so the
    /// house has no cover protection of its own.
    /// </summary>
    public sealed class ClaimedHouse : ICombatTarget
    {
        private readonly List<FortificationSlot> _slots;

        public ClaimedHouse(GarrisonBuilding building, IReadOnlyList<FortificationSlot> slots, float baseHealthPoints)
        {
            if (slots == null)
            {
                throw new ArgumentNullException(nameof(slots));
            }

            if (slots.Any(slot => slot == null))
            {
                throw new ArgumentException("Slots must not contain null.", nameof(slots));
            }

            if (!(baseHealthPoints > 0f) || float.IsInfinity(baseHealthPoints))
            {
                throw new ArgumentOutOfRangeException(nameof(baseHealthPoints), baseHealthPoints, "Base health must be positive and finite.");
            }

            Building = building ?? throw new ArgumentNullException(nameof(building));
            _slots = slots.ToList();
            MaxHealth = baseHealthPoints;
            CurrentHealth = baseHealthPoints;
        }

        public event Action Died;

        public event Action<FortificationSlot> FortificationBuilt;

        public GarrisonBuilding Building { get; }

        public IReadOnlyList<FortificationSlot> Slots => _slots;

        /// <summary>The street door, where enemies aim and loot is delivered.</summary>
        public Vector3 Position => Building.Entrance;

        public float MaxHealth { get; private set; }

        public float CurrentHealth { get; private set; }

        public float HealthFraction => CurrentHealth / MaxHealth;

        public bool IsAlive => CurrentHealth > 0f;

        public CombatProtection Protection => CombatProtection.None;

        public bool TryFindFreeSlot(FortificationSlotKind kind, out FortificationSlot slot)
        {
            slot = _slots.FirstOrDefault(candidate => candidate.Kind == kind && !candidate.IsFilled);
            return slot != null;
        }

        public void TakeDamage(float amount)
        {
            if (float.IsNaN(amount) || amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Damage must be a non-negative number.");
            }

            if (!IsAlive)
            {
                return;
            }

            CurrentHealth = Math.Max(0f, CurrentHealth - amount);
            if (!IsAlive)
            {
                Died?.Invoke();
            }
        }

        /// <summary>Builds into the slot and adds its health bonus to both maximum and current health.</summary>
        internal void Build(FortificationSlot slot, FortificationDefinition fortification)
        {
            if (!_slots.Contains(slot))
            {
                throw new ArgumentException("Slot does not belong to this house.", nameof(slot));
            }

            if (slot.Kind != fortification.SlotKind)
            {
                throw new ArgumentException($"'{fortification.Id}' needs a {fortification.SlotKind} slot, not {slot.Kind}.", nameof(fortification));
            }

            slot.Fill(fortification);
            MaxHealth += fortification.HealthBonusPoints;
            CurrentHealth += fortification.HealthBonusPoints;
            FortificationBuilt?.Invoke(slot);
        }
    }
}
