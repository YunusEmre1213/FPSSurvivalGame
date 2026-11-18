using UnityEngine;
using UnityEngine.UI;
using Project.Core;
using Project.Gameplay.Player;

namespace Project.UI
{
    public class HealthHUD : MonoBehaviour
    {
        [Tooltip("Image Type: Filled, Fill Method: Horizontal olarak ayarlanmis olmali.")]
        [SerializeField] private Image healthFillImage;

        [Header("Kritik Uyari")]
        [Tooltip("Bu oranin altinda bar nabiz gibi parlar (ornegin 0.10 = %10).")]
        [Range(0f, 1f)]
        [SerializeField] private float pulseThreshold = 0.10f;
        [SerializeField] private float pulseSpeed = 4f;
        [Range(0f, 1f)]
        [SerializeField] private float pulseBrightness = 0.35f;

        private Color _baseColor;
        private bool _isCritical;

        private void Awake()
        {
            _baseColor = healthFillImage.color;
        }

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
            healthFillImage.fillAmount = evt.MaxHealth > 0f ? evt.CurrentHealth / evt.MaxHealth : 0f;
            _isCritical = healthFillImage.fillAmount < pulseThreshold;
        }

        private void Update()
        {
            healthFillImage.color = ApplyPulse(_baseColor, _isCritical);
        }

        private Color ApplyPulse(Color baseColor, bool isCritical)
        {
            if (!isCritical) return baseColor;

            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            return Color.Lerp(baseColor, Color.white, pulse * pulseBrightness);
        }
    }
}