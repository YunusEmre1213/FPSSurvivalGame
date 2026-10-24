using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Project.Core;
using Project.Systems;

namespace Project.Gameplay.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn noktalari")]
        [SerializeField] private Transform[] spawnPoints;
        [Tooltip("Bos birakilirsa kurtlar da yukaridaki genel listeyi kullanir. Doldurulursa kurtlar SADECE buradaki noktalarda (orn. orman kenari) spawn olur - her kurt bu nokta etrafinda rastgele dagilir.")]
        [SerializeField] private Transform[] wolfSpawnPoints;
        [Tooltip("Kurtlarin spawn noktasi etrafinda ne kadar yayilacagi (metre). 0 = tam nokta ustunde ust uste dogar.")]
        [SerializeField] private float wolfSpawnScatterRadius = 6f;

        [Header("Dusman prefableri")]
        [SerializeField] private GameObject oncuPrefab;
        [SerializeField] private GameObject suruPrefab;
        [SerializeField] private GameObject elitePrefab;
        [SerializeField] private GameObject wolfPrefab;

        [Header("Faz basina Oncu spawn sayisi")]
        [SerializeField] private int oncuDayCount = 1;
        [SerializeField] private int oncuDuskCount = 2;
        [SerializeField] private int oncuNightCount = 1;

        [Header("Faz basina Suru spawn sayisi")]
        [SerializeField] private int suruDayCount = 0;
        [SerializeField] private int suruDuskCount = 0;
        [SerializeField] private int suruNightCount = 4;

        [Header("Elit spawn (sadece gece, sans bazli)")]
        [Range(0f, 1f)]
        [SerializeField] private float eliteNightSpawnChance = 0.3f;
        [SerializeField] private int eliteCountWhenSpawned = 1;

        [Header("Faz basina Kurt spawn sayisi (gece yogun/sabit, gunduz-alacakaranlik rastgele az sayida)")]
        [Tooltip("Kurtlar gercekte alacakaranlikta/gece aktif - gunduz/alacakaranlikta bazen 0, bazen 1-2 kalabilir.")]
        [SerializeField] private int wolfDayMinCount = 0;
        [SerializeField] private int wolfDayMaxCount = 2;
        [SerializeField] private int wolfDuskMinCount = 0;
        [SerializeField] private int wolfDuskMaxCount = 2;
        [Tooltip("Gece sabit, kalabalik suru boyutu (arastirmaya gore orta boy av icin ~4 civari).")]
        [SerializeField] private int wolfNightCount = 4;

        [Header("Zorluk olcekleme")]
        [Tooltip("Her tamamlanan gece basina stat artis orani. 0.12 = gece basina %12 daha guclu.")]
        [SerializeField] private float difficultyScalePerNight = 0.12f;

        [Header("Performans testi")]
        [Tooltip("KAPATIRSAN pooling devre disi kalir - her spawn'da yeni Instantiate, her recycle'da Destroy. Sadece performans karsilastirmasi icin - normal oyunda ACIK kalmali.")]
        [SerializeField] private bool usePooling = true;

        [Header("Test / Debug")]
        [Tooltip("Acik oldugunda, asagidaki tusa basildiginda oyuncunun hemen onune garanti bir kurt doðar - saldiri/kacma davranisini test etmek icin. Release build'de kapat.")]
        [SerializeField] private bool enableTestSpawn = true;
        [SerializeField] private KeyCode testSpawnKey = KeyCode.F9;
        [SerializeField] private float testSpawnDistanceFromPlayer = 5f;

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly List<GameObject> _activeEnemies = new List<GameObject>();
        private readonly Dictionary<GameObject, GameObject> _instanceSourcePrefab = new Dictionary<GameObject, GameObject>();

        private void OnEnable()
        {
            EventBus.Subscribe<DayNightPhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DayNightPhaseChangedEvent>(OnPhaseChanged);
        }

        private void Start()
        {
            var dayNightService = ServiceLocator.Instance.Get<IDayNightService>();
            SpawnForPhase(dayNightService.CurrentPhase);
        }

        private void Update()
        {
            if (enableTestSpawn && Input.GetKeyDown(testSpawnKey))
            {
                SpawnTestWolf();
            }
        }

        private void OnPhaseChanged(DayNightPhaseChangedEvent evt)
        {
            SpawnForPhase(evt.NewPhase);
        }

        private void SpawnForPhase(DayNightPhase phase)
        {
            RecycleActiveEnemies();

            var (oncuCount, suruCount) = phase switch
            {
                DayNightPhase.Day => (oncuDayCount, suruDayCount),
                DayNightPhase.Dusk => (oncuDuskCount, suruDuskCount),
                DayNightPhase.Night => (oncuNightCount, suruNightCount),
                _ => (oncuDayCount, suruDayCount)
            };

            int eliteCount = 0;
            if (phase == DayNightPhase.Night && Random.value < eliteNightSpawnChance)
            {
                eliteCount = eliteCountWhenSpawned;
            }

            int wolfCount = phase switch
            {
                DayNightPhase.Day => Random.Range(wolfDayMinCount, wolfDayMaxCount + 1),
                DayNightPhase.Dusk => Random.Range(wolfDuskMinCount, wolfDuskMaxCount + 1),
                DayNightPhase.Night => wolfNightCount,
                _ => Random.Range(wolfDayMinCount, wolfDayMaxCount + 1)
            };

            var dayNightService = ServiceLocator.Instance.Get<IDayNightService>();
            float difficultyMultiplier = 1f + (dayNightService.NightsSurvived * difficultyScalePerNight);

            Debug.Log($"[EnemySpawner] Faz: {phase} - {oncuCount} oncu, {suruCount} suru, {eliteCount} elit, {wolfCount} kurt spawn ediliyor. " +
                      $"(Zorluk carpani: {difficultyMultiplier:F2}x, {dayNightService.NightsSurvived}. gece tamamlandi)");

            SpawnCount(oncuPrefab, oncuCount, difficultyMultiplier, spawnPoints);
            SpawnCount(suruPrefab, suruCount, difficultyMultiplier, spawnPoints);
            SpawnCount(elitePrefab, eliteCount, difficultyMultiplier, spawnPoints);
            SpawnCount(wolfPrefab, wolfCount, difficultyMultiplier, wolfSpawnPoints, wolfSpawnScatterRadius);
        }

        private void SpawnTestWolf()
        {
            if (wolfPrefab == null)
            {
                Debug.LogWarning("[EnemySpawner] Test spawn: Wolf prefab atanmamis.");
                return;
            }

            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogWarning("[EnemySpawner] Test spawn: sahnede 'Player' tag'li obje bulunamadi.");
                return;
            }

            Vector3 desiredPos = playerObj.transform.position + playerObj.transform.forward * testSpawnDistanceFromPlayer;

            if (NavMesh.SamplePosition(desiredPos, out var hit, testSpawnDistanceFromPlayer + 5f, NavMesh.AllAreas))
            {
                desiredPos = hit.position;
            }

            // Test spawn'da difficulty scaling yok (1x) - base stat'larla temiz test icin.
            SpawnFromPool(wolfPrefab, desiredPos, Quaternion.identity, 1f);
            Debug.Log($"[EnemySpawner] TEST: Kurt oyuncunun {testSpawnDistanceFromPlayer}m onune spawn edildi (F9).");
        }

        private void SpawnCount(GameObject prefab, int count, float difficultyMultiplier, Transform[] points, float scatterRadius = 0f)
        {
            if (prefab == null) return;

            var usePoints = (points != null && points.Length > 0) ? points : spawnPoints;
            if (usePoints == null || usePoints.Length == 0) return;

            for (int i = 0; i < count; i++)
            {
                var point = usePoints[Random.Range(0, usePoints.Length)];
                Vector3 spawnPos = scatterRadius > 0f ? GetScatteredPosition(point.position, scatterRadius) : point.position;
                SpawnFromPool(prefab, spawnPos, point.rotation, difficultyMultiplier);
            }
        }

        private Vector3 GetScatteredPosition(Vector3 center, float radius)
        {
            Vector2 offset2D = Random.insideUnitCircle * radius;
            Vector3 candidate = center + new Vector3(offset2D.x, 0f, offset2D.y);

            if (NavMesh.SamplePosition(candidate, out var hit, radius + 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return center;
        }

        private void SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation, float difficultyMultiplier)
        {
            GameObject instance;
            EnemyController enemyController;

            if (usePooling)
            {
                if (!_pools.TryGetValue(prefab, out var pool))
                {
                    pool = new Queue<GameObject>();
                    _pools[prefab] = pool;
                }

                if (pool.Count > 0)
                {
                    instance = pool.Dequeue();
                    enemyController = instance.GetComponent<EnemyController>();
                    enemyController.ResetForPool(position, rotation);
                }
                else
                {
                    instance = Instantiate(prefab, position, rotation);
                    _instanceSourcePrefab[instance] = prefab;
                    enemyController = instance.GetComponent<EnemyController>();
                }
            }
            else
            {
                instance = Instantiate(prefab, position, rotation);
                enemyController = instance.GetComponent<EnemyController>();
            }

            if (enemyController != null)
            {
                enemyController.ApplyDifficultyScale(difficultyMultiplier);
            }

            _activeEnemies.Add(instance);
        }

        private void RecycleActiveEnemies()
        {
            foreach (var enemy in _activeEnemies)
            {
                if (enemy == null) continue;

                if (usePooling)
                {
                    enemy.SetActive(false);
                    if (_instanceSourcePrefab.TryGetValue(enemy, out var sourcePrefab))
                    {
                        _pools[sourcePrefab].Enqueue(enemy);
                    }
                }
                else
                {
                    Destroy(enemy);
                }
            }
            _activeEnemies.Clear();
        }
    }
}