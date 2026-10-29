using UnityEditor;
using UnityEngine;
using Project.Gameplay.Player;
using Project.Gameplay.Weapons;
using Project.Data;

namespace Project.Editor
{
    public class WeaponHoldPoseEditorWindow : EditorWindow
    {
        private HeldWeaponArmPose _target;
        private WeaponController _weaponController;
        private WeaponBaseData _baseWeaponData;
        private GameObject _weaponPreview;
        private Transform _selectedBone;
        private bool _weaponSelected;
        private Vector2 _scrollPosition;

        [MenuItem("Tools/Bozulma/Weapon Hold Pose Positioner")]
        public static void ShowWindow() => GetWindow<WeaponHoldPoseEditorWindow>("Weapon Hold Pose");

        private void OnDisable()
        {
            if (_weaponPreview != null) { DestroyImmediate(_weaponPreview); _weaponPreview = null; }
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.HelpBox("Bu arac sadece Edit Mode'da calisir.", MessageType.Info);
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Play Mode'dasin. Lutfen durdur.", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUI.BeginChangeCheck();
            var newTarget = (HeldWeaponArmPose)EditorGUILayout.ObjectField("Karakter (HeldWeaponArmPose)", _target, typeof(HeldWeaponArmPose), true);
            if (EditorGUI.EndChangeCheck()) { _target = newTarget; if (_target != null) _target.EditorResolveBones(); }

            _weaponController = (WeaponController)EditorGUILayout.ObjectField("Silah (WeaponController)", _weaponController, typeof(WeaponController), true);
            if (_target == null)
            {
                EditorGUILayout.HelpBox("Once karakter uzerindeki HeldWeaponArmPose'u surukle.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }
            _target.EditorResolveBones();

            EditorGUILayout.Space(10);
            if (GUILayout.Button(_weaponPreview == null ? "Silahi Goster" : "Silah Gosteriliyor", GUILayout.Height(30))) ShowWeaponPreview();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Sag kol kemiklerini sirayla sec, Scene'de Rotate (E) ile dondur:", EditorStyles.boldLabel);
            if (GUILayout.Button("Ust Kol", GUILayout.Height(30))) SelectBone(_target.EditorRightUpperArm);
            if (GUILayout.Button("On Kol", GUILayout.Height(30))) SelectBone(_target.EditorRightLowerArm);
            if (GUILayout.Button("El", GUILayout.Height(30))) SelectBone(_target.EditorRightHand);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Bas Parmak:", EditorStyles.boldLabel);
            if (GUILayout.Button("1. Bogum", GUILayout.Height(26))) SelectBone(_target.EditorThumb);
            if (GUILayout.Button("2. Bogum", GUILayout.Height(26))) SelectBone(_target.EditorThumbMid);
            if (GUILayout.Button("Uc", GUILayout.Height(26))) SelectBone(_target.EditorThumbTip);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Isaret Parmagi:", EditorStyles.boldLabel);
            if (GUILayout.Button("1. Bogum", GUILayout.Height(26))) SelectBone(_target.EditorIndex);
            if (GUILayout.Button("2. Bogum", GUILayout.Height(26))) SelectBone(_target.EditorIndexMid);
            if (GUILayout.Button("Uc", GUILayout.Height(26))) SelectBone(_target.EditorIndexTip);
            if (GUILayout.Button("Isaret Parmagi Pozisyonunu Sabitle", GUILayout.Height(28))) FixIndexPosition();

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Orta Parmak:", EditorStyles.boldLabel);
            if (GUILayout.Button("1. Bogum", GUILayout.Height(26))) SelectBone(_target.EditorMiddle);
            if (GUILayout.Button("2. Bogum", GUILayout.Height(26))) SelectBone(_target.EditorMiddleMid);
            if (GUILayout.Button("Uc", GUILayout.Height(26))) SelectBone(_target.EditorMiddleTip);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Yuzuk Parmagi:", EditorStyles.boldLabel);
            if (GUILayout.Button("1. Bogum", GUILayout.Height(26))) SelectBone(_target.EditorRing);
            if (GUILayout.Button("2. Bogum", GUILayout.Height(26))) SelectBone(_target.EditorRingMid);
            if (GUILayout.Button("Uc", GUILayout.Height(26))) SelectBone(_target.EditorRingTip);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Serce Parmak:", EditorStyles.boldLabel);
            if (GUILayout.Button("1. Bogum", GUILayout.Height(26))) SelectBone(_target.EditorPinky);
            if (GUILayout.Button("2. Bogum", GUILayout.Height(26))) SelectBone(_target.EditorPinkyMid);
            if (GUILayout.Button("Uc", GUILayout.Height(26))) SelectBone(_target.EditorPinkyTip);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Silahin kendisi:", EditorStyles.boldLabel);
            if (GUILayout.Button("Silah (Weapon)", GUILayout.Height(32))) SelectWeaponForEditing();

            if (_selectedBone != null && !_weaponSelected)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Secili: {_selectedBone.name}");
                EditorGUILayout.HelpBox("Rotate aracini (E tusu) kullanarak dondur.", MessageType.None);
            }
            else if (_weaponSelected && _weaponPreview != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Secili: Silah");
                EditorGUILayout.HelpBox("Move (W) / Rotate (E) ile silahin konumunu/acisini ayarla.", MessageType.None);
            }

            EditorGUILayout.Space(15);
            if (GUILayout.Button("Pozu Kaydet", GUILayout.Height(35))) SavePose();

            EditorGUILayout.EndScrollView();
        }

        private void ShowWeaponPreview()
        {
            if (_weaponController == null) { Debug.LogWarning("[WeaponHoldPoseEditorWindow] Once 'Silah (WeaponController)' alanina surukle."); return; }

            var so = new SerializedObject(_weaponController);
            var socketProp = so.FindProperty("weaponSocket");
            var baseWeaponProp = so.FindProperty("baseWeapon");
            var socket = socketProp != null ? socketProp.objectReferenceValue as Transform : null;
            var baseWeapon = baseWeaponProp != null ? baseWeaponProp.objectReferenceValue as WeaponBaseData : null;

            if (socket == null || baseWeapon == null || baseWeapon.weaponModelPrefab == null)
            {
                Debug.LogWarning("[WeaponHoldPoseEditorWindow] WeaponController'in Weapon Socket / Base Weapon / Weapon Model Prefab alanlari eksik.");
                return;
            }

            _baseWeaponData = baseWeapon;

            if (_weaponPreview == null)
            {
                _weaponPreview = Instantiate(baseWeapon.weaponModelPrefab, socket);
                _weaponPreview.name = "_HoldPosePreview_Weapon";
                _weaponPreview.transform.localPosition = baseWeapon.modelPositionOffset;
                _weaponPreview.transform.localRotation = Quaternion.Euler(baseWeapon.modelRotationOffset);
            }
        }

        private void SelectBone(Transform bone)
        {
            if (bone == null) { Debug.LogWarning("[WeaponHoldPoseEditorWindow] Kemik bulunamadi."); return; }
            _selectedBone = bone; _weaponSelected = false;
            Selection.activeGameObject = bone.gameObject;
            Tools.current = Tool.Rotate;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        private void SelectWeaponForEditing()
        {
            if (_weaponPreview == null) { Debug.LogWarning("[WeaponHoldPoseEditorWindow] Once 'Silahi Goster'e bas."); return; }
            _selectedBone = null; _weaponSelected = true;
            Selection.activeGameObject = _weaponPreview;
            Tools.current = Tool.Move;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        private void FixIndexPosition()
        {
            var index = _target.EditorIndex;
            var indexMid = _target.EditorIndexMid;
            var indexTip = _target.EditorIndexTip;

            var so = new SerializedObject(_target);
            if (index != null) so.FindProperty("indexLocalPosition").vector3Value = index.localPosition;
            if (indexMid != null) so.FindProperty("indexMidLocalPosition").vector3Value = indexMid.localPosition;
            if (indexTip != null) so.FindProperty("indexTipLocalPosition").vector3Value = indexTip.localPosition;
            so.FindProperty("fixIndexPosition").boolValue = true;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_target);
            Debug.Log("[WeaponHoldPoseEditorWindow] Isaret parmagi pozisyonu sabitlendi (artik her karede korunacak).");
        }

        private void SavePose()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[WeaponHoldPoseEditorWindow] Sahnede 'MainCamera' tag'li bir kamera bulunamadi.");
                return;
            }
            Quaternion camInverse = Quaternion.Inverse(cam.transform.rotation);

            var ru = _target.EditorRightUpperArm;
            var rl = _target.EditorRightLowerArm;
            var rh = _target.EditorRightHand;

            var so = new SerializedObject(_target);
            if (ru != null) so.FindProperty("rightUpperArmRelativeEuler").vector3Value = (camInverse * ru.rotation).eulerAngles;
            if (rl != null) so.FindProperty("rightLowerArmRelativeEuler").vector3Value = (camInverse * rl.rotation).eulerAngles;
            if (rh != null) so.FindProperty("rightHandRelativeEuler").vector3Value = (camInverse * rh.rotation).eulerAngles;

            SaveFinger(so, _target.EditorThumb, "thumbEuler");
            SaveFinger(so, _target.EditorThumbMid, "thumbMidEuler");
            SaveFinger(so, _target.EditorThumbTip, "thumbTipEuler");

            SaveFinger(so, _target.EditorIndex, "indexEuler");
            SaveFinger(so, _target.EditorIndexMid, "indexMidEuler");
            SaveFinger(so, _target.EditorIndexTip, "indexTipEuler");

            SaveFinger(so, _target.EditorMiddle, "middleEuler");
            SaveFinger(so, _target.EditorMiddleMid, "middleMidEuler");
            SaveFinger(so, _target.EditorMiddleTip, "middleTipEuler");

            SaveFinger(so, _target.EditorRing, "ringEuler");
            SaveFinger(so, _target.EditorRingMid, "ringMidEuler");
            SaveFinger(so, _target.EditorRingTip, "ringTipEuler");

            SaveFinger(so, _target.EditorPinky, "pinkyEuler");
            SaveFinger(so, _target.EditorPinkyMid, "pinkyMidEuler");
            SaveFinger(so, _target.EditorPinkyTip, "pinkyTipEuler");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_target);

            if (_weaponPreview != null && _baseWeaponData != null)
            {
                var weaponSo = new SerializedObject(_baseWeaponData);
                weaponSo.FindProperty("modelPositionOffset").vector3Value = _weaponPreview.transform.localPosition;
                weaponSo.FindProperty("modelRotationOffset").vector3Value = _weaponPreview.transform.localEulerAngles;
                weaponSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(_baseWeaponData);
            }

            if (_weaponPreview != null) { DestroyImmediate(_weaponPreview); _weaponPreview = null; }
            Debug.Log("[WeaponHoldPoseEditorWindow] Poz kaydedildi.");
        }

        private void SaveFinger(SerializedObject so, Transform bone, string propertyName)
        {
            if (bone == null) return;
            var prop = so.FindProperty(propertyName);
            if (prop != null) prop.vector3Value = bone.localEulerAngles;
        }
    }
}