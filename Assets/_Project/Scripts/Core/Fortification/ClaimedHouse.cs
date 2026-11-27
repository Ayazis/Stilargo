using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Stilargo.Core.Combat;
using Stilargo.Core.Garrison;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// A claimed building with fortification slots, its own hit points, an upkeep condition and an owner (Blue, Red
    /// or none). Enemies can shoot it like a unit; when it is destroyed it becomes unoccupied but stays damaged, so
    /// whoever claims it next has to repair it. Fortifications only add strength, so the house has no cover
    /// protection of its own.
    /// </summary>
    public sealed class ClaimedHouse : ICombatTarget
    {
        public const float FullCondition = 1f;

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

        /// <summary>Raised when the house is taken over or becomes unoccupied.</summary>
        public event Action<ClaimedHouse> OwnerChanged;

        /// <summary>The team that holds the house, or null while it is unoccupied.</summary>
        public Team? Owner { get; private set; }

        public bool IsUnoccupied => Owner == null;

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

        /// <summary>Upkeep from 1 (well kept) down to 0 (run down). It wears over time and is repaired with money.</summary>
        public float Condition { get; private set; } = FullCondition;

        /// <summary>True when worn or damaged, so a repair would change something.</summary>
        public bool NeedsRepair => Condition < FullCondition || CurrentHealth < MaxHealth;

        /// <summary>Share of the hit points that are missing, 0 to 1.</summary>
        public float DamagedFraction => 1f - HealthFraction;

        /// <summary>How well the house is kept, 0 to 1: both worn condition and battle damage lower it.</summary>
        public float Maintenance => Condition * HealthFraction;

        /// <summary>Share of the fortification slots that hold a fortification, 0 to 1.</summary>
        public float FortifiedFraction => FractionOf(_slots.Count(slot => slot.IsFilled), _slots.Count);

        /// <summary>Share of the building's garrison slots that hold a unit, 0 to 1.</summary>
        public float OccupiedFraction => FractionOf(Building.Slots.Count(slot => slot.IsOccupied), Building.Slots.Count);

        /// <summary>Floor area in cells: footprint times storeys, so bigger and taller buildings count for more.</summary>
        public int FloorCells => Building.Plot.Area.Width * Building.Plot.Area.Height * Building.Plot.Storeys;

        public bool IsOwnedBy(Team team)
        {
            return Owner == team;
        }

        /// <summary>A team can take a house it does not own when it is unoccupied or lies in ruins.</summary>
        public bool IsClaimableBy(Team team)
        {
            return !IsOwnedBy(team) && (IsUnoccupied || !IsAlive);
        }

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
                ChangeOwner(null);
                Died?.Invoke();
            }
        }

        /// <summary>Lowers the condition by <paramref name="amount"/>; it never drops below zero.</summary>
        public void Wear(float amount)
        {
            if (float.IsNaN(amount) || amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Wear must be a non-negative number.");
            }

            Condition = Math.Max(0f, Condition - amount);
        }

        internal void TakeOver(Team team)
        {
            if (!IsClaimableBy(team))
            {
                throw new InvalidOperationException($"{team} cannot take over a house held by {Owner}.");
            }

            ChangeOwner(team);
        }

        /// <summary>Restores full condition and full health, which brings a ruined house back into use.</summary>
        internal void Restore()
        {
            Condition = FullCondition;
            CurrentHealth = MaxHealth;
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

        private void ChangeOwner(Team? owner)
        {
            if (Owner == owner)
            {
                return;
            }

            Owner = owner;
            OwnerChanged?.Invoke(this);
        }

        private static float FractionOf(int part, int whole)
        {
            return whole == 0 ? 0f : part / (float)whole;
        }
    }
}
