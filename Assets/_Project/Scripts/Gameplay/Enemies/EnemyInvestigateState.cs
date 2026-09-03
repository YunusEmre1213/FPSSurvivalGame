using UnityEngine;

namespace Project.Gameplay.Enemies
{
    public class EnemyInvestigateState : IEnemyState
    {
        private readonly EnemyController _enemy;
        private readonly Vector3 _lastKnownPosition;
        private float _investigateTimer;
        private bool _hasArrived;

        public EnemyInvestigateState(EnemyController enemy, Vector3 lastKnownPosition)
        {
            _enemy = enemy;
            _lastKnownPosition = lastKnownPosition;
        }

        public void Enter()
        {
            _enemy.Agent.isStopped = false;
            _enemy.Agent.SetDestination(_lastKnownPosition);
            _investigateTimer = _enemy.InvestigateDuration;
            _hasArrived = false;
        }

        public void Update()
        {
            if (_enemy.CanSeePlayer())
            {
                _enemy.ChangeState(new EnemyChaseState(_enemy));
                return;
            }

            if (!_hasArrived && !_enemy.Agent.pathPending && _enemy.Agent.remainingDistance <= _enemy.Agent.stoppingDistance)
            {
                _hasArrived = true; 
            }

            if (_hasArrived)
            {
                _investigateTimer -= Time.deltaTime;
                if (_investigateTimer <= 0f)
                {
                    _enemy.ChangeState(new EnemyPatrolState(_enemy));
                }
            }
        }

        public void Exit()
        {
        }
    }
}