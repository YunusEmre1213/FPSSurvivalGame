namespace Project.Gameplay.Weapons
{
    public interface IFireStrategy
    {
        /// <summary>
        /// Her karede cagrilir. Bu karede gercekten ates edilmesi gerekiyorsa true doner.
        /// </summary>
        /// <param name="triggerHeld">Ates butonu o an basili mi.</param>
        /// <param name="triggerPressedThisFrame">Ates butonuna bu karede YENI basildi mi.</param>
        /// <param name="fireRate">Saniyedeki atis sayisi (WeaponStats'tan gelir).</param>
        bool TryFire(bool triggerHeld, bool triggerPressedThisFrame, float fireRate);
    }
}