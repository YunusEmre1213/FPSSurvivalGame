using UnityEngine;
using TMPro;
using Project.Core;
using Project.Gameplay.Player;

namespace Project.UI
{
    public class HealthHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text healthText;

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
        }

        private void OnHealthChanged(PlayerHealthChangedEvent evt)
        {
            healthText.text = $"Can: {evt.CurrentHealth:F0} / {evt.MaxHealth:F0}";
        }
    }
}