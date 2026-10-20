using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RockScatterWindow : EditorWindow
{
    [System.Serializable]
    public class ExclusionZone
    {
        public Vector3 center;
        public float radius = 20f;
    }

    public Terrain terrain;
    public List<GameObject> rockPrefabs = new List<GameObject>();

    public Vector3 baseCenter = Vector3.zero;
    public float safeZoneRadius = 50f;

    public int maxRockCount = 120;
    public int totalAttempts = 3000;

    [Header("Eðim Filtresi (derece)")]
    public float minSlopeAngle = 12f;
    public float maxSlopeAngle = 55f;
    [Range(0f, 1f)]
    public float flatBonusChance = 0.12f; 

    public float minSpacing = 6f;
    public float scaleMin = 0.6f;
    public float scaleMax = 1.8f;
    [Range(0f, 0.5f)]
    public float sinkDepthRatio = 0.2f; 
    public float tiltRandomness = 12f;  

    public List<ExclusionZone> exclusionZones = new List<ExclusionZone>();

    private const string ContainerName = "RockContainer";
    private Vector2 scroll;
    private SerializedObject so;

    [MenuItem("Tools/Bozulma/Rock Scatter")]
    public static void ShowWindow()
    {
        GetWindow<RockScatterWindow>("Rock Scatter");
    }

    private void OnGUI()
    {
        if (so == null) so = new SerializedObject(this);
        so.Update();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Terrain ve Kaya Prefablarý", EditorStyles.boldLabel);
        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);

        SerializedProperty prefabsProp = so.FindProperty("rockPrefabs");
        EditorGUILayout.PropertyField(prefabsProp, new GUIContent("Kaya Prefablarý (rock_set_01..04)"), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Üs ve Güvenli Bölge", EditorStyles.boldLabel);
        baseCenter = EditorGUILayout.Vector3Field("Üs Merkezi (World Pos)", baseCenter);
        safeZoneRadius = EditorGUILayout.FloatField("Güvenli Bölge Yarýçapý", safeZoneRadius);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Yoðunluk", EditorStyles.boldLabel);
        maxRockCount = EditorGUILayout.IntField("Maks. Kaya Sayýsý", maxRockCount);
        totalAttempts = EditorGUILayout.IntField("Deneme Sayýsý (iç mekanizma)", totalAttempts);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Eðim Filtresi", EditorStyles.boldLabel);
        minSlopeAngle = EditorGUILayout.FloatField("Min. Eðim (derece)", minSlopeAngle);
        maxSlopeAngle = EditorGUILayout.FloatField("Maks. Eðim (derece)", maxSlopeAngle);
        flatBonusChance = EditorGUILayout.Slider("Düz Zeminde Çýkma Þansý", flatBonusChance, 0f, 1f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Görünüm Ayarlarý", EditorStyles.boldLabel);
        minSpacing = EditorGUILayout.FloatField("Min. Kaya Arasý Mesafe", minSpacing);
        scaleMin = EditorGUILayout.FloatField("Min Ölçek", scaleMin);
        scaleMax = EditorGUILayout.FloatField("Max Ölçek", scaleMax);
        sinkDepthRatio = EditorGUILayout.Slider("Zemine Gömülme Oraný", sinkDepthRatio, 0f, 0.5f);
        tiltRandomness = EditorGUILayout.FloatField("Rastgele Eðim (derece)", tiltRandomness);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Hariç Tutulacak Bölgeler", EditorStyles.boldLabel);
        SerializedProperty exclProp = so.FindProperty("exclusionZones");
        EditorGUILayout.PropertyField(exclProp, true);

        so.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Kayalarý Oluþtur", GUILayout.Height(35)))
            GenerateRocks();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Önceki Kayalarý Temizle", GUILayout.Height(25)))
            ClearRocks();

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();
    }

    private void ClearRocks()
    {
        GameObject container = GameObject.Find(ContainerName);
        if (container != null)
            Undo.DestroyObjectImmediate(container);
    }

    private void GenerateRocks()
    {
        if (terrain == null)
        {
            EditorUtility.DisplayDialog("Hata", "Terrain atanmadý.", "Tamam");
            return;
        }
        if (rockPrefabs == null || rockPrefabs.Count == 0)
        {
            EditorUtility.DisplayDialog("Hata", "En az bir kaya prefabý ekle.", "Tamam");
            return;
        }

        ClearRocks();

        GameObject container = new GameObject(ContainerName);
        Undo.RegisterCreatedObjectUndo(container, "Create Rocks");

        List<Vector3> placed = new List<Vector3>();
        int placedCount = ScatterRocks(container.transform, placed);

        Debug.Log($"[Rock Scatter] {placedCount} kaya yerleþtirildi ({totalAttempts} denemeden).");
    }

    private int ScatterRocks(Transform parent, List<Vector3> placed)
    {
        int placedCount = 0;
        int attempts = 0;

        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 terrainPos = terrain.transform.position;

        while (placedCount < maxRockCount && attempts < totalAttempts)
        {
            attempts++;

            float worldX = terrainPos.x + Random.Range(0f, terrainSize.x);
            float worldZ = terrainPos.z + Random.Range(0f, terrainSize.z);

            float distToBase = Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(baseCenter.x, baseCenter.z));
            if (distToBase < safeZoneRadius) continue;

            bool excluded = false;
            foreach (var zone in exclusionZones)
            {
                if (Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(zone.center.x, zone.center.z)) < zone.radius)
                { excluded = true; break; }
            }
            if (excluded) continue;

            bool tooClose = false;
            foreach (var p in placed)
            {
                if (Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(p.x, p.z)) < minSpacing)
                { tooClose = true; break; }
            }
            if (tooClose) continue;

            float localX = (worldX - terrainPos.x) / terrainSize.x;
            float localZ = (worldZ - terrainPos.z) / terrainSize.z;

            if (localX < 0f || localX > 1f || localZ < 0f || localZ > 1f) continue;

            Vector3 normal = terrain.terrainData.GetInterpolatedNormal(localX, localZ);
            float slopeAngle = Vector3.Angle(normal, Vector3.up);

            bool slopeOk = slopeAngle >= minSlopeAngle && slopeAngle <= maxSlopeAngle;
            bool flatBonus = Random.value < flatBonusChance;
            if (!slopeOk && !flatBonus) continue;

            Vector3 worldPos = new Vector3(worldX, 0f, worldZ);
            float height = terrain.SampleHeight(worldPos) + terrainPos.y;
            worldPos.y = height;

            Quaternion alignToGround = Quaternion.FromToRotation(Vector3.up, normal);
            Quaternion spin = Quaternion.AngleAxis(Random.Range(0f, 360f), normal);
            Quaternion tilt = Quaternion.Euler(Random.Range(-tiltRandomness, tiltRandomness), 0f, Random.Range(-tiltRandomness, tiltRandomness));
            Quaternion finalRotation = alignToGround * spin * tilt;

            GameObject prefab = rockPrefabs[Random.Range(0, rockPrefabs.Count)];
            if (prefab == null) continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = worldPos;
            instance.transform.rotation = finalRotation;

            float scale = Random.Range(scaleMin, scaleMax);
            instance.transform.localScale = Vector3.one * scale;

            float sinkAmount = scale * sinkDepthRatio;
            instance.transform.position -= instance.transform.up * sinkAmount;

            Undo.RegisterCreatedObjectUndo(instance, "Scatter Rock");

            placed.Add(worldPos);
            placedCount++;
        }

        return placedCount;
    }
}