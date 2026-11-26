using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.City
{
    /// <summary>Turns each outskirts block into a neighbourhood: first the plazas, then a weighted pick of the rest.</summary>
    public static class NeighbourhoodPicker
    {
        public static List<TownBlock> Pick(IReadOnlyList<TownBlock> blocks, NeighbourhoodSettings settings, Random random)
        {
            if (blocks == null || settings == null || random == null)
            {
                throw new ArgumentNullException(blocks == null ? nameof(blocks) : settings == null ? nameof(settings) : nameof(random));
            }

            settings.Validate();
            var result = blocks.ToList();
            List<int> outskirts = Enumerable.Range(0, result.Count).Where(index => result[index].Role == BlockRole.Outskirts).ToList();
            for (int placed = 0; placed < settings.PlazaCount && outskirts.Count > 0; placed++)
            {
                int pick = random.Next(outskirts.Count);
                result[outskirts[pick]] = result[outskirts[pick]].As(BlockRole.Plaza);
                outskirts.RemoveAt(pick);
            }

            foreach (int index in outskirts)
            {
                result[index] = result[index].As(Weighted(settings, random));
            }

            return result;
        }

        private static BlockRole Weighted(NeighbourhoodSettings settings, Random random)
        {
            int roll = random.Next(settings.TotalWeight);
            if (roll < settings.OldTownWeight)
            {
                return BlockRole.OldTown;
            }

            return roll < settings.OldTownWeight + settings.VillasWeight ? BlockRole.Villas : BlockRole.Workshops;
        }
    }
}
