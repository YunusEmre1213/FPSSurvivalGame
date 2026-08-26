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

        [Header("Dusman prefabi")]
        [SerializeField] private GameObject enemyPrefab;

        [Header("Faz basina spawn sayisi")]
        [Tooltip("Oncu tip gunduz de nadiren gorulur.")]
        [SerializeField] private int daySpawnCount = 1;
        [SerializeField] private int duskSpawnCount = 2;
        [SerializeField] private int nightSpawnCount = 4;

        private readonly List<GameObject> _spawnedEnemies = new List<GameObject>();

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
            ClearSpawnedEnemies();

            int count = phase switch
            {
                DayNightPhase.Day => daySpawnCount,
                DayNightPhase.Dusk => duskSpawnCount,
                DayNightPhase.Night => nightSpawnCount,
                _ => daySpawnCount
            };

            Debug.Log($"[EnemySpawner] Faz: {phase} - {count} dusman spawn ediliyor.");

            for (int i = 0; i < count; i++)
            {
                if (spawnPoints == null || spawnPoints.Length == 0) break;
                var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var enemy = Instantiate(enemyPrefab, point.position, point.rotation);
                _spawnedEnemies.Add(enemy);
            }
        }

        private void ClearSpawnedEnemies()
        {
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            _spawnedEnemies.Clear();
        }
    }
}