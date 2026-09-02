using UnityEngine;

namespace Project.Gameplay.Environment
{
    [RequireComponent(typeof(Light))]
    public class FireFlicker : MonoBehaviour
    {
        [SerializeField] private float baseIntensity = 3f;
        [SerializeField] private float flickerAmount = 0.6f;
        [SerializeField] private float flickerSpeed = 8f;

        private Light _light;
        private float _noiseSeed;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _noiseSeed = Random.value * 100f;
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(_noiseSeed, Time.time * flickerSpeed);
            _light.intensity = baseIntensity + (noise - 0.5f) * flickerAmount;
        }
    }
}