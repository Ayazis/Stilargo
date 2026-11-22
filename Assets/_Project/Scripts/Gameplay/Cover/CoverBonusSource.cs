using System;
using Stilargo.Core.Combat;
using Stilargo.Core.Cover;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Units;
using UnityEngine;

namespace Stilargo.Gameplay.Cover
{
    /// <summary>Grants a unit the cover bonus of the cover point it currently stands in.</summary>
    public sealed class CoverBonusSource : IPositionBonusSource
    {
        private readonly CoverMap _map;
        private readonly Transform _transform;

        public CoverBonusSource(CoverMap map, Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            _map = map ?? throw new ArgumentNullException(nameof(map));
            _transform = transform;
        }

        public PositionBonus CurrentBonus => _map.BonusAt(_transform.position.ToNumerics());
    }
}
