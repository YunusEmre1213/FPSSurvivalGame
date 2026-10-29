using UnityEngine;

namespace Project.Gameplay.Player
{
    public class HeldWeaponArmPose : MonoBehaviour
    {
        [Header("Karakter Animator'i ve Kamera")]
        [SerializeField] private Animator characterAnimator;
        [Tooltip("Pozun gore hesaplanacagi kamera - bos birakirsan Camera.main kullanilir.")]
        [SerializeField] private Transform cameraTransform;

        [Header("Tabanca tutus pozu - kameraya GORE acilar (WeaponHoldPoseEditorWindow ile doldurulur)")]
        [SerializeField] private Vector3 rightUpperArmRelativeEuler;
        [SerializeField] private Vector3 rightLowerArmRelativeEuler;
        [SerializeField] private Vector3 rightHandRelativeEuler;
        [SerializeField] private Vector3 leftUpperArmRelativeEuler;
        [SerializeField] private Vector3 leftLowerArmRelativeEuler;
        [SerializeField] private Vector3 leftHandRelativeEuler;

        [Header("Zamanlama")]
        [Tooltip("Pozun kalkma suresi (saniye).")]
        [SerializeField] private float riseDuration = 0.08f;
        [Tooltip("Son atistan sonra pozda kalma suresi (saniye).")]
        [SerializeField] private float holdTimeout = 0.35f;
        [Tooltip("Normale donme suresi (saniye).")]
        [SerializeField] private float returnDuration = 0.25f;

        private Transform _rightUpperArm;
        private Transform _rightLowerArm;
        private Transform _rightHand;
        private Transform _leftUpperArm;
        private Transform _leftLowerArm;
        private Transform _leftHand;

        private float _currentWeight;
        private float _lastFireTime = -999f;

        private void Awake()
        {
            if (characterAnimator == null)
            {
                characterAnimator = GetComponent<Animator>();
            }
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            ResolveBones();
        }

        private void ResolveBones()
        {
            if (characterAnimator == null) return;
            _rightUpperArm = characterAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _rightLowerArm = characterAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            _rightHand = characterAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            _leftUpperArm = characterAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            _leftLowerArm = characterAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            _leftHand = characterAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
        }

        public void TriggerFire()
        {
            _lastFireTime = Time.time;
        }

        private void LateUpdate()
        {
            bool wantsRaised = (Time.time - _lastFireTime) <= holdTimeout;

            if (!wantsRaised && _currentWeight <= 0f) return;
            if (cameraTransform == null) return;

            if (_rightUpperArm == null)
            {
                ResolveBones();
                if (_rightUpperArm == null) return;
            }

            float target = wantsRaised ? 1f : 0f;
            float duration = wantsRaised ? riseDuration : returnDuration;
            float speed = duration > 0f ? 1f / duration : 999f;

            _currentWeight = Mathf.MoveTowards(_currentWeight, target, speed * Time.deltaTime);
            if (_currentWeight <= 0f) return;

            ApplyBone(_rightUpperArm, rightUpperArmRelativeEuler);
            ApplyBone(_rightLowerArm, rightLowerArmRelativeEuler);
            ApplyBone(_rightHand, rightHandRelativeEuler);
            ApplyBone(_leftUpperArm, leftUpperArmRelativeEuler);
            ApplyBone(_leftLowerArm, leftLowerArmRelativeEuler);
            ApplyBone(_leftHand, leftHandRelativeEuler);
        }

        private void ApplyBone(Transform bone, Vector3 relativeEuler)
        {
            if (bone == null) return;
            Quaternion desiredWorld = cameraTransform.rotation * Quaternion.Euler(relativeEuler);
            bone.rotation = Quaternion.Slerp(bone.rotation, desiredWorld, _currentWeight);
        }

#if UNITY_EDITOR
        public Transform EditorRightUpperArm => _rightUpperArm != null ? _rightUpperArm : GetBone(HumanBodyBones.RightUpperArm);
        public Transform EditorRightLowerArm => _rightLowerArm != null ? _rightLowerArm : GetBone(HumanBodyBones.RightLowerArm);
        public Transform EditorRightHand => _rightHand != null ? _rightHand : GetBone(HumanBodyBones.RightHand);
        public Transform EditorLeftUpperArm => _leftUpperArm != null ? _leftUpperArm : GetBone(HumanBodyBones.LeftUpperArm);
        public Transform EditorLeftLowerArm => _leftLowerArm != null ? _leftLowerArm : GetBone(HumanBodyBones.LeftLowerArm);
        public Transform EditorLeftHand => _leftHand != null ? _leftHand : GetBone(HumanBodyBones.LeftHand);

        private Transform GetBone(HumanBodyBones bone)
        {
            return characterAnimator != null ? characterAnimator.GetBoneTransform(bone) : null;
        }

        public void EditorResolveBones() => ResolveBones();
#endif
    }
}