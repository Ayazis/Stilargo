using System;
using System.Collections.Generic;
using Stilargo.Core.Hud;
using UnityEngine.UIElements;

namespace Stilargo.UI.Hud
{
    /// <summary>
    /// Offers a claim while the squad stands in an unclaimed building. Once a house is claimed it shows the total
    /// income, the selected house's health and upkeep, a repair button and one button per fortification.
    /// </summary>
    internal sealed class FortifyPanel : VisualElement
    {
        private readonly FortifyMenu _menu;
        private readonly VisualElement _claimSection = new VisualElement();
        private readonly VisualElement _shopSection = new VisualElement();
        private readonly Label _claimHint = HudStyle.CreateLabel(FortifyMenu.ClaimHint);
        private readonly Button _claimButton;
        private readonly Label _incomeLabel = HudStyle.CreateLabel(string.Empty);
        private readonly Label _healthLabel = HudStyle.CreateLabel(string.Empty);
        private readonly Label _upkeepLabel = HudStyle.CreateLabel(string.Empty);
        private readonly Button _repairButton;
        private readonly Label _messageLabel = HudStyle.CreateLabel(string.Empty);
        private readonly List<Button> _offerButtons = new List<Button>();

        public FortifyPanel(FortifyMenu menu)
        {
            _menu = menu ?? throw new ArgumentNullException(nameof(menu));
            _claimButton = HudStyle.CreateButton(FortifyMenu.ClaimLabel, () => _menu.TryClaim());
            _repairButton = HudStyle.CreateButton(UpkeepMenu.NothingToRepairLabel, () => _menu.Upkeep.Repair());

            _claimSection.Add(_claimHint);
            _claimSection.Add(_claimButton);
            _shopSection.Add(_incomeLabel);
            _shopSection.Add(_healthLabel);
            _shopSection.Add(_upkeepLabel);
            _shopSection.Add(_repairButton);
            AddOfferButtons();
            _shopSection.Add(_messageLabel);
            Add(_shopSection);
            Add(_claimSection);
        }

        public void Refresh()
        {
            bool isClaimed = _menu.IsClaimed;
            bool canClaim = _menu.CanClaim;
            SetVisible(_claimSection, !isClaimed || canClaim);
            SetVisible(_claimHint, !isClaimed);
            SetVisible(_shopSection, isClaimed);
            _claimButton.SetEnabled(canClaim);

            if (isClaimed)
            {
                RefreshShop();
            }
        }

        private void AddOfferButtons()
        {
            for (int index = 0; index < _menu.OfferCount; index++)
            {
                int offerIndex = index;
                Button button = HudStyle.CreateButton(_menu.OfferLabel(offerIndex), () => _messageLabel.text = _menu.Buy(offerIndex));
                _offerButtons.Add(button);
                _shopSection.Add(button);
            }
        }

        private void RefreshShop()
        {
            UpkeepMenu upkeep = _menu.Upkeep;
            _incomeLabel.text = upkeep.IncomeText();
            _healthLabel.text = _menu.HouseHealthText();
            _upkeepLabel.text = upkeep.HouseText();
            _repairButton.text = upkeep.RepairLabel();
            _repairButton.SetEnabled(upkeep.CanRepair());
            for (int index = 0; index < _offerButtons.Count; index++)
            {
                _offerButtons[index].SetEnabled(_menu.IsOfferAvailable(index));
            }
        }

        private static void SetVisible(VisualElement element, bool isVisible)
        {
            element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
