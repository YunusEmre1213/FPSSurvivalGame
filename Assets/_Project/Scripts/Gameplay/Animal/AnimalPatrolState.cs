using UnityEngine;
using UnityEngine.AI;

namespace Project.Gameplay.Animals
{
    public class AnimalPatrolState : IAnimalState
    {
        private readonly AnimalController _animal;
        private float _waitTimer;
        private bool _isWaiting;

        public AnimalPatrolState(AnimalController animal)
        {
            _animal = animal;
        }

        public void Enter()
        {
            _isWaiting = false;
            PickNewDestination();
        }

        public void Update()
        {
            if (_animal.CanSensePlayer())
            {
                _animal.ChangeState(new AnimalFleeState(_animal));
                return;
            }

            if (!_animal.Agent.pathPending && _animal.Agent.remainingDistance <= _animal.Agent.stoppingDistance)
            {
                if (!_isWaiting)
                {
                    _isWaiting = true;
                    _waitTimer = _animal.PatrolWaitTime;
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
            var randomOffset = Random.insideUnitSphere * _animal.PatrolRadius;
            randomOffset.y = 0f;
            var destination = _animal.SpawnPosition + randomOffset;

            if (NavMesh.SamplePosition(destination, out var hit, _animal.PatrolRadius, NavMesh.AllAreas))
            {
                _animal.Agent.SetDestination(hit.position);
            }
        }
    }
}