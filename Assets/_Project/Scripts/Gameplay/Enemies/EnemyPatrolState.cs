using UnityEngine;
using UnityEngine.AI;

namespace Project.Gameplay.Enemies
{
    public class EnemyPatrolState : IEnemyState
    {
        private readonly EnemyController _enemy;
        private float _waitTimer;
        private bool _isWaiting;

        public EnemyPatrolState(EnemyController enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _isWaiting = false;
            PickNewDestination();
        }

        public void Update()
        {
            if (_enemy.DistanceToPlayer() <= _enemy.DetectionRange)
            {
                _enemy.ChangeState(new EnemyChaseState(_enemy));
                return;
            }

            if (!_enemy.Agent.pathPending && _enemy.Agent.remainingDistance <= _enemy.Agent.stoppingDistance)
            {
                if (!_isWaiting)
                {
                    _isWaiting = true;
                    _waitTimer = _enemy.PatrolWaitTime;
                }
                else
                {
                    _waitTimer -= Time.deltaTime;
                    if (_waitTimer <= 0f)
                    {
                        _isWaiting = false;
                        PickNewDestination();
                    }
                }
            }
        }

        public void Exit()
        {
        }

        private void PickNewDestination()
        {
            var randomOffset = Random.insideUnitSphere * _enemy.PatrolRadius;
            randomOffset.y = 0f;
            var destination = _enemy.SpawnPosition + randomOffset;

            if (NavMesh.SamplePosition(destination, out var hit, _enemy.PatrolRadius, NavMesh.AllAreas))
            {
                _enemy.Agent.SetDestination(hit.position);
            }
        }
    }
}