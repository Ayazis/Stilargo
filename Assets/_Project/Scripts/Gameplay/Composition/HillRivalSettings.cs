using System;
using Stilargo.Core.Hill;
using Stilargo.Core.Rival;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>The two sections of <c>Hill.json</c>, kept together because they come from one file.</summary>
    public sealed class HillRivalSettings
    {
        public HillRivalSettings(HillSettings hill, RivalSettings rival)
        {
            Hill = hill ?? throw new ArgumentNullException(nameof(hill));
            Rival = rival ?? throw new ArgumentNullException(nameof(rival));
        }

        public HillSettings Hill { get; }

        public RivalSettings Rival { get; }
    }
}
