using UnityEngine;

namespace Project.Gameplay
{
    public class TestDummy : MonoBehaviour, IDamageable
    {
        [SerializeField] private float health = 100f;

        public void TakeDamage(float amount)
        {
            health -= amount;
            Debug.Log($"[TestDummy] {gameObject.name} {amount} hasar aldi, kalan can: {health}");
        }
    }
}