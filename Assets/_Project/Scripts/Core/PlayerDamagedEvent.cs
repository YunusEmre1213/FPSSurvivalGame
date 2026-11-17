namespace Project.Core
{
    public readonly struct PlayerDamagedEvent : IGameEvent
    {
        public readonly float Amount;
        public readonly float ResultingHealth;

        public PlayerDamagedEvent(float amount, float resultingHealth)
        {
            Amount = amount;
            ResultingHealth = resultingHealth;
        }
    }
}