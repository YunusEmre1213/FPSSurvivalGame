using UnityEngine;

namespace Project.Gameplay.Weapons
{
    public class AutoFireStrategy : IFireStrategy
    {
        private float _lastFireTime = -999f;

        public bool TryFire(bool triggerHeld, bool triggerPressedThisFrame, float fireRate)
        {
            if (!triggerHeld) return false;

            float minInterval = 1f / Mathf.Max(fireRate, 0.01f);
            if (Time.time - _lastFireTime < minInterval) return false;

            _lastFireTime = Time.time;
            return true;
        }
    }
}