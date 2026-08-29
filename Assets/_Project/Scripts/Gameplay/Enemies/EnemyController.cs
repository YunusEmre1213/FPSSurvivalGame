using UnityEngine;
using UnityEngine.AI;

namespace Project.Gameplay.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour, IDamageable
    {
        [Header("Referanslar")]
        [SerializeField] private Transform player;

        [Header("Algilama")]
        [SerializeField] private float detectionRange = 12f;
        [SerializeField] private float attackRange = 2f;
        [Tooltip("Oyuncu bu mesafenin disina cikarsa kovalamayi birak, devriyeye don.")]
        [SerializeField] private float loseTargetRange = 18f;

        [Header("Hareket")]
        [SerializeField] private float patrolRadius = 6f;
        [SerializeField] private float patrolWaitTime = 2f;

        [Header("Savas")]
        [SerializeField] private float health = 30f;
        [SerializeField] private float damage = 5f;
        [SerializeField] private float attackCooldown = 1.5f;
        [Tooltip("Saldiridan once bekleme suresi (saniye). 0 = aninda vurur (oncu/suru). >0 = 'belirgin saldiri patern' - elit tipte kullanilir, oyuncuya tepki verme sansi tanir.")]
        [SerializeField] private float attackWindupTime = 0f;

        public NavMeshAgent Agent { get; private set; }
        public Transform Player => player;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float LoseTargetRange => loseTargetRange;
        public float PatrolRadius => patrolRadius;
        public float PatrolWaitTime => patrolWaitTime;
        public float AttackCooldown => attackCooldown;
        public float AttackWindupTime => attackWindupTime;
        public Vector3 SpawnPosition { get; private set; }

        private EnemyStateMachine _stateMachine;
        private float _maxHealth;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            SpawnPosition = transform.position;
            _stateMachine = new EnemyStateMachine();
            _maxHealth = health;

            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
                else
                {
                    Debug.LogError("[EnemyController] Player bulunamadi - sahnede 'Player' tag'li obje var mi kontrol et.");
                }
            }
        }

        private void Start()
        {
            _stateMachine.ChangeState(new EnemyPatrolState(this));
        }
        public void ResetForPool(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            gameObject.SetActive(true);
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            SpawnPosition = spawnPosition;
            health = _maxHealth;

            if (Agent != null)
            {
                Agent.Warp(spawnPosition);
            }

            _stateMachine.ChangeState(new EnemyPatrolState(this));
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        public float DistanceToPlayer()
        {
            return player == null ? Mathf.Infinity : Vector3.Distance(transform.position, player.position);
        }

        public void ChangeState(IEnemyState newState)
        {
            _stateMachine.ChangeState(newState);
        }

        public void DealDamageToPlayer()
        {
            var damageable = player != null ? player.GetComponent<IDamageable>() : null;
            damageable?.TakeDamage(damage);
            Debug.Log($"[EnemyController] {gameObject.name} oyuncuya {damage} hasar verdi.");
        }

        public void TakeDamage(float amount)
        {
            health -= amount;
            Debug.Log($"[EnemyController] {gameObject.name} {amount} hasar aldi, kalan can: {health}");

            if (health <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"[EnemyController] {gameObject.name} oldu.");
            Destroy(gameObject);
        }
    }
}