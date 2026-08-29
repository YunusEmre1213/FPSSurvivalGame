using System.Collections.Generic;
using UnityEngine;
using Project.Core;
using Project.Systems;

namespace Project.Gameplay.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn noktalari")]
        [SerializeField] private Transform[] spawnPoints;

        [Header("Dusman prefableri")]
        [SerializeField] private GameObject oncuPrefab;
        [SerializeField] private GameObject suruPrefab;
        [SerializeField] private GameObject elitePrefab;

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

            Debug.Log($"[EnemySpawner] Faz: {phase} - {oncuCount} oncu, {suruCount} suru, {eliteCount} elit spawn ediliyor.");

            SpawnCount(oncuPrefab, oncuCount);
            SpawnCount(suruPrefab, suruCount);
            SpawnCount(elitePrefab, eliteCount);
        }

        private void SpawnCount(GameObject prefab, int count)
        {
            if (prefab == null) return;

            for (int i = 0; i < count; i++)
            {
                if (spawnPoints == null || spawnPoints.Length == 0) break;
                var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                SpawnFromPool(prefab, point.position, point.rotation);
            }
        }

        private void SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>();
                _pools[prefab] = pool;
            }

            GameObject instance;
            if (pool.Count > 0)
            {
                instance = pool.Dequeue();
                var enemyController = instance.GetComponent<EnemyController>();
                enemyController.ResetForPool(position, rotation);
            }
            else
            {
                instance = Instantiate(prefab, position, rotation);
                _instanceSourcePrefab[instance] = prefab;
            }

            _activeEnemies.Add(instance);
        }

        private void RecycleActiveEnemies()
        {
            foreach (var enemy in _activeEnemies)
            {
                if (enemy == null) continue;

                enemy.SetActive(false);

                if (_instanceSourcePrefab.TryGetValue(enemy, out var sourcePrefab))
                {
                    _pools[sourcePrefab].Enqueue(enemy);
                }
            }
            _activeEnemies.Clear();
        }
    }
}