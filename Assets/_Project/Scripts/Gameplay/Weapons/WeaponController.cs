using UnityEngine;
using Project.Core;
using Project.Data;
using Project.Systems;
using Project.Gameplay.Player;
using Project.Gameplay.Enemies;

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
        [Tooltip("Kameraya bagli, silah modelinin yerlestirilecegi bos transform.")]
        [SerializeField] private Transform weaponSocket;

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
        [Tooltip("Her atisin yaydigi gurultu yaricapi - bu mesafedeki dusmanlar seni gormeseler bile duyup arastirmaya gelir.")]
        [SerializeField] private float gunshotNoiseRadius = 25f;

        [Header("Ses")]
        [SerializeField] private AudioSource weaponAudioSource;
        [SerializeField] private AudioClip fireSound;
        [Tooltip("Sarjor bosken tetige basinca calinan 'tik' sesi.")]
        [SerializeField] private AudioClip emptySound;
        [SerializeField] private AudioClip reloadSound;

        [Header("Geri tepme (recoil) animasyonu")]
        [Tooltip("Her atista modelin ani olarak kaydigi yerel konum (genelde geriye, -Z).")]
        [SerializeField] private Vector3 recoilKickPosition = new Vector3(0f, 0f, -0.06f);
        [Tooltip("Her atista modelin ani olarak dondugu aci (genelde yukari, negatif X = namlu yukari kalkar).")]
        [SerializeField] private Vector3 recoilKickRotation = new Vector3(-6f, 0f, 0f);
        [Tooltip("Tekme sonrasi normale donme hizi - buyuk deger = hizli toparlanma.")]
        [SerializeField] private float recoilRecoverySpeed = 10f;

        [Header("Hareket sallanmasi (weapon bob)")]
        [SerializeField] private float walkBobSpeed = 8f;
        [SerializeField] private float walkBobAmplitude = 0.015f;
        [SerializeField] private float sprintBobSpeed = 13f;
        [SerializeField] private float sprintBobAmplitude = 0.03f;
        [Tooltip("Baslama/durma gecisinin yumusakligi - buyuk deger = daha ani gecis.")]
        [SerializeField] private float bobBlendSpeed = 6f;

        [Header("Gorsel efektler")]
        [Tooltip("Silah modelinin Animator Controller'indaki tetikleyici (Trigger) parametre adi.")]
        [SerializeField] private string fireAnimationTrigger = "Fire";
        [Tooltip("Doldurma animasyonunu tetikleyen Trigger parametre adi - yoksa bos birak.")]
        [SerializeField] private string reloadAnimationTrigger = "Reload";
        [SerializeField] private GameObject muzzleFlashPrefab;
        [Tooltip("Silah modelinin ICINDE, namlu ucuna yakin bos objenin ADI - her yeni model kopyasinda bu isimle aranir, sabit referans TUTULMAZ.")]
        [SerializeField] private string muzzlePointName = "MuzzlePoint";

        [Header("Sarjor dusurme efekti")]
        [Tooltip("Doldururken yere dusen, fizige tabi eski sarjor prefabi (paketteki 'Magazine' prefabi).")]
        [SerializeField] private GameObject magazineDropPrefab;
        [Tooltip("Sarjorun dusecegi baslangic noktasinin ADI - silah modeli icinde, sarjor yuvasina yakin bos bir obje olmali.")]
        [SerializeField] private string magazinePointName = "MagazinePoint";
        [SerializeField] private float magazineDropForce = 1.5f;
        [SerializeField] private float magazineDespawnDelay = 3f;

        [Header("Doldurma - prosedurel egilme animasyonu")]
        [Tooltip("Hazir bir Animator state'i olmasa bile calisir - silah bu sure boyunca asagi/yana egilip geri kalkar.")]
        [SerializeField] private float reloadTiltDuration = 0.9f;
        [SerializeField] private Vector3 reloadTiltPosition = new Vector3(-0.08f, -0.15f, 0f);
        [SerializeField] private Vector3 reloadTiltRotation = new Vector3(25f, 0f, -18f);

        private IInputProvider _input;
        private WeaponAssembly _assembly;
        private IFireStrategy _fireStrategy;
        private WeaponStats _stats;
        private int _currentMagazineAmmo;
        private GameObject _weaponModelInstance;
        private Vector3 _recoilPosOffset;
        private Vector3 _recoilRotOffset;
        private Animator _weaponAnimator;
        private Transform _muzzlePoint;
        private Transform _magazinePoint;
        private float _bobTimer;
        private float _bobAmplitude;
        private Vector3 _bobOffset;
        private float _reloadTimer;
        private Vector3 _reloadPosOffset;
        private Vector3 _reloadRotOffset;

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

            SpawnWeaponModel();

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

            UpdateRecoilRecovery();
            UpdateWeaponBob();
            UpdateReloadTilt();
            ApplyWeaponTransform();
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
        private void SpawnWeaponModel()
        {
            if (weaponSocket == null || baseWeapon.weaponModelPrefab == null) return;

            if (_weaponModelInstance != null)
            {
                Destroy(_weaponModelInstance);
            }

            _weaponModelInstance = Instantiate(baseWeapon.weaponModelPrefab, weaponSocket);
            _weaponModelInstance.transform.localPosition = baseWeapon.modelPositionOffset;
            _weaponModelInstance.transform.localRotation = Quaternion.Euler(baseWeapon.modelRotationOffset);

            _weaponAnimator = _weaponModelInstance.GetComponentInChildren<Animator>();

            _muzzlePoint = FindDeepChild(_weaponModelInstance.transform, muzzlePointName);
            if (_muzzlePoint == null)
            {
                Debug.LogWarning($"[WeaponController] Model icinde '{muzzlePointName}' adinda bir obje bulunamadi - namlu alevi calismayacak.");
            }

            _magazinePoint = FindDeepChild(_weaponModelInstance.transform, magazinePointName);
            if (_magazinePoint == null)
            {
                Debug.LogWarning($"[WeaponController] Model icinde '{magazinePointName}' adinda bir obje bulunamadi - sarjor dusurme efekti calismayacak.");
            }
        }
        private Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;

                var found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
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
            PlaySound(reloadSound);
            PlayReloadAnimation();
            DropMagazine();
            _reloadTimer = reloadTiltDuration;
            Debug.Log($"[WeaponController] Doldurma: +{removed} mermi, sarjor: {_currentMagazineAmmo}/{_stats.AmmoCapacity}");
        }

        private void AttemptFire()
        {
            if (_currentMagazineAmmo <= 0)
            {
                Debug.Log("[WeaponController] *tik* - sarjor bos, R ile doldur.");
                PlaySound(emptySound);
                return;
            }

            _currentMagazineAmmo--;
            EventBus.Publish(new AmmoChangedEvent(_currentMagazineAmmo, _stats.AmmoCapacity));
            EventBus.Publish(new NoiseEvent(transform.position, gunshotNoiseRadius));
            PlaySound(fireSound);
            ApplyRecoilKick();
            PlayFireAnimation();
            SpawnMuzzleFlash();

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

        private void PlaySound(AudioClip clip)
        {
            if (weaponAudioSource != null && clip != null)
            {
                weaponAudioSource.PlayOneShot(clip);
            }
        }
        private void ApplyRecoilKick()
        {
            _recoilPosOffset += recoilKickPosition;
            _recoilRotOffset += recoilKickRotation;
        }

        private void UpdateRecoilRecovery()
        {
            _recoilPosOffset = Vector3.Lerp(_recoilPosOffset, Vector3.zero, recoilRecoverySpeed * Time.deltaTime);
            _recoilRotOffset = Vector3.Lerp(_recoilRotOffset, Vector3.zero, recoilRecoverySpeed * Time.deltaTime);
        }

        private void UpdateWeaponBob()
        {
            bool isMoving = _input.MoveInput.sqrMagnitude > 0.01f;
            bool isSprinting = isMoving && _input.SprintHeld;

            float targetSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
            float targetAmplitude = isMoving ? (isSprinting ? sprintBobAmplitude : walkBobAmplitude) : 0f;

            _bobAmplitude = Mathf.Lerp(_bobAmplitude, targetAmplitude, bobBlendSpeed * Time.deltaTime);

            if (isMoving)
            {
                _bobTimer += Time.deltaTime * targetSpeed;
            }

            _bobOffset = new Vector3(
                Mathf.Sin(_bobTimer) * _bobAmplitude,
                Mathf.Sin(_bobTimer * 2f) * _bobAmplitude * 0.5f,
                0f);
        }

        private void ApplyWeaponTransform()
        {
            if (_weaponModelInstance == null) return;

            _weaponModelInstance.transform.localPosition = baseWeapon.modelPositionOffset + _recoilPosOffset + _bobOffset + _reloadPosOffset;
            _weaponModelInstance.transform.localRotation = Quaternion.Euler(baseWeapon.modelRotationOffset + _recoilRotOffset + _reloadRotOffset);
        }

        private void PlayFireAnimation()
        {
            if (_weaponAnimator != null)
            {
                _weaponAnimator.SetTrigger(fireAnimationTrigger);
            }
        }

        private void SpawnMuzzleFlash()
        {
            if (muzzleFlashPrefab == null || _muzzlePoint == null) return;

            var flash = Instantiate(muzzleFlashPrefab, _muzzlePoint.position, _muzzlePoint.rotation, _muzzlePoint);
            Destroy(flash, 0.2f); 
        }
        private void PlayReloadAnimation()
        {
            if (_weaponAnimator == null || string.IsNullOrEmpty(reloadAnimationTrigger)) return;

            foreach (var param in _weaponAnimator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger && param.name == reloadAnimationTrigger)
                {
                    _weaponAnimator.SetTrigger(reloadAnimationTrigger);
                    return;
                }
            }
        }
        private void UpdateReloadTilt()
        {
            if (_reloadTimer > 0f)
            {
                _reloadTimer -= Time.deltaTime;
                float t = Mathf.Clamp01(_reloadTimer / reloadTiltDuration); 
                float curve = Mathf.Sin(t * Mathf.PI); 

                _reloadPosOffset = reloadTiltPosition * curve;
                _reloadRotOffset = reloadTiltRotation * curve;
            }
            else
            {
                _reloadPosOffset = Vector3.zero;
                _reloadRotOffset = Vector3.zero;
            }
        }
        private void DropMagazine()
        {
            if (magazineDropPrefab == null || _magazinePoint == null) return;

            var magazineInstance = Instantiate(magazineDropPrefab, _magazinePoint.position, _magazinePoint.rotation);

            var rb = magazineInstance.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = magazineInstance.AddComponent<Rigidbody>();
            }

            var pushDirection = -_magazinePoint.up + Random.insideUnitSphere * 0.3f;
            rb.AddForce(pushDirection * magazineDropForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

            Destroy(magazineInstance, magazineDespawnDelay);
        }
    }
}