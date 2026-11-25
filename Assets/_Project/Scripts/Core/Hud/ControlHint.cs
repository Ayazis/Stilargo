using System;

namespace Stilargo.Core.Hud
{
    /// <summary>One line of the controls list: what the player wants to do and which keys do it.</summary>
    public sealed class ControlHint
    {
        public ControlHint(string action, string keys)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("A control hint needs an action.", nameof(action));
            }

            if (string.IsNullOrWhiteSpace(keys))
            {
                throw new ArgumentException("A control hint needs keys.", nameof(keys));
            }

            Action = action;
            Keys = keys;
        }

        public string Action { get; }

        public string Keys { get; }
    }
}
