using System;
using Stilargo.Core.Hud;
using Stilargo.Core.Input;
using UnityEngine;
using UnityEngine.UIElements;

namespace Stilargo.UI.Hud
{
    /// <summary>
    /// The player's HUD, built in code with UI Toolkit: money, the hill timer, stance buttons and the
    /// claim-and-fortify menu, plus the Esc pause menu. It only reads Core state each frame and forwards button clicks to Core menus.
    /// </summary>
    public sealed class HudView : MonoBehaviour, IUiPointerBlocker
    {
        private const float EdgeMarginPixels = 10f;
        private const float TopOffsetPixels = 40f;

        [SerializeField] private PanelSettings _panelSettings;

        private IPlayerInput _input;
        private HudContent _content;
        private VisualElement _root;
        private Label _moneyLabel;
        private Label _hillLabel;
        private StanceBar _stanceBar;
        private FortifyPanel _fortifyPanel;
        private PauseMenu _pauseMenu;
        private PauseMenuPanel _pauseMenuPanel;
        private int _shownBalance = -1;
        private int _shownHillSeconds = -1;

        public bool IsPointerOverUi
        {
            get
            {
                if (_root == null)
                {
                    return false;
                }

                System.Numerics.Vector2 pointer = _input.PointerScreenPosition;
                var screenPoint = new Vector2(pointer.X, Screen.height - pointer.Y);
                Vector2 panelPoint = RuntimePanelUtils.ScreenToPanel(_root.panel, screenPoint);
                return _root.panel.Pick(panelPoint) != null;
            }
        }

        public void Construct(HudContent content, IPlayerInput input, PauseMenu pauseMenu)
        {
            if (_root != null)
            {
                throw new InvalidOperationException($"{nameof(HudView)} is already constructed.");
            }

            _content = content ?? throw new ArgumentNullException(nameof(content));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _pauseMenu = pauseMenu ?? throw new ArgumentNullException(nameof(pauseMenu));
            _root = CreateRoot();
            BuildWidgets();
        }

        private void Update()
        {
            if (_root == null)
            {
                return;
            }

            if (_input.WasPauseMenuPressedThisFrame)
            {
                _pauseMenu.HandleEscape();
            }

            _pauseMenuPanel.Refresh();
            RefreshMoney();
            RefreshHill();
            _stanceBar.Refresh();
            _fortifyPanel.Refresh();
        }

        private VisualElement CreateRoot()
        {
            if (_panelSettings == null)
            {
                throw new InvalidOperationException($"{nameof(HudView)} on '{name}' has no PanelSettings assigned.");
            }

            var document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = _panelSettings;
            if (document.rootVisualElement == null)
            {
                throw new InvalidOperationException($"{nameof(HudView)} on '{name}' got no UI root from its UIDocument.");
            }

            return document.rootVisualElement;
        }

        private void BuildWidgets()
        {
            var column = new VisualElement();
            column.pickingMode = PickingMode.Ignore;
            column.style.position = Position.Absolute;
            column.style.left = EdgeMarginPixels;
            column.style.top = TopOffsetPixels;

            _moneyLabel = AddPanelWith(column, HudStyle.CreateLabel(string.Empty));
            _hillLabel = AddPanelWith(column, HudStyle.CreateLabel(string.Empty));
            _stanceBar = AddPanelWith(column, new StanceBar(_content.StanceMenu));
            _fortifyPanel = AddPanelWith(column, new FortifyPanel(_content.FortifyMenu));
            _root.Add(column);
            _pauseMenuPanel = new PauseMenuPanel(_pauseMenu);
            _root.Add(_pauseMenuPanel);
        }

        private static T AddPanelWith<T>(VisualElement column, T content) where T : VisualElement
        {
            VisualElement panel = HudStyle.CreatePanel();
            panel.Add(content);
            column.Add(panel);
            return content;
        }

        private void RefreshMoney()
        {
            int balance = _content.Wallet.Balance;
            if (balance == _shownBalance)
            {
                return;
            }

            _shownBalance = balance;
            _moneyLabel.text = HudText.Money(balance);
        }

        private void RefreshHill()
        {
            IHillStatus hill = _content.HillStatus;
            _hillLabel.parent.style.display = hill.IsActive ? DisplayStyle.Flex : DisplayStyle.None;
            if (!hill.IsActive)
            {
                return;
            }

            int wholeSeconds = Mathf.CeilToInt(hill.SecondsRemaining);
            if (wholeSeconds == _shownHillSeconds)
            {
                return;
            }

            _shownHillSeconds = wholeSeconds;
            _hillLabel.text = HudText.HillTimer(hill.SecondsRemaining);
        }
    }
}
