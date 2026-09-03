using UnityEngine;
using Project.Data;
using Project.Systems;

namespace Project.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Servis bagimliliklari")]
        [Tooltip("SaveService'in kayittaki itemId'leri gercek asset'lere cevirmesi icin gerekli.")]
        [SerializeField] private ItemDatabase itemDatabase;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            RegisterServices();
        }

        private void RegisterServices()
        {
            ServiceLocator.Instance.Register<IItemInventoryService>(new ItemInventoryService(24));
            ServiceLocator.Instance.Register<ISaveService>(new SaveService(itemDatabase));
            ServiceLocator.Instance.Register<IKeyItemService>(new KeyItemService());
        }

        private void OnApplicationQuit()
        {
            ServiceLocator.Instance.ShutdownAll();
            EventBus.Clear();
            UIInputLock.Reset();
        }
    }
}