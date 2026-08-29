using UnityEngine;
using Project.Core;
using Project.Gameplay;

namespace Project.Gameplay.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        [Tooltip("Respawn'da oyuncunun isinlanacagi nokta - guvenli us icinde bir yer olmali.")]
        [SerializeField] private Transform respawnPoint;

        private float _currentHealth;
        private CharacterController _characterController;

        public bool IsDead { get; private set; }

        private void Awake()
        {
            _currentHealth = maxHealth;
            _characterController = GetComponent<CharacterController>();
            EventBus.Publish(new PlayerHealthChangedEvent(_currentHealth, maxHealth));
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return; 

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            Debug.Log($"[PlayerHealth] Oyuncu {amount} hasar aldi, kalan can: {_currentHealth}/{maxHealth}");

            EventBus.Publish(new PlayerHealthChangedEvent(_currentHealth, maxHealth));

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            IsDead = true;
            UIInputLock.Lock();

            ServiceLocator.Instance.Get<Systems.IInventoryService>().ClearAll();

            Debug.Log("[PlayerHealth] Oyuncu oldu, stok envanteri kayboldu.");
            EventBus.Publish(new PlayerDiedEvent());
        }

        public void Respawn()
        {
            if (!IsDead) return;

            _currentHealth = maxHealth;
            IsDead = false;

            if (respawnPoint != null)
            {
                TeleportTo(respawnPoint.position);
            }

            UIInputLock.Unlock();
            EventBus.Publish(new PlayerHealthChangedEvent(_currentHealth, maxHealth));
            EventBus.Publish(new PlayerRespawnedEvent());
            Debug.Log("[PlayerHealth] Oyuncu yeniden dogdu.");
        }

        private void TeleportTo(Vector3 position)
        {
            if (_characterController != null) _characterController.enabled = false;
            transform.position = position;
            if (_characterController != null) _characterController.enabled = true;
        }
    }
}