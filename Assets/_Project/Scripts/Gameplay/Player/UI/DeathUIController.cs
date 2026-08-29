using UnityEngine;
using UnityEngine.UI;
using Project.Core;
using Project.Gameplay.Player;

namespace Project.UI
{
    public class DeathUIController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button respawnButton;
        [SerializeField] private PlayerHealth playerHealth;

        private void Awake()
        {
            respawnButton.onClick.AddListener(OnRespawnPressed);
            panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            panelRoot.SetActive(true);
        }

        private void OnPlayerRespawned(PlayerRespawnedEvent evt)
        {
            panelRoot.SetActive(false);
        }

        private void OnRespawnPressed()
        {
            playerHealth.Respawn();
        }
    }
}