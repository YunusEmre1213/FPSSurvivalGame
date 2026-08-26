using UnityEngine;

namespace Project.Gameplay.Enemies
{
    public class EnemyAttackState : IEnemyState
    {
        private readonly EnemyController _enemy;
        private float _attackTimer;

        public EnemyAttackState(EnemyController enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _enemy.Agent.isStopped = true;
            _attackTimer = 0f;
        }

        public void Update()
        {
            float distance = _enemy.DistanceToPlayer();

            if (distance > _enemy.AttackRange)
            {
                _enemy.ChangeState(new EnemyChaseState(_enemy));
                return;
            }

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _enemy.DealDamageToPlayer();
                _attackTimer = _enemy.AttackCooldown;
            }
        }

        public void Exit()
        {
            _enemy.Agent.isStopped = false;
        }
    }
}