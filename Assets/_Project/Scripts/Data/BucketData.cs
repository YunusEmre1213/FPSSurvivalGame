using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "NewBucket", menuName = "MobilFPS/Bucket")]
    public class BucketData : ItemData
    {
        [Header("Model")]
        public GameObject bucketModelPrefab;

        [Header("Sesler")]
        public AudioClip fillSound;
        public AudioClip emptySound;
    }
}