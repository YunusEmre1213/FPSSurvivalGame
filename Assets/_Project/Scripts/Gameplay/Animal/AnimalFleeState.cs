using UnityEngine;
using UnityEngine.AI;

namespace Project.Gameplay.Animals
{
    public class AnimalFleeState : IAnimalState
    {
        private readonly AnimalController _animal;
        private float _originalSpeed;

        public AnimalFleeState(AnimalController animal)
        {
            _animal = animal;
        }

        public void Enter()
        {
            _animal.Agent.isStopped = false;
            _originalSpeed = _animal.Agent.speed;
            _animal.Agent.speed = _originalSpeed * _animal.FleeSpeedMultiplier;
            SetFleeDestination();
        }

        public void Update()
        {
            float distance = _animal.DistanceToPlayer();

            if (distance >= _animal.FleeSafeDistance)
            {
                _animal.ChangeState(new AnimalPatrolState(_animal));
                return;
            }
            if (!_animal.Agent.pathPending && _animal.Agent.remainingDistance <= _animal.Agent.stoppingDistance)
            {
                SetFleeDestination();
            }
        }

        public void Exit()
        {
            _animal.Agent.speed = _originalSpeed;
        }

        private void SetFleeDestination()
        {
            if (_animal.Player == null) return;

            Vector3 awayFromPlayer = (_animal.transform.position - _animal.Player.position).normalized;
            Vector3 fleeTarget = _animal.transform.position + awayFromPlayer * _animal.FleeSafeDistance;

            if (NavMesh.SamplePosition(fleeTarget, out var hit, _animal.FleeSafeDistance, NavMesh.AllAreas))
            {
                _animal.Agent.SetDestination(hit.position);
            }
        }
    }
}