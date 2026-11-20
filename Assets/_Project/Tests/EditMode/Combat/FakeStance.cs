using Stilargo.Core.Combat.Stances;

namespace Stilargo.Tests.EditMode.Combat
{
    public sealed class FakeStance : IStance
    {
        public FakeStance(StanceAction action)
        {
            Action = action;
        }

        public string Name => "Fake";

        public StanceAction Action { get; set; }

        public StanceContext LastContext { get; private set; }

        public int DecideCount { get; private set; }

        public StanceAction Decide(StanceContext context)
        {
            LastContext = context;
            DecideCount++;
            return Action;
        }
    }
}
