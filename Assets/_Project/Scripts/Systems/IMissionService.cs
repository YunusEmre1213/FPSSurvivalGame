using Project.Core;
using Project.Data;

namespace Project.Systems
{
    public interface IMissionService : IGameService
    {
        MissionData ActiveMission { get; }
        ObjectiveData CurrentObjective { get; }

        void StartMission(MissionData mission);
    }
}