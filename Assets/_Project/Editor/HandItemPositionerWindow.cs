#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Project.Gameplay.Weapons;
using Project.Gameplay.Items;
using Project.Gameplay.Tools;

namespace Project.EditorTools
{
    public class HandItemPositionerWindow : EditorWindow
    {
        private enum ItemType { Weapon, Flashlight, Axe, Bucket }

        private ItemType _selectedType = ItemType.Weapon;
        private GameObject _player;
        private GameObject _previewInstance;
        private Vector3 _previewPosition;
        private Vector3 _previewRotationEuler;

        private const string PreviewObjectName = "__HandItemPreview__";
        private const float NudgeStepPosition = 0.005f;
        private const float NudgeStepRotation = 1f;

        [MenuItem("Bozulma/Hand Item Positioner")]
        public static void ShowWindow()
        {
            var window = GetWindow<HandItemPositionerWindow>("El Item Konumlandirici");
            window.minSize = new Vector2(380, 520);
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            DestroyPreview();
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                DestroyPreview();
            }
        }

        private void OnEditorUpdate()
        {
            if (_previewInstance == null) return;

            var liveLocalPos = _previewInstance.transform.localPosition;
            var liveLocalRot = _previewInstance.transform.localRotation.eulerAngles;

            if (liveLocalPos != _previewPosition || liveLocalRot != _previewRotationEuler)
            {
                _previewPosition = liveLocalPos;
                _previewRotationEuler = liveLocalRot;
                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("El Item Konumlandirici", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Item secip 'Onizlemeyi Baslat'a bas, sahnede beliren objeyi Move (W) / Rotate (E) " +
                "araclariyla surukle, begenince Kaydet'e bas. Play modunda calismaz.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Play modundasin - once Play'den cik.", MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();
            _player = (GameObject)EditorGUILayout.ObjectField("Player (sahnedeki)", _player, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                DestroyPreview();
            }

            if (_player == null)
            {
                EditorGUILayout.HelpBox("Hierarchy'den Player objesini yukariya surukle.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(8);
            EditorGUI.BeginChangeCheck();
            _selectedType = (ItemType)GUILayout.Toolbar((int)_selectedType, new[] { "Tabanca", "Fener", "Balta", "Kova" });
            if (EditorGUI.EndChangeCheck())
            {
                DestroyPreview();
            }

            EditorGUILayout.Space(10);

            var context = BuildContext();
            if (context == null) return;

            EditorGUILayout.LabelField("Controller", context.ControllerGO.name);
            EditorGUILayout.LabelField("Socket", context.Socket.name);
            EditorGUILayout.LabelField("Model Prefab", context.Prefab != null ? context.Prefab.name : "(atanmamis!)");

            EditorGUILayout.Space(10);

            using (new EditorGUI.DisabledScope(context.Prefab == null))
            {
                if (_previewInstance == null)
                {
                    if (GUILayout.Button("Onizlemeyi Baslat", GUILayout.Height(32)))
                    {
                        StartPreview(context);
                    }
                }
                else
                {
                    DrawPreviewControls(context);
                }
            }
        }

        private void DrawPreviewControls(ItemContext context)
        {
            EditorGUILayout.LabelField("Pozisyon (Local)", EditorStyles.boldLabel);
            var newPos = EditorGUILayout.Vector3Field(GUIContent.none, _previewPosition);
            if (newPos != _previewPosition)
            {
                _previewPosition = newPos;
                _previewInstance.transform.localPosition = newPos;
            }
            DrawNudgeRow(isRotation: false, NudgeStepPosition);

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Rotasyon (Local, derece)", EditorStyles.boldLabel);
            var newRot = EditorGUILayout.Vector3Field(GUIContent.none, _previewRotationEuler);
            if (newRot != _previewRotationEuler)
            {
                _previewRotationEuler = newRot;
                _previewInstance.transform.localRotation = Quaternion.Euler(newRot);
            }
            DrawNudgeRow(isRotation: true, NudgeStepRotation);

            EditorGUILayout.Space(16);

            if (GUILayout.Button("FPS Acisindan Bak (Main Camera)", GUILayout.Height(24)))
            {
                SnapSceneViewToCamera();
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Sifirla", GUILayout.Height(28)))
                {
                    SetPreviewTransform(Vector3.zero, Vector3.zero);
                }

                var prevColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
                if (GUILayout.Button("Kaydet", GUILayout.Height(28)))
                {
                    SaveOffsets(context);
                }
                GUI.backgroundColor = prevColor;
            }

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Onizlemeyi Kapat"))
            {
                DestroyPreview();
            }
        }

        private void DrawNudgeRow(bool isRotation, float step)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(isRotation ? "Ince ayar (°):" : "Ince ayar (m):", GUILayout.Width(90));

                var current = isRotation ? _previewRotationEuler : _previewPosition;

                current.x = NudgeAxis("X", current.x, step);
                current.y = NudgeAxis("Y", current.y, step);
                current.z = NudgeAxis("Z", current.z, step);

                if (isRotation)
                {
                    _previewRotationEuler = current;
                }
                else
                {
                    _previewPosition = current;
                }
            }

            if (_previewInstance != null)
            {
                SetPreviewTransform(_previewPosition, _previewRotationEuler);
            }
        }

        private float NudgeAxis(string label, float value, float step)
        {
            GUILayout.Label(label, GUILayout.Width(12));
            if (GUILayout.Button("-", GUILayout.Width(22))) value -= step;
            if (GUILayout.Button("+", GUILayout.Width(22))) value += step;
            return value;
        }

        private void SetPreviewTransform(Vector3 pos, Vector3 rotEuler)
        {
            _previewPosition = pos;
            _previewRotationEuler = rotEuler;
            if (_previewInstance != null)
            {
                _previewInstance.transform.localPosition = pos;
                _previewInstance.transform.localRotation = Quaternion.Euler(rotEuler);
            }
        }

        private void SnapSceneViewToCamera()
        {
            var cam = _player.GetComponentInChildren<Camera>();
            if (cam == null)
            {
                Debug.LogWarning("[HandItemPositioner] Player altinda Camera bulunamadi.");
                return;
            }

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            sceneView.AlignViewToObject(cam.transform);
        }

        private class ItemContext
        {
            public GameObject ControllerGO;
            public Transform Socket;
            public GameObject Prefab;
            public SerializedObject OffsetTarget;
            public string PositionPropertyName;
            public string RotationPropertyName;
        }

        private ItemContext BuildContext()
        {
            switch (_selectedType)
            {
                case ItemType.Weapon: return BuildWeaponContext();
                case ItemType.Flashlight: return BuildFlashlightContext();
                case ItemType.Axe: return BuildAxeContext();
                case ItemType.Bucket: return BuildBucketContext();
            }
            return null;
        }

        private ItemContext BuildWeaponContext()
        {
            var controller = _player.GetComponentInChildren<WeaponController>(true);
            if (controller == null)
            {
                EditorGUILayout.HelpBox("Player altinda WeaponController bulunamadi.", MessageType.Error);
                return null;
            }

            var so = new SerializedObject(controller);
            var socket = so.FindProperty("weaponSocket").objectReferenceValue as Transform;
            var baseWeapon = so.FindProperty("baseWeapon").objectReferenceValue;

            if (socket == null)
            {
                EditorGUILayout.HelpBox("WeaponController -> Weapon Socket atanmamis.", MessageType.Error);
                return null;
            }
            if (baseWeapon == null)
            {
                EditorGUILayout.HelpBox("WeaponController -> Base Weapon atanmamis.", MessageType.Error);
                return null;
            }

            var dataSo = new SerializedObject(baseWeapon);
            var prefab = dataSo.FindProperty("weaponModelPrefab").objectReferenceValue as GameObject;

            return new ItemContext
            {
                ControllerGO = controller.gameObject,
                Socket = socket,
                Prefab = prefab,
                OffsetTarget = dataSo,
                PositionPropertyName = "modelPositionOffset",
                RotationPropertyName = "modelRotationOffset"
            };
        }

        private ItemContext BuildFlashlightContext()
        {
            var controller = _player.GetComponentInChildren<FlashlightController>(true);
            if (controller == null)
            {
                EditorGUILayout.HelpBox("Player altinda FlashlightController bulunamadi.", MessageType.Error);
                return null;
            }

            var so = new SerializedObject(controller);
            var socket = so.FindProperty("flashlightSocket").objectReferenceValue as Transform;
            var prefab = so.FindProperty("flashlightModelPrefab").objectReferenceValue as GameObject;

            if (socket == null)
            {
                EditorGUILayout.HelpBox("FlashlightController -> Flashlight Socket atanmamis.", MessageType.Error);
                return null;
            }

            return new ItemContext
            {
                ControllerGO = controller.gameObject,
                Socket = socket,
                Prefab = prefab,
                OffsetTarget = so,
                PositionPropertyName = "modelPositionOffset",
                RotationPropertyName = "modelRotationOffset"
            };
        }

        private ItemContext BuildAxeContext()
        {
            var controller = _player.GetComponentInChildren<AxeController>(true);
            if (controller == null)
            {
                EditorGUILayout.HelpBox("Player altinda AxeController bulunamadi.", MessageType.Error);
                return null;
            }

            var so = new SerializedObject(controller);
            var socket = so.FindProperty("axeSocket").objectReferenceValue as Transform;
            var axeData = so.FindProperty("axeData").objectReferenceValue;

            if (socket == null)
            {
                EditorGUILayout.HelpBox("AxeController -> Axe Socket atanmamis.", MessageType.Error);
                return null;
            }

            GameObject prefab = null;
            if (axeData != null)
            {
                var dataSo = new SerializedObject(axeData);
                prefab = dataSo.FindProperty("axeModelPrefab").objectReferenceValue as GameObject;
            }

            return new ItemContext
            {
                ControllerGO = controller.gameObject,
                Socket = socket,
                Prefab = prefab,
                OffsetTarget = so,
                PositionPropertyName = "restPositionOffset",
                RotationPropertyName = "restRotationOffset"
            };
        }

        private ItemContext BuildBucketContext()
        {
            var controller = _player.GetComponentInChildren<BucketController>(true);
            if (controller == null)
            {
                EditorGUILayout.HelpBox("Player altinda BucketController bulunamadi.", MessageType.Error);
                return null;
            }

            var so = new SerializedObject(controller);
            var socket = so.FindProperty("bucketSocket").objectReferenceValue as Transform;
            var bucketData = so.FindProperty("bucketData").objectReferenceValue;

            if (socket == null)
            {
                EditorGUILayout.HelpBox("BucketController -> Bucket Socket atanmamis.", MessageType.Error);
                return null;
            }

            GameObject prefab = null;
            if (bucketData != null)
            {
                var dataSo = new SerializedObject(bucketData);
                prefab = dataSo.FindProperty("bucketModelPrefab").objectReferenceValue as GameObject;
            }

            return new ItemContext
            {
                ControllerGO = controller.gameObject,
                Socket = socket,
                Prefab = prefab,
                OffsetTarget = so,
                PositionPropertyName = "restPositionOffset",
                RotationPropertyName = "restRotationOffset"
            };
        }

        private void StartPreview(ItemContext context)
        {
            DestroyPreview();
            if (context.Prefab == null) return;

            _previewInstance = PrefabUtility.InstantiatePrefab(context.Prefab) as GameObject;
            if (_previewInstance == null)
            {
                Debug.LogError("[HandItemPositioner] Prefab instantiate edilemedi.");
                return;
            }

            _previewInstance.transform.SetParent(context.Socket, false);
            _previewInstance.name = PreviewObjectName;
            _previewInstance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

            var posProp = context.OffsetTarget.FindProperty(context.PositionPropertyName);
            var rotProp = context.OffsetTarget.FindProperty(context.RotationPropertyName);

            SetPreviewTransform(posProp.vector3Value, rotProp.vector3Value);

            Selection.activeGameObject = _previewInstance;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        private void SaveOffsets(ItemContext context)
        {
            if (_previewInstance == null) return;

            var so = context.OffsetTarget;
            so.Update();

            var posProp = so.FindProperty(context.PositionPropertyName);
            var rotProp = so.FindProperty(context.RotationPropertyName);

            posProp.vector3Value = _previewInstance.transform.localPosition;
            rotProp.vector3Value = _previewInstance.transform.localRotation.eulerAngles;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(so.targetObject);

            if (so.targetObject is Component comp)
            {
                EditorSceneManager.MarkSceneDirty(comp.gameObject.scene);
            }
            else
            {
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"[HandItemPositioner] Kaydedildi -> Pos: {posProp.vector3Value}, Rot: {rotProp.vector3Value}");
        }

        private void DestroyPreview()
        {
            if (_previewInstance != null)
            {
                DestroyImmediate(_previewInstance);
                _previewInstance = null;
            }
        }
    }
}
#endif