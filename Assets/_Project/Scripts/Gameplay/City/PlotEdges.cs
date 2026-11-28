using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// Lines a plot's four edges with a tileable segment model (a garden wall, a hedge), each segment running along
    /// its local X and stretched a little so whole segments fill every edge exactly.
    /// </summary>
    internal static class PlotEdges
    {
        private const float Half = 0.5f;
        private const float QuarterTurnDegrees = 90f;

        // Half the wall's thickness, so the segments stand just inside the plot.
        private const float InsetMetres = 0.3f;

        public static void Line(PropFactory props, PlotFrame frame, GameObject segment, float segmentLength)
        {
            float east = frame.West + frame.Width;
            float north = frame.South + frame.Depth;
            LineEdge(props, segment, segmentLength, new Vector3(frame.West, 0f, frame.South + InsetMetres), new Vector3(east, 0f, frame.South + InsetMetres));
            LineEdge(props, segment, segmentLength, new Vector3(frame.West, 0f, north - InsetMetres), new Vector3(east, 0f, north - InsetMetres));
            LineEdge(props, segment, segmentLength, new Vector3(frame.West + InsetMetres, 0f, frame.South), new Vector3(frame.West + InsetMetres, 0f, north));
            LineEdge(props, segment, segmentLength, new Vector3(east - InsetMetres, 0f, frame.South), new Vector3(east - InsetMetres, 0f, north));
        }

        private static void LineEdge(PropFactory props, GameObject segment, float segmentLength, Vector3 from, Vector3 to)
        {
            float length = Vector3.Distance(from, to);
            int count = Mathf.Max(1, Mathf.RoundToInt(length / segmentLength));
            float stretch = length / (count * segmentLength);
            Quaternion along = Mathf.Approximately(from.z, to.z) ? Quaternion.identity : Quaternion.Euler(0f, QuarterTurnDegrees, 0f);
            for (int index = 0; index < count; index++)
            {
                Vector3 centre = Vector3.Lerp(from, to, (index + Half) / count);
                GameObject piece = props.Model(segment, centre, along);
                piece.transform.localScale = new Vector3(stretch, 1f, 1f);
            }
        }
    }
}
