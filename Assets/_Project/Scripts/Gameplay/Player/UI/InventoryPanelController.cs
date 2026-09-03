using System.Collections.Generic;
using UnityEngine;
using Project.Core;
using Project.Systems;
using Project.Gameplay.Player;

namespace Project.UI
{
    public class InventoryPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotUIPrefab;

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
                _slotUIs.Add(instance.GetComponent<SlotUI>());
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