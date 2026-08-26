using UnityEngine;
using Project.Data;

namespace Project.Gameplay.Weapons
{
    public class WeaponAssemblyTest : MonoBehaviour
    {
        [Header("Taban silah")]
        [SerializeField] private WeaponBaseData baseWeapon;

        [Header("Temiz parcalar")]
        [SerializeField] private WeaponPartData barrel;
        [SerializeField] private WeaponPartData magazine;
        [SerializeField] private WeaponPartData stock;

        [Header("Riskli/bozulmus parca")]
        [SerializeField] private WeaponPartData corruptedSight;

        private void Start()
        {
            var cleanBuild = new WeaponAssembly(baseWeapon);
            cleanBuild.EquipPart(barrel);
            cleanBuild.EquipPart(magazine);
            cleanBuild.EquipPart(stock);

            Debug.Log($"[WeaponAssemblyTest] TEMIZ SETUP -> {cleanBuild.CalculateStats()}");

            var riskyBuild = new WeaponAssembly(baseWeapon);
            riskyBuild.EquipPart(barrel);
            riskyBuild.EquipPart(magazine);
            riskyBuild.EquipPart(stock);
            riskyBuild.EquipPart(corruptedSight);

            Debug.Log($"[WeaponAssemblyTest] RISKLI SETUP (bozulmus sight ile) -> {riskyBuild.CalculateStats()}");
        }
    }
}