using System;
using Stilargo.Core.Fog;

namespace Stilargo.Gameplay.Garrison
{
    /// <summary>Extra fog reveal for a unit standing on a roof: its own sight plus the roof's bonus, from where it stands.</summary>
    public sealed class RooftopVision : IVisionSource
    {
        private readonly IVisionSource _ownSight;
        private readonly float _bonusMetres;

        public RooftopVision(IVisionSource ownSight, float bonusMetres)
        {
            _ownSight = ownSight ?? throw new ArgumentNullException(nameof(ownSight));
            _bonusMetres = bonusMetres;
        }

        public WorldPoint Position => _ownSight.Position;

        public float VisionRadius => _ownSight.VisionRadius + _bonusMetres;

        public float EyeHeightMetres => _ownSight.EyeHeightMetres;
    }
}
