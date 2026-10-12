using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "NewConsumable", menuName = "MobilFPS/Consumable")]
    public class ConsumableData : ItemData
    {
        [Header("Geri kazanim")]
        [Tooltip("Tuketilince aclik degerine eklenir (0-100 araliginda dusunulmeli).")]
        public float hungerRestore;
        [Tooltip("Tuketilince susuzluk degerine eklenir (0-100 araliginda dusunulmeli).")]
        public float thirstRestore;

        [Header("Risk (ozellikle cig et icin)")]
        [Range(0f, 1f)]
        [Tooltip("Tuketildiginde hastalanma olasiligi. 0 = risk yok (pismis yiyecekler icin boyle birak).")]
        public float sicknessChance;
        [Tooltip("Hastalik tetiklenirse kaybedilecek can miktari.")]
        public float sicknessDamage;
    }
}