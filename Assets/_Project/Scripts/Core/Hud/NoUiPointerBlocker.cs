namespace Stilargo.Core.Hud
{
    /// <summary>The blocker used while there is no HUD: the pointer is never over it.</summary>
    public sealed class NoUiPointerBlocker : IUiPointerBlocker
    {
        public bool IsPointerOverUi => false;
    }
}
