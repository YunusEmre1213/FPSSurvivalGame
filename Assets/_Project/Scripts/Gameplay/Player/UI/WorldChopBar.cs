using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(RectTransform))]
    public class WorldChopBar : MonoBehaviour
    {
        [Header("Boyut (Dunya Biriminde Cap)")]
        [Tooltip("Barin dunyada kapladigi gercek genislik (metre). Kucuk tut, orn. 0.3 - 0.5")]
        [SerializeField] private float worldDiameter = 0.4f;

        [Header("Referanslar")]
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private Image fillImage;
        [SerializeField] private float hideDelay = 1.5f;

        private float _hideTimer;
        private bool _visible;
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;

            // Canvas ayarlarini kod icinde zorla dogru yap - Inspector'daki
            // Scale/Render Mode degerlerine guvenmiyoruz, boylece "dev kirmizi
            // daire" hatasi bir daha olamaz.
            Canvas canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform rt = GetComponent<RectTransform>();
            if (rt.sizeDelta.x <= 0f) rt.sizeDelta = new Vector2(100f, 100f);

            float scale = worldDiameter / rt.sizeDelta.x;
            transform.localScale = new Vector3(scale, scale, scale);

            if (visualRoot != null) visualRoot.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return;
            }

            transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position);

            if (_visible)
            {
                _hideTimer -= Time.deltaTime;
                if (_hideTimer <= 0f) Hide();
            }
        }

        public void SetProgress(float normalized)
        {
            _visible = true;
            _hideTimer = hideDelay;

            if (visualRoot != null) visualRoot.SetActive(true);
            if (fillImage != null) fillImage.fillAmount = normalized;
        }

        public void Hide()
        {
            _visible = false;
            if (visualRoot != null) visualRoot.SetActive(false);
        }
    }
}