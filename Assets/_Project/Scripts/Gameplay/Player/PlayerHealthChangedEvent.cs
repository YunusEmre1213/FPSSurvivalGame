using Project.Core;

namespace Project.Gameplay.Player
{
    public readonly struct PlayerHealthChangedEvent : IGameEvent
    {
        public readonly float CurrentHealth;
        public readonly float MaxHealth;

        public PlayerHealthChangedEvent(float currentHealth, float maxHealth)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }
}