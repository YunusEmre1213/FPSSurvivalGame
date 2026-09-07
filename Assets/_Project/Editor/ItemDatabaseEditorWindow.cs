using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Project.Data;

namespace Project.EditorTools
{
    public class ItemDatabaseEditorWindow : EditorWindow
    {
        private ItemDatabase _database;
        private string _searchText = "";
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private ItemData _selectedItem;

        [MenuItem("Bozulma/Item Database Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<ItemDatabaseEditorWindow>("Item Database");
            window.minSize = new Vector2(520, 420);
        }

        private void OnEnable()
        {
            if (_database == null)
            {
                var guids = AssetDatabase.FindAssets("t:ItemDatabase");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("BOZULMA — ITEM DATABASE", EditorStyles.boldLabel);
            _database = (ItemDatabase)EditorGUILayout.ObjectField("Database", _database, typeof(ItemDatabase), false);

            if (_database == null || _database.allItems == null)
            {
                EditorGUILayout.HelpBox("Bir ItemDatabase asset'i bulunamadý ya da atanmadý.", MessageType.Warning);
                return;
            }

            _searchText = EditorGUILayout.TextField("Ara", _searchText);
            EditorGUILayout.Space();
            DrawCategoryCounts();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            DrawItemList();
            DrawItemDetail();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCategoryCounts()
        {
            var counts = new Dictionary<string, int>();
            foreach (var item in _database.allItems)
            {
                if (item == null) continue;
                string typeName = item.GetType().Name.Replace("Data", "");
                counts.TryGetValue(typeName, out int current);
                counts[typeName] = current + 1;
            }

            foreach (var kvp in counts.OrderBy(k => k.Key))
            {
                EditorGUILayout.LabelField(kvp.Key, kvp.Value.ToString());
            }
        }

        private void DrawItemList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(230));
            EditorGUILayout.LabelField("Eþyalar", EditorStyles.boldLabel);

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);

            var filtered = _database.allItems
                .Where(item => item != null)
                .Where(item => string.IsNullOrEmpty(_searchText)
                    || item.itemName.ToLower().Contains(_searchText.ToLower())
                    || item.itemId.ToLower().Contains(_searchText.ToLower()))
                .OrderBy(item => item.GetType().Name)
                .ThenBy(item => item.itemName);

            foreach (var item in filtered)
            {
                bool isSelected = _selectedItem == item;
                var previousColor = GUI.backgroundColor;
                if (isSelected) GUI.backgroundColor = new Color(0.5f, 0.75f, 1f);

                string label = $"[{item.GetType().Name.Replace("Data", "")}] {item.itemName}";
                if (GUILayout.Button(label))
                {
                    _selectedItem = item;
                }

                GUI.backgroundColor = previousColor;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawItemDetail()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Detay", EditorStyles.boldLabel);

            if (_selectedItem == null)
            {
                EditorGUILayout.HelpBox("Detaylarýný görmek için soldan bir eþya seç.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);

            var serializedObject = new SerializedObject(_selectedItem);
            serializedObject.Update();

            var property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                if (property.name == "m_Script")
                {
                    enterChildren = false;
                    continue;
                }
                EditorGUILayout.PropertyField(property, true);
                enterChildren = false;
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Project'te Göster"))
            {
                EditorGUIUtility.PingObject(_selectedItem);
                Selection.activeObject = _selectedItem;
            }

            EditorGUILayout.EndVertical();
        }
    }
}