using System;

namespace Stilargo.Core.City
{
    /// <summary>A building plot: the cells one building occupies, how tall it is and what it is used for.</summary>
    public sealed class Plot
    {
        public const float StandardIncomeMultiplier = 1f;

        public Plot(GridRect area, int storeys)
            : this(area, storeys, PlotUse.House)
        {
        }

        public Plot(GridRect area, int storeys, PlotUse use)
        {
            if (storeys <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(storeys), storeys, "Storeys must be positive.");
            }

            Area = area;
            Storeys = storeys;
            Use = use;
        }

        private Plot(Plot source, PlotUseRule rule)
        {
            Area = source.Area;
            Storeys = rule.Storeys > 0 ? rule.Storeys : source.Storeys;
            Use = rule.Use;
            TowerStoreys = rule.TowerStoreys;
            IncomeMultiplier = rule.IncomeMultiplier;
            Style = source.Style;
            Front = source.Front;
        }

        private Plot(Plot source, HouseStyle style, Facing front)
        {
            Area = source.Area;
            Storeys = source.Storeys;
            Use = source.Use;
            TowerStoreys = source.TowerStoreys;
            IncomeMultiplier = source.IncomeMultiplier;
            Style = style;
            Front = front;
        }

        public GridRect Area { get; }

        public int Storeys { get; }

        public PlotUse Use { get; } = PlotUse.House;

        /// <summary>Storeys of the bell tower that rises above the rest of the building; 0 when there is none.</summary>
        public int TowerStoreys { get; }

        public bool HasTower => TowerStoreys > Storeys;

        /// <summary>The kind of house; set by <see cref="HouseStyler"/>, <see cref="HouseStyle.Row"/> until then.</summary>
        public HouseStyle Style { get; } = HouseStyle.Row;

        /// <summary>The side the building's front and door face; set by <see cref="HouseStyler"/>, south until then.</summary>
        public Facing Front { get; } = Facing.South;

        /// <summary>Scales what the building earns once claimed.</summary>
        public float IncomeMultiplier { get; } = StandardIncomeMultiplier;

        /// <summary>
        /// Only houses and the landmark buildings have walls. Parks, water, squares, gardens and yards are open
        /// ground: nothing to garrison or claim, nothing blocks the view.
        /// </summary>
        public bool HasBuilding => Use == PlotUse.House || Use == PlotUse.Church || Use == PlotUse.School || Use == PlotUse.Tavern;

        /// <summary>The same plot drawn as another kind of house, facing another way.</summary>
        public Plot Styled(HouseStyle style, Facing front)
        {
            return new Plot(this, style, front);
        }

        /// <summary>The same plot given over to the rule's use, storeys and tower.</summary>
        public Plot UsedFor(PlotUseRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            return new Plot(this, rule);
        }
    }
}
