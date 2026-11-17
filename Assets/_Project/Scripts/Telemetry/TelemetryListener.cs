using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using Project.Core;
using Project.Gameplay.Player;

namespace Project.Systems
{
    public class TelemetryListener : MonoBehaviour
    {
        [Header("Referanslar")]
        [Tooltip("Oyuncunun Transform'u - bos birakilirsa 'Player' tag'li obje otomatik aranir.")]
        [SerializeField] private Transform player;
        [Tooltip("Us/kamp konumu - ELLE ATAMAN GEREKIYOR, oyuncunun ussten uzakligini hesaplamak icin kullanilir.")]
        [SerializeField] private Transform baseTransform;
        [Tooltip("Bos birakilirsa oyuncu objesinden otomatik GetComponent ile alinir.")]
        [SerializeField] private PlayerNeeds playerNeeds;
        [SerializeField] private PlayerStamina playerStamina;

        [Header("Periyodik Anlik Goruntu")]
        [SerializeField] private float snapshotInterval = 60f;

        [Header("Dosya")]
        [SerializeField] private string outputFolderName = "Telemetry";

        private string _sessionId;
        private float _lastKnownHealth;
        private float _lastKnownMaxHealth;
        private float _snapshotTimer;
        private StreamWriter _writer;

        private void Awake()
        {
            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    if (playerNeeds == null) playerNeeds = playerObj.GetComponent<PlayerNeeds>();
                    if (playerStamina == null) playerStamina = playerObj.GetComponent<PlayerStamina>();
                }
            }

            _sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);

            string folderPath = Path.Combine(Application.persistentDataPath, outputFolderName);
            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, $"session_{_sessionId}.csv");
            _writer = new StreamWriter(filePath, append: true);
            _writer.WriteLine("timestamp_unix,game_time,session_id,event_type,data_json");
            _writer.Flush();

            Debug.Log($"[TelemetryListener] Oturum basladi: {_sessionId} -> {filePath}");
            LogEvent("SessionStarted", "{}");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DayNightPhaseChangedEvent>(OnDayNightPhaseChanged);
            EventBus.Subscribe<TreeChoppedEvent>(OnTreeChopped);
            EventBus.Subscribe<AnimalDiedEvent>(OnAnimalHunted);
            EventBus.Subscribe<ItemConsumedEvent>(OnItemConsumed);
            EventBus.Subscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<EnemyWaveSpawnedEvent>(OnEnemyWaveSpawned);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DayNightPhaseChangedEvent>(OnDayNightPhaseChanged);
            EventBus.Unsubscribe<TreeChoppedEvent>(OnTreeChopped);
            EventBus.Unsubscribe<AnimalDiedEvent>(OnAnimalHunted);
            EventBus.Unsubscribe<ItemConsumedEvent>(OnItemConsumed);
            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<EnemyWaveSpawnedEvent>(OnEnemyWaveSpawned);
        }

        private void Update()
        {
            _snapshotTimer += Time.deltaTime;
            if (_snapshotTimer >= snapshotInterval)
            {
                _snapshotTimer = 0f;
                TakeSnapshot();
            }
        }

        private void OnApplicationQuit()
        {
            LogEvent("SessionEnded", "{}");
            _writer?.Flush();
            _writer?.Close();
        }

        private void OnDayNightPhaseChanged(DayNightPhaseChangedEvent evt)
        {
            var dayNightService = ServiceLocator.Instance.Get<IDayNightService>();
            var payload = new DayNightPhasePayload(evt.NewPhase.ToString(), dayNightService.NightsSurvived);
            LogEvent("DayNightPhaseChanged", JsonUtility.ToJson(payload));
        }

        private void OnTreeChopped(TreeChoppedEvent evt)
        {
            LogEvent("WoodChopped", JsonUtility.ToJson(new WoodChoppedPayload(evt.WoodAmount, evt.Position)));
        }

        private void OnAnimalHunted(AnimalDiedEvent evt)
        {
            LogEvent("AnimalHunted", JsonUtility.ToJson(new AnimalHuntedPayload(evt.AnimalType, evt.MeatAmount, evt.Position)));
        }

        private void OnItemConsumed(ItemConsumedEvent evt)
        {
            LogEvent("ItemConsumed", JsonUtility.ToJson(new ItemConsumedPayload(evt.ItemName, evt.HungerRestore, evt.ThirstRestore, evt.CausedSickness)));
        }

        private void OnPlayerHealthChanged(PlayerHealthChangedEvent evt)
        {
            _lastKnownHealth = evt.CurrentHealth;
            _lastKnownMaxHealth = evt.MaxHealth;
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            LogEvent("PlayerDamaged", JsonUtility.ToJson(new PlayerDamagedPayload(evt.Amount, evt.ResultingHealth)));
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            var dayNightService = ServiceLocator.Instance.Get<IDayNightService>();
            Vector3 pos = player != null ? player.position : Vector3.zero;
            LogEvent("PlayerDied", JsonUtility.ToJson(new PlayerDiedPayload(pos, dayNightService.NightsSurvived)));
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            LogEvent("EnemyKilled", JsonUtility.ToJson(new EnemyKilledPayload(evt.EnemyType, evt.Position)));
        }

        private void OnEnemyWaveSpawned(EnemyWaveSpawnedEvent evt)
        {
            var payload = new EnemyWaveSpawnedPayload(evt.Phase.ToString(), evt.OncuCount, evt.SuruCount, evt.EliteCount, evt.WolfCount, evt.DifficultyMultiplier, evt.NightsSurvived);
            LogEvent("EnemySpawned", JsonUtility.ToJson(payload));
        }

        private void TakeSnapshot()
        {
            float distanceFromBase = (player != null && baseTransform != null)
                ? Vector3.Distance(player.position, baseTransform.position)
                : -1f;

            var dayNightService = ServiceLocator.Instance.Get<IDayNightService>();

            var payload = new PlayerStateSnapshotPayload(
                _lastKnownHealth, _lastKnownMaxHealth,
                playerNeeds != null ? playerNeeds.Hunger : -1f,
                playerNeeds != null ? playerNeeds.Thirst : -1f,
                playerStamina != null ? playerStamina.CurrentStamina : -1f,
                player != null ? player.position.x : 0f,
                player != null ? player.position.z : 0f,
                distanceFromBase,
                dayNightService.CurrentPhase.ToString(),
                dayNightService.NightsSurvived
            );

            LogEvent("PlayerStateSnapshot", JsonUtility.ToJson(payload));
        }

        private void LogEvent(string eventType, string dataJson)
        {
            string safeJson = dataJson.Replace("\"", "\"\"");
            string line = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},{Time.time.ToString(CultureInfo.InvariantCulture)},{_sessionId},{eventType},\"{safeJson}\"";
            _writer.WriteLine(line);
            _writer.Flush();
        }
    }
}