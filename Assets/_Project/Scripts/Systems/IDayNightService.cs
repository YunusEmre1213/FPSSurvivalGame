using Project.Core;

namespace Project.Systems
{
    public interface IDayNightService : IGameService
    {
        DayNightPhase CurrentPhase { get; }

        float PhaseTimeRemaining { get; }

        float PhaseProgress01 { get; }
    }
}