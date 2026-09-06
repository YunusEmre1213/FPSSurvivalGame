using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "NewObjective", menuName = "MobilFPS/Mission/Objective")]
    public class ObjectiveData : ScriptableObject
    {
        [Header("Kimlik")]
        public string objectiveId;
        [TextArea] public string description;
        public ObjectiveType type;

        [Header("CollectItem icin")]
        public ItemData requiredItem;
        public int requiredAmount = 1;

        [Header("KillEnemies icin")]
        public int requiredKillCount = 1;

        [Header("ReachLocation / InteractWithTarget icin")]
        [Tooltip("Sahnedeki hedef objenin ObjectiveTarget bilesenindeki Target Id ile BIREBIR eslesmeli.")]
        public string targetId;
    }
}