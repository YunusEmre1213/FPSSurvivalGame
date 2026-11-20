using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.UI
{

    public class SettingsPanelController : MonoBehaviour
    {
        private const string VolumePrefKey = "MasterVolume";
        private const string QualityPrefKey = "GraphicsQuality";
        private const string SensitivityPrefKey = "MouseSensitivity";

        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;

        [Header("Grafik Kalitesi")]
        [SerializeField] private Button qualityButton;
        [SerializeField] private TMP_Text qualityButtonText;

        [Header("Ses Seviyesi")]
        [SerializeField] private Button volumeButton;
        [SerializeField] private TMP_Text volumeButtonText;

        [Header("Fare Hassasiyeti")]
        [SerializeField] private Button sensitivityButton;
        [SerializeField] private TMP_Text sensitivityButtonText;

        [Header("FPS Gostergesi")]
        [SerializeField] private Button toggleFpsButton;
        [SerializeField] private TMP_Text toggleFpsButtonText;

        [Header("Kapat")]
        [SerializeField] private Button closeButton;

        private static readonly float[] VolumeSteps = { 0f, 0.25f, 0.5f, 0.75f, 1f };
        private static readonly float[] SensitivitySteps = { 0.5f, 1f, 1.5f, 2f, 2.5f };

        private int _volumeIndex;
        private int _qualityIndex;
        private int _sensitivityIndex;


        public static float MouseSensitivity { get; private set; } = 1f;

        private void Awake()
        {
            qualityButton.onClick.AddListener(OnQualityButtonPressed);
            volumeButton.onClick.AddListener(OnVolumeButtonPressed);
            sensitivityButton.onClick.AddListener(OnSensitivityButtonPressed);
            toggleFpsButton.onClick.AddListener(OnToggleFpsPressed);
            closeButton.onClick.AddListener(Close);


            panelRoot.SetActive(false);

            LoadSavedSettings();
        }

        private void LoadSavedSettings()
        {
            float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
            _volumeIndex = ClosestIndex(VolumeSteps, savedVolume);
            ApplyVolume();

            int qualityCount = Mathf.Max(QualitySettings.names.Length, 1);
            _qualityIndex = Mathf.Clamp(
                PlayerPrefs.GetInt(QualityPrefKey, QualitySettings.GetQualityLevel()),
                0, qualityCount - 1);
            ApplyQuality();

            float savedSensitivity = PlayerPrefs.GetFloat(SensitivityPrefKey, 1f);
            _sensitivityIndex = ClosestIndex(SensitivitySteps, savedSensitivity);
            ApplySensitivity();
        }

        private static int ClosestIndex(float[] steps, float value)
        {
            int best = 0;
            float bestDiff = Mathf.Abs(steps[0] - value);
            for (int i = 1; i < steps.Length; i++)
            {
                float diff = Mathf.Abs(steps[i] - value);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    best = i;
                }
            }
            return best;
        }

        public void Open()
        {
            panelRoot.SetActive(true);
            RefreshAllLabels();
        }

        public void Close()
        {
            panelRoot.SetActive(false);
        }

        private void OnQualityButtonPressed()
        {
            int count = Mathf.Max(QualitySettings.names.Length, 1);
            _qualityIndex = (_qualityIndex + 1) % count;
            ApplyQuality();
            RefreshQualityLabel();
        }

        private void ApplyQuality()
        {
            QualitySettings.SetQualityLevel(_qualityIndex, true);
            PlayerPrefs.SetInt(QualityPrefKey, _qualityIndex);
        }

        private void OnVolumeButtonPressed()
        {
            _volumeIndex = (_volumeIndex + 1) % VolumeSteps.Length;
            ApplyVolume();
            RefreshVolumeLabel();
        }

        private void ApplyVolume()
        {
            float volume = VolumeSteps[_volumeIndex];
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(VolumePrefKey, volume);
        }

        private void OnSensitivityButtonPressed()
        {
            _sensitivityIndex = (_sensitivityIndex + 1) % SensitivitySteps.Length;
            ApplySensitivity();
            RefreshSensitivityLabel();
        }

        private void ApplySensitivity()
        {
            MouseSensitivity = SensitivitySteps[_sensitivityIndex];
            PlayerPrefs.SetFloat(SensitivityPrefKey, MouseSensitivity);
        }

        private void OnToggleFpsPressed()
        {
            FPSCounterController.Instance.Toggle();
            RefreshFpsLabel();
        }

        private void RefreshAllLabels()
        {
            RefreshQualityLabel();
            RefreshVolumeLabel();
            RefreshSensitivityLabel();
            RefreshFpsLabel();
        }

        private void RefreshQualityLabel()
        {
            string name = QualitySettings.names.Length > 0
                ? QualitySettings.names[_qualityIndex].ToUpperInvariant()
                : "VARSAYILAN";
            qualityButtonText.text = $"GRAFÝK: {name}";
        }

        private void RefreshVolumeLabel()
        {
            int percent = Mathf.RoundToInt(VolumeSteps[_volumeIndex] * 100f);
            volumeButtonText.text = $"SES: %{percent}";
        }

        private void RefreshSensitivityLabel()
        {
            sensitivityButtonText.text = $"HASSASÝYET: {SensitivitySteps[_sensitivityIndex]:0.0}x";
        }

        private void RefreshFpsLabel()
        {
            bool showing = FPSCounterController.Instance.IsVisible;
            toggleFpsButtonText.text = showing ? "FPS: AÇIK" : "FPS: KAPALI";
        }
    }
}