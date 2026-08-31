using UnityEngine;
using Project.Core;

namespace Project.Systems
{
    public class DayNightLightingController : MonoBehaviour
    {
        [System.Serializable]
        public class PhaseLighting
        {
            public Color lightColor = Color.white;
            [Range(0f, 3f)] public float lightIntensity = 1f;
            public Color fogColor = Color.gray;
            [Range(0f, 0.1f)] public float fogDensity = 0.01f;
            [Range(0f, 3f)] public float ambientIntensity = 1f;
        }

        [Header("Referanslar")]
        [Tooltip("Sahnedeki gunes/ay isigi olarak kullanilan Directional Light.")]
        [SerializeField] private Light directionalLight;

        [Header("Faz basina hedef isik ayarlari")]
        [SerializeField] private PhaseLighting dayLighting;
        [SerializeField] private PhaseLighting duskLighting;
        [SerializeField] private PhaseLighting nightLighting;

        [Header("Gecis")]
        [Tooltip("Hedef degerlere ne kadar hizli yaklasilacagi - kucuk deger = yavas/yumusak gecis, buyuk deger = hizli/ani.")]
        [SerializeField] private float transitionSpeed = 0.5f;

        private PhaseLighting _target;

        private void OnEnable()
        {
            EventBus.Subscribe<DayNightPhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DayNightPhaseChangedEvent>(OnPhaseChanged);
        }

        private void Start()
        {
            
            _target = dayLighting;
            ApplyImmediate(_target);
        }

        private void OnPhaseChanged(DayNightPhaseChangedEvent evt)
        {
            _target = evt.NewPhase switch
            {
                DayNightPhase.Day => dayLighting,
                DayNightPhase.Dusk => duskLighting,
                DayNightPhase.Night => nightLighting,
                _ => dayLighting
            };
        }

        private void Update()
        {
            if (_target == null) return;

           
            float t = 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime);

            if (directionalLight != null)
            {
                directionalLight.color = Color.Lerp(directionalLight.color, _target.lightColor, t);
                directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, _target.lightIntensity, t);
            }

            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, _target.fogColor, t);
            RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, _target.fogDensity, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(RenderSettings.ambientIntensity, _target.ambientIntensity, t);
        }

        private void ApplyImmediate(PhaseLighting lighting)
        {
            if (directionalLight != null)
            {
                directionalLight.color = lighting.lightColor;
                directionalLight.intensity = lighting.lightIntensity;
            }
            RenderSettings.fogColor = lighting.fogColor;
            RenderSettings.fogDensity = lighting.fogDensity;
            RenderSettings.ambientIntensity = lighting.ambientIntensity;
        }
    }
}