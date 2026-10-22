using UnityEngine;
using UnityEngine.UI;
using Project.Core;
using Project.Gameplay.Items;

namespace Project.UI
{
    public class CampfirePanelUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;

        [Header("Et Pisirme")]
        [SerializeField] private Button cookMeatButton;
        [SerializeField] private Text cookMeatButtonText;

        [Header("Su Kaynatma")]
        [SerializeField] private Button boilWaterButton;
        [SerializeField] private Text boilWaterButtonText;

        [Header("Odun Ekle")]
        [SerializeField] private Button addWoodButton;
        [SerializeField] private Text addWoodButtonText;

        [Header("Durum")]
        [SerializeField] private Text statusText;

        [Header("Kapat")]
        [SerializeField] private Button closeButton;

        [Header("Renkler")]
        [SerializeField] private Color availableColor = new Color(0.35f, 0.85f, 0.35f);
        [SerializeField] private Color unavailableColor = new Color(0.85f, 0.3f, 0.3f);

        private CampfireInteractable _currentCampfire;

        private void Awake()
        {
            cookMeatButton.onClick.AddListener(OnCookMeatPressed);
            boilWaterButton.onClick.AddListener(OnBoilWaterPressed);
            addWoodButton.onClick.AddListener(OnAddWoodPressed);
            closeButton.onClick.AddListener(Close);

            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void Update()
        {
            if (panelRoot != null && panelRoot.activeSelf && _currentCampfire != null)
            {
                Refresh();
            }
        }

        public void Open(CampfireInteractable campfire)
        {
            if (panelRoot != null && panelRoot.activeSelf) return;

            _currentCampfire = campfire;
            if (panelRoot != null) panelRoot.SetActive(true);
            UIInputLock.Lock();
            Refresh();
        }

        public void Close()
        {
            if (panelRoot == null || !panelRoot.activeSelf) return;

            _currentCampfire = null;
            panelRoot.SetActive(false);
            UIInputLock.Unlock();
        }

        public void CloseIfOwner(CampfireInteractable campfire)
        {
            if (_currentCampfire == campfire) Close();
        }

        private void Refresh()
        {
            if (_currentCampfire == null) return;

            bool busy = _currentCampfire.IsBusy;

            bool canCook = _currentCampfire.HasRawMeat() && !busy;
            cookMeatButton.interactable = canCook;
            cookMeatButtonText.text = _currentCampfire.RawMeatLabel;
            cookMeatButtonText.color = canCook ? availableColor : unavailableColor;

            bool canBoil = _currentCampfire.HasFullBucketReady() && !busy;
            boilWaterButton.interactable = canBoil;
            boilWaterButtonText.text = _currentCampfire.BoilLabel;
            boilWaterButtonText.color = canBoil ? availableColor : unavailableColor;

            bool canAddWood = _currentCampfire.HasWood();
            addWoodButton.interactable = canAddWood;
            addWoodButtonText.text = _currentCampfire.WoodLabel;
            addWoodButtonText.color = canAddWood ? availableColor : unavailableColor;

            if (statusText != null)
            {
                string status = _currentCampfire.StatusLabel;
                statusText.gameObject.SetActive(status != null);
                if (status != null) statusText.text = status;
            }
        }

        private void OnCookMeatPressed()
        {
            _currentCampfire?.StartCookingFromPanel();
            Refresh();
        }

        private void OnBoilWaterPressed()
        {
            _currentCampfire?.StartBoilingFromPanel();
            Refresh();
        }

        private void OnAddWoodPressed()
        {
            _currentCampfire?.AddWoodFromPanel();
            Refresh();
        }
    }
}