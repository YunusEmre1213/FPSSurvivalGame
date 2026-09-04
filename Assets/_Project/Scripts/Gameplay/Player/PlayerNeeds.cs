using UnityEngine;
using Project.Core;
using Project.Data;

namespace Project.Gameplay.Player
{
    public class PlayerNeeds : MonoBehaviour
    {
        [Header("Azalma hizi (birim/saniye)")]
        [SerializeField] private float hungerDecayRate = 0.5f;
        [SerializeField] private float thirstDecayRate = 0.7f;

        [Header("Aclik/susuzluktan can kaybi")]
        [Tooltip("Aclik YA DA susuzluk 0'dayken saniyede kaybedilen can.")]
        [SerializeField] private float starvationDamagePerSecond = 2f;

        public float Hunger { get; private set; }
        public float Thirst { get; private set; }
        public float MaxValue { get; } = 100f;

        private PlayerHealth _playerHealth;

        private void Awake()
        {
            _playerHealth = GetComponent<PlayerHealth>();
            Hunger = MaxValue;
            Thirst = MaxValue;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerRespawnedEvent>(OnRespawned);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerRespawnedEvent>(OnRespawned);
        }

        private void Update()
        {
            if (_playerHealth != null && _playerHealth.IsDead) return;

            Hunger = Mathf.Max(0f, Hunger - hungerDecayRate * Time.deltaTime);
            Thirst = Mathf.Max(0f, Thirst - thirstDecayRate * Time.deltaTime);

            if (Hunger <= 0f || Thirst <= 0f)
            {
                _playerHealth?.TakeDamage(starvationDamagePerSecond * Time.deltaTime);
            }
        }
        public void Consume(ConsumableData item)
        {
            Hunger = Mathf.Min(MaxValue, Hunger + item.hungerRestore);
            Thirst = Mathf.Min(MaxValue, Thirst + item.thirstRestore);
            Debug.Log($"[PlayerNeeds] {item.itemName} tuketildi - Aclik: {Hunger:F0}/{MaxValue:F0}, Susuzluk: {Thirst:F0}/{MaxValue:F0}");
        }

        private void OnRespawned(PlayerRespawnedEvent evt)
        {
            Hunger = MaxValue;
            Thirst = MaxValue;
        }
    }
}