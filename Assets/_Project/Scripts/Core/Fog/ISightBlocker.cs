namespace Stilargo.Core.Fog
{
    /// <summary>What stands in the way of sight, per fog cell: the height of its obstacle in metres (0 for open ground).</summary>
    public interface ISightBlocker
    {
        float ObstacleHeightAt(int cellX, int cellY);
    }
}
