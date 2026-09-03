using UnityEngine;
using UnityEngine.AI;

namespace Project.Gameplay.Enemies
{
    public class EnemyFleeState : IEnemyState
    {
        private readonly EnemyController _enemy;
        private float _originalSpeed;

        public EnemyFleeState(EnemyController enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _enemy.Agent.isStopped = false;
            _originalSpeed = _enemy.Agent.speed;
            _enemy.Agent.speed = _originalSpeed * _enemy.FleeSpeedMultiplier;
            SetFleeDestination();
        }

        public void Update()
        {
            float distance = _enemy.DistanceToPlayer();

            if (distance >= _enemy.FleeSafeDistance)
            {
                _enemy.ChangeState(new EnemyPatrolState(_enemy));
                return;
            }
            if (!_enemy.Agent.pathPending && _enemy.Agent.remainingDistance <= _enemy.Agent.stoppingDistance)
            {
                SetFleeDestination();
            }
        }

        public void Exit()
        {
            _enemy.Agent.speed = _originalSpeed;
        }

        private void SetFleeDestination()
        {
            if (_enemy.Player == null) return;

            Vector3 awayFromPlayer = (_enemy.transform.position - _enemy.Player.position).normalized;
            Vector3 fleeTarget = _enemy.transform.position + awayFromPlayer * _enemy.FleeSafeDistance;

            if (NavMesh.SamplePosition(fleeTarget, out var hit, _enemy.FleeSafeDistance, NavMesh.AllAreas))
            {
                _enemy.Agent.SetDestination(hit.position);
            }
        }
    }
}