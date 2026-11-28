using System;
using System.Collections.Generic;
using Stilargo.Core.Fog;

namespace Stilargo.Core.City
{
    /// <summary>The city's roofs, so things placed on a roof (turrets) can sit on its tiles instead of inside it.</summary>
    public sealed class RoofHeights
    {
        private readonly List<IRoofSurface> _roofs = new List<IRoofSurface>();

        public void Add(IRoofSurface roof)
        {
            _roofs.Add(roof ?? throw new ArgumentNullException(nameof(roof)));
        }

        /// <summary>
        /// The highest roof point under a square deck of the given half size, so a level deck there clears the tiles.
        /// Returns <paramref name="fallbackHeight"/> where no roof covers the deck.
        /// </summary>
        public float HighestUnder(WorldPoint centre, float halfSizeMetres, float fallbackHeight)
        {
            if (!(halfSizeMetres >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(halfSizeMetres), halfSizeMetres, "Deck half size must not be negative.");
            }

            float highest = float.NegativeInfinity;
            foreach (WorldPoint point in DeckPoints(centre, halfSizeMetres))
            {
                foreach (IRoofSurface roof in _roofs)
                {
                    if (roof.TryGetHeight(point, out float height) && height > highest)
                    {
                        highest = height;
                    }
                }
            }

            return float.IsNegativeInfinity(highest) ? fallbackHeight : highest;
        }

        // A 3x3 grid over the deck: corners, edge middles and centre. A ridge crossing the deck between samples is
        // missed by at most a few centimetres on a deck this small, which the deck's thickness hides.
        private static IEnumerable<WorldPoint> DeckPoints(WorldPoint centre, float half)
        {
            for (int row = -1; row <= 1; row++)
            {
                for (int column = -1; column <= 1; column++)
                {
                    yield return new WorldPoint(centre.X + (column * half), centre.Z + (row * half));
                }
            }
        }
    }
}
