namespace Stilargo.Gameplay.City
{
    /// <summary>What makes one kind of open ground look like itself: anything built on and around it.</summary>
    internal interface IPlotDressing
    {
        void Dress(PlotFrame frame, PropFactory props);
    }
}
