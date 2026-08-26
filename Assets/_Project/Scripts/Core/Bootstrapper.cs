using UnityEngine;
using Project.Systems;

namespace Project.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            RegisterServices();
        }

        private void RegisterServices()
        {
            ServiceLocator.Instance.Register<IInventoryService>(new InventoryService());
            
        }

        private void OnApplicationQuit()
        {
            ServiceLocator.Instance.ShutdownAll();
            EventBus.Clear();
            UIInputLock.Reset();
        }
    }
}