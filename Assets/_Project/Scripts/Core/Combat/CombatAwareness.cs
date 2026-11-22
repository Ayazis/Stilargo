using System;
using System.Numerics;

namespace Stilargo.Core.Combat
{
    /// <summary>How far a unit notices enemies, in metres on the ground plane, and whether its side can see them.</summary>
    public sealed class CombatAwareness
    {
        private readonly IVisibilityQuery _visibility;
        private float _sightBonus;

        public CombatAwareness(float sightRange)
            : this(sightRange, new AlwaysVisible())
        {
        }

        public CombatAwareness(float sightRange, IVisibilityQuery visibility)
        {
            if (!(sightRange > 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(sightRange), sightRange, "Sight range must be positive.");
            }

            SightRange = sightRange;
            _visibility = visibility ?? throw new ArgumentNullException(nameof(visibility));
        }

        /// <summary>Base sight range, without position bonus.</summary>
        public float SightRange { get; }

        public float EffectiveSightRange => SightRange + _sightBonus;

        public void SetSightBonus(float metres)
        {
            _sightBonus = BonusMetres.Require(metres, nameof(metres));
        }

        public bool CanSee(Vector3 from, Vector3 to)
        {
            if (GroundDistance.Between(from, to) > EffectiveSightRange)
            {
                return false;
            }

            return _visibility.IsVisible(to);
        }
    }
}
