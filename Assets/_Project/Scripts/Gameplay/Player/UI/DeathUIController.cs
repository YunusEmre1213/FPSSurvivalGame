using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Project.Core;
using Project.Gameplay.Player;

namespace Project.UI
{
    public class DeathUIController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Butonlar")]
        [SerializeField] private Button respawnButton;
        [SerializeField] private Button returnToMenuButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Ölüm Aný Kamera Düþüþü")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float fallDuration = 1.4f;
        [SerializeField] private float fallDropAmount = 0.6f;
        [SerializeField] private float fallTiltAngle = 70f;

        [Header("Ölünce Gizlenecek HUD Elemanlarý")]
        [SerializeField] private GameObject[] hudElementsToHide;

        private Vector3 _cameraStartLocalPos;
        private Quaternion _cameraStartLocalRot;
        private Coroutine _deathSequenceCoroutine;

        private void Awake()
        {
            respawnButton.onClick.AddListener(OnRespawnPressed);
            returnToMenuButton.onClick.AddListener(OnReturnToMenuPressed);
            quitButton.onClick.AddListener(OnQuitPressed);
            panelRoot.SetActive(false);

            if (cameraTransform != null)
            {
                _cameraStartLocalPos = cameraTransform.localPosition;
                _cameraStartLocalRot = cameraTransform.localRotation;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            SetHudVisible(false);

            panelRoot.SetActive(true);
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;

            if (_deathSequenceCoroutine != null)
            {
                StopCoroutine(_deathSequenceCoroutine);
            }
            _deathSequenceCoroutine = StartCoroutine(PlayDeathSequence());
        }

        private void OnPlayerRespawned(PlayerRespawnedEvent evt)
        {
            if (_deathSequenceCoroutine != null)
            {
                StopCoroutine(_deathSequenceCoroutine);
                _deathSequenceCoroutine = null;
            }

            panelRoot.SetActive(false);
            SetHudVisible(true);

            if (cameraTransform != null)
            {
                cameraTransform.localPosition = _cameraStartLocalPos;
                cameraTransform.localRotation = _cameraStartLocalRot;
            }
        }

        private IEnumerator PlayDeathSequence()
        {
            float elapsed = 0f;

            Vector3 fallTargetPos = _cameraStartLocalPos + Vector3.down * fallDropAmount;
            Quaternion fallTargetRot = _cameraStartLocalRot * Quaternion.Euler(fallTiltAngle, 0f, 0f);

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fallDuration);
                float eased = 1f - (1f - t) * (1f - t); 

                if (cameraTransform != null)
                {
                    cameraTransform.localPosition = Vector3.Lerp(_cameraStartLocalPos, fallTargetPos, eased);
                    cameraTransform.localRotation = Quaternion.Slerp(_cameraStartLocalRot, fallTargetRot, eased);
                }


                panelCanvasGroup.alpha = eased;

                yield return null;
            }

            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
            _deathSequenceCoroutine = null;
        }

        private void SetHudVisible(bool visible)
        {
            foreach (var element in hudElementsToHide)
            {
                if (element != null)
                {
                    element.SetActive(visible);
                }
            }
        }

        private void OnRespawnPressed()
        {
            playerHealth.Respawn();
        }

        private void OnReturnToMenuPressed()
        {
            // Time.timeScale = 1f;
            // SceneManager.LoadScene("MainMenu");
        }

        private void OnQuitPressed()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}