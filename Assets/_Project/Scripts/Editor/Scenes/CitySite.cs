using System;
using Stilargo.Core.City;
using Stilargo.Core.City.CanalTown;
using Stilargo.Gameplay.World;
using Stilargo.Infrastructure.Config;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.Scenes
{
    /// <summary>
    /// Where things can stand in the canal town a <see cref="WorldInstaller"/> will generate. The layout depends only
    /// on the installer's seed and <c>Town.json</c>, so the scene builder can regenerate it at edit time and place
    /// each squad on the high road it enters by.
    /// </summary>
    public sealed class CitySite
    {
        private const string SeedField = "_seed";
        private const float CellCentre = 0.5f;

        // Squads start a little way in from the map edge, so the whole formation stands on the high road.
        private const float StartInsetMetres = 8f;

        private readonly CityLayout _layout;

        private CitySite(CanalTown town)
        {
            _layout = town.Layout;
            Home = GroundPointOf(town.PlayerEntry) + (Vector3.right * StartInsetMetres);
            RivalStart = GroundPointOf(town.RivalEntry) + (Vector3.left * StartInsetMetres);
        }

        /// <summary>Where the player's squad starts, just inside the west map edge.</summary>
        public Vector3 Home { get; }

        /// <summary>Where the rival's squad starts, just inside the east map edge.</summary>
        public Vector3 RivalStart { get; }

        /// <summary>World X/Z extent of the map; its south-west corner is at the origin.</summary>
        public Vector2 Size => new Vector2(_layout.Width * _layout.CellSize, _layout.Height * _layout.CellSize);

        public static CitySite Of(WorldInstaller world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            int seed = new SerializedObject(world).FindProperty(SeedField).intValue;
            return new CitySite(new CanalTownGenerator(seed, TownConfig.Load().CanalTown).Generate());
        }

        /// <summary>The ground point at <paramref name="offset"/> from home. Fails fast if it is not on a road.</summary>
        public Vector3 RoadPointNearHome(Vector3 offset)
        {
            return RequireRoad(Home + offset);
        }

        /// <summary>The ground point at <paramref name="offset"/> from the rival's start. Fails fast if it is not on a road.</summary>
        public Vector3 RoadPointNearRivalStart(Vector3 offset)
        {
            return RequireRoad(RivalStart + offset);
        }

        private Vector3 RequireRoad(Vector3 point)
        {
            if (!_layout.IsRoadAt(point.x, point.z))
            {
                throw new InvalidOperationException(
                    $"Spawn point {point} is not on a road. Move the start offsets or the high roads in Town.json.");
            }

            return point;
        }

        private Vector3 GroundPointOf(GridPosition cell)
        {
            return new Vector3((cell.X + CellCentre) * _layout.CellSize, 0f, (cell.Y + CellCentre) * _layout.CellSize);
        }
    }
}
