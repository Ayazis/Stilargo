using System;
using System.Collections.Generic;
using Stilargo.Core.Hud;
using UnityEngine.UIElements;

namespace Stilargo.UI.Hud
{
    /// <summary>
    /// Offers the claim until a house is claimed, then the house health and one button per fortification.
    /// </summary>
    internal sealed class FortifyPanel : VisualElement
    {
        private readonly FortifyMenu _menu;
        private readonly VisualElement _claimSection = new VisualElement();
        private readonly VisualElement _shopSection = new VisualElement();
        private readonly Label _claimHint = HudStyle.CreateLabel(FortifyMenu.ClaimHint);
        private readonly Button _claimButton;
        private readonly Label _healthLabel = HudStyle.CreateLabel(string.Empty);
        private readonly Label _messageLabel = HudStyle.CreateLabel(string.Empty);
        private readonly List<Button> _offerButtons = new List<Button>();

        public FortifyPanel(FortifyMenu menu)
        {
            _menu = menu ?? throw new ArgumentNullException(nameof(menu));
            _claimButton = HudStyle.CreateButton(FortifyMenu.ClaimLabel, () => _menu.TryClaim());

            _claimSection.Add(_claimHint);
            _claimSection.Add(_claimButton);
            _shopSection.Add(_healthLabel);
            AddOfferButtons();
            _shopSection.Add(_messageLabel);
            Add(_claimSection);
            Add(_shopSection);
        }

        public void Refresh()
        {
            bool isClaimed = _menu.IsClaimed;
            SetVisible(_claimSection, !isClaimed);
            SetVisible(_shopSection, isClaimed);

            if (isClaimed)
            {
                RefreshShop();
                return;
            }

            _claimButton.SetEnabled(_menu.CanClaim);
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
            _healthLabel.text = _menu.HouseHealthText();
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
