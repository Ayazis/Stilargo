using System;
using System.Collections.Generic;
using Stilargo.Core.Fortification;
using Stilargo.Core.Garrison;
using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>
    /// Placeholder claim-and-buy panel below the money label, until the minimal UI of plan step 11.
    /// Claiming takes the building the squad is garrisoned in; each button buys one fortification.
    /// </summary>
    public sealed class FortificationPanelView : MonoBehaviour
    {
        private const string ClaimButtonText = "Claim the house your units are in";
        private const string ClaimHintText = "Garrison units in a building to claim it.";
        private const string HealthFormat = "House: {0:0} / {1:0} HP";
        private const string BuyButtonFormat = "{0} (${1})";

        private static readonly IReadOnlyDictionary<PurchaseResult, string> ResultMessages = new Dictionary<PurchaseResult, string>
        {
            { PurchaseResult.Built, "Built." },
            { PurchaseResult.NoClaimedHouse, "Claim a house first." },
            { PurchaseResult.HouseDestroyed, "The house is destroyed." },
            { PurchaseResult.NoFreeSlot, "No free spot for that." },
            { PurchaseResult.CannotAfford, "Not enough money." },
        };

        [SerializeField] private Rect _area = new Rect(10f, 86f, 300f, 220f);

        private HouseClaim _claim;
        private FortificationShop _shop;
        private ClaimCandidatePicker _picker;
        private string _message = string.Empty;

        public void Construct(HouseClaim claim, FortificationShop shop, ClaimCandidatePicker picker)
        {
            if (_claim != null)
            {
                throw new InvalidOperationException($"{nameof(FortificationPanelView)} is already constructed.");
            }

            _claim = claim ?? throw new ArgumentNullException(nameof(claim));
            _shop = shop ?? throw new ArgumentNullException(nameof(shop));
            _picker = picker ?? throw new ArgumentNullException(nameof(picker));
        }

        private void OnGUI()
        {
            if (_claim == null)
            {
                return;
            }

            GUILayout.BeginArea(_area);
            if (_claim.IsClaimed)
            {
                DrawShop();
            }
            else
            {
                DrawClaim();
            }

            GUILayout.Label(_message);
            GUILayout.EndArea();
        }

        private void DrawClaim()
        {
            if (!_picker.TryPick(out GarrisonBuilding building))
            {
                GUILayout.Label(ClaimHintText);
                return;
            }

            if (GUILayout.Button(ClaimButtonText))
            {
                _claim.TryClaim(building);
            }
        }

        private void DrawShop()
        {
            ClaimedHouse house = _claim.House;
            GUILayout.Label(string.Format(HealthFormat, house.CurrentHealth, house.MaxHealth));
            foreach (FortificationDefinition fortification in _shop.Catalogue)
            {
                GUI.enabled = _shop.Check(fortification) == PurchaseResult.Built;
                bool isClicked = GUILayout.Button(string.Format(BuyButtonFormat, fortification.DisplayName, fortification.CostMoney));
                GUI.enabled = true;
                if (isClicked)
                {
                    _message = ResultMessages[_shop.TryBuy(fortification)];
                }
            }
        }
    }
}
