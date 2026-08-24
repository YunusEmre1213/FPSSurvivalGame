namespace Project.Core
{
    public enum DayNightPhase
    {
        Day,
        Dusk,   
        Night
    }
    public readonly struct DayNightPhaseChangedEvent : IGameEvent
    {
        public readonly DayNightPhase NewPhase;

        public DayNightPhaseChangedEvent(DayNightPhase newPhase)
        {
            NewPhase = newPhase;
        }
    }
}