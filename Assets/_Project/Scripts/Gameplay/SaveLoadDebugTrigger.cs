using UnityEngine;
using Project.Core;
using Project.Systems;

namespace Project.Gameplay
{
    public class SaveLoadDebugTrigger : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ServiceLocator.Instance.Get<ISaveService>().Save();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                ServiceLocator.Instance.Get<ISaveService>().Load();
            }
        }
    }
}