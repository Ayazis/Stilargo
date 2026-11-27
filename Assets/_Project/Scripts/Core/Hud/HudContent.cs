using System;
using Stilargo.Core.Economy;

namespace Stilargo.Core.Hud
{
    /// <summary>Everything the HUD shows. The hill is optional until step 10 adds one.</summary>
    public sealed class HudContent
    {
        public HudContent(Wallet wallet, StanceMenu stanceMenu, FortifyMenu fortifyMenu)
        {
            Wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            StanceMenu = stanceMenu ?? throw new ArgumentNullException(nameof(stanceMenu));
            FortifyMenu = fortifyMenu ?? throw new ArgumentNullException(nameof(fortifyMenu));
            HillStatus = new NoHillStatus();
        }

        public Wallet Wallet { get; }

        public StanceMenu StanceMenu { get; }

        public FortifyMenu FortifyMenu { get; }

        public IHillStatus HillStatus { get; private set; }

        /// <summary>The settings page's play options; null until <see cref="WithOptions"/> is called.</summary>
        public GameOptions Options { get; private set; }

        public HudContent WithOptions(GameOptions options)
        {
            HudContent copy = (HudContent)MemberwiseClone();
            copy.Options = options ?? throw new ArgumentNullException(nameof(options));
            return copy;
        }

        public HudContent WithHillStatus(IHillStatus hillStatus)
        {
            HudContent copy = (HudContent)MemberwiseClone();
            copy.HillStatus = hillStatus ?? throw new ArgumentNullException(nameof(hillStatus));
            return copy;
        }
    }
}
