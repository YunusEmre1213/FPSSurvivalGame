using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Gameplay.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAudioController : MonoBehaviour
    {
        [Header("Audio Source'lar")]
        [SerializeField] private AudioSource actionAudioSource;
        [SerializeField] private AudioSource howlAudioSource;

        [Header("Adim Sesleri")]
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private float footstepIntervalAtWalkSpeed = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float footstepVolume = 0.7f;

        [Header("Saldiri Sesi")]
        [SerializeField] private AudioClip[] attackClips;
        [Range(0f, 1f)]
        [SerializeField] private float attackVolume = 0.9f;

        [Header("Olum Sesi")]
        [SerializeField] private AudioClip[] deathClips;
        [Range(0f, 1f)]
        [SerializeField] private float deathVolume = 1f;

        [Header("Rastgele Uluma (Korku Atmosferi)")]
        [SerializeField] private AudioClip[] howlClips;
        [SerializeField] private float howlMinInterval = 15f;
        [SerializeField] private float howlMaxInterval = 40f;
        [Tooltip("Ulumanin ne kadar uzaktan duyulabilecegi (metre).")]
        [SerializeField] private float howlMaxDistance = 80f;
        [Range(0f, 1f)]
        [SerializeField] private float howlVolume = 1f;

        [Header("Ara Sira Oturma (Idle Varyasyonu)")]
        [SerializeField] private float sitMinInterval = 12f;
        [SerializeField] private float sitMaxInterval = 25f;
        [SerializeField] private float idleSpeedThreshold = 0.15f;

        [Header("Test / Debug")]
        [Tooltip("Acikken asagidaki tusa basinca, beklemeden aninda uluma tetiklenir - kurulumu dogrulamak icin.")]
        [SerializeField] private bool enableForceHowlKey = true;
        [SerializeField] private KeyCode forceHowlKey = KeyCode.H;

        private NavMeshAgent _agent;
        private Animator _animator;
        private EnemyController _enemyController;
        private float _footstepTimer;
        private Coroutine _howlRoutine;
        private Coroutine _sitRoutine;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _enemyController = GetComponent<EnemyController>();
            ConfigureHowlSource();
        }

        private void OnEnable()
        {
            _footstepTimer = 0f;
            _howlRoutine = StartCoroutine(HowlLoop());
            _sitRoutine = StartCoroutine(SitLoop());

            if (_enemyController != null)
            {
                _enemyController.OnAttackTriggered += PlayAttackSound;
            }
        }

        private void OnDisable()
        {
            if (_howlRoutine != null)
            {
                StopCoroutine(_howlRoutine);
                _howlRoutine = null;
            }

            if (_sitRoutine != null)
            {
                StopCoroutine(_sitRoutine);
                _sitRoutine = null;
            }

            if (_enemyController != null)
            {
                _enemyController.OnAttackTriggered -= PlayAttackSound;
            }
        }

        private void ConfigureHowlSource()
        {
            if (howlAudioSource == null) return;

            howlAudioSource.spatialBlend = 1f;
            howlAudioSource.rolloffMode = AudioRolloffMode.Custom;
            howlAudioSource.maxDistance = howlMaxDistance;
            howlAudioSource.minDistance = 10f;

            var curve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.6f, 0.55f),
                new Keyframe(1f, 0f)
            );
            howlAudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
        }

        private void Update()
        {
            HandleFootsteps();

            if (enableForceHowlKey && Input.GetKeyDown(forceHowlKey))
            {
                TriggerHowl(isTest: true);
            }
        }

        private void HandleFootsteps()
        {
            if (_agent == null || !_agent.enabled || footstepClips == null || footstepClips.Length == 0) return;

            float speed = _agent.velocity.magnitude;
            if (speed < 0.1f)
            {
                _footstepTimer = 0f;
                return;
            }

            float speedRatio = Mathf.Clamp(speed / Mathf.Max(_agent.speed, 0.01f), 0.3f, 2f);
            float interval = footstepIntervalAtWalkSpeed / speedRatio;

            _footstepTimer += Time.deltaTime;
            if (_footstepTimer >= interval)
            {
                _footstepTimer = 0f;
                PlayRandomClip(actionAudioSource, footstepClips, footstepVolume);
            }
        }

        private void PlayAttackSound()
        {
            PlayRandomClip(actionAudioSource, attackClips, attackVolume);
        }

        public void PlayDeathSound()
        {
            PlayRandomClip(actionAudioSource, deathClips, deathVolume);
        }

        private IEnumerator HowlLoop()
        {
            while (true)
            {
                float wait = Random.Range(howlMinInterval, howlMaxInterval);
                yield return new WaitForSeconds(wait);
                TriggerHowl(isTest: false);
            }
        }

        private void TriggerHowl(bool isTest)
        {
            if (howlClips != null && howlClips.Length > 0 && howlAudioSource != null)
            {
                howlAudioSource.volume = howlVolume;
                howlAudioSource.PlayOneShot(howlClips[Random.Range(0, howlClips.Length)]);
            }
            else if (isTest)
            {
                Debug.LogWarning($"[EnemyAudioController] {gameObject.name} - Uluma calinamadi: " +
                    $"HowlClips bos mu? {(howlClips == null || howlClips.Length == 0)} / HowlAudioSource atanmamis mi? {howlAudioSource == null}");
            }

            _animator?.SetTrigger("Howl");

            if (isTest)
            {
                Debug.Log($"[EnemyAudioController] {gameObject.name} TEST uluma tetiklendi (H).");
            }
        }

        private IEnumerator SitLoop()
        {
            while (true)
            {
                float wait = Random.Range(sitMinInterval, sitMaxInterval);
                yield return new WaitForSeconds(wait);

                bool isIdle = _agent != null && _agent.enabled && _agent.velocity.magnitude <= idleSpeedThreshold;
                if (isIdle && _animator != null)
                {
                    _animator.SetTrigger("Sit");
                }
            }
        }

        private void PlayRandomClip(AudioSource source, AudioClip[] clips, float volume)
        {
            if (source == null || clips == null || clips.Length == 0) return;
            var clip = clips[Random.Range(0, clips.Length)];
            source.PlayOneShot(clip, volume);
        }
    }
}