namespace Project.Gameplay.Enemies
{
    public class EnemyChaseState : IEnemyState
    {
        private readonly EnemyController _enemy;

        public EnemyChaseState(EnemyController enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _enemy.Agent.isStopped = false;
        }

        public void Update()
        {
            float distance = _enemy.DistanceToPlayer();

            if (distance > _enemy.LoseTargetRange)
            {
                _enemy.ChangeState(new EnemyPatrolState(_enemy));
                return;
            }

            if (distance <= _enemy.AttackRange)
            {
                _enemy.ChangeState(new EnemyAttackState(_enemy));
                return;
            }

            if (_enemy.Player != null)
            {
                _enemy.Agent.SetDestination(_enemy.Player.position);
            }
        }

        public void Exit()
        {
        }
    }
}