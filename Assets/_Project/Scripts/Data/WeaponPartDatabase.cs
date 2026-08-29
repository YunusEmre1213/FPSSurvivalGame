using System.Collections.Generic;
using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "WeaponPartDatabase", menuName = "MobilFPS/Weapon Part Database")]
    public class WeaponPartDatabase : ScriptableObject
    {
        public List<WeaponPartData> allParts = new List<WeaponPartData>();

        public WeaponPartData FindById(string partId)
        {
            foreach (var part in allParts)
            {
                if (part != null && part.partId == partId)
                {
                    return part;
                }
            }
            return null;
        }
    }
}