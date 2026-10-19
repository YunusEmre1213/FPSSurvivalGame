using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ForestScatterWindow : EditorWindow
{
    [System.Serializable]
    public class ExclusionZone
    {
        public Vector3 center;
        public float radius = 20f;
    }

    public Terrain terrain;
    public List<GameObject> treePrefabs = new List<GameObject>();

    public Vector3 baseCenter = Vector3.zero;

    public float zoneInnerRadius = 60f;
    public float zoneOuterRadius = 200f;
    public int zoneTreeCount = 180;

    public float outerZoneInnerRadius = 200f;
    public float outerZoneOuterRadius = 320f;
    public int outerZoneTreeCount = 70;

    public float minSpacing = 4f;
    public float maxSlopeAngle = 45f;
    public float scaleMin = 0.85f;
    public float scaleMax = 1.15f;

    public List<ExclusionZone> exclusionZones = new List<ExclusionZone>();

    private const string ContainerName = "ForestContainer";
    private Vector2 scroll;
    private SerializedObject so;

    [MenuItem("Tools/Bozulma/Forest Scatter")]
    public static void ShowWindow()
    {
        GetWindow<ForestScatterWindow>("Forest Scatter");
    }

    private void OnGUI()
    {
        if (so == null) so = new SerializedObject(this);
        so.Update();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Terrain ve Aðaç Prefablarý", EditorStyles.boldLabel);
        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);

        SerializedProperty prefabsProp = so.FindProperty("treePrefabs");
        EditorGUILayout.PropertyField(prefabsProp, new GUIContent("Aðaç Prefablarý (Small/Medium/Tall/Bare)"), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Üs Merkezi", EditorStyles.boldLabel);
        baseCenter = EditorGUILayout.Vector3Field("Üs Merkezi (World Pos)", baseCenter);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Orta Bölge (Tepeler)", EditorStyles.boldLabel);
        zoneInnerRadius = EditorGUILayout.FloatField("Ýç Yarýçap", zoneInnerRadius);
        zoneOuterRadius = EditorGUILayout.FloatField("Dýþ Yarýçap", zoneOuterRadius);
        zoneTreeCount = EditorGUILayout.IntField("Aðaç Sayýsý", zoneTreeCount);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dýþ Bölge (Dað Eteði)", EditorStyles.boldLabel);
        outerZoneInnerRadius = EditorGUILayout.FloatField("Ýç Yarýçap", outerZoneInnerRadius);
        outerZoneOuterRadius = EditorGUILayout.FloatField("Dýþ Yarýçap", outerZoneOuterRadius);
        outerZoneTreeCount = EditorGUILayout.IntField("Aðaç Sayýsý", outerZoneTreeCount);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Genel Ayarlar", EditorStyles.boldLabel);
        minSpacing = EditorGUILayout.FloatField("Min. Aðaç Arasý Mesafe", minSpacing);
        maxSlopeAngle = EditorGUILayout.FloatField("Maks. Eðim (derece)", maxSlopeAngle);
        scaleMin = EditorGUILayout.FloatField("Min Ölçek", scaleMin);
        scaleMax = EditorGUILayout.FloatField("Max Ölçek", scaleMax);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Hariç Tutulacak Bölgeler (terk edilmiþ evler vb.)", EditorStyles.boldLabel);

        SerializedProperty exclProp = so.FindProperty("exclusionZones");
        EditorGUILayout.PropertyField(exclProp, true);

        so.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Ormaný Oluþtur", GUILayout.Height(35)))
            GenerateForest();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Önceki Ormaný Temizle", GUILayout.Height(25)))
            ClearForest();

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();
    }

    private void ClearForest()
    {
        GameObject container = GameObject.Find(ContainerName);
        if (container != null)
            Undo.DestroyObjectImmediate(container);
    }

    private void GenerateForest()
    {
        if (terrain == null)
        {
            EditorUtility.DisplayDialog("Hata", "Terrain atanmadý.", "Tamam");
            return;
        }
        if (treePrefabs == null || treePrefabs.Count == 0)
        {
            EditorUtility.DisplayDialog("Hata", "En az bir aðaç prefabý ekle.", "Tamam");
            return;
        }

        ClearForest();

        GameObject container = new GameObject(ContainerName);
        Undo.RegisterCreatedObjectUndo(container, "Create Forest");

        List<Vector3> placed = new List<Vector3>();

        int totalPlaced = 0;
        totalPlaced += ScatterInRing(container.transform, zoneInnerRadius, zoneOuterRadius, zoneTreeCount, placed);
        totalPlaced += ScatterInRing(container.transform, outerZoneInnerRadius, outerZoneOuterRadius, outerZoneTreeCount, placed);

        Debug.Log($"[Forest Scatter] {totalPlaced} aðaç yerleþtirildi.");
    }

    private int ScatterInRing(Transform parent, float innerR, float outerR, int targetCount, List<Vector3> placed)
    {
        int placedCount = 0;
        int attempts = 0;
        int maxAttempts = Mathf.Max(targetCount * 25, 500);

        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 terrainPos = terrain.transform.position;

        while (placedCount < targetCount && attempts < maxAttempts)
        {
            attempts++;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Mathf.Sqrt(Random.Range(innerR * innerR, outerR * outerR));

            Vector3 worldPos = baseCenter + new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);

            Vector3 local = worldPos - terrainPos;
            if (local.x < 0 || local.x > terrainSize.x || local.z < 0 || local.z > terrainSize.z)
                continue;

            bool excluded = false;
            foreach (var zone in exclusionZones)
            {
                Vector2 a = new Vector2(worldPos.x, worldPos.z);
                Vector2 b = new Vector2(zone.center.x, zone.center.z);
                if (Vector2.Distance(a, b) < zone.radius) { excluded = true; break; }
            }
            if (excluded) continue;

            bool tooClose = false;
            foreach (var p in placed)
            {
                Vector2 a = new Vector2(worldPos.x, worldPos.z);
                Vector2 b = new Vector2(p.x, p.z);
                if (Vector2.Distance(a, b) < minSpacing) { tooClose = true; break; }
            }
            if (tooClose) continue;

            float normX = local.x / terrainSize.x;
            float normZ = local.z / terrainSize.z;

            Vector3 normal = terrain.terrainData.GetInterpolatedNormal(normX, normZ);
            float slopeAngle = Vector3.Angle(normal, Vector3.up);
            if (slopeAngle > maxSlopeAngle) continue;

            float height = terrain.SampleHeight(worldPos) + terrainPos.y;
            worldPos.y = height;

            GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Count)];
            if (prefab == null) continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = worldPos;
            instance.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            float scale = Random.Range(scaleMin, scaleMax);
            instance.transform.localScale = Vector3.one * scale;

            Undo.RegisterCreatedObjectUndo(instance, "Scatter Tree");

            placed.Add(worldPos);
            placedCount++;
        }

        return placedCount;
    }
}