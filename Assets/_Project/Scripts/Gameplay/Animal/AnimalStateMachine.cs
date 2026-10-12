namespace Project.Gameplay.Animals
{
    public class AnimalStateMachine
    {
        public IAnimalState CurrentState { get; private set; }

        public void ChangeState(IAnimalState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Update()
        {
            CurrentState?.Update();
        }
    }
}