using System;
using Stilargo.Core.Combat;
using UnityEngine;

namespace Stilargo.Gameplay.Combat
{
    /// <summary>
    /// What a health bar shows for one unit: where it floats, how full it is and whose side it is on.
    /// A bar is shown only while the player can see the unit, so the fog of war keeps hiding enemies.
    /// </summary>
    public sealed class HealthBarSource
    {
        // The avatar is about 1.8 m tall and its pivot sits a metre up, so this puts the bar just above the head.
        private const float AnchorAbovePivotMetres = 1.25f;

        private readonly CombatUnit _unit;
        private readonly IVisibilityQuery _visibility;

        public HealthBarSource(CombatUnit unit, IVisibilityQuery visibility, bool isHostile)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            _unit = unit;
            _visibility = visibility ?? throw new ArgumentNullException(nameof(visibility));
            IsHostile = isHostile;
        }

        public bool IsHostile { get; }

        public Vector3 Anchor => _unit.transform.position + (Vector3.up * AnchorAbovePivotMetres);

        public float HealthFraction => _unit.HealthFraction;

        public bool IsShown => _unit != null && _unit.IsAlive && _visibility.IsVisible(_unit.Position);
    }
}
