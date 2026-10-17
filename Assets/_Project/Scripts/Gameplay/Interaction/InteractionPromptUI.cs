using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        public static InteractionPromptUI Instance { get; private set; }

        [SerializeField] private GameObject promptRoot;
        [SerializeField] private Text promptText;

        private GameObject _currentOwner;

        private void Awake()
        {
            Instance = this;
            if (promptRoot != null) promptRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Show(GameObject owner, string text)
        {
            _currentOwner = owner;
            if (promptText != null) promptText.text = text;
            if (promptRoot != null) promptRoot.SetActive(true);
        }

        public void Hide(GameObject owner)
        {
            if (_currentOwner != owner) return;
            _currentOwner = null;
            if (promptRoot != null) promptRoot.SetActive(false);
        }
    }
}