using System;
using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.Selection
{
    /// <summary>
    /// Finds the selectables whose on-screen position falls inside a drag rectangle.
    /// </summary>
    public static class BoxSelection
    {
        public static IReadOnlyList<ISelectable> Within(
            ScreenRect rect, IEnumerable<ISelectable> candidates, IScreenProjector projector)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            if (projector == null)
            {
                throw new ArgumentNullException(nameof(projector));
            }

            var inside = new List<ISelectable>();
            foreach (ISelectable candidate in candidates)
            {
                if (candidate == null)
                {
                    throw new ArgumentException("Candidates must not contain null.", nameof(candidates));
                }

                if (projector.TryProject(candidate.Position, out Vector2 screen) && rect.Contains(screen))
                {
                    inside.Add(candidate);
                }
            }

            return inside;
        }
    }
}
