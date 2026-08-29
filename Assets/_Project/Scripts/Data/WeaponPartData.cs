using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "NewWeaponPart", menuName = "MobilFPS/Weapon Part")]
    public class WeaponPartData : ScriptableObject
    {
        [Header("Kimlik")]
        [Tooltip("Kayit sisteminde bu parcayi tanimlayan KALICI ID. Asset adiyla ayni tutmani oneririm (ornegin 'Barrel_Standart'). Bir kere belirledikten sonra DEGISTIRME - degistirirsen eski save dosyalari bu parcayi bulamaz.")]
        public string partId;
        public string partName = "Yeni Parca";
        public WeaponPartType partType;

        [Header("Stat degistiricileri (taban stat'lara eklenir)")]
        public float damageModifier;
        [Tooltip("Pozitif = daha isabetli, negatif = daha az isabetli.")]
        public float accuracyModifier;
        [Tooltip("Saniyedeki atis sayisina eklenir.")]
        public float fireRateModifier;
        [Tooltip("Pozitif = daha fazla geri tepme (kotu), negatif = daha az geri tepme (iyi).")]
        public float recoilModifier;
        public float durabilityModifier;
        [Tooltip("Sarjor kapasitesine eklenir (genelde sadece Magazine tipi parcalarda kullanilir).")]
        public int ammoCapacityModifier;

        [Header("Bozulmus parca ozellikleri")]
        [Tooltip("0 = temiz parca, guvenilir. 0'dan buyukse bozulmus parca, her atiste ariza riski tasir.")]
        [Range(0f, 1f)]
        public float malfunctionChance = 0f;

        public bool IsCorrupted => malfunctionChance > 0f;
    }
}