using UnityEngine;
using Project.Gameplay;

namespace Project.Gameplay.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        private float _currentHealth;

        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            Debug.Log($"[PlayerHealth] Oyuncu {amount} hasar aldi, kalan can: {_currentHealth}/{maxHealth}");

            if (_currentHealth <= 0f)
            {
                Debug.Log("[PlayerHealth] Oyuncu oldu (olum/respawn mantigi henuz yok).");
            }
        }
    }
}