using System;
using Stilargo.Core.Hud;
using UnityEngine;
using UnityEngine.UIElements;

namespace Stilargo.UI.Hud
{
    /// <summary>
    /// Full-screen Esc menu: a dimmed backdrop that swallows clicks, with the main page and the settings page.
    /// While open it stops game time, so units and timers freeze.
    /// </summary>
    internal sealed class PauseMenuPanel : VisualElement
    {
        private const float PausedTimeScale = 0f;
        private const float PanelWidthPixels = 320f;

        private static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.55f);

        private readonly PauseMenu _menu;
        private readonly VisualElement _mainPage = new VisualElement();
        private readonly VisualElement _settingsPage = new VisualElement();
        private readonly TextField _moneyField = new TextField(PauseMenu.MoneyFieldLabel);
        private readonly Label _settingsMessage = HudStyle.CreateLabel(string.Empty);
        private float _timeScaleBeforePause = 1f;

        public PauseMenuPanel(PauseMenu menu)
        {
            _menu = menu ?? throw new ArgumentNullException(nameof(menu));
            StyleBackdrop();
            VisualElement panel = HudStyle.CreatePanel();
            panel.style.width = PanelWidthPixels;
            BuildMainPage();
            BuildSettingsPage();
            panel.Add(_mainPage);
            panel.Add(_settingsPage);
            Add(panel);
            _menu.OpenChanged += OnOpenChanged;
            Refresh();
        }

        public void Refresh()
        {
            style.display = _menu.IsOpen ? DisplayStyle.Flex : DisplayStyle.None;
            SetVisible(_mainPage, _menu.Page == PauseMenuPage.Main);
            SetVisible(_settingsPage, _menu.Page == PauseMenuPage.Settings);
        }

        private void StyleBackdrop()
        {
            style.position = Position.Absolute;
            style.left = 0;
            style.right = 0;
            style.top = 0;
            style.bottom = 0;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            style.backgroundColor = BackdropColor;
        }

        private void BuildMainPage()
        {
            _mainPage.Add(HudStyle.CreateLabel(PauseMenu.Title));
            _mainPage.Add(HudStyle.CreateButton(PauseMenu.ResumeLabel, _menu.Resume));
            _mainPage.Add(HudStyle.CreateButton(PauseMenu.SettingsLabel, OpenSettings));
            _mainPage.Add(HudStyle.CreateButton(PauseMenu.QuitLabel, _menu.Quit));
        }

        private void BuildSettingsPage()
        {
            _settingsPage.Add(HudStyle.CreateLabel(PauseMenu.SettingsTitle));
            _settingsPage.Add(_moneyField);
            _settingsPage.Add(HudStyle.CreateButton(PauseMenu.ApplyMoneyLabel, ApplyMoney));
            _settingsPage.Add(_settingsMessage);
            _settingsPage.Add(HudStyle.CreateButton(PauseMenu.BackLabel, _menu.ShowMain));
        }

        private void OpenSettings()
        {
            _menu.ShowSettings();
            _moneyField.value = _menu.CurrentMoney.ToString();
            _settingsMessage.text = string.Empty;
        }

        private void ApplyMoney()
        {
            _settingsMessage.text = _menu.ApplyMoney(_moneyField.value);
        }

        private void OnOpenChanged(bool isOpen)
        {
            if (isOpen)
            {
                _timeScaleBeforePause = Time.timeScale;
                Time.timeScale = PausedTimeScale;
                return;
            }

            Time.timeScale = _timeScaleBeforePause;
        }

        private static void SetVisible(VisualElement element, bool isVisible)
        {
            element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
