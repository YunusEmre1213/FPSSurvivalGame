using UnityEngine;

namespace Project.Gameplay.Enemies
{
    public class EnemyChaseState : IEnemyState
    {
        private readonly EnemyController _enemy;
        private float _timeSinceLastSeen;

        public EnemyChaseState(EnemyController enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _enemy.Agent.isStopped = false;
            _timeSinceLastSeen = 0f;
        }

        public void Update()
        {
            bool canSee = _enemy.CanSeePlayer();
            float distance = _enemy.DistanceToPlayer();

            _timeSinceLastSeen = canSee ? 0f : _timeSinceLastSeen + Time.deltaTime;

            if (distance > _enemy.LoseTargetRange || _timeSinceLastSeen > _enemy.SightMemoryDuration)
            {
                _enemy.ChangeState(new EnemyInvestigateState(_enemy, _enemy.Player.position));
                return;
            }

            if (canSee && distance <= _enemy.AttackRange)
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