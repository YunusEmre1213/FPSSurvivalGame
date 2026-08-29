using UnityEngine;
using UnityEngine.UI;
using Project.Core;
using Project.Systems;
using Project.Gameplay.Player;

namespace Project.UI
{
    public class PauseUIController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button quitButton;

        private bool _isPaused;

        private void Awake()
        {
            resumeButton.onClick.AddListener(Resume);
            saveButton.onClick.AddListener(OnSavePressed);
            loadButton.onClick.AddListener(OnLoadPressed);
            quitButton.onClick.AddListener(OnQuitPressed);
            panelRoot.SetActive(false);
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

        private void OnSavePressed()
        {
            ServiceLocator.Instance.Get<ISaveService>().Save();
        }

        private void OnLoadPressed()
        {
            ServiceLocator.Instance.Get<ISaveService>().Load();
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