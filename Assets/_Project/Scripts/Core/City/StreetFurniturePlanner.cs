using System;
using System.Collections.Generic;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Decides where the street life goes: lamps at regular spacing and some clutter (crates, barrels, planters,
    /// carts) on the pavement along buildings, and trees along parks, gardens and squares. Only the edge of a street
    /// at least two cells wide gets a prop, so alleys stay clear. The same seed always gives the same streets.
    /// </summary>
    public sealed class StreetFurniturePlanner
    {
        private static readonly GridPosition[] Directions =
        {
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
            new GridPosition(0, 1),
            new GridPosition(0, -1),
        };

        private static readonly StreetPropKind[] Clutter =
        {
            StreetPropKind.Crates,
            StreetPropKind.Barrels,
            StreetPropKind.Planter,
            StreetPropKind.Crates,
            StreetPropKind.Barrels,
            StreetPropKind.Cart,
        };

        private readonly StreetLifeSettings _settings;

        public StreetFurniturePlanner(StreetLifeSettings settings)
        {
            _settings = (settings ?? throw new ArgumentNullException(nameof(settings))).Validate();
        }

        public List<StreetProp> Plan(CityLayout layout, int seed)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            var random = new Random(seed);
            Plot[] plotAt = MapPlots(layout);
            var props = new List<StreetProp>();
            foreach (GridPosition cell in layout.RoadCells())
            {
                foreach (GridPosition step in Directions)
                {
                    Plot beside = PlotAt(layout, plotAt, Offset(cell, step));
                    if (beside == null || !layout.IsRoad(Offset(cell, Opposite(step))))
                    {
                        continue;
                    }

                    if (TryPick(cell, beside, random, out StreetPropKind kind))
                    {
                        props.Add(new StreetProp(kind, cell, step));
                    }

                    break;
                }
            }

            return props;
        }

        private bool TryPick(GridPosition cell, Plot beside, Random random, out StreetPropKind kind)
        {
            int along = cell.X + cell.Y;
            if (beside.HasBuilding)
            {
                if (along % _settings.LampSpacingCells == 0)
                {
                    kind = StreetPropKind.Lamp;
                    return true;
                }

                kind = Clutter[random.Next(Clutter.Length)];
                return random.NextDouble() < _settings.ClutterChance;
            }

            kind = StreetPropKind.Tree;
            return beside.Use != PlotUse.Water && along % _settings.TreeSpacingCells == 0;
        }

        private static Plot[] MapPlots(CityLayout layout)
        {
            var plotAt = new Plot[layout.Width * layout.Height];
            foreach (Plot plot in layout.Plots)
            {
                foreach (GridPosition cell in plot.Area.Cells())
                {
                    if (layout.IsInside(cell))
                    {
                        plotAt[(cell.Y * layout.Width) + cell.X] = plot;
                    }
                }
            }

            return plotAt;
        }

        private static Plot PlotAt(CityLayout layout, Plot[] plotAt, GridPosition cell)
        {
            return layout.IsInside(cell) ? plotAt[(cell.Y * layout.Width) + cell.X] : null;
        }

        private static GridPosition Offset(GridPosition cell, GridPosition step)
        {
            return new GridPosition(cell.X + step.X, cell.Y + step.Y);
        }

        private static GridPosition Opposite(GridPosition step)
        {
            return new GridPosition(-step.X, -step.Y);
        }
    }
}
