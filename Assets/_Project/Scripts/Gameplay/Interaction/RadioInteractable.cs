using UnityEngine;
using Project.Gameplay.Interaction;
using Project.Gameplay.Player;

namespace Project.Gameplay.Items
{
    [RequireComponent(typeof(Collider))]
    public class RadioInteractable : MonoBehaviour, IInteractable
    {
        [Header("Referanslar")]
        [SerializeField] private Transform antennaTransform;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip musicClip;

        [Header("Anten Animasyonu")]
        [Tooltip("Anten acikken kac derece donsun (yerel Z ekseni).")]
        [SerializeField] private float antennaOpenAngle = 90f;
        [Tooltip("Anten saniyede kac derece donsun - dusuk deger = yavas/yumusak acilis.")]
        [SerializeField] private float antennaRotateSpeed = 60f;

        private bool _isOn;
        private bool _playerInRange;
        private float _currentAngle;
        private float _targetAngle;
        private Quaternion _baseRotation;
        private IInputProvider _input;

        public string InteractionPrompt => _isOn ? "Radyoyu kapat" : "Radyoyu ac";

        private void Start()
        {
            _input = ActiveInput.Provider;

            if (antennaTransform != null)
            {
                _baseRotation = antennaTransform.localRotation;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = false;
        }

        private void Update()
        {
            UpdateAntenna();

            if (!_playerInRange || _input == null) return;

            if (_input.InteractPressedThisFrame)
            {
                Interact();
            }
        }

        private void UpdateAntenna()
        {
            if (antennaTransform == null) return;

            _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, antennaRotateSpeed * Time.deltaTime);
            antennaTransform.localRotation = _baseRotation * Quaternion.Euler(0f, 0f, _currentAngle);
        }

        public void Interact()
        {
            _isOn = !_isOn;
            _targetAngle = _isOn ? antennaOpenAngle : 0f;

            if (_isOn)
            {
                PlayMusic();
            }
            else
            {
                StopMusic();
            }
        }

        private void PlayMusic()
        {
            if (audioSource == null || musicClip == null) return;

            audioSource.clip = musicClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        private void StopMusic()
        {
            if (audioSource == null) return;
            audioSource.Stop();
        }
    }
}