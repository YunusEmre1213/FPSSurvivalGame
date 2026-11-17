namespace Project.Core
{
    public readonly struct EnemyWaveSpawnedEvent : IGameEvent
    {
        public readonly DayNightPhase Phase;
        public readonly int OncuCount;
        public readonly int SuruCount;
        public readonly int EliteCount;
        public readonly int WolfCount;
        public readonly float DifficultyMultiplier;
        public readonly int NightsSurvived;

        public EnemyWaveSpawnedEvent(DayNightPhase phase, int oncuCount, int suruCount, int eliteCount, int wolfCount, float difficultyMultiplier, int nightsSurvived)
        {
            Phase = phase;
            OncuCount = oncuCount;
            SuruCount = suruCount;
            EliteCount = eliteCount;
            WolfCount = wolfCount;
            DifficultyMultiplier = difficultyMultiplier;
            NightsSurvived = nightsSurvived;
        }
    }
}