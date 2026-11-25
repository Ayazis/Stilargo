using System;
using Stilargo.Gameplay.Composition;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The shape of <c>Hill.json</c>: the hill zone and the AI rival that contests it.</summary>
    [Serializable]
    public sealed class HillConfig
    {
        public const string FileName = "Hill.json";

        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private HillEntry hill = new HillEntry();
        [SerializeField] private RivalEntry rival = new RivalEntry();

        public static HillRivalSettings Load()
        {
            return ConfigFile.Read<HillConfig>(FileName).ToSettings();
        }

        public HillRivalSettings ToSettings()
        {
            try
            {
                return new HillRivalSettings(hill.ToSettings(), rival.ToSettings());
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"{FileName} is invalid: {exception.Message}", exception);
            }
        }
    }
}
