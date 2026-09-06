using UnityEngine;
using Project.Core;
using Project.Data;
using Project.Systems;

namespace Project.Gameplay.Missions
{
    public class MissionStarter : MonoBehaviour
    {
        [SerializeField] private MissionData startingMission;

        private void Start()
        {
            ServiceLocator.Instance.Get<IMissionService>().StartMission(startingMission);
        }
    }
}