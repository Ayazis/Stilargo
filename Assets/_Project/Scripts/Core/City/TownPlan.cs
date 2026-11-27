using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Decides what each block of a grid city becomes. The middle block is the market square, the ring of blocks
    /// around it holds the tall house blocks, and further out are the outskirts, some of which become parks.
    /// </summary>
    public static class TownPlan
    {
        private const int HouseBlockRing = 1;

        /// <summary>Roles indexed like the generator's blocks: row by row from the south-west corner.</summary>
        public static BlockRole[] Assign(int blocksX, int blocksY, int parkBlockCount, Random random)
        {
            if (blocksX <= 0 || blocksY <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blocksX), "Block counts must be positive.");
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            int centreX = blocksX / 2;
            int centreY = blocksY / 2;
            var roles = new BlockRole[blocksX * blocksY];
            for (int y = 0; y < blocksY; y++)
            {
                for (int x = 0; x < blocksX; x++)
                {
                    int ring = Math.Max(Math.Abs(x - centreX), Math.Abs(y - centreY));
                    roles[(y * blocksX) + x] = RoleForRing(ring);
                }
            }

            PlaceParks(roles, parkBlockCount, random);
            return roles;
        }

        private static BlockRole RoleForRing(int ring)
        {
            if (ring == 0)
            {
                return BlockRole.MarketSquare;
            }

            return ring == HouseBlockRing ? BlockRole.HouseBlock : BlockRole.Outskirts;
        }

        // Parks take outskirts blocks first, so the house blocks keep framing the square.
        private static void PlaceParks(BlockRole[] roles, int parkBlockCount, Random random)
        {
            for (int placed = 0; placed < parkBlockCount; placed++)
            {
                List<int> candidates = IndicesWith(roles, BlockRole.Outskirts);
                if (candidates.Count == 0)
                {
                    candidates = IndicesWith(roles, BlockRole.HouseBlock);
                }

                if (candidates.Count == 0)
                {
                    return;
                }

                roles[candidates[random.Next(candidates.Count)]] = BlockRole.Park;
            }
        }

        private static List<int> IndicesWith(BlockRole[] roles, BlockRole role)
        {
            return Enumerable.Range(0, roles.Length).Where(index => roles[index] == role).ToList();
        }
    }
}
