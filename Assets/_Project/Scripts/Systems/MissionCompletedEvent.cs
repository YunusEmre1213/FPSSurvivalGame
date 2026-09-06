using Project.Core;
using Project.Data;

namespace Project.Systems
{
    public readonly struct MissionCompletedEvent : IGameEvent
    {
        public readonly MissionData Mission;

        public MissionCompletedEvent(MissionData mission)
        {
            Mission = mission;
        }
    }
}