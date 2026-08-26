using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "NewWeaponBase", menuName = "MobilFPS/Weapon Base")]
    public class WeaponBaseData : ScriptableObject
    {
        public string weaponName = "Yeni Silah";

        [Header("Taban stat'lar (parca eklenmeden once)")]
        public float baseDamage = 10f;
        [Range(0f, 1f)] public float baseAccuracy = 0.7f;
        public float baseFireRate = 2f;
        public float baseRecoil = 0.2f;
        public float baseDurability = 100f;
        public int baseAmmoCapacity = 10;
    }
}