using UnityEngine;
using Project.Core;
using Project.Systems;

namespace Project.Gameplay.Items
{
    [RequireComponent(typeof(Collider))]
    public class SectorGate : MonoBehaviour
    {
        [SerializeField] private string requiredKeyId;

        private Collider _blockingCollider;
        private bool _isOpen;

        private void Awake()
        {
            _blockingCollider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (_isOpen) return;

            var keyItems = ServiceLocator.Instance.Get<IKeyItemService>();
            if (keyItems.HasKey(requiredKeyId))
            {
                Open();
            }
        }

        private void Open()
        {
            _isOpen = true;
            _blockingCollider.enabled = false;
            Debug.Log($"[SectorGate] {gameObject.name} acildi - '{requiredKeyId}' elde edildi.");

            gameObject.SetActive(false);
        }
    }
}