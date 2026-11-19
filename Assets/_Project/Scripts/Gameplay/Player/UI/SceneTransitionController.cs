using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.UI
{

    public class SceneTransitionController : MonoBehaviour
    {
        public static SceneTransitionController Instance { get; private set; }

        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeDuration = 0.8f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(FadeAndLoad(sceneName));
        }

        private IEnumerator FadeAndLoad(string sceneName)
        {
            fadeGroup.blocksRaycasts = true;

            yield return Fade(0f, 1f);

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncOp.isDone)
            {
                yield return null;
            }

            yield return Fade(1f, 0f);

            fadeGroup.blocksRaycasts = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                fadeGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            fadeGroup.alpha = to;
        }
    }
}