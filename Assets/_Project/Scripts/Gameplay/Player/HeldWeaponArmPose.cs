using UnityEngine;

namespace Project.Gameplay.Player
{
    public class HeldWeaponArmPose : MonoBehaviour
    {
        [Header("Karakter Animator'i ve Kamera")]
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private Transform cameraTransform;

        [Header("Sag kol - kameraya GORE acilar (ates pozu)")]
        [SerializeField] private Vector3 rightUpperArmRelativeEuler;
        [SerializeField] private Vector3 rightLowerArmRelativeEuler;
        [SerializeField] private Vector3 rightHandRelativeEuler;

        [Header("Bas Parmak - ele GORE (local) acilar")]
        [SerializeField] private Vector3 thumbEuler;
        [SerializeField] private Vector3 thumbMidEuler;
        [SerializeField] private Vector3 thumbTipEuler;

        [Header("Isaret Parmagi - ele GORE (local) acilar")]
        [SerializeField] private Vector3 indexEuler;
        [SerializeField] private Vector3 indexMidEuler;
        [SerializeField] private Vector3 indexTipEuler;

        [Header("Isaret Parmagi - POZISYON sabitleme (animasyon bozuyorsa)")]
        [SerializeField] private bool fixIndexPosition;
        [SerializeField] private Vector3 indexLocalPosition;
        [SerializeField] private Vector3 indexMidLocalPosition;
        [SerializeField] private Vector3 indexTipLocalPosition;

        [Header("Orta Parmak - ele GORE (local) acilar")]
        [SerializeField] private Vector3 middleEuler;
        [SerializeField] private Vector3 middleMidEuler;
        [SerializeField] private Vector3 middleTipEuler;

        [Header("Yuzuk Parmagi - ele GORE (local) acilar")]
        [SerializeField] private Vector3 ringEuler;
        [SerializeField] private Vector3 ringMidEuler;
        [SerializeField] private Vector3 ringTipEuler;

        [Header("Serce Parmak - ele GORE (local) acilar")]
        [SerializeField] private Vector3 pinkyEuler;
        [SerializeField] private Vector3 pinkyMidEuler;
        [SerializeField] private Vector3 pinkyTipEuler;

        [Header("Zamanlama")]
        [SerializeField] private float riseDuration = 0.08f;
        [SerializeField] private float holdTimeout = 1.5f;
        [SerializeField] private float returnDuration = 0.3f;

        [Header("Mod: false = Ates/Vurus tetiklemeli (Silah/Balta), true = Acik/Kapali (Fener)")]
        [SerializeField] private bool useToggleMode = false;

        [Header("Geri Tepme (Recoil)")]
        [SerializeField] private Vector3 recoilKickEuler = new Vector3(-8f, 0f, 0f);
        [SerializeField] private float recoilRecoverySpeed = 8f;

        [Header("Vurus Sirasinda Kol Hareketi (Balta icin)")]
        [SerializeField] private Vector3 swingUpperArmExtraEuler;
        [SerializeField] private Vector3 swingLowerArmExtraEuler;
        [SerializeField] private Vector3 swingHandExtraEuler;

        private Transform _rightUpperArm;
        private Transform _rightLowerArm;
        private Transform _rightHand;

        private Transform _thumb, _thumbMid, _thumbTip;
        private Transform _index, _indexMid, _indexTip;
        private Transform _middle, _middleMid, _middleTip;
        private Transform _ring, _ringMid, _ringTip;
        private Transform _pinky, _pinkyMid, _pinkyTip;

        private float _currentWeight;
        private float _lastFireTime = -999f;
        private bool _isOn;
        private Vector3 _recoilCurrent;
        private float _swingBlend;

        private void Awake()
        {
            if (characterAnimator == null) characterAnimator = GetComponent<Animator>();
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
            ResolveBones();
        }

        private void ResolveBones()
        {
            if (characterAnimator == null) return;
            _rightUpperArm = characterAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _rightLowerArm = characterAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            _rightHand = characterAnimator.GetBoneTransform(HumanBodyBones.RightHand);

            _thumb = FindDeep(_rightHand, "thumb_01_r");
            _thumbMid = FindDeep(_rightHand, "thumb_02_r");
            _thumbTip = FindDeep(_rightHand, "thumb_03_r");

            _index = FindDeep(_rightHand, "index_01_r");
            _indexMid = FindDeep(_rightHand, "index_02_r");
            _indexTip = FindDeep(_rightHand, "index_03_r");

            _middle = FindDeep(_rightHand, "middle_01_r");
            _middleMid = FindDeep(_rightHand, "middle_02_r");
            _middleTip = FindDeep(_rightHand, "middle_03_r");

            _ring = FindDeep(_rightHand, "ring_01_r");
            _ringMid = FindDeep(_rightHand, "ring_02_r");
            _ringTip = FindDeep(_rightHand, "ring_03_r");

            _pinky = FindDeep(_rightHand, "pinky_01_r");
            _pinkyMid = FindDeep(_rightHand, "pinky_02_r");
            _pinkyTip = FindDeep(_rightHand, "pinky_03_r");
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            if (parent == null) return null;
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                var found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }

