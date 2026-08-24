using UnityEngine;

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
        }

        private void OnApplicationQuit()
        {
            ServiceLocator.Instance.ShutdownAll();
            EventBus.Clear();
        }
    }
}