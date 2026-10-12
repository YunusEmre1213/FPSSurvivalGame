using UnityEngine;
using UnityEngine.AI;
using Project.Core;

namespace Project.Gameplay.Animals
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AnimalController : MonoBehaviour, IDamageable
    {
        [Header("Referanslar")]
        [SerializeField] private Transform player;

        [Header("Algilama")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float fieldOfViewAngle = 150f;
        [SerializeField] private float eyeHeight = 1f;

        [Header("Hareket")]
        [SerializeField] private float patrolRadius = 8f;
        [SerializeField] private float patrolWaitTime = 3f;

        [Header("Can")]
        [SerializeField] private float health = 20f;

        [Header("Kacma")]
        [SerializeField] private float fleeSafeDistance = 15f;
        [SerializeField] private float fleeSpeedMultiplier = 1.8f;

        [Header("Et Dususu")]
        [SerializeField] private GameObject rawMeatPickupPrefab;
        [SerializeField] private int meatDropCount = 2;
        [SerializeField] private float meatDropHeight = 1f;

        public NavMeshAgent Agent { get; private set; }
        public Transform Player => player;
        public float PatrolRadius => patrolRadius;
        public float PatrolWaitTime => patrolWaitTime;
        public float FleeSafeDistance => fleeSafeDistance;
        public float FleeSpeedMultiplier => fleeSpeedMultiplier;
        public Vector3 SpawnPosition { get; private set; }

        private AnimalStateMachine _stateMachine;
        private bool _isDead;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            SpawnPosition = transform.position;
            _stateMachine = new AnimalStateMachine();

            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
            }
        }

        private void Start()
        {
            _stateMachine.ChangeState(new AnimalPatrolState(this));
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        public void ChangeState(IAnimalState newState)
        {
            _stateMachine.ChangeState(newState);
        }

        public float DistanceToPlayer()
        {
            return player == null ? Mathf.Infinity : Vector3.Distance(transform.position, player.position);
        }

        public bool CanSensePlayer()
        {
            if (player == null) return false;

            Vector3 toPlayer = player.position - transform.position;
            float distance = toPlayer.magnitude;

            if (distance > detectionRange) return false;

            float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
            if (angle > fieldOfViewAngle * 0.5f) return false;

            Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
            Vector3 targetPosition = player.position + Vector3.up * 0.9f;

            if (Physics.Linecast(eyePosition, targetPosition, out var hit))
            {
                if (!hit.collider.CompareTag("Player"))
                {
                    return false;
                }
            }

            return true;
        }

        public void TakeDamage(float amount)
        {
            if (_isDead) return;

            health -= amount;

            if (health <= 0f)
            {
                Die();
                return;
            }

            // Hayvan esik beklemez, ilk darbede kacmaya baslar
            if (!(_stateMachine.CurrentState is AnimalFleeState))
            {
                ChangeState(new AnimalFleeState(this));
            }
        }

        private void Die()
        {
            _isDead = true;

            EventBus.Publish(new AnimalDiedEvent(transform.position));

            DropMeat();

            Destroy(gameObject);
        }

        private void DropMeat()
        {
            if (rawMeatPickupPrefab == null) return;

            for (int i = 0; i < meatDropCount; i++)
            {
                Vector3 horizontalOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
                Vector3 spawnPos = transform.position + horizontalOffset + Vector3.up * meatDropHeight;

                GameObject meat = Instantiate(rawMeatPickupPrefab, spawnPos, Random.rotation);

                Rigidbody rb = meat.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
                    rb.AddForce(Random.insideUnitSphere * 0.3f, ForceMode.Impulse);
                }
            }
        }
    }
}