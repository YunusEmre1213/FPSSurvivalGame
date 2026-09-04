using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "NewWeaponBase", menuName = "MobilFPS/Weapon Base")]
    public class WeaponBaseData : ScriptableObject
    {
        public string weaponName = "Yeni Silah";

        [Header("Gorsel")]
        [Tooltip("Kameraya bagli silah yuvasina yerlestirilecek 3D model prefabi. Yeni bir silah eklemek icin sadece yeni bir WeaponBaseData olusturup kendi prefabini atamak yeterli.")]
        public GameObject weaponModelPrefab;
        [Tooltip("Modelin yuvaya gore konum/rotasyon ofseti - farkli modellerin pivot noktasi farkli olabilir, bu sayede kod degistirmeden her modeli 'elde tutuyormus gibi' hizalayabilirsin.")]
        public Vector3 modelPositionOffset;
        public Vector3 modelRotationOffset;

        [Header("Taban stat'lar (parca eklenmeden once)")]
        public float baseDamage = 10f;
        [Range(0f, 1f)] public float baseAccuracy = 0.7f;
        public float baseFireRate = 2f;
        public float baseRecoil = 0.2f;
        public float baseDurability = 100f;
        public int baseAmmoCapacity = 10;
    }
}