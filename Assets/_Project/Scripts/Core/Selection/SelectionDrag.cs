using System;
using System.Numerics;

namespace Stilargo.Core.Selection
{
    /// <summary>
    /// Tracks one left-mouse selection gesture and decides whether it is a click or a box drag.
    /// After <see cref="End"/> the last gesture stays readable so the caller can act on the release.
    /// </summary>
    public sealed class SelectionDrag
    {
        private readonly float _dragThresholdSquared;

        public SelectionDrag(float dragThresholdPixels)
        {
            if (!float.IsFinite(dragThresholdPixels) || dragThresholdPixels < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dragThresholdPixels), dragThresholdPixels, "Must be a finite number of zero or more.");
            }

            _dragThresholdSquared = dragThresholdPixels * dragThresholdPixels;
        }

        public bool IsActive { get; private set; }

        /// <summary>
        /// True once the pointer has moved further than the threshold from the start. It stays true even if
        /// the pointer comes back, so a drag the player started is never turned back into a click.
        /// </summary>
        public bool IsBox { get; private set; }

        public Vector2 StartPoint { get; private set; }

        public Vector2 CurrentPoint { get; private set; }

        public ScreenRect Rect => ScreenRect.FromCorners(StartPoint, CurrentPoint);

        /// <summary>
        /// Starts a new gesture. Restarting while active is allowed because a release can be lost,
        /// for example when the window loses focus mid-drag.
        /// </summary>
        public void Begin(Vector2 screenPoint)
        {
            StartPoint = screenPoint;
            CurrentPoint = screenPoint;
            IsBox = false;
            IsActive = true;
        }

        public void MoveTo(Vector2 screenPoint)
        {
            EnsureActive(nameof(MoveTo));
            CurrentPoint = screenPoint;

            if (Vector2.DistanceSquared(StartPoint, CurrentPoint) > _dragThresholdSquared)
            {
                IsBox = true;
            }
        }

        public void End()
        {
            EnsureActive(nameof(End));
            IsActive = false;
        }

        private void EnsureActive(string operation)
        {
            if (!IsActive)
            {
                throw new InvalidOperationException($"{operation} called without an active gesture; call Begin first.");
            }
        }
    }
}
