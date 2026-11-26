using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Which landmarks the city gets, loaded from <c>Buildings.json</c>. Rules are applied in order, so earlier
    /// landmarks get first pick of the plots. Every plot no rule takes stays a house. Parks and the market square
    /// take whole blocks, so they come from <see cref="TownSettings"/> instead.
    /// </summary>
    public sealed class DistrictSettings
    {
        public const int DefaultChurchCount = 1;
        public const int DefaultChurchStoreys = 2;
        public const int DefaultChurchTowerStoreys = 6;
        public const float DefaultChurchIncomeMultiplier = 1.2f;
        public const int DefaultSchoolCount = 1;
        public const int DefaultSchoolStoreys = 2;
        public const float DefaultSchoolIncomeMultiplier = 1.4f;
        public const int DefaultTavernCount = 2;
        public const float DefaultTavernIncomeMultiplier = 1.8f;
        public const int DefaultWaterCount = 1;

        private readonly List<PlotUseRule> _rules;

        public DistrictSettings()
            : this(DefaultRules())
        {
        }

        public DistrictSettings(IReadOnlyList<PlotUseRule> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (rules.Any(rule => rule == null))
            {
                throw new ArgumentException("Rules must not contain null.", nameof(rules));
            }

            if (rules.Any(rule => rule.Use == PlotUse.House))
            {
                throw new ArgumentException("Houses fill every plot left over; they need no rule.", nameof(rules));
            }

            if (rules.Any(rule => rule.Use == PlotUse.Market))
            {
                throw new ArgumentException("The market square is a whole block laid out by the town plan; it needs no rule.", nameof(rules));
            }

            if (rules.Select(rule => rule.Use).Distinct().Count() != rules.Count)
            {
                throw new ArgumentException("Each use may have only one rule.", nameof(rules));
            }

            _rules = rules.ToList();
        }

        public IReadOnlyList<PlotUseRule> Rules => _rules;

        public static IReadOnlyList<PlotUseRule> DefaultRules()
        {
            return new[]
            {
                new PlotUseRule(PlotUse.Church, DefaultChurchCount, PlotPlacement.FacingSquare)
                    .WithHeight(DefaultChurchStoreys, DefaultChurchTowerStoreys)
                    .WithIncomeMultiplier(DefaultChurchIncomeMultiplier),
                new PlotUseRule(PlotUse.School, DefaultSchoolCount, PlotPlacement.Largest)
                    .WithHeight(DefaultSchoolStoreys, 0)
                    .WithIncomeMultiplier(DefaultSchoolIncomeMultiplier),
                new PlotUseRule(PlotUse.Tavern, DefaultTavernCount, PlotPlacement.FacingSquare)
                    .WithIncomeMultiplier(DefaultTavernIncomeMultiplier),
                new PlotUseRule(PlotUse.Water, DefaultWaterCount, PlotPlacement.Anywhere),
            };
        }
    }
}
