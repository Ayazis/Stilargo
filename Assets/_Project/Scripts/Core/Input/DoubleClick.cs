using System;

namespace Stilargo.Core.Input
{
    /// <summary>
    /// Recognises a second click on the same target within a short time. Garrisoning needs one, so a single stray
    /// right-click on a building no longer sends the squad inside.
    /// </summary>
    public sealed class DoubleClick
    {
        public const float DefaultWindowSeconds = 0.4f;

        private readonly float _windowSeconds;
        private object _lastTarget;
        private float _lastClickSeconds = float.NegativeInfinity;

        public DoubleClick(float windowSeconds)
        {
            if (!(windowSeconds > 0f) || float.IsInfinity(windowSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(windowSeconds), windowSeconds, "The window must be a positive number.");
            }

            _windowSeconds = windowSeconds;
        }

        /// <summary>Records a click and returns true when it completes a double click on <paramref name="target"/>.</summary>
        public bool Register(object target, float timeSeconds)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            bool isDouble = ReferenceEquals(target, _lastTarget) && timeSeconds - _lastClickSeconds <= _windowSeconds;

            // A completed double click starts over, so a third click is a new first click.
            _lastTarget = isDouble ? null : target;
            _lastClickSeconds = timeSeconds;
            return isDouble;
        }
    }
}
