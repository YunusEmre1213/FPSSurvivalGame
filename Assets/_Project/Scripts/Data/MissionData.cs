using System.Collections.Generic;
using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "NewMission", menuName = "MobilFPS/Mission/Mission")]
    public class MissionData : ScriptableObject
    {
        public string missionId;
        public string missionName;
        [TextArea] public string description;
        public List<ObjectiveData> objectives = new List<ObjectiveData>();
    }
}