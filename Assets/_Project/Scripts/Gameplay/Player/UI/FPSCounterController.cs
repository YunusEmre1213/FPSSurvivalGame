using UnityEngine;
using TMPro;

namespace Project.UI
{

    public class FPSCounterController : MonoBehaviour
    {
        public static FPSCounterController Instance { get; private set; }

        private const string PrefKey = "ShowFPS";

        [SerializeField] private TMP_Text fpsText;
        [SerializeField] private float updateInterval = 0.5f;

        private float _timer;
        private int _frameCount;

        private bool _wantVisible;
        private bool _suppressed;

        public bool IsVisible => _wantVisible;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _wantVisible = PlayerPrefs.GetInt(PrefKey, 0) == 1;
            RefreshActiveState();
        }

        public void SetVisible(bool visible)
        {
            _wantVisible = visible;
            PlayerPrefs.SetInt(PrefKey, visible ? 1 : 0);
            RefreshActiveState();
        }

        public void Toggle()
        {
            SetVisible(!IsVisible);
        }

        public void SetSuppressed(bool suppressed)
        {
            _suppressed = suppressed;
            RefreshActiveState();
        }

        private void RefreshActiveState()
        {
            fpsText.gameObject.SetActive(_wantVisible && !_suppressed);
        }

        private void Update()
        {
            if (!fpsText.gameObject.activeSelf) return;

            _timer += Time.unscaledDeltaTime;
            _frameCount++;

            if (_timer >= updateInterval)
            {
                float fps = _frameCount / _timer;
                fpsText.text = $"FPS: {fps:F0}";
                _timer = 0f;
                _frameCount = 0;
            }
        }
    }
}