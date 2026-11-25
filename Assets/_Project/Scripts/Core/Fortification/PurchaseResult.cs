namespace Stilargo.Core.Fortification
{
    /// <summary>Why a fortification purchase did or did not happen, so the UI can say so.</summary>
    public enum PurchaseResult
    {
        Built,
        NoClaimedHouse,
        HouseDestroyed,
        NoFreeSlot,
        CannotAfford
    }
}