        public void TriggerFire()
        {
            _lastFireTime = Time.time;
            _recoilCurrent += recoilKickEuler;
            _recoilCurrent = Vector3.ClampMagnitude(_recoilCurrent, recoilKickEuler.magnitude * 3f);
        }

        public void SetOn(bool on)
        {
            _isOn = on;
        }

        public void SetSwingProgress(float t)
        {
            _swingBlend = Mathf.Clamp01(t);
        }

        private void LateUpdate()
        {

            if (_rightUpperArm == null)
            {
                ResolveBones();
            }

            if (fixIndexPosition)
            {
                if (_index != null) _index.localPosition = indexLocalPosition;
                if (_indexMid != null) _indexMid.localPosition = indexMidLocalPosition;
                if (_indexTip != null) _indexTip.localPosition = indexTipLocalPosition;
            }

            bool wantsRaised = useToggleMode ? _isOn : (Time.time - _lastFireTime) <= holdTimeout;
            float target = wantsRaised ? 1f : 0f;
            float duration = wantsRaised ? riseDuration : returnDuration;
            float speed = duration > 0f ? 1f / duration : 999f;
            _currentWeight = Mathf.MoveTowards(_currentWeight, target, speed * Time.deltaTime);

            _recoilCurrent = Vector3.Lerp(_recoilCurrent, Vector3.zero, recoilRecoverySpeed * Time.deltaTime);

            if (_currentWeight <= 0f) return;
            if (cameraTransform == null) return;
            if (_rightUpperArm == null) return;

            ApplyBone(_rightUpperArm, rightUpperArmRelativeEuler + _recoilCurrent * 0.3f + swingUpperArmExtraEuler * _swingBlend);
            ApplyBone(_rightLowerArm, rightLowerArmRelativeEuler + _recoilCurrent * 0.6f + swingLowerArmExtraEuler * _swingBlend);
            ApplyBone(_rightHand, rightHandRelativeEuler + _recoilCurrent + swingHandExtraEuler * _swingBlend);

            ApplyFinger(_thumb, thumbEuler);
            ApplyFinger(_thumbMid, thumbMidEuler);
            ApplyFinger(_thumbTip, thumbTipEuler);

            ApplyFinger(_index, indexEuler);
            ApplyFinger(_indexMid, indexMidEuler);
            ApplyFinger(_indexTip, indexTipEuler);

            ApplyFinger(_middle, middleEuler);
            ApplyFinger(_middleMid, middleMidEuler);
            ApplyFinger(_middleTip, middleTipEuler);

            ApplyFinger(_ring, ringEuler);
            ApplyFinger(_ringMid, ringMidEuler);
            ApplyFinger(_ringTip, ringTipEuler);

            ApplyFinger(_pinky, pinkyEuler);
            ApplyFinger(_pinkyMid, pinkyMidEuler);
            ApplyFinger(_pinkyTip, pinkyTipEuler);
        }

        private void ApplyBone(Transform bone, Vector3 relativeEuler)
        {
            if (bone == null) return;
            Quaternion desiredWorld = cameraTransform.rotation * Quaternion.Euler(relativeEuler);
            bone.rotation = Quaternion.Slerp(bone.rotation, desiredWorld, _currentWeight);
        }

        private void ApplyFinger(Transform bone, Vector3 localEuler)
        {
            if (bone == null) return;
            bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(localEuler), _currentWeight);
        }

#if UNITY_EDITOR
        public Transform EditorRightUpperArm => _rightUpperArm != null ? _rightUpperArm : GetBone(HumanBodyBones.RightUpperArm);
        public Transform EditorRightLowerArm => _rightLowerArm != null ? _rightLowerArm : GetBone(HumanBodyBones.RightLowerArm);
        public Transform EditorRightHand => _rightHand != null ? _rightHand : GetBone(HumanBodyBones.RightHand);

        public Transform EditorThumb => _thumb;
        public Transform EditorThumbMid => _thumbMid;
        public Transform EditorThumbTip => _thumbTip;

        public Transform EditorIndex => _index;
        public Transform EditorIndexMid => _indexMid;
        public Transform EditorIndexTip => _indexTip;

        public Transform EditorMiddle => _middle;
        public Transform EditorMiddleMid => _middleMid;
        public Transform EditorMiddleTip => _middleTip;

        public Transform EditorRing => _ring;
        public Transform EditorRingMid => _ringMid;
        public Transform EditorRingTip => _ringTip;

        public Transform EditorPinky => _pinky;
        public Transform EditorPinkyMid => _pinkyMid;
        public Transform EditorPinkyTip => _pinkyTip;

        private Transform GetBone(HumanBodyBones bone) => characterAnimator != null ? characterAnimator.GetBoneTransform(bone) : null;
        public void EditorResolveBones() => ResolveBones();
#endif
    }
}