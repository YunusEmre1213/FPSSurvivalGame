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
        [Tooltip("Kosarken gorunur, dinlenirken kaybolur.")]
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("Saniyede ne kadar hizli belirip kaybolacagi (alpha/saniye).")]
        [SerializeField] private float fadeSpeed = 6f;

        private void Update()
        {
            if (playerStamina == null) return;

            staminaFillImage.fillAmount = playerStamina.CurrentStamina / playerStamina.MaxStamina;

            if (canvasGroup != null)
            {
                float targetAlpha = playerStamina.IsSprinting ? 1f : 0f;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            }
        }
    }
}