namespace Project.Gameplay.Player
{
    public static class ActiveInput
    {
        public static IInputProvider Provider { get; private set; }

        public static void SetProvider(IInputProvider provider)
        {
            Provider = provider;
        }
    }
}