using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.Combat;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// Tunable values for claiming and fortifying a house, loaded from <c>Fortification.json</c>.
    /// The constructor validates everything, so an invalid file fails at startup with a clear message.
    /// </summary>
    public sealed class FortificationSettings
    {
        public const float DefaultHouseBaseHealthPoints = 400f;

        public FortificationSettings(float houseBaseHealthPoints, IReadOnlyList<FortificationDefinition> definitions)
        {
            if (!(houseBaseHealthPoints > 0f) || float.IsInfinity(houseBaseHealthPoints))
            {
                throw new ArgumentOutOfRangeException(nameof(houseBaseHealthPoints), houseBaseHealthPoints, "House base health must be positive and finite.");
            }

            RequireValid(definitions);
            HouseBaseHealthPoints = houseBaseHealthPoints;
            Definitions = definitions.ToList();
        }

        /// <summary>Barricade, reinforced window and turret, matching the shipped <c>Fortification.json</c>.</summary>
        public static FortificationSettings Default => new FortificationSettings(
            DefaultHouseBaseHealthPoints,
            new[]
            {
                new FortificationDefinition("barricade", FortificationSlotKind.Entrance, 40)
                    .WithDisplayName("Barricade")
                    .WithHealthBonus(150f),
                new FortificationDefinition("reinforcedWindow", FortificationSlotKind.Window, 25)
                    .WithDisplayName("Reinforced window")
                    .WithHealthBonus(60f),
                new FortificationDefinition("turret", FortificationSlotKind.Roof, 120)
                    .WithDisplayName("Turret")
                    .WithTurret(new WeaponSettings(12f, 16f, 0.8f).WithHitChance(0.7f)),
            });

        public float HouseBaseHealthPoints { get; }

        public IReadOnlyList<FortificationDefinition> Definitions { get; }

        private static void RequireValid(IReadOnlyList<FortificationDefinition> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            if (definitions.Any(definition => definition == null))
            {
                throw new ArgumentException("Fortification definitions must not contain null.", nameof(definitions));
            }

            string duplicate = definitions
                .GroupBy(definition => definition.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();
            if (duplicate != null)
            {
                throw new ArgumentException($"Fortification id '{duplicate}' is used more than once.", nameof(definitions));
            }
        }
    }
}
