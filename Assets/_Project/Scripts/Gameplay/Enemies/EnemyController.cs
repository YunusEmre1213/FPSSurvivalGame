using UnityEngine;
using UnityEngine.AI;
using Project.Core;

namespace Project.Gameplay.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour, IDamageable
    {
        [Header("Referanslar")]
        [Tooltip("Sahnedeki oyuncu Transform'u. Simdilik elle atanýyor, ileride PlayerService ile otomatiklestirilebilir.")]
        [SerializeField] private Transform player;

        [Header("Algilama - Mesafe")]
        [SerializeField] private float detectionRange = 12f;
        [SerializeField] private float attackRange = 2f;
        [Tooltip("Oyuncu bu mesafenin disina cikarsa kovalamayi tamamen birak.")]
        [SerializeField] private float loseTargetRange = 18f;

        [Header("Algilama - Gorus Konisi ve Hatti")]
        [Tooltip("Dusmanin on yonune gore toplam gorus acisi (derece). 110 = insana yakin bir aci.")]
        [SerializeField] private float fieldOfViewAngle = 110f;
        [Tooltip("Raycast'in basladigi goz yuksekligi (yerden).")]
        [SerializeField] private float eyeHeight = 1.5f;
        [Tooltip("Gorus kaybedildikten sonra hedefi 'hatirlama' suresi (saniye) - aninda pes etmesin diye.")]
        [SerializeField] private float sightMemoryDuration = 2f;
        [Tooltip("Gorus tamamen kaybedilince, son bilinen konumda arastirma yapma suresi.")]
        [SerializeField] private float investigateDuration = 5f;

        [Header("Grup uyarisi")]
        [Tooltip("Bir digeri oyuncuyu ilk gordugunde, bu yaricap icindeysem ben de arastirmaya giderim. Suru tipi icin daha genis, oncu icin dar tutulabilir.")]
        [SerializeField] private float alertRadius = 12f;

        [Header("Hareket")]
        [SerializeField] private float patrolRadius = 6f;
        [SerializeField] private float patrolWaitTime = 2f;

        [Header("Savas")]
        [SerializeField] private float health = 30f;
        [SerializeField] private float damage = 5f;
        [SerializeField] private float attackCooldown = 1.5f;
        [Tooltip("Saldiridan once bekleme suresi (saniye). 0 = aninda vurur (oncu/suru). >0 = 'belirgin saldiri patern' - elit tipte kullanilir, oyuncuya tepki verme sansi tanir.")]
        [SerializeField] private float attackWindupTime = 0f;

        [Header("Kacma (dusuk can)")]
        [Tooltip("Can, maksimumun bu oranin altina dusunce kacmaya baslar. 0 = hic kacmaz.")]
        [Range(0f, 1f)]
        [SerializeField] private float fleeHealthThreshold = 0.25f;
        [Tooltip("Oyuncudan bu mesafeye ulasinca 'kurtuldum' sayip devriyeye doner.")]
        [SerializeField] private float fleeSafeDistance = 20f;
        [Tooltip("Kacarken normal hizin kac kati - panik hizlanmasi.")]
        [SerializeField] private float fleeSpeedMultiplier = 1.4f;

        public NavMeshAgent Agent { get; private set; }
        public Transform Player => player;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float LoseTargetRange => loseTargetRange;
        public float FieldOfViewAngle => fieldOfViewAngle;
        public float SightMemoryDuration => sightMemoryDuration;
        public float InvestigateDuration => investigateDuration;
        public float PatrolRadius => patrolRadius;
        public float PatrolWaitTime => patrolWaitTime;
        public float AttackCooldown => attackCooldown;
        public float AttackWindupTime => attackWindupTime;
        public float FleeSafeDistance => fleeSafeDistance;
        public float FleeSpeedMultiplier => fleeSpeedMultiplier;
        public Vector3 SpawnPosition { get; private set; }

        private EnemyStateMachine _stateMachine;
        private float _maxHealth;

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyAlertEvent>(OnEnemyAlert);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyAlertEvent>(OnEnemyAlert);
        }

        private void OnEnemyAlert(EnemyAlertEvent evt)
        {
            if (evt.Source == gameObject) return;

            if (_stateMachine.CurrentState is EnemyChaseState || _stateMachine.CurrentState is EnemyAttackState)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, evt.AlertPosition);
            if (distance <= alertRadius)
            {
                ChangeState(new EnemyInvestigateState(this, evt.AlertPosition));
            }
        }

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
        public bool CanSeePlayer()
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
                return;
            }

            bool alreadyFleeing = _stateMachine.CurrentState is EnemyFleeState;
            if (!alreadyFleeing && fleeHealthThreshold > 0f && health <= _maxHealth * fleeHealthThreshold)
            {
                ChangeState(new EnemyFleeState(this));
            }
        }

        private void Die()
        {
            Debug.Log($"[EnemyController] {gameObject.name} oldu.");
            Destroy(gameObject);
        }
    }
}