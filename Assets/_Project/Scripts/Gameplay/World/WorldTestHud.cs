using System;
using Stilargo.Core.Economy;
using Stilargo.Core.Scavenging;
using UnityEngine;

namespace Stilargo.Gameplay.World
{
    /// <summary>
    /// Temporary on-screen readout for the WorldTest scene until the real UI (plan step 11) exists.
    /// Delete it with the WorldTest scene.
    /// </summary>
    public sealed class WorldTestHud : MonoBehaviour
    {
        private static readonly Rect LabelArea = new Rect(12f, 12f, 400f, 60f);
        private const int FontSize = 22;

        private Wallet _wallet;
        private LootCarrier _carrier;
        private GUIStyle _style;

        public void Construct(Wallet wallet, LootCarrier carrier)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _carrier = carrier ?? throw new ArgumentNullException(nameof(carrier));
        }

        private void OnGUI()
        {
            if (_wallet == null)
            {
                return;
            }

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label) { fontSize = FontSize };
                _style.normal.textColor = Color.white;
            }

            GUI.Label(LabelArea, $"Money: {_wallet.Balance}\nCarrying: {_carrier.CarriedValue}", _style);
        }
    }
}
