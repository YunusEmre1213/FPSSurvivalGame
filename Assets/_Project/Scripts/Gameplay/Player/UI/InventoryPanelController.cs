using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Project.Core;
using Project.Data;
using Project.Systems;
using Project.Gameplay.Player;
using Project.Gameplay.Items;

namespace Project.UI
{
    public class InventoryPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotUIPrefab;
        [SerializeField] private PlayerNeeds playerNeeds;
        [SerializeField] private FlashlightController flashlightController;

        private readonly List<SlotUI> _slotUIs = new List<SlotUI>();
        private bool _isOpen;

        private void Awake()
        {
            panelRoot.SetActive(false);
        }

        private void Start()
        {
            BuildSlots();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InventorySlotChangedEvent>(OnInventoryChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InventorySlotChangedEvent>(OnInventoryChanged);
        }

        private void BuildSlots()
        {
            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                var instance = Instantiate(slotUIPrefab, slotContainer);
                var slotUI = instance.GetComponent<SlotUI>();
                _slotUIs.Add(slotUI);

                var button = instance.GetComponent<Button>();

                int slotIndex = i; 
                button.onClick.AddListener(() => OnSlotClicked(slotIndex));
            }
        }

        private void Update()
        {
            var input = ActiveInput.Provider;
            if (input == null || !input.InventoryPressedThisFrame) return;

            if (_isOpen)
            {
                Close();
            }
            else if (!UIInputLock.IsLocked)
            {
                Open();
            }
        }

        private void OnInventoryChanged(InventorySlotChangedEvent evt)
        {
            if (_isOpen)
            {
                RefreshAll();
            }
        }

        private void OnSlotClicked(int slotIndex)
        {
            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            var slot = inventory.GetSlot(slotIndex);

            if (slot.IsEmpty) return;

            if (slot.Item is ConsumableData consumable && playerNeeds != null)
            {
                playerNeeds.Consume(consumable);
                inventory.RemoveItem(consumable, 1); 
            }
            else if (slot.Item is BatteryData battery && flashlightController != null)
            {
                flashlightController.Recharge(battery);
                inventory.RemoveItem(battery, 1);
            }
        }

        private void Open()
        {
            _isOpen = true;
            panelRoot.SetActive(true);
            UIInputLock.Lock();
            RefreshAll();
        }

        private void Close()
        {
            _isOpen = false;
            panelRoot.SetActive(false);
            UIInputLock.Unlock();
        }

        private void RefreshAll()
        {
            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            for (int i = 0; i < _slotUIs.Count; i++)
            {
                _slotUIs[i].Refresh(inventory.GetSlot(i));
            }
        }
    }
}