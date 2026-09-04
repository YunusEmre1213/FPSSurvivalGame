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

        private void Update()
        {
            if (playerNeeds == null) return;

            hungerFillImage.fillAmount = playerNeeds.Hunger / playerNeeds.MaxValue;
            thirstFillImage.fillAmount = playerNeeds.Thirst / playerNeeds.MaxValue;
        }
    }
}