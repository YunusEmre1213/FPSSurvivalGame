using UnityEngine;
using TMPro;
using Project.Core;
using Project.Systems;

namespace Project.UI
{
    public class ObjectiveHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text objectiveText;

        private void OnEnable()
        {
            EventBus.Subscribe<ObjectiveChangedEvent>(OnObjectiveChanged);
            EventBus.Subscribe<MissionCompletedEvent>(OnMissionCompleted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ObjectiveChangedEvent>(OnObjectiveChanged);
            EventBus.Unsubscribe<MissionCompletedEvent>(OnMissionCompleted);
        }

        private void OnObjectiveChanged(ObjectiveChangedEvent evt)
        {
            objectiveText.text = evt.NewObjective != null ? $"Görev: {evt.NewObjective.description}" : "";
        }

        private void OnMissionCompleted(MissionCompletedEvent evt)
        {
            objectiveText.text = $"Görev tamamlandý: {evt.Mission.missionName}";
        }
    }
}