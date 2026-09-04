using UnityEngine;
using UnityEngine.UI;
using Project.Gameplay.Player;

namespace Project.UI
{
    public class StaminaHUD : MonoBehaviour
    {
        [SerializeField] private PlayerStamina playerStamina;
        [Tooltip("Image Type: Filled, Fill Method: Horizontal olarak ayarlanmis olmali.")]
        [SerializeField] private Image staminaFillImage;
        [Tooltip("Opsiyonel - atanirsa cubuk dolu iken otomatik saydamlasir.")]
        [SerializeField] private CanvasGroup canvasGroup;

        private void Update()
        {
            if (playerStamina == null) return;

            float ratio = playerStamina.CurrentStamina / playerStamina.MaxStamina;
            staminaFillImage.fillAmount = ratio;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = ratio < 0.99f ? 1f : 0f;
            }
        }
    }
}