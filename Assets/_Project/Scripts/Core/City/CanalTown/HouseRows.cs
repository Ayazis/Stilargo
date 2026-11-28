using System;
using System.Collections.Generic;

namespace Stilargo.Core.City.CanalTown
{
    /// <summary>Splits strips of land into row houses with varied frontages; corner houses rise higher.</summary>
    internal sealed class HouseRows
    {
        private readonly CanalTownSettings _settings;
        private readonly Random _random;

        public HouseRows(CanalTownSettings settings, Random random)
        {
            _settings = settings;
            _random = random;
        }

        /// <summary>Houses along a strip; <paramref name="block"/> tells which houses sit on a corner.</summary>
        public void Fill(GridRect strip, GridRect block, int extraStoreys, List<Plot> plots)
        {
            bool runsAlongX = strip.Width >= strip.Height;
            int length = runsAlongX ? strip.Width : strip.Height;
            int start = 0;
            while (start < length)
            {
                int frontage = NextFrontage(length - start);
                GridRect area = runsAlongX
                    ? RectCarving.Rect(strip.Min.X + start, strip.Min.Y, frontage, strip.Height)
                    : RectCarving.Rect(strip.Min.X, strip.Min.Y + start, strip.Width, frontage);
                plots.Add(new Plot(area, StoreysFor(area, block, extraStoreys)));
                start += frontage;
            }
        }

        private int NextFrontage(int remaining)
        {
            int frontage = _random.Next(_settings.MinFrontageCells, _settings.MaxFrontageCells + 1);
            return remaining - frontage < _settings.MinFrontageCells ? remaining : frontage;
        }

        private int StoreysFor(GridRect area, GridRect block, int extraStoreys)
        {
            int storeys = _random.Next(_settings.MinStoreys, _settings.MaxStoreys + 1) + extraStoreys;
            return IsCorner(area, block) ? storeys + _settings.ProminentExtraStoreys : storeys;
        }

        private static bool IsCorner(GridRect area, GridRect block)
        {
            bool touchesWestOrEast = area.Min.X == block.Min.X || area.XMax == block.XMax;
            bool touchesSouthOrNorth = area.Min.Y == block.Min.Y || area.YMax == block.YMax;
            return touchesWestOrEast && touchesSouthOrNorth;
        }
    }
}
