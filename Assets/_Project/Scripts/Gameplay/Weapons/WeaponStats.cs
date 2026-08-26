namespace Project.Gameplay.Weapons
{
    public readonly struct WeaponStats
    {
        public readonly float Damage;
        public readonly float Accuracy;
        public readonly float FireRate;
        public readonly float Recoil;
        public readonly float Durability;
        public readonly int AmmoCapacity;
        public readonly float MalfunctionChance;

        public WeaponStats(float damage, float accuracy, float fireRate, float recoil,
            float durability, int ammoCapacity, float malfunctionChance)
        {
            Damage = damage;
            Accuracy = accuracy;
            FireRate = fireRate;
            Recoil = recoil;
            Durability = durability;
            AmmoCapacity = ammoCapacity;
            MalfunctionChance = malfunctionChance;
        }

        public override string ToString()
        {
            return $"Damage:{Damage:F1} Accuracy:{Accuracy:F2} FireRate:{FireRate:F1} " +
                   $"Recoil:{Recoil:F2} Durability:{Durability:F0} AmmoCap:{AmmoCapacity} " +
                   $"MalfunctionChance:{MalfunctionChance:F2}";
        }
    }
}