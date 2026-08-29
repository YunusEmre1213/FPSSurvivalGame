using UnityEngine;
using Project.Data;
using Project.Systems;

namespace Project.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Servis bagimliliklari")]
        [Tooltip("SaveService'in kayittaki partId'leri gercek asset'lere cevirmesi icin gerekli.")]
        [SerializeField] private WeaponPartDatabase weaponPartDatabase;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            RegisterServices();
        }

        private void RegisterServices()
        {
            ServiceLocator.Instance.Register<IInventoryService>(new InventoryService());
            ServiceLocator.Instance.Register<ISaveService>(new SaveService(weaponPartDatabase));
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