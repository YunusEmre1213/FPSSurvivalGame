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

        [Header("Skybox blend (Mat_SkyboxBlend + Cubemap dokular)")]
        [Tooltip("SkyboxBlend shader'ini kullanan materyal - RenderSettings.skybox'a bir kere atanir, bir daha degismez, sadece dokular/blend guncellenir.")]
        [SerializeField] private Material skyboxBlendMaterial;
        [SerializeField] private Cubemap daySkyTexture;
        [SerializeField] private Cubemap duskSkyTexture;
        [SerializeField] private Cubemap nightSkyTexture;
        [Tooltip("Gokyuzu gecisinin hizi. Isik gecisinden (transitionSpeed) genelde biraz daha hizli tutmak iyi sonuc verir.")]
        [SerializeField] private float skyboxTransitionSpeed = 0.8f;

        [Header("Gecis")]
        [Tooltip("Hedef degerlere ne kadar hizli yaklasilacagi - kucuk deger = yavas/yumusak gecis, buyuk deger = hizli/ani.")]
        [SerializeField] private float transitionSpeed = 0.5f;

        private PhaseLighting _target;

        private Cubemap _currentSkyTexture;
        private Cubemap _targetSkyTexture;
        private float _skyboxBlend = 1f;

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

            _currentSkyTexture = daySkyTexture;
            _targetSkyTexture = daySkyTexture;
            _skyboxBlend = 1f;

            if (skyboxBlendMaterial != null)
            {
                RenderSettings.skybox = skyboxBlendMaterial;
                skyboxBlendMaterial.SetTexture("_TextureA", daySkyTexture);
                skyboxBlendMaterial.SetTexture("_TextureB", daySkyTexture);
                skyboxBlendMaterial.SetFloat("_Blend", 0f);
                DynamicGI.UpdateEnvironment();
            }
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

            var newSkyTexture = evt.NewPhase switch
            {
                DayNightPhase.Day => daySkyTexture,
                DayNightPhase.Dusk => duskSkyTexture,
                DayNightPhase.Night => nightSkyTexture,
                _ => daySkyTexture
            };
            StartSkyboxTransition(newSkyTexture);
        }

        private void StartSkyboxTransition(Cubemap newTexture)
        {
            if (newTexture == null || skyboxBlendMaterial == null) return;

            _currentSkyTexture = _targetSkyTexture;
            _targetSkyTexture = newTexture;
            _skyboxBlend = 0f;

            skyboxBlendMaterial.SetTexture("_TextureA", _currentSkyTexture);
            skyboxBlendMaterial.SetTexture("_TextureB", _targetSkyTexture);
        }

        private void Update()
        {
            if (_target != null)
            {
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

            if (skyboxBlendMaterial != null && _skyboxBlend < 1f)
            {
                _skyboxBlend = Mathf.Min(1f, _skyboxBlend + skyboxTransitionSpeed * Time.deltaTime);
                skyboxBlendMaterial.SetFloat("_Blend", _skyboxBlend);

                if (_skyboxBlend >= 1f)
                {
                    DynamicGI.UpdateEnvironment();
                }
            }
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