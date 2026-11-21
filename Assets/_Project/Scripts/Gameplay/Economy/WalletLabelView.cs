using System;
using Stilargo.Core.Economy;
using UnityEngine;

namespace Stilargo.Gameplay.Economy
{
    /// <summary>
    /// Shows the player's money below the stance label. Placeholder until the minimal UI (plan step 11).
    /// </summary>
    public sealed class WalletLabelView : MonoBehaviour
    {
        private const string LabelFormat = "Money: {0}";

        [SerializeField] private Rect _area = new Rect(10f, 58f, 420f, 24f);

        private Wallet _wallet;
        private string _label;

        public void Construct(Wallet wallet)
        {
            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (_wallet != null)
            {
                throw new InvalidOperationException($"{nameof(WalletLabelView)} is already constructed.");
            }

            _wallet = wallet;
            UpdateLabel(_wallet.Balance);
            _wallet.BalanceChanged += UpdateLabel;
        }

        private void OnDestroy()
        {
            if (_wallet == null)
            {
                return;
            }

            _wallet.BalanceChanged -= UpdateLabel;
        }

        private void OnGUI()
        {
            if (_wallet == null)
            {
                return;
            }

            GUI.Label(_area, _label);
        }

        // Formats only when the balance changes, so OnGUI does not allocate a string every call.
        private void UpdateLabel(int balance)
        {
            _label = string.Format(LabelFormat, balance);
        }
    }
}
