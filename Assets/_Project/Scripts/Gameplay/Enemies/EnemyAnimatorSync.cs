using UnityEngine;
using UnityEngine.AI;

namespace Project.Gameplay.Enemies
{
    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAnimatorSync : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator animator;
        [Tooltip("Asset'in Animator Controller'indaki hiz parametresinin adi (float).")]
        [SerializeField] private string speedParam = "Speed";
        [Tooltip("Asset'in Animator Controller'indaki saldiri parametresinin adi (trigger).")]
        [SerializeField] private string attackTrigger = "Attack";
        [Tooltip("Agent hizini 0-1 araligina normalize etmek icin referans hiz (genelde Agent'in Max Speed degeri).")]
        [SerializeField] private float speedNormalizer = 4f;

        private NavMeshAgent _agent;
        private EnemyController _controller;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _controller = GetComponent<EnemyController>();
        }

        private void OnEnable()
        {
            _controller.OnAttackTriggered += HandleAttackTriggered;
        }

        private void OnDisable()
        {
            _controller.OnAttackTriggered -= HandleAttackTriggered;
        }

        private void Update()
        {
            if (animator == null || _agent == null) return;

            float normalizedSpeed = speedNormalizer > 0f ? _agent.velocity.magnitude / speedNormalizer : 0f;
            animator.SetFloat(speedParam, normalizedSpeed);
        }

        private void HandleAttackTriggered()
        {
            if (animator != null) animator.SetTrigger(attackTrigger);
        }
    }
}