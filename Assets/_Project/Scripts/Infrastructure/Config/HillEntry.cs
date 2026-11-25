using System;
using Stilargo.Core.Hill;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The <c>hill</c> section of <c>Hill.json</c>. Defaults match <see cref="HillSettings.Default"/>.</summary>
    [Serializable]
    public sealed class HillEntry
    {
        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private float centerXMetres = HillSettings.DefaultCenterXMetres;
        [SerializeField] private float centerZMetres = HillSettings.DefaultCenterZMetres;
        [SerializeField] private float radiusMetres = HillSettings.DefaultRadiusMetres;
        [SerializeField] private float holdDurationSeconds = HillSettings.DefaultHoldDurationSeconds;

        public HillSettings ToSettings()
        {
            try
            {
                return new HillSettings(centerXMetres, centerZMetres, radiusMetres)
                    .WithHoldDuration(holdDurationSeconds);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException($"hill: {exception.Message}", exception);
            }
        }
    }
}
