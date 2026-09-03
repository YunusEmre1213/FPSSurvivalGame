using Project.Core;

namespace Project.Gameplay.Weapons
{
    public readonly struct AmmoChangedEvent : IGameEvent
    {
        public readonly int CurrentAmmo;
        public readonly int MagazineCapacity;

        public AmmoChangedEvent(int currentAmmo, int magazineCapacity)
        {
            CurrentAmmo = currentAmmo;
            MagazineCapacity = magazineCapacity;
        }
    }
}