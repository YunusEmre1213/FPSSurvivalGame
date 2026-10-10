using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "NewAxe", menuName = "MobilFPS/Axe")]
    public class AxeData : ItemData
    {
        [Header("Balta Ozellikleri")]
        [Tooltip("Her vurusta agaca verilen hasar")]
        public int chopDamage = 1;

        [Header("Modeller ve Sesler")]
        public GameObject axeModelPrefab;
        public AudioClip swingSound;
        public AudioClip hitSound;
    }
}