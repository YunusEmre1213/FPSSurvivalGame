using Project.Core;
using Project.Data;
using Project.Gameplay.Enemies;
using Project.Gameplay.Interaction;

namespace Project.Systems
{
    public class MissionService : IMissionService
    {
        public MissionData ActiveMission { get; private set; }
        public ObjectiveData CurrentObjective =>
            ActiveMission != null && _objectiveIndex < ActiveMission.objectives.Count
                ? ActiveMission.objectives[_objectiveIndex]
                : null;

        private int _objectiveIndex;
        private int _killCount;

        public void Initialize()
        {
            EventBus.Subscribe<InventorySlotChangedEvent>(OnInventoryChanged);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<ObjectiveTargetTriggeredEvent>(OnTargetTriggered);
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<InventorySlotChangedEvent>(OnInventoryChanged);
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<ObjectiveTargetTriggeredEvent>(OnTargetTriggered);
        }

        public void StartMission(MissionData mission)
        {
            ActiveMission = mission;
            _objectiveIndex = 0;
            _killCount = 0;

            UnityEngine.Debug.Log($"[MissionService] Gorev basladi: {mission.missionName}");
            EventBus.Publish(new ObjectiveChangedEvent(CurrentObjective));

            TryCompleteCollectObjective();
        }

        private void OnInventoryChanged(InventorySlotChangedEvent evt)
        {
            TryCompleteCollectObjective();
        }

        private void TryCompleteCollectObjective()
        {
            var objective = CurrentObjective;
            if (objective == null || objective.type != ObjectiveType.CollectItem) return;

            var inventory = ServiceLocator.Instance.Get<IItemInventoryService>();
            if (inventory.GetItemCount(objective.requiredItem) >= objective.requiredAmount)
            {
                CompleteCurrentObjective();
            }
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            var objective = CurrentObjective;
            if (objective == null || objective.type != ObjectiveType.KillEnemies) return;

            _killCount++;
            if (_killCount >= objective.requiredKillCount)
            {
                CompleteCurrentObjective();
            }
        }

        private void OnTargetTriggered(ObjectiveTargetTriggeredEvent evt)
        {
            var objective = CurrentObjective;
            if (objective == null) return;
            if (objective.type != ObjectiveType.ReachLocation && objective.type != ObjectiveType.InteractWithTarget) return;
            if (objective.targetId != evt.TargetId) return;

            CompleteCurrentObjective();
        }

        private void CompleteCurrentObjective()
        {
            UnityEngine.Debug.Log($"[MissionService] Hedef tamamlandi: {CurrentObjective.description}");

            _objectiveIndex++;
            _killCount = 0;

            if (_objectiveIndex >= ActiveMission.objectives.Count)
            {
                UnityEngine.Debug.Log($"[MissionService] GOREV TAMAMLANDI: {ActiveMission.missionName}");
                EventBus.Publish(new MissionCompletedEvent(ActiveMission));
                return;
            }

            EventBus.Publish(new ObjectiveChangedEvent(CurrentObjective));
            TryCompleteCollectObjective(); 
        }
    }
}