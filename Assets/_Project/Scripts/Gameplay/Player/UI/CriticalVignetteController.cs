using UnityEngine;
using Project.Core;
using Project.Gameplay.Player;

namespace Project.UI
{
    public class CriticalVignetteController : MonoBehaviour
    {
        [SerializeField] private PlayerNeeds playerNeeds;
        [SerializeField] private CanvasGroup vignetteGroup;

        [Range(0f, 1f)]
        [SerializeField] private float needsCriticalThreshold = 0.2f;
        [Range(0f, 1f)]
        [SerializeField] private float needsVignetteAlpha = 0.18f;
        [SerializeField] private float fadeSpeed = 3f;

        private float _healthRatio = 1f;

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
        }

        private void OnHealthChanged(PlayerHealthChangedEvent evt)
        {
            _healthRatio = evt.MaxHealth > 0f ? evt.CurrentHealth / evt.MaxHealth : 0f;
        }

        private void Update()
        {
            float healthAlpha = ComputeHealthVignetteAlpha(_healthRatio);

            bool hungerCritical = playerNeeds != null && (playerNeeds.Hunger / playerNeeds.MaxValue) < needsCriticalThreshold;
            bool thirstCritical = playerNeeds != null && (playerNeeds.Thirst / playerNeeds.MaxValue) < needsCriticalThreshold;
            float needsAlpha = (hungerCritical || thirstCritical) ? needsVignetteAlpha : 0f;

            float targetAlpha = Mathf.Max(healthAlpha, needsAlpha);

            if (vignetteGroup != null)
            {
                vignetteGroup.alpha = Mathf.MoveTowards(vignetteGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            }
        }

        private float ComputeHealthVignetteAlpha(float ratio)
        {
            if (ratio >= 0.5f) return 0f;
            if (ratio >= 0.25f) return Mathf.Lerp(0f, 0.12f, Mathf.InverseLerp(0.5f, 0.25f, ratio));
            if (ratio >= 0.10f) return Mathf.Lerp(0.12f, 0.28f, Mathf.InverseLerp(0.25f, 0.10f, ratio));
            return Mathf.Lerp(0.28f, 0.45f, Mathf.InverseLerp(0.10f, 0f, ratio));
        }
    }
}