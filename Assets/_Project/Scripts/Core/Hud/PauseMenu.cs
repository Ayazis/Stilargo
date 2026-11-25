using System;
using System.Globalization;
using Stilargo.Core.Economy;

namespace Stilargo.Core.Hud
{
    /// <summary>
    /// The Esc menu: opens and closes, switches between the main page and settings, sets the player's money
    /// and quits. The view only shows this state and forwards clicks.
    /// </summary>
    public sealed class PauseMenu
    {
        public const string Title = "Paused";
        public const string SettingsTitle = "Settings";
        public const string ResumeLabel = "Resume";
        public const string SettingsLabel = "Settings";
        public const string QuitLabel = "Quit game";
        public const string BackLabel = "Back";
        public const string MoneyFieldLabel = "Money";
        public const string ApplyMoneyLabel = "Set money";
        public const string InvalidMoneyMessage = "Enter a whole number of 0 or more.";

        private readonly Wallet _wallet;
        private readonly IGameQuitter _quitter;

        public PauseMenu(Wallet wallet, IGameQuitter quitter)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _quitter = quitter ?? throw new ArgumentNullException(nameof(quitter));
        }

        public event Action<bool> OpenChanged;

        public bool IsOpen { get; private set; }

        public PauseMenuPage Page { get; private set; } = PauseMenuPage.Main;

        public int CurrentMoney => _wallet.Balance;

        /// <summary>Esc closes settings first, so the player backs out one level at a time.</summary>
        public void HandleEscape()
        {
            if (IsOpen && Page == PauseMenuPage.Settings)
            {
                ShowMain();
                return;
            }

            SetOpen(!IsOpen);
        }

        public void Resume()
        {
            SetOpen(false);
        }

        public void ShowSettings()
        {
            RequireOpen();
            Page = PauseMenuPage.Settings;
        }

        public void ShowMain()
        {
            RequireOpen();
            Page = PauseMenuPage.Main;
        }

        /// <summary>Sets the money from what the player typed; returns the message to show.</summary>
        public string ApplyMoney(string typedAmount)
        {
            if (!int.TryParse(typedAmount, NumberStyles.Integer, CultureInfo.InvariantCulture, out int amount) || amount < 0)
            {
                return InvalidMoneyMessage;
            }

            _wallet.SetBalance(amount);
            return $"Money set to {amount}.";
        }

        public void Quit()
        {
            _quitter.Quit();
        }

        private void SetOpen(bool isOpen)
        {
            Page = PauseMenuPage.Main;
            if (IsOpen == isOpen)
            {
                return;
            }

            IsOpen = isOpen;
            OpenChanged?.Invoke(IsOpen);
        }

        private void RequireOpen()
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException("The pause menu is closed.");
            }
        }
    }
}
