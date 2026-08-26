namespace Project.Gameplay.Enemies
{
    public interface IEnemyState
    {
        void Enter();
        void Update();
        void Exit();
    }
}