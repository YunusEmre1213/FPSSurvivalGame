using UnityEngine;
using Project.Data;
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
        [Tooltip("IInputProvider implemente eden bilesen (MobileInputProvider veya KeyboardMouseInputProvider).")]
        [SerializeField] private MonoBehaviour inputProviderSource;
        [SerializeField] private Camera fireCamera;

        [Header("Silah verisi")]
        [SerializeField] private WeaponBaseData baseWeapon;
        [SerializeField] private WeaponPartData barrel;
        [SerializeField] private WeaponPartData magazine;
        [SerializeField] private WeaponPartData stock;

        [Header("Ates modu")]
        [SerializeField] private FireMode fireMode = FireMode.SemiAuto;
        [SerializeField] private float maxRange = 100f;

        private IInputProvider _input;
        private WeaponAssembly _assembly;
        private IFireStrategy _fireStrategy;
        private WeaponStats _stats;

        private void Awake()
        {
            _input = inputProviderSource as IInputProvider;
            if (_input == null)
            {
                Debug.LogError("[WeaponController] inputProviderSource alani IInputProvider implemente etmiyor.");
            }

            _assembly = new WeaponAssembly(baseWeapon);
            _assembly.EquipPart(barrel);
            _assembly.EquipPart(magazine);
            _assembly.EquipPart(stock);
            _stats = _assembly.CalculateStats();

            // Strategy Pattern hangi somut sinifin kullanilacagina burada tek bir yerde karar veriliyor.
            _fireStrategy = fireMode == FireMode.SemiAuto
                ? new SemiAutoFireStrategy()
                : new AutoFireStrategy();

            Debug.Log($"[WeaponController] {baseWeapon.weaponName} hazir ({fireMode}) -> {_stats}");
        }

        private void Update()
        {
            if (_input == null) return;

            bool shouldFire = _fireStrategy.TryFire(_input.FireHeld, _input.FirePressedThisFrame, _stats.FireRate);
            if (shouldFire)
            {
                AttemptFire();
            }
        }

        private void AttemptFire()
        {
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