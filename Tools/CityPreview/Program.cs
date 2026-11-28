using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Stilargo.Core.City;
using Stilargo.Core.City.CanalTown;

namespace Stilargo.Tools.CityPreview
{
    /// <summary>
    /// Writes the city the game would generate for a seed as a Markdown text map.
    /// Usage: dotnet run -- [seed] [outputPath]. Uses the CityConfig asset's values and TownSettings defaults.
    /// </summary>
    public static class Program
    {
        private static readonly Dictionary<PlotUse, char> UseGlyphs = new Dictionary<PlotUse, char>
        {
            { PlotUse.Church, 'C' }, { PlotUse.School, 'S' }, { PlotUse.Tavern, 'T' }, { PlotUse.Park, '"' },
            { PlotUse.Water, '~' }, { PlotUse.Market, 'M' }, { PlotUse.Plaza, 'P' }, { PlotUse.Garden, 'g' },
            { PlotUse.Yard, 'y' },
        };

        private static readonly Dictionary<StreetPropKind, char> PropGlyphs = new Dictionary<StreetPropKind, char>
        {
            { StreetPropKind.Lamp, 'i' }, { StreetPropKind.Tree, 't' }, { StreetPropKind.Crates, 'x' },
            { StreetPropKind.Barrels, 'o' }, { StreetPropKind.Planter, 'v' }, { StreetPropKind.Cart, 'c' },
        };

        public static void Main(string[] args)
        {
            int seed = args.Length > 0 ? int.Parse(args[0]) : 0;
            string output = args.Length > 1 ? args[1] : "CityPreview.md";

            CanalTown canalTown = new CanalTownGenerator(seed, new CanalTownSettings()).Generate();
            CityLayout layout = canalTown.Layout;
            var town = new TownSettings();
            List<StreetProp> props = new StreetFurniturePlanner(town.StreetLife).Plan(layout, seed);

            File.WriteAllText(output, Render(layout, props, seed));
            string html = Path.ChangeExtension(output, ".html");
            File.WriteAllText(html, $"<!doctype html><title>City preview seed {seed}</title><body style='margin:0;background:#f2ede4'>{IsometricView.Render(layout, props, canalTown.PlayerEntry, canalTown.RivalEntry)}</body>");
            Console.WriteLine($"Wrote {Path.GetFullPath(output)} and {Path.GetFullPath(html)}");
        }

        private static string Render(CityLayout layout, List<StreetProp> props, int seed)
        {
            char[,] map = new char[layout.Width, layout.Height];
            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    map[x, y] = layout.IsRoad(new GridPosition(x, y)) ? '.' : ' ';
                }
            }

            int plotIndex = 0;
            foreach (Plot plot in layout.Plots)
            {
                char glyph = GlyphFor(plot, plotIndex++);
                for (int y = plot.Area.Min.Y; y < plot.Area.Min.Y + plot.Area.Height; y++)
                {
                    for (int x = plot.Area.Min.X; x < plot.Area.Min.X + plot.Area.Width; x++)
                    {
                        map[x, y] = glyph;
                    }
                }
            }

            foreach (StreetProp prop in props)
            {
                map[prop.Cell.X, prop.Cell.Y] = PropGlyphs[prop.Kind];
            }

            var text = new StringBuilder();
            text.AppendLine($"# City preview (seed {seed})");
            text.AppendLine();
            text.AppendLine("Houses: storeys as a digit (`1`-`9`) or letter (`a`=1, `b`=2, ...); neighbouring plots alternate so each house shows. ");
            text.AppendLine("`C` church, `S` school, `T` tavern, `M` market, `P` plaza, `\"` park, `~` water, `g` garden, `y` yard, `.` road, blank = unused.");
            text.AppendLine("Street props: `i` lamp, `t` tree, `x` crates, `o` barrels, `v` planter, `c` cart. North is up.");
            text.AppendLine();
            text.AppendLine("```");
            for (int y = layout.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    text.Append(map[x, y]);
                }

                text.AppendLine();
            }

            text.AppendLine("```");
            text.AppendLine();
            text.AppendLine("| Plot use | Count | Storeys |");
            text.AppendLine("|---|---|---|");
            foreach (IGrouping<PlotUse, Plot> group in layout.Plots.GroupBy(plot => plot.Use).OrderBy(g => g.Key))
            {
                string storeys = string.Join(", ", group.GroupBy(p => p.Storeys).OrderBy(g => g.Key).Select(g => $"{g.Key}×{g.Count()}"));
                text.AppendLine($"| {group.Key} | {group.Count()} | {storeys} |");
            }

            text.AppendLine();
            GridPosition centre = new GridPosition(layout.Width / 2, layout.Height / 2);
            GridPosition road = layout.RoadCells().OrderBy(cell => Math.Abs(cell.X - centre.X) + Math.Abs(cell.Y - centre.Y)).First();
            text.AppendLine($"Road nearest the map centre (e.g. for the hill in Hill.json): x {(road.X + 0.5f) * layout.CellSize} m, z {(road.Y + 0.5f) * layout.CellSize} m");
            text.AppendLine();
            text.AppendLine($"Street props: {string.Join(", ", props.GroupBy(p => p.Kind).Select(g => $"{g.Key} {g.Count()}"))}");
            return text.ToString();
        }

        // Alternating digit/letter per plot keeps neighbouring houses of the same height visibly separate.
        private static char GlyphFor(Plot plot, int index)
        {
            if (UseGlyphs.TryGetValue(plot.Use, out char glyph))
            {
                return glyph;
            }

            int storeys = Math.Min(plot.Storeys, 9);
            return index % 2 == 0 ? (char)('0' + storeys) : (char)('a' + storeys - 1);
        }
    }
}
