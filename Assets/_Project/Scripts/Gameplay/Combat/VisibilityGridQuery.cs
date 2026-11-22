using System;
using Stilargo.Core.Combat;
using Stilargo.Core.Fog;

namespace Stilargo.Gameplay.Combat
{
    /// <summary>Lets the player's units see only what FL's fog of war currently shows.</summary>
    public sealed class VisibilityGridQuery : IVisibilityQuery
    {
        private readonly VisibilityGrid _grid;

        public VisibilityGridQuery(VisibilityGrid grid)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        }

        public bool IsVisible(System.Numerics.Vector3 position)
        {
            return _grid.IsVisible(new WorldPoint(position.X, position.Z));
        }
    }
}
