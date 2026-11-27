using System;
using System.Numerics;

namespace Stilargo.Core.Input
{
    /// <summary>
    /// Tells a click from a drag on one mouse button. While held, it becomes a drag once the pointer has moved
    /// further than the threshold; a release that never became a drag is a click. This lets the right mouse
    /// button orbit the camera and still give orders.
    /// </summary>
    public sealed class DragOrClick
    {
        private readonly float _thresholdPixels;
        private Vector2 _pressedAt;
        private bool _wasHeld;

        public DragOrClick(float thresholdPixels)
        {
            if (!(thresholdPixels >= 0f) || float.IsInfinity(thresholdPixels))
            {
                throw new ArgumentOutOfRangeException(nameof(thresholdPixels), thresholdPixels, "Threshold must be a non-negative number.");
            }

            _thresholdPixels = thresholdPixels;
        }

        /// <summary>True while the button is held and the pointer has moved past the threshold since the press.</summary>
        public bool IsDragging { get; private set; }

        /// <summary>True for the one update in which the button was released without dragging.</summary>
        public bool WasClicked { get; private set; }

        /// <summary>Call once per frame with the button's state and the pointer position.</summary>
        public void Update(bool isHeld, Vector2 pointer)
        {
            WasClicked = false;
            if (isHeld && !_wasHeld)
            {
                _pressedAt = pointer;
                IsDragging = false;
            }
            else if (isHeld && Vector2.Distance(pointer, _pressedAt) > _thresholdPixels)
            {
                IsDragging = true;
            }
            else if (!isHeld && _wasHeld)
            {
                WasClicked = !IsDragging;
                IsDragging = false;
            }

            _wasHeld = isHeld;
        }
    }
}
