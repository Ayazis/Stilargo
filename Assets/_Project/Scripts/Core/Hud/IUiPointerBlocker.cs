namespace Stilargo.Core.Hud
{
    /// <summary>
    /// Says whether the pointer is over the HUD, so a click on a button is not also read as a click in the world.
    /// </summary>
    public interface IUiPointerBlocker
    {
        bool IsPointerOverUi { get; }
    }
}
