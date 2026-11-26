using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.Input
{
    /// <summary>The camera preset the player has picked in the settings menu; input and hints follow it live.</summary>
    public sealed class CameraControlChoice
    {
        private readonly List<CameraControlPreset> _presets;
        private int _index;

        public CameraControlChoice()
            : this(CameraControlPreset.All)
        {
        }

        public CameraControlChoice(IReadOnlyList<CameraControlPreset> presets)
        {
            if (presets == null || presets.Count == 0 || presets.Any(preset => preset == null))
            {
                throw new ArgumentException("Give at least one preset and no empty ones.", nameof(presets));
            }

            _presets = presets.ToList();
        }

        public event Action Changed;

        public CameraControlPreset Current => _presets[_index];

        public IReadOnlyList<CameraControlPreset> Presets => _presets;

        /// <summary>Switches to the next preset, wrapping round after the last.</summary>
        public void Next()
        {
            _index = (_index + 1) % _presets.Count;
            Changed?.Invoke();
        }
    }
}
