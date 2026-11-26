using System;

namespace Stilargo.Core.City
{
    /// <summary>Checks for chances and shares, which must lie between 0 and 1.</summary>
    internal static class Share
    {
        public static void Require(float value, string name)
        {
            if (!(value >= 0f && value <= 1f))
            {
                throw new ArgumentOutOfRangeException(name, value, "Must be between 0 and 1.");
            }
        }
    }
}
