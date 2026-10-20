using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DeerScatterWindow : EditorWindow
{
    [System.Serializable]
    public class ExclusionZone
    {
        public Vector3 center;
        public float radius = 20f;
    }

    public Terrain terrain;
    public GameObject deerPrefab;

    public Vector3 baseCenter = Vector3.zero;
    public float safeZoneRadius = 70f;
    public float maxDistanceFromBase = 280f;

    [Header("Sürü Ayarlarý")]
    public int herdCount = 4;
    public int deerPerHerdMin = 2;
    public int deerPerHerdMax = 3;
    public float herdSpreadRadius = 18f;
    public float minHerdSpacing = 50f;

    [Header("Arazi Filtresi")]
    public float maxSlopeAngle = 22f;
    public float minDeerSpacing = 4f;

    public List<ExclusionZone> exclusionZones = new List<ExclusionZone>();

    private const string ContainerName = "DeerContainer";
    private Vector2 scroll;
    private SerializedObject so;

    [MenuItem("Tools/Bozulma/Deer Scatter")]
    public static void ShowWindow()
    {
        GetWindow<DeerScatterWindow>("Deer Scatter");
    }

    private void OnGUI()
    {
        if (so == null) so = new SerializedObject(this);
        so.Update();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Terrain ve Prefab", EditorStyles.boldLabel);
        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);
        deerPrefab = (GameObject)EditorGUILayout.ObjectField("Deer Prefab", deerPrefab, typeof(GameObject), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Üs ve Güvenli Bölge", EditorStyles.boldLabel);
        baseCenter = EditorGUILayout.Vector3Field("Üs Merkezi (World Pos)", baseCenter);
        safeZoneRadius = EditorGUILayout.FloatField("Güvenli Bölge Yarýçapý", safeZoneRadius);
        maxDistanceFromBase = EditorGUILayout.FloatField("Üsten Maks. Mesafe (oyun alaný sýnýrý)", maxDistanceFromBase);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sürü Ayarlarý", EditorStyles.boldLabel);
        herdCount = EditorGUILayout.IntField("Sürü Sayýsý", herdCount);
        deerPerHerdMin = EditorGUILayout.IntField("Sürü Baþýna Min. Geyik", deerPerHerdMin);
        deerPerHerdMax = EditorGUILayout.IntField("Sürü Baþýna Maks. Geyik", deerPerHerdMax);
        herdSpreadRadius = EditorGUILayout.FloatField("Sürü Yayýlma Yarýçapý", herdSpreadRadius);
        minHerdSpacing = EditorGUILayout.FloatField("Sürüler Arasý Min. Mesafe", minHerdSpacing);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Arazi Filtresi", EditorStyles.boldLabel);
        maxSlopeAngle = EditorGUILayout.FloatField("Maks. Eðim (derece)", maxSlopeAngle);
        minDeerSpacing = EditorGUILayout.FloatField("Geyikler Arasý Min. Mesafe", minDeerSpacing);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Hariç Tutulacak Bölgeler (su, ev vb.)", EditorStyles.boldLabel);
        SerializedProperty exclProp = so.FindProperty("exclusionZones");
        EditorGUILayout.PropertyField(exclProp, true);

        so.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Geyikleri Oluþtur", GUILayout.Height(35)))
            GenerateDeer();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Önceki Geyikleri Temizle", GUILayout.Height(25)))
            ClearDeer();

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();
    }

    private void ClearDeer()
    {
        GameObject container = GameObject.Find(ContainerName);
        if (container != null)
            Undo.DestroyObjectImmediate(container);
    }

    private bool IsExcluded(float worldX, float worldZ)
    {
        float distToBase = Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(baseCenter.x, baseCenter.z));
        if (distToBase < safeZoneRadius) return true;
        if (distToBase > maxDistanceFromBase) return true;

        foreach (var zone in exclusionZones)
        {
            if (Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(zone.center.x, zone.center.z)) < zone.radius)
                return true;
        }
        return false;
    }

    private bool TryGetSlope(float worldX, float worldZ, out float slopeAngle, out Vector3 worldPos)
    {
        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 terrainPos = terrain.transform.position;

        float localX = (worldX - terrainPos.x) / terrainSize.x;
        float localZ = (worldZ - terrainPos.z) / terrainSize.z;

        worldPos = new Vector3(worldX, 0f, worldZ);
        slopeAngle = 999f;

        if (localX < 0f || localX > 1f || localZ < 0f || localZ > 1f) return false;

        Vector3 normal = terrain.terrainData.GetInterpolatedNormal(localX, localZ);
        slopeAngle = Vector3.Angle(normal, Vector3.up);

        float height = terrain.SampleHeight(worldPos) + terrainPos.y;
        worldPos.y = height;

        return true;
    }

    // Önce Character Controller'ýn tanýmladýðý gerçek "ayak tabaný"ný kullan
    // (en güvenilir kaynak bu, çünkü prefab'ýn kendi fizik ayarý). Yoksa mesh
    // sýnýrlarýna geri düþ.
    private float GetGroundOffset(GameObject obj)
    {
        CharacterController cc = obj.GetComponentInChildren<CharacterController>();
        if (cc != null)
        {
            float scaleY = obj.transform.lossyScale.y;
            float bottomLocal = cc.center.y - (cc.height * 0.5f);
            return -bottomLocal * scaleY;
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return 0f;

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        return obj.transform.position.y - combined.min.y;
    }

    private void GenerateDeer()
    {
        if (terrain == null)
        {
            EditorUtility.DisplayDialog("Hata", "Terrain atanmadý.", "Tamam");
            return;
        }
        if (deerPrefab == null)
        {
            EditorUtility.DisplayDialog("Hata", "Deer prefabý atanmadý.", "Tamam");
            return;
        }

        ClearDeer();

        GameObject container = new GameObject(ContainerName);
        Undo.RegisterCreatedObjectUndo(container, "Create Deer");

        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 terrainPos = terrain.transform.position;

        List<Vector3> herdCenters = new List<Vector3>();
        List<Vector3> placedDeer = new List<Vector3>();

        int herdAttempts = 0;
        int maxHerdAttempts = herdCount * 200;

        while (herdCenters.Count < herdCount && herdAttempts < maxHerdAttempts)
        {
            herdAttempts++;

            float worldX = terrainPos.x + Random.Range(0f, terrainSize.x);
            float worldZ = terrainPos.z + Random.Range(0f, terrainSize.z);

            if (IsExcluded(worldX, worldZ)) continue;

            if (!TryGetSlope(worldX, worldZ, out float slopeAngle, out Vector3 worldPos)) continue;
            if (slopeAngle > maxSlopeAngle) continue;

            bool tooClose = false;
            foreach (var h in herdCenters)
            {
                if (Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(h.x, h.z)) < minHerdSpacing)
                { tooClose = true; break; }
            }
            if (tooClose) continue;

            herdCenters.Add(worldPos);
        }

        if (herdCenters.Count < herdCount)
        {
            Debug.LogWarning($"[Deer Scatter] Sadece {herdCenters.Count}/{herdCount} sürü merkezi bulunabildi.");
        }

        foreach (var center in herdCenters)
        {
            int deerCount = Random.Range(deerPerHerdMin, deerPerHerdMax + 1);
            int placedInHerd = 0;
            int individualAttempts = 0;
            int maxIndividualAttempts = deerCount * 40;

            while (placedInHerd < deerCount && individualAttempts < maxIndividualAttempts)
            {
                individualAttempts++;

                Vector2 offset = Random.insideUnitCircle * herdSpreadRadius;
                float worldX = center.x + offset.x;
                float worldZ = center.z + offset.y;

                if (IsExcluded(worldX, worldZ)) continue;
                if (!TryGetSlope(worldX, worldZ, out float slopeAngle, out Vector3 worldPos)) continue;
                if (slopeAngle > maxSlopeAngle) continue;

                bool tooClose = false;
                foreach (var d in placedDeer)
                {
                    if (Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(d.x, d.z)) < minDeerSpacing)
                    { tooClose = true; break; }
                }
                if (tooClose) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(deerPrefab, container.transform);
                instance.transform.position = worldPos;
                instance.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                float groundOffset = GetGroundOffset(instance);
                Vector3 correctedPos = instance.transform.position;
                correctedPos.y += groundOffset;
                instance.transform.position = correctedPos;

                Undo.RegisterCreatedObjectUndo(instance, "Scatter Deer");

                placedDeer.Add(worldPos);
                placedInHerd++;
            }
        }

        Debug.Log($"[Deer Scatter] {herdCenters.Count} sürü, toplam {placedDeer.Count} geyik yerleþtirildi.");
    }
}