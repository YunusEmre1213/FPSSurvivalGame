using UnityEngine;
using Project.Core;
using Project.Data;
using Project.Systems;
using Project.Gameplay.Player;

namespace Project.Gameplay.Weapons
{
    public enum FireMode
    {
        SemiAuto,
        Automatic
    }
    public class WeaponController : MonoBehaviour
    {
        [Header("Bagimliliklar")]
        [SerializeField] private Camera fireCamera;

        [Header("Silah verisi")]
        [SerializeField] private WeaponBaseData baseWeapon;
        [SerializeField] private WeaponPartData barrel;
        [SerializeField] private WeaponPartData magazine;
        [SerializeField] private WeaponPartData stock;

        [Header("Mermi")]
        [Tooltip("Envanterde tutulan mermi esyasi - dolum bu ItemData'dan cekilir.")]
        [SerializeField] private ResourceData ammoItem;

        [Header("Ates modu")]
        [SerializeField] private FireMode fireMode = FireMode.SemiAuto;
        [SerializeField] private float maxRange = 100f;

        private IInputProvider _input;
        private WeaponAssembly _assembly;
        private IFireStrategy _fireStrategy;
        private WeaponStats _stats;
        private int _currentMagazineAmmo;

        private void Awake()
        {
            _assembly = new WeaponAssembly(baseWeapon);
            _assembly.EquipPart(barrel);
            _assembly.EquipPart(magazine);
            _assembly.EquipPart(stock);
            _stats = _assembly.CalculateStats();

            _fireStrategy = fireMode == FireMode.SemiAuto
                ? new SemiAutoFireStrategy()
                : new AutoFireStrategy();

            _currentMagazineAmmo = 0;

            Debug.Log($"[WeaponController] {baseWeapon.weaponName} hazir ({fireMode}) -> {_stats}");
        }

        private void Start()
        {
            _input = ActiveInput.Provider;
            if (_input == null)
            {
                Debug.LogError("[WeaponController] ActiveInput.Provider hala null - sahnede InputSourceSelector var mi kontrol et.");
            }

            EventBus.Publish(new AmmoChangedEvent(_currentMagazineAmmo, _stats.AmmoCapacity));
        }

        private void Update()
        {
            if (_input == null) return;

            if (_input.ReloadPressedThisFrame)
            {
                TryReload();
            }

            bool shouldFire = _fireStrategy.TryFire(_input.FireHeld, _input.FirePressedThisFrame, _stats.FireRate);
            if (shouldFire)
            {
                AttemptFire();
            }
        }
        public void ApplyAssembly(WeaponPartData barrelPart, WeaponPartData magazinePart,
            WeaponPartData stockPart, WeaponPartData sightPart)
        {
            _assembly = new WeaponAssembly(baseWeapon);
            if (barrelPart != null) _assembly.EquipPart(barrelPart);
            if (magazinePart != null) _assembly.EquipPart(magazinePart);
            if (stockPart != null) _assembly.EquipPart(stockPart);
            if (sightPart != null) _assembly.EquipPart(sightPart);

            _stats = _assembly.CalculateStats();
            _currentMagazineAmmo = Mathf.Min(_currentMagazineAmmo, _stats.AmmoCapacity);

            EventBus.Publish(new AmmoChangedEvent(_currentMagazineAmmo, _stats.AmmoCapacity));
            Debug.Log($"[WeaponController] Yeni kurulum uygulandi -> {_stats}");
        }
        public WeaponPartData GetEquippedPart(WeaponPartType slot)
        {
            return _assembly?.GetEquippedPart(slot);
        }

        private void TryReload()
        {
            int needed = _stats.AmmoCapacity - _currentMagazineAmmo;
            if (needed <= 0)
            {
                Debug.Log("[WeaponController] Sarjor zaten dolu.");
                return;
            }

            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            int removed = inventory.RemoveItem(ammoItem, needed);

            if (removed <= 0)
            {
                Debug.Log("[WeaponController] Envanterde mermi yok - once mermi bulman gerekiyor.");
                return;
            }

            _currentMagazineAmmo += removed;
            EventBus.Publish(new AmmoChangedEvent(_currentMagazineAmmo, _stats.AmmoCapacity));
            Debug.Log($"[WeaponController] Doldurma: +{removed} mermi, sarjor: {_currentMagazineAmmo}/{_stats.AmmoCapacity}");
        }

        private void AttemptFire()
        {
            if (_currentMagazineAmmo <= 0)
            {
                Debug.Log("[WeaponController] *tik* - sarjor bos, R ile doldur.");
                return;
            }

            _currentMagazineAmmo--;
            EventBus.Publish(new AmmoChangedEvent(_currentMagazineAmmo, _stats.AmmoCapacity));

            if (Random.value < _stats.MalfunctionChance)
            {
                Debug.Log("[WeaponController] ARIZA! Silah sikisti, atis iptal.");
                return;
            }

            var origin = fireCamera.transform.position;
            var direction = fireCamera.transform.forward;

            if (Physics.Raycast(origin, direction, out var hit, maxRange))
            {
                var damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(_stats.Damage);
                    Debug.Log($"[WeaponController] Vurdu: {hit.collider.name} - {_stats.Damage} hasar");
                }
                else
                {
                    Debug.Log($"[WeaponController] Isabet ama hasar alamaz: {hit.collider.name}");
                }
            }
            else
            {
                Debug.Log("[WeaponController] Iskalandi (hicbir seye isabet etmedi)");
            }
        }
    }
}