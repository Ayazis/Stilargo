using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Stilargo.UI.Hud
{
    /// <summary>The one look shared by every HUD widget, so panels and buttons stay consistent.</summary>
    internal static class HudStyle
    {
        private const float PanelPaddingPixels = 8f;
        private const float WidgetGapPixels = 4f;
        private const float CornerRadiusPixels = 4f;
        private const int FontSizePixels = 18;

        private static readonly Color PanelColor = new Color(0.08f, 0.09f, 0.10f, 0.8f);
        private static readonly Color ButtonColor = new Color(0.22f, 0.24f, 0.27f, 1f);
        private static readonly Color ActiveButtonColor = new Color(0.78f, 0.58f, 0.16f, 1f);
        private static readonly Color TextColor = new Color(0.92f, 0.92f, 0.90f, 1f);

        public static VisualElement CreatePanel()
        {
            var panel = new VisualElement();
            panel.style.backgroundColor = PanelColor;
            panel.style.paddingTop = PanelPaddingPixels;
            panel.style.paddingBottom = PanelPaddingPixels;
            panel.style.paddingLeft = PanelPaddingPixels;
            panel.style.paddingRight = PanelPaddingPixels;
            panel.style.marginBottom = WidgetGapPixels;
            panel.style.borderTopLeftRadius = CornerRadiusPixels;
            panel.style.borderTopRightRadius = CornerRadiusPixels;
            panel.style.borderBottomLeftRadius = CornerRadiusPixels;
            panel.style.borderBottomRightRadius = CornerRadiusPixels;
            return panel;
        }

        public static Label CreateLabel(string text)
        {
            var label = new Label(text);
            label.style.color = TextColor;
            label.style.fontSize = FontSizePixels;
            label.pickingMode = PickingMode.Ignore;
            return label;
        }

        public static Button CreateButton(string text, Action onClick)
        {
            var button = new Button(onClick) { text = text };
            button.style.color = TextColor;
            button.style.fontSize = FontSizePixels;
            button.style.marginTop = WidgetGapPixels;
            button.style.marginRight = WidgetGapPixels;
            button.style.backgroundColor = ButtonColor;
            return button;
        }

        public static void MarkActive(Button button, bool isActive)
        {
            button.style.backgroundColor = isActive ? ActiveButtonColor : ButtonColor;
        }
    }
}
