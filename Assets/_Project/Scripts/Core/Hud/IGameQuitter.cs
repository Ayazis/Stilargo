namespace Stilargo.Core.Hud
{
    /// <summary>Ends the running game. Hides the engine call so the menu stays testable.</summary>
    public interface IGameQuitter
    {
        void Quit();
    }
}
