using System;
using System.Collections.Generic;
using Stilargo.Core.Combat.Stances;
using Stilargo.Core.Hud;
using UnityEngine.UIElements;

namespace Stilargo.UI.Hud
{
    /// <summary>One button per stance for the selected units; the shared stance shows as pressed.</summary>
    internal sealed class StanceBar : VisualElement
    {
        private const string TitleText = "Stance (Z / X / C)";

        private readonly StanceMenu _menu;
        private readonly List<Button> _buttons = new List<Button>();

        public StanceBar(StanceMenu menu)
        {
            _menu = menu ?? throw new ArgumentNullException(nameof(menu));
            Add(HudStyle.CreateLabel(TitleText));

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            foreach (IStance stance in menu.Options)
            {
                IStance chosen = stance;
                Button button = HudStyle.CreateButton(chosen.Name, () => _menu.Choose(chosen));
                _buttons.Add(button);
                row.Add(button);
            }

            Add(row);
        }

        public void Refresh()
        {
            for (int index = 0; index < _buttons.Count; index++)
            {
                _buttons[index].SetEnabled(_menu.HasSelection);
                HudStyle.MarkActive(_buttons[index], _menu.IsActive(_menu.Options[index]));
            }
        }
    }
}
