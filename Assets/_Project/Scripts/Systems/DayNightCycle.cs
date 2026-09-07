using UnityEngine;
using Project.Core;

namespace Project.Systems
{

    public class DayNightCycle : MonoBehaviour, IDayNightService
    {
        [Header("Faz sureleri (saniye)")]
        [Tooltip("Gunduz fazinin suresi")]
        [SerializeField] private float dayDuration = 420f;   

        [Tooltip("Alacakaranlik - kisa gecis fazi")]
        [SerializeField] private float duskDuration = 60f;   

        [Tooltip("Gece faz, en yuksek tehlike.")]
        [SerializeField] private float nightDuration = 240f; 

        [Header("Test")]
        [SerializeField] private bool useTestDurations = false;
        [SerializeField] private float testDurationMultiplier = 0.05f;

        public DayNightPhase CurrentPhase { get; private set; } = DayNightPhase.Day;
        public float PhaseTimeRemaining => Mathf.Max(0f, _currentPhaseDuration - _elapsedInPhase);
        public float PhaseProgress01 => _currentPhaseDuration <= 0f ? 0f : Mathf.Clamp01(_elapsedInPhase / _currentPhaseDuration);
        public int NightsSurvived { get; private set; }

        private float _elapsedInPhase;
        private float _currentPhaseDuration;

        private void Awake()
        {
            ServiceLocator.Instance.Register<IDayNightService>(this);
        }

        public void Initialize()
        {
            CurrentPhase = DayNightPhase.Day;
            _elapsedInPhase = 0f;
            _currentPhaseDuration = GetDuration(CurrentPhase);
        }

        public void Shutdown()
        {
        }

        private void Update()
        {
            _elapsedInPhase += Time.deltaTime;

            if (_elapsedInPhase >= _currentPhaseDuration)
            {
                AdvancePhase();
            }
        }

        private void AdvancePhase()
        {
            var previousPhase = CurrentPhase;
            CurrentPhase = GetNextPhase(CurrentPhase);
            _elapsedInPhase = 0f;
            _currentPhaseDuration = GetDuration(CurrentPhase);

            if (CurrentPhase == DayNightPhase.Night)
            {
                NightsSurvived++;
            }

            Debug.Log($"[DayNightCycle] Faz degisti: {previousPhase} -> {CurrentPhase} ({NightsSurvived}. gece)");
            EventBus.Publish(new DayNightPhaseChangedEvent(CurrentPhase));
        }

        private static DayNightPhase GetNextPhase(DayNightPhase current)
        {
            switch (current)
            {
                case DayNightPhase.Day: return DayNightPhase.Dusk;
                case DayNightPhase.Dusk: return DayNightPhase.Night;
                case DayNightPhase.Night: return DayNightPhase.Day;
                default: return DayNightPhase.Day;
            }
        }

        private float GetDuration(DayNightPhase phase)
        {
            float duration = phase switch
            {
                DayNightPhase.Day => dayDuration,
                DayNightPhase.Dusk => duskDuration,
                DayNightPhase.Night => nightDuration,
                _ => dayDuration
            };

            return useTestDurations ? duration * testDurationMultiplier : duration;
        }
    }
}