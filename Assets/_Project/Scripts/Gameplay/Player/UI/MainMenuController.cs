using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Butonlar")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            playButton.onClick.AddListener(OnPlayPressed);
            settingsButton.onClick.AddListener(OnSettingsPressed);
            quitButton.onClick.AddListener(OnQuitPressed);
        }

        [Header("Sahne Gecisi")]
        [SerializeField] private string gameplaySceneName = "SampleScene";

        private void OnPlayPressed()
        {
            SceneTransitionController.Instance.LoadScene(gameplaySceneName);
        }

        private void OnSettingsPressed()
        {

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