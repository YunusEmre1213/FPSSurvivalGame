using UnityEngine;

namespace Project.Gameplay.Enemies
{
    public class EnemyAttackState : IEnemyState
    {
        private readonly EnemyController _enemy;
        private float _attackTimer;
        private bool _isWindingUp;
        private float _windupTimer;

        public EnemyAttackState(EnemyController enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _enemy.Agent.isStopped = true;
            _attackTimer = 0f;
            _isWindingUp = false;
        }

        public void Update()
        {
            float distance = _enemy.DistanceToPlayer();

            if (distance > _enemy.AttackRange || !_enemy.CanSeePlayer())
            {
                _enemy.ChangeState(new EnemyChaseState(_enemy));
                return;
            }

            if (_isWindingUp)
            {
                _windupTimer -= Time.deltaTime;
                if (_windupTimer <= 0f)
                {
                    _enemy.DealDamageToPlayer();
                    _isWindingUp = false;
                    _attackTimer = _enemy.AttackCooldown;
                }
                return;
            }

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                if (_enemy.AttackWindupTime > 0f)
                {
                    _isWindingUp = true;
                    _windupTimer = _enemy.AttackWindupTime;
                    Debug.Log($"[EnemyAttackState] {_enemy.name} saldiri hazirliginda...");
                }
                else
                {
                    _enemy.DealDamageToPlayer();
                    _attackTimer = _enemy.AttackCooldown;
                }
            }
        }

        public void Exit()
        {
            _enemy.Agent.isStopped = false;
        }
    }
}