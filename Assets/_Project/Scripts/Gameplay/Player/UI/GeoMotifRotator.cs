using UnityEngine;

namespace Project.UI
{
    public class GeoMotifRotator : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 2f;

        private void Update()
        {
            transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
        }
    }
}