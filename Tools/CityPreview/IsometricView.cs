using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Stilargo.Core.City;

namespace Stilargo.Tools.CityPreview
{
    /// <summary>
    /// Draws the city as an isometric SVG: buildings are boxes extruded by storeys with their in-game wall colour,
    /// open ground is flat colour and street props are dots. Close enough to judge massing and variety.
    /// </summary>
    internal static class IsometricView
    {
        private const double TileWidth = 14;
        private const double TileHeight = 7;
        private const double StoreyPixels = 9;

        // Same tints as Editor/World/CityLookAssets.WallColors, picked the same way the spawner does.
        private static readonly string[] WallColours = { "#d1a366", "#e0c785", "#b35c42", "#d6947a", "#e0d4b3", "#99a6ad" };
        private const string RoofColour = "#b5583a";

        private static readonly Dictionary<PlotUse, string> GroundColours = new Dictionary<PlotUse, string>
        {
            { PlotUse.Park, "#56702f" }, { PlotUse.Garden, "#6b8a3c" }, { PlotUse.Yard, "#8a7a62" },
            { PlotUse.Water, "#1f4a52" }, { PlotUse.Meadow, "#7d9a48" }, { PlotUse.Field, "#c9b25e" }, { PlotUse.Orchard, "#6e8c3a" }, { PlotUse.Fountain, "#6fa6b0" }, { PlotUse.Market, "#cbb48e" }, { PlotUse.Plaza, "#d8c6a2" },
        };

        private static readonly Dictionary<PlotUse, string> LandmarkWalls = new Dictionary<PlotUse, string>
        {
            { PlotUse.Church, "#bdb39e" }, { PlotUse.School, "#dba852" }, { PlotUse.Tavern, "#a3543d" },
        };

        private static readonly Dictionary<StreetPropKind, string> PropColours = new Dictionary<StreetPropKind, string>
        {
            { StreetPropKind.Lamp, "#222" }, { StreetPropKind.Tree, "#2f5a1e" }, { StreetPropKind.Crates, "#7a5230" },
            { StreetPropKind.Barrels, "#5a3a22" }, { StreetPropKind.Planter, "#4f7a2a" }, { StreetPropKind.Cart, "#6b4a2a" },
        };

        public static string Render(CityLayout layout, IReadOnlyList<StreetProp> props, GridPosition playerEntry, GridPosition rivalEntry)
        {
            double offsetX = layout.Height * TileWidth / 2 + 20;
            double offsetY = 6 * StoreyPixels + 40;
            double width = (layout.Width + layout.Height) * TileWidth / 2 + 40;
            double height = (layout.Width + layout.Height) * TileHeight / 2 + offsetY + 20;
            var svg = new StringBuilder();
            svg.Append(Inv($"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {width:0} {height:0}' style='background:#f2ede4;max-width:100%'>"));
            svg.Append(Polygon(Corners(0, 0, layout.Width, layout.Height, 0, offsetX, offsetY), "#a59a88"));

            // Painter's order: draw what is furthest from the viewer (low x + y) first.
            var drawables = layout.Plots.Select(plot => (Depth: plot.Area.Min.X + plot.Area.Width + plot.Area.Min.Y + plot.Area.Height, Draw: (Action)(() => DrawPlot(svg, plot, offsetX, offsetY))))
                .Concat(props.Select(prop => (Depth: prop.Cell.X + prop.Cell.Y + 1, Draw: (Action)(() => DrawProp(svg, prop, offsetX, offsetY)))))
                .OrderBy(item => item.Depth);
            foreach (var item in drawables)
            {
                item.Draw();
            }

            DrawEntryFlag(svg, playerEntry, "#2f5fd0", "Player enters", offsetX, offsetY);
            DrawEntryFlag(svg, rivalEntry, "#c8322a", "Rival enters", offsetX, offsetY);
            svg.Append("</svg>");
            return svg.ToString();
        }

        private static void DrawPlot(StringBuilder svg, Plot plot, double offsetX, double offsetY)
        {
            GridRect area = plot.Area;
            if (!plot.HasBuilding)
            {
                string colour = GroundColours.TryGetValue(plot.Use, out string ground) ? ground : "#999";
                svg.Append(Polygon(Corners(area.Min.X, area.Min.Y, area.Width, area.Height, 0, offsetX, offsetY), colour));
                if (plot.Use == PlotUse.Orchard)
                {
                    DrawOrchardTrees(svg, area, offsetX, offsetY);
                }

                return;
            }

            string wall = LandmarkWalls.TryGetValue(plot.Use, out string landmark) ? landmark : WallColours[PlotVariation.Pick(plot, WallColours.Length)];
            double top = plot.Storeys * StoreyPixels;
            double x0 = area.Min.X, y0 = area.Min.Y, x1 = x0 + area.Width, y1 = y0 + area.Height;
            // The two faces that look at the viewer: the +X side and the +Y side.
            svg.Append(Polygon(new[] { Project(x1, y0, 0, offsetX, offsetY), Project(x1, y1, 0, offsetX, offsetY), Project(x1, y1, top, offsetX, offsetY), Project(x1, y0, top, offsetX, offsetY) }, Shade(wall, 0.78)));
            svg.Append(Polygon(new[] { Project(x0, y1, 0, offsetX, offsetY), Project(x1, y1, 0, offsetX, offsetY), Project(x1, y1, top, offsetX, offsetY), Project(x0, y1, top, offsetX, offsetY) }, Shade(wall, 0.92)));
            svg.Append(Polygon(Corners(area.Min.X, area.Min.Y, area.Width, area.Height, top, offsetX, offsetY), RoofColour));
        }

        private static void DrawOrchardTrees(StringBuilder svg, GridRect area, double offsetX, double offsetY)
        {
            for (int y = area.Min.Y + 1; y < area.YMax; y += 3)
            {
                for (int x = area.Min.X + 1; x < area.XMax; x += 3)
                {
                    (double px, double py) = Project(x + 0.5, y + 0.5, 6, offsetX, offsetY);
                    svg.Append(Inv($"<circle cx='{px:0.#}' cy='{py:0.#}' r='4' fill='#3f6a26'/>"));
                }
            }
        }

        // A flag where a side's soldiers march in.
        private static void DrawEntryFlag(StringBuilder svg, GridPosition entry, string colour, string label, double offsetX, double offsetY)
        {
            (double x, double y) = Project(entry.X + 0.5, entry.Y + 0.5, 0, offsetX, offsetY);
            svg.Append(Inv($"<line x1='{x:0.#}' y1='{y:0.#}' x2='{x:0.#}' y2='{y - 30:0.#}' stroke='#222' stroke-width='1.5'/>"));
            svg.Append(Inv($"<polygon points='{x:0.#},{y - 30:0.#} {x + 16:0.#},{y - 25:0.#} {x:0.#},{y - 20:0.#}' fill='{colour}'/>"));
            svg.Append(Inv($"<text x='{x + 4:0.#}' y='{y - 34:0.#}' font-family='sans-serif' font-size='9' fill='#222'>{label}</text>"));
        }

        private static void DrawProp(StringBuilder svg, StreetProp prop, double offsetX, double offsetY)
        {
            (double x, double y) = Project(prop.Cell.X + 0.5, prop.Cell.Y + 0.5, prop.Kind == StreetPropKind.Tree ? 8 : 2, offsetX, offsetY);
            double radius = prop.Kind == StreetPropKind.Tree ? 4.5 : 2;
            svg.Append(Inv($"<circle cx='{x:0.#}' cy='{y:0.#}' r='{radius}' fill='{PropColours[prop.Kind]}'/>"));
        }

        private static (double X, double Y)[] Corners(double x, double y, double w, double h, double lift, double offsetX, double offsetY)
        {
            return new[] { Project(x, y, lift, offsetX, offsetY), Project(x + w, y, lift, offsetX, offsetY), Project(x + w, y + h, lift, offsetX, offsetY), Project(x, y + h, lift, offsetX, offsetY) };
        }

        private static (double X, double Y) Project(double x, double y, double lift, double offsetX, double offsetY)
        {
            return (offsetX + (x - y) * TileWidth / 2, offsetY + (x + y) * TileHeight / 2 - lift);
        }

        private static string Polygon((double X, double Y)[] points, string fill)
        {
            string coordinates = string.Join(" ", points.Select(p => Inv($"{p.X:0.#},{p.Y:0.#}")));
            return $"<polygon points='{coordinates}' fill='{fill}' stroke='#00000030' stroke-width='0.5'/>";
        }

        private static string Shade(string hex, double factor)
        {
            int r = (int)(Convert.ToInt32(hex.Substring(1, 2), 16) * factor);
            int g = (int)(Convert.ToInt32(hex.Substring(3, 2), 16) * factor);
            int b = (int)(Convert.ToInt32(hex.Substring(5, 2), 16) * factor);
            return $"#{r:x2}{g:x2}{b:x2}";
        }

        private static string Inv(FormattableString text)
        {
            return text.ToString(CultureInfo.InvariantCulture);
        }
    }
}
