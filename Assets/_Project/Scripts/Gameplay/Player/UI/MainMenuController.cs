using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Project.Core;

namespace Project.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private string gameplaySceneName = "SampleScene";

        private void Awake()
        {
            newGameButton.onClick.AddListener(StartNewGame);
            continueButton.onClick.AddListener(ContinueGame);
            quitButton.onClick.AddListener(QuitGame);
            continueButton.interactable = SaveFileExistsOnDisk();
        }

        private void StartNewGame()
        {
            PlayerPrefs.SetInt(GameFlowKeys.ShouldLoadSaveOnStart, 0);
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void ContinueGame()
        {
            PlayerPrefs.SetInt(GameFlowKeys.ShouldLoadSaveOnStart, 1);
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void QuitGame()
        {
            Debug.Log("[MainMenuController] Oyundan cikiliyor.");
            Application.Quit();
        }
        private bool SaveFileExistsOnDisk()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "save.json");
            return System.IO.File.Exists(path);
        }
    }
}