using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.City.CanalTown
{
    /// <summary>
    /// The rural land between the town's ring road and the map edge: country lanes run out from the town, and the
    /// parcels between them become fields, orchards, meadows or farms. The canal flows on through it to the edge,
    /// and the high roads the two sides march in on cross it.
    /// </summary>
    internal sealed class Countryside
    {
        private const int FieldWeight = 4;
        private const int OrchardWeight = 2;
        private const int MeadowWeight = 2;
        private const int FarmWeight = 3;
        private const int MinFarmhouseStoreys = 1;
        private const int MaxFarmhouseStoreys = 2;

        private readonly CanalTownSettings _settings;
        private readonly Random _random;

        public Countryside(CanalTownSettings settings, Random random)
        {
            _settings = settings;
            _random = random;
        }

        public void Lay(CityGrid grid, GridRect town, GridRect canalWater, IReadOnlyList<GridRect> highRoads, List<Plot> plots)
        {
            GridRect map = RectCarving.Rect(0, 0, grid.Width, grid.Height);
            GridRect canalOutToTheEdges = RectCarving.Rect(0, canalWater.Min.Y, grid.Width, canalWater.Height);
            foreach (GridRect side in RectCarving.Around(map, town))
            {
                foreach (GridRect land in CutAcross(side, canalOutToTheEdges, cut => plots.Add(new Plot(cut, 1, PlotUse.Water))))
                {
                    IEnumerable<GridRect> parcelLands = new[] { land };
                    foreach (GridRect highRoad in highRoads)
                    {
                        parcelLands = parcelLands.SelectMany(part => CutAcross(part, highRoad, cut => MarkRoad(grid, cut))).ToList();
                    }

                    foreach (GridRect parcelLand in parcelLands.SelectMany(part => SplitOffCorners(part, town, plots)))
                    {
                        LayLand(grid, parcelLand, town, plots);
                    }
                }
            }
        }

        // The map corners beyond the town's sides have no road to reach, so they stay single parcels without lanes;
        // a lane there would lead nowhere. Returns the part alongside the town, which does get lanes.
        private IEnumerable<GridRect> SplitOffCorners(GridRect land, GridRect town, List<Plot> plots)
        {
            bool reachesPastTheTownsSides = land.Min.X < town.Min.X && land.XMax > town.XMax;
            if (!reachesPastTheTownsSides)
            {
                return new[] { land };
            }

            GridRect alongside = RectCarving.Rect(town.Min.X, land.Min.Y, town.Width, land.Height);
            foreach (GridRect corner in RectCarving.Around(land, alongside))
            {
                Furnish(new Parcel(corner, corner.Width >= corner.Height, Parcel.NoLane), town, plots);
            }

            return new[] { alongside };
        }

        // Where a horizontal band (the canal, the high road) crosses the land, hand the crossing to useCut and return the rest.
        private static IEnumerable<GridRect> CutAcross(GridRect land, GridRect band, Action<GridRect> useCut)
        {
            bool crosses = band.Min.Y >= land.Min.Y && band.YMax <= land.YMax && land.Height > band.Height && land.Overlaps(band);
            if (!crosses)
            {
                return new[] { land };
            }

            GridRect cut = RectCarving.Rect(land.Min.X, band.Min.Y, land.Width, band.Height);
            useCut(cut);
            return RectCarving.Around(land, cut);
        }

        // Lanes run across the strip, away from the town, so every parcel reaches a road.
        private void LayLand(CityGrid grid, GridRect land, GridRect town, List<Plot> plots)
        {
            bool runsAlongX = land.Width >= land.Height;
            int length = runsAlongX ? land.Width : land.Height;
            int street = _settings.StreetWidthCells;
            int start = 0;
            while (start < length)
            {
                int size = _random.Next(_settings.MinParcelCells, _settings.MaxParcelCells + 1);
                bool isLast = length - start - size - street < _settings.MinParcelCells;
                if (isLast)
                {
                    size = length - start;
                }

                GridRect parcel = runsAlongX
                    ? RectCarving.Rect(land.Min.X + start, land.Min.Y, size, land.Height)
                    : RectCarving.Rect(land.Min.X, land.Min.Y + start, land.Width, size);
                int laneSide = isLast ? (start > 0 ? Parcel.LaneBefore : Parcel.NoLane) : Parcel.LaneAfter;
                Furnish(new Parcel(parcel, runsAlongX, laneSide), town, plots);
                if (!isLast)
                {
                    GridRect lane = runsAlongX
                        ? RectCarving.Rect(parcel.XMax, land.Min.Y, street, land.Height)
                        : RectCarving.Rect(land.Min.X, parcel.YMax, land.Width, street);
                    MarkRoad(grid, lane);
                }

                start += size + street;
            }
        }

        private void Furnish(Parcel parcelInfo, GridRect town, List<Plot> plots)
        {
            GridRect parcel = parcelInfo.Area;
            int roll = _random.Next(FieldWeight + OrchardWeight + MeadowWeight + FarmWeight);
            if (roll < FieldWeight)
            {
                plots.Add(new Plot(parcel, 1, PlotUse.Field));
            }
            else if (roll < FieldWeight + OrchardWeight)
            {
                plots.Add(new Plot(parcel, 1, PlotUse.Orchard));
            }
            else if (roll < FieldWeight + OrchardWeight + MeadowWeight)
            {
                plots.Add(new Plot(parcel, 1, PlotUse.Meadow));
            }
            else
            {
                Farm(parcelInfo, town, plots);
            }
        }

        // The farmyard lies against the parcel's lane, on the side nearest the town, with the farmhouse on the lane so
        // it can be entered and garrisoned; fields lie behind. A parcel without a lane becomes a plain field.
        private void Farm(Parcel parcelInfo, GridRect town, List<Plot> plots)
        {
            GridRect parcel = parcelInfo.Area;
            if (parcelInfo.LaneSide == Parcel.NoLane)
            {
                plots.Add(new Plot(parcel, 1, PlotUse.Field));
                return;
            }

            int yardWidth = Math.Min(_settings.FarmyardCells, parcel.Width);
            int yardHeight = Math.Min(_settings.FarmyardCells, parcel.Height);
            bool laneAfter = parcelInfo.LaneSide == Parcel.LaneAfter;
            int yardX = parcelInfo.RunsAlongX ? AtSide(parcel.Min.X, parcel.XMax, yardWidth, laneAfter) : AtSide(parcel.Min.X, parcel.XMax, yardWidth, parcel.Min.X < town.XMax);
            int yardY = parcelInfo.RunsAlongX ? AtSide(parcel.Min.Y, parcel.YMax, yardHeight, parcel.Min.Y < town.YMax) : AtSide(parcel.Min.Y, parcel.YMax, yardHeight, laneAfter);
            GridRect yard = RectCarving.Rect(yardX, yardY, yardWidth, yardHeight);

            int houseWidth = Math.Min(_settings.FarmhouseCells, yardWidth);
            int houseDepth = Math.Min(_settings.FarmhouseCells, yardHeight);
            int houseX = parcelInfo.RunsAlongX ? AtSide(yard.Min.X, yard.XMax, houseWidth, laneAfter) : AtSide(yard.Min.X, yard.XMax, houseWidth, parcel.Min.X < town.XMax);
            int houseY = parcelInfo.RunsAlongX ? AtSide(yard.Min.Y, yard.YMax, houseDepth, parcel.Min.Y < town.YMax) : AtSide(yard.Min.Y, yard.YMax, houseDepth, laneAfter);
            GridRect house = RectCarving.Rect(houseX, houseY, houseWidth, houseDepth);

            plots.Add(new Plot(house, _random.Next(MinFarmhouseStoreys, MaxFarmhouseStoreys + 1)));
            foreach (GridRect part in RectCarving.Around(yard, house))
            {
                plots.Add(new Plot(part, 1, PlotUse.Yard));
            }

            foreach (GridRect field in RectCarving.Around(parcel, yard))
            {
                plots.Add(new Plot(field, 1, PlotUse.Field));
            }
        }

        // The start of a span of `size` cells at the high end of [min, max) when towardsMax holds, else at the low end.
        private static int AtSide(int min, int max, int size, bool towardsMax)
        {
            return towardsMax ? max - size : min;
        }

        private static void MarkRoad(CityGrid grid, GridRect area)
        {
            foreach (GridPosition cell in area.Cells())
            {
                grid.MarkRoad(cell);
            }
        }
    }
}
