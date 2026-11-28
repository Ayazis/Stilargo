using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>Turns a grid <see cref="Facing"/> into the rotation that points a Blender-built model's front that way.</summary>
    internal static class FacingRotation
    {
        // Imported from Blender, a model's front faces +Z (grid north) without rotation.
        private const float QuarterTurn = 90f;

        public static Quaternion Of(Facing facing)
        {
            switch (facing)
            {
                case Facing.East:
                    return Quaternion.Euler(0f, QuarterTurn, 0f);
                case Facing.South:
                    return Quaternion.Euler(0f, QuarterTurn * 2f, 0f);
                case Facing.West:
                    return Quaternion.Euler(0f, QuarterTurn * 3f, 0f);
                default:
                    return Quaternion.identity;
            }
        }
    }
}
