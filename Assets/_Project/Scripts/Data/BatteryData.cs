using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "NewBattery", menuName = "MobilFPS/Battery")]
    public class BatteryData : ItemData
    {
        [Tooltip("Tuketilince fenerin piline eklenir.")]
        public float chargeRestore = 50f;
    }
}