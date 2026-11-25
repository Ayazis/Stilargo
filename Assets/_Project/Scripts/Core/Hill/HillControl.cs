using System;
using System.Collections.Generic;
using Stilargo.Core.Combat;
using Stilargo.Core.Economy;

namespace Stilargo.Core.Hill
{
    /// <summary>
    /// King-of-the-hill rules: whoever alone has living units in the zone holds it. While the player holds it,
    /// income goes to the wallet and hold time adds up toward the win; a contested or rival-held hill pays nobody.
    /// </summary>
    public sealed class HillControl
    {
        private readonly Wallet _wallet;
        private float _unpaidIncome;

        public HillControl(HillZone zone, HillSettings settings, Wallet wallet)
        {
            Zone = zone ?? throw new ArgumentNullException(nameof(zone));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        }

        public event Action<HillState> StateChanged;

        /// <summary>Raised once, the moment the player's total hold time reaches the hold duration.</summary>
        public event Action HoldCompleted;

        public HillZone Zone { get; }

        public HillSettings Settings { get; }

        public HillState State { get; private set; } = HillState.Empty;

        /// <summary>Total seconds the player has held the hill, capped at the hold duration.</summary>
        public float HeldSeconds { get; private set; }

        public float RemainingHoldSeconds => Settings.HoldDurationSeconds - HeldSeconds;

        public bool IsHoldComplete => HeldSeconds >= Settings.HoldDurationSeconds;

        /// <param name="playerUnits">Live list; dead units are skipped before their position is read.</param>
        /// <param name="rivalUnits">Live list; dead units are skipped before their position is read.</param>
        public void Tick(float deltaSeconds, IReadOnlyList<ICombatTarget> playerUnits, IReadOnlyList<ICombatTarget> rivalUnits)
        {
            if (!(deltaSeconds >= 0f) || float.IsInfinity(deltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), deltaSeconds, "Delta time must be zero or more and finite.");
            }

            if (playerUnits == null)
            {
                throw new ArgumentNullException(nameof(playerUnits));
            }

            if (rivalUnits == null)
            {
                throw new ArgumentNullException(nameof(rivalUnits));
            }

            SetState(StateFor(HasUnitInZone(playerUnits), HasUnitInZone(rivalUnits)));
            if (State == HillState.HeldByPlayer)
            {
                Hold(deltaSeconds);
            }
        }

        private static HillState StateFor(bool isPlayerPresent, bool isRivalPresent)
        {
            if (isPlayerPresent && isRivalPresent)
            {
                return HillState.Contested;
            }

            if (isPlayerPresent)
            {
                return HillState.HeldByPlayer;
            }

            return isRivalPresent ? HillState.HeldByRival : HillState.Empty;
        }

        private bool HasUnitInZone(IReadOnlyList<ICombatTarget> units)
        {
            foreach (ICombatTarget unit in units)
            {
                // IsAlive first: a destroyed Unity unit can no longer report its position.
                if (unit != null && unit.IsAlive && Zone.Contains(unit.Position))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetState(HillState state)
        {
            if (state == State)
            {
                return;
            }

            State = state;
            StateChanged?.Invoke(state);
        }

        private void Hold(float deltaSeconds)
        {
            PayIncome(deltaSeconds);
            if (IsHoldComplete)
            {
                return;
            }

            HeldSeconds = Math.Min(Settings.HoldDurationSeconds, HeldSeconds + deltaSeconds);
            if (IsHoldComplete)
            {
                HoldCompleted?.Invoke();
            }
        }

        // The wallet takes whole amounts, so fractions carry over to the next tick.
        private void PayIncome(float deltaSeconds)
        {
            _unpaidIncome += Settings.IncomeMoneyPerSecond * deltaSeconds;
            int payable = (int)Math.Floor(_unpaidIncome);
            if (payable <= 0)
            {
                return;
            }

            _unpaidIncome -= payable;
            _wallet.Deposit(payable);
        }
    }
}
