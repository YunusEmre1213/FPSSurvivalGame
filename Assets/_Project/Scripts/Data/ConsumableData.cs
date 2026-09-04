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
    }
}