using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Project.Core;
using Project.Systems;
using Project.Gameplay.Player;

namespace Project.UI
{
    public class PauseUIController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;

        [Header("Butonlar")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("Ayarlar Paneli")]
        [Tooltip("Bu sahneye ozel (yerel) Ayarlar paneli kopyasi - ana menudekiyle ayni script, ayri instance.")]
        [SerializeField] private SettingsPanelController settingsPanelController;

        [Header("Sahne Gecisi")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private bool _isPaused;

        private void Awake()
        {
            resumeButton.onClick.AddListener(Resume);
            settingsButton.onClick.AddListener(OnSettingsPressed);
            saveButton.onClick.AddListener(OnSavePressed);
            loadButton.onClick.AddListener(OnLoadPressed);
            mainMenuButton.onClick.AddListener(OnMainMenuPressed);
            quitButton.onClick.AddListener(OnQuitPressed);
            panelRoot.SetActive(false);

            if (settingsPanelController != null)
            {
                settingsPanelController.OnClosed += OnSettingsClosed;
            }
        }

        private void OnDestroy()
        {
            if (settingsPanelController != null)
            {
                settingsPanelController.OnClosed -= OnSettingsClosed;
            }
        }

        private void Update()
        {
            var input = ActiveInput.Provider;
            if (input == null || !input.PausePressedThisFrame) return;

            if (_isPaused)
            {
                Resume();
            }
            else if (!UIInputLock.IsLocked)
            {
                Pause();
            }
        }

        private void Pause()
        {
            _isPaused = true;
            panelRoot.SetActive(true);
            UIInputLock.Lock();
            Time.timeScale = 0f;
        }

        private void Resume()
        {
            _isPaused = false;
            panelRoot.SetActive(false);
            UIInputLock.Unlock();
            Time.timeScale = 1f;
        }

        private void OnSettingsPressed()
        {

            panelRoot.SetActive(false);
            settingsPanelController.Open();
        }

        private void OnSettingsClosed()
        {

            if (_isPaused)
            {
                panelRoot.SetActive(true);
            }
        }

        private void OnSavePressed()
        {
            ServiceLocator.Instance.Get<ISaveService>().Save();
        }

        private void OnLoadPressed()
        {
            ServiceLocator.Instance.Get<ISaveService>().Load();
        }

        private void OnMainMenuPressed()
        {

            _isPaused = false;
            UIInputLock.Unlock();
            Time.timeScale = 1f;

            if (SceneTransitionController.Instance != null)
            {
                SceneTransitionController.Instance.LoadScene(mainMenuSceneName);
            }
            else
            {

                SceneManager.LoadScene(mainMenuSceneName);
            }
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