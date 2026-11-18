using UnityEngine;
using UnityEngine.UI;
using Project.Gameplay.Player;

namespace Project.UI
{
    public class NeedsHUD : MonoBehaviour
    {
        [SerializeField] private PlayerNeeds playerNeeds;
        [Tooltip("Image Type: Filled, Fill Method: Horizontal olarak ayarlanmis olmali.")]
        [SerializeField] private Image hungerFillImage;
        [SerializeField] private Image thirstFillImage;

        [Header("Kritik Uyari")]
        [Range(0f, 1f)]
        [SerializeField] private float criticalThreshold = 0.2f;
        [SerializeField] private float pulseSpeed = 4f;
        [Range(0f, 1f)]
        [SerializeField] private float pulseBrightness = 0.35f;

        private Color _hungerBaseColor;
        private Color _thirstBaseColor;

        private void Start()
        {
            _hungerBaseColor = hungerFillImage.color;
            _thirstBaseColor = thirstFillImage.color;
        }

        private void Update()
        {
            if (playerNeeds == null) return;

            float hungerRatio = playerNeeds.Hunger / playerNeeds.MaxValue;
            float thirstRatio = playerNeeds.Thirst / playerNeeds.MaxValue;

            hungerFillImage.fillAmount = hungerRatio;
            thirstFillImage.fillAmount = thirstRatio;

            hungerFillImage.color = ApplyPulse(_hungerBaseColor, hungerRatio < criticalThreshold);
            thirstFillImage.color = ApplyPulse(_thirstBaseColor, thirstRatio < criticalThreshold);
        }

        private Color ApplyPulse(Color baseColor, bool isCritical)
        {
            if (!isCritical) return baseColor;

            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            return Color.Lerp(baseColor, Color.white, pulse * pulseBrightness);
        }
    }
}