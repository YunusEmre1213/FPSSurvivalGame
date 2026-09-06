using Project.Core;
using Project.Data;

namespace Project.Systems
{
    public readonly struct ObjectiveChangedEvent : IGameEvent
    {
        public readonly ObjectiveData NewObjective;

        public ObjectiveChangedEvent(ObjectiveData newObjective)
        {
            NewObjective = newObjective;
        }
    }
}