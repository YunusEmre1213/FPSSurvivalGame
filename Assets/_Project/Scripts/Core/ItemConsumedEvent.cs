namespace Project.Core
{
    public readonly struct ItemConsumedEvent : IGameEvent
    {
        public readonly string ItemName;
        public readonly float HungerRestore;
        public readonly float ThirstRestore;
        public readonly bool CausedSickness;

        public ItemConsumedEvent(string itemName, float hungerRestore, float thirstRestore, bool causedSickness)
        {
            ItemName = itemName;
            HungerRestore = hungerRestore;
            ThirstRestore = thirstRestore;
            CausedSickness = causedSickness;
        }
    }
}