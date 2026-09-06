using Project.Core;

namespace Project.Gameplay.Interaction
{
    public readonly struct ObjectiveTargetTriggeredEvent : IGameEvent
    {
        public readonly string TargetId;

        public ObjectiveTargetTriggeredEvent(string targetId)
        {
            TargetId = targetId;
        }
    }
}