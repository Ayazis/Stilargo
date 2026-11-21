using System;
using Stilargo.Core.City;
using Stilargo.Gameplay.City;
using Stilargo.Gameplay.World;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.Scenes
{
    /// <summary>
    /// Where things can stand in the city a <see cref="WorldInstaller"/> will generate. The layout depends only on
    /// the installer's seed and config, so the scene builder can regenerate it at edit time and place units on roads.
    /// </summary>
    public sealed class CitySite
    {
        private const string SeedField = "_seed";
        private const string CityConfigField = "_cityConfig";
        private const string DropOffPointField = "_dropOffPoint";

        private readonly CityLayout _layout;

        private CitySite(CityLayout layout, Vector3 home)
        {
            _layout = layout;
            Home = new Vector3(home.x, 0f, home.z);
        }

        /// <summary>The drop-off point on the ground, where the player's squad starts.</summary>
        public Vector3 Home { get; }

        /// <summary>World X/Z extent of the city; its south-west corner is at the origin.</summary>
        public Vector2 Size => new Vector2(_layout.Width * _layout.CellSize, _layout.Height * _layout.CellSize);

        public static CitySite Of(WorldInstaller world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            var fields = new SerializedObject(world);
            var config = fields.FindProperty(CityConfigField).objectReferenceValue as CityConfigAsset;
            var home = fields.FindProperty(DropOffPointField).objectReferenceValue as Transform;
            if (config == null || home == null)
            {
                throw new InvalidOperationException($"{world.name} needs a city config and a drop-off point to place units.");
            }

            int seed = fields.FindProperty(SeedField).intValue;
            CityLayout layout = new CityGenerator(seed, config.ToConfig()).Generate();
            return new CitySite(layout, home.position);
        }

        /// <summary>The ground point at <paramref name="offset"/> from home. Fails fast if it is not on a road.</summary>
        public Vector3 RoadPointNearHome(Vector3 offset)
        {
            Vector3 point = Home + offset;
            if (!_layout.IsRoadAt(point.x, point.z))
            {
                throw new InvalidOperationException(
                    $"Spawn point {point} is not on a road. Move the start offsets or the drop-off point to match the city config.");
            }

            return point;
        }
    }
}
