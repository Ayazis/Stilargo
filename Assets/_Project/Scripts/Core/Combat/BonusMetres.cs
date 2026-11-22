using System;

namespace Stilargo.Core.Combat
{
    /// <summary>Validation shared by range and sight bonuses, in metres.</summary>
    internal static class BonusMetres
    {
        public static float Require(float metres, string name)
        {
            if (!(metres >= 0f) || float.IsInfinity(metres))
            {
                throw new ArgumentOutOfRangeException(name, metres, "Bonus must be a non-negative finite number.");
            }

            return metres;
        }
    }
}
