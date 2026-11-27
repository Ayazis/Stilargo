namespace Stilargo.Core.City
{
    /// <summary>Which open ground a neighbourhood mixes in between its houses, and for what share (0–1) of its plots.</summary>
    public readonly struct OpenGroundShare
    {
        public OpenGroundShare(PlotUse use, float share)
        {
            Share.Require(share, nameof(share));
            Use = use;
            Value = share;
        }

        public static OpenGroundShare None => new OpenGroundShare(PlotUse.Garden, 0f);

        public PlotUse Use { get; }

        public float Value { get; }
    }
}
