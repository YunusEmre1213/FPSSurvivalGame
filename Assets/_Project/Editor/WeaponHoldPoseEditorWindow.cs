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

        [MenuItem("Tools/Bozulma/Weapon Hold Pose Positioner")]
        public static void ShowWindow()
        {
            GetWindow<WeaponHoldPoseEditorWindow>("Weapon Hold Pose");
        }

        private void OnDisable()
        {
            if (_weaponPreview != null)
            {
                DestroyImmediate(_weaponPreview);
                _weaponPreview = null;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Bu arac sadece Edit Mode'da calisir. Play Mode'daki degisiklikler kaydedilmez.",
                MessageType.Info);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Play Mode'dasin. Lutfen durdur.", MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();
            var newTarget = (HeldWeaponArmPose)EditorGUILayout.ObjectField(
                "Karakter (HeldWeaponArmPose)", _target, typeof(HeldWeaponArmPose), true);
            if (EditorGUI.EndChangeCheck())
            {
                _target = newTarget;
                if (_target != null) _target.EditorResolveBones();
            }

            _weaponController = (WeaponController)EditorGUILayout.ObjectField(
                "Silah (WeaponController)", _weaponController, typeof(WeaponController), true);

            if (_target == null)
            {
                EditorGUILayout.HelpBox("Once karakter uzerindeki HeldWeaponArmPose'u surukle.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button(_weaponPreview == null ? "Silahi Goster" : "Silah Gosteriliyor", GUILayout.Height(30)))
            {
                ShowWeaponPreview();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Kol kemiklerini sirayla sec ve Scene'de dondur:", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Sag Kol (tetik eli):");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ust Kol", GUILayout.Height(32))) SelectBone(_target.EditorRightUpperArm);
            if (GUILayout.Button("On Kol", GUILayout.Height(32))) SelectBone(_target.EditorRightLowerArm);
            if (GUILayout.Button("El", GUILayout.Height(32))) SelectBone(_target.EditorRightHand);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Sol Kol (destek eli):");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ust Kol", GUILayout.Height(32))) SelectBone(_target.EditorLeftUpperArm);
            if (GUILayout.Button("On Kol", GUILayout.Height(32))) SelectBone(_target.EditorLeftLowerArm);
            if (GUILayout.Button("El", GUILayout.Height(32))) SelectBone(_target.EditorLeftHand);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Silahin kendisi:", EditorStyles.boldLabel);
            if (GUILayout.Button("Silah (Weapon)", GUILayout.Height(32)))
            {
                SelectWeaponForEditing();
            }

            if (_selectedBone != null && !_weaponSelected)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Secili: {_selectedBone.name}");
                EditorGUILayout.HelpBox("Rotate aracini (E tusu) kullanarak kemigi, silahi kavrayacak sekilde dondur.", MessageType.None);
            }
            else if (_weaponSelected && _weaponPreview != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Secili: Silah");
                EditorGUILayout.HelpBox("Move (W) / Rotate (E) ile silahin konumunu/acisini ayarla.", MessageType.None);
            }

            if (_weaponPreview != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.Vector3Field("Silah Konum", _weaponPreview.transform.localPosition);
                EditorGUILayout.Vector3Field("Silah Aci", _weaponPreview.transform.localEulerAngles);
            }

            EditorGUILayout.Space(15);
            if (GUILayout.Button("Kaydet (Save)", GUILayout.Height(35)))
            {
                SavePose();
            }
        }

        private void ShowWeaponPreview()
        {
            if (_weaponController == null)
            {
                Debug.LogWarning("[WeaponHoldPoseEditorWindow] Once 'Silah (WeaponController)' alanina surukle.");
                return;
            }

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
            if (bone == null)
            {
                Debug.LogWarning("[WeaponHoldPoseEditorWindow] Kemik bulunamadi.");
                return;
            }
            _selectedBone = bone;
            _weaponSelected = false;
            Selection.activeGameObject = bone.gameObject;
            Tools.current = Tool.Rotate;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        private void SelectWeaponForEditing()
        {
            if (_weaponPreview == null)
            {
                Debug.LogWarning("[WeaponHoldPoseEditorWindow] Once 'Silahi Goster'e bas.");
                return;
            }
            _selectedBone = null;
            _weaponSelected = true;
            Selection.activeGameObject = _weaponPreview;
            Tools.current = Tool.Move;
            SceneView.lastActiveSceneView?.FrameSelected();
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
            var lu = _target.EditorLeftUpperArm;
            var ll = _target.EditorLeftLowerArm;
            var lh = _target.EditorLeftHand;

            var so = new SerializedObject(_target);
            if (ru != null) so.FindProperty("rightUpperArmRelativeEuler").vector3Value = (camInverse * ru.rotation).eulerAngles;
            if (rl != null) so.FindProperty("rightLowerArmRelativeEuler").vector3Value = (camInverse * rl.rotation).eulerAngles;
            if (rh != null) so.FindProperty("rightHandRelativeEuler").vector3Value = (camInverse * rh.rotation).eulerAngles;
            if (lu != null) so.FindProperty("leftUpperArmRelativeEuler").vector3Value = (camInverse * lu.rotation).eulerAngles;
            if (ll != null) so.FindProperty("leftLowerArmRelativeEuler").vector3Value = (camInverse * ll.rotation).eulerAngles;
            if (lh != null) so.FindProperty("leftHandRelativeEuler").vector3Value = (camInverse * lh.rotation).eulerAngles;
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

            if (_weaponPreview != null)
            {
                DestroyImmediate(_weaponPreview);
                _weaponPreview = null;
            }

            Debug.Log("[WeaponHoldPoseEditorWindow] Tutus pozu ve silah konumu kaydedildi.");
        }
    }
}