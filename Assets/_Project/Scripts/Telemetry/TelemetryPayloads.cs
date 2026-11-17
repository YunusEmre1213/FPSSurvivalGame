using System;
using UnityEngine;

namespace Project.Systems
{
    [Serializable]
    public class WoodChoppedPayload
    {
        public int woodAmount;
        public float posX, posY, posZ;

        public WoodChoppedPayload(int woodAmount, Vector3 position)
        {
            this.woodAmount = woodAmount;
            posX = position.x; posY = position.y; posZ = position.z;
        }
    }

    [Serializable]
    public class AnimalHuntedPayload
    {
        public string animalType;
        public int meatAmount;
        public float posX, posY, posZ;

        public AnimalHuntedPayload(string animalType, int meatAmount, Vector3 position)
        {
            this.animalType = animalType;
            this.meatAmount = meatAmount;
            posX = position.x; posY = position.y; posZ = position.z;
        }
    }

    [Serializable]
    public class ItemConsumedPayload
    {
        public string itemName;
        public float hungerRestore;
        public float thirstRestore;
        public bool causedSickness;

        public ItemConsumedPayload(string itemName, float hungerRestore, float thirstRestore, bool causedSickness)
        {
            this.itemName = itemName;
            this.hungerRestore = hungerRestore;
            this.thirstRestore = thirstRestore;
            this.causedSickness = causedSickness;
        }
    }

    [Serializable]
    public class PlayerDamagedPayload
    {
        public float amount;
        public float resultingHealth;

        public PlayerDamagedPayload(float amount, float resultingHealth)
        {
            this.amount = amount;
            this.resultingHealth = resultingHealth;
        }
    }

    [Serializable]
    public class PlayerDiedPayload
    {
        public float posX, posY, posZ;
        public int nightsSurvived;

        public PlayerDiedPayload(Vector3 position, int nightsSurvived)
        {
            posX = position.x; posY = position.y; posZ = position.z;
            this.nightsSurvived = nightsSurvived;
        }
    }

    [Serializable]
    public class EnemyKilledPayload
    {
        public string enemyType;
        public float posX, posY, posZ;

        public EnemyKilledPayload(string enemyType, Vector3 position)
        {
            this.enemyType = enemyType;
            posX = position.x; posY = position.y; posZ = position.z;
        }
    }

    [Serializable]
    public class EnemyWaveSpawnedPayload
    {
        public string phase;
        public int oncuCount, suruCount, eliteCount, wolfCount;
        public float difficultyMultiplier;
        public int nightsSurvived;

        public EnemyWaveSpawnedPayload(string phase, int oncuCount, int suruCount, int eliteCount, int wolfCount, float difficultyMultiplier, int nightsSurvived)
        {
            this.phase = phase;
            this.oncuCount = oncuCount;
            this.suruCount = suruCount;
            this.eliteCount = eliteCount;
            this.wolfCount = wolfCount;
            this.difficultyMultiplier = difficultyMultiplier;
            this.nightsSurvived = nightsSurvived;
        }
    }

    [Serializable]
    public class DayNightPhasePayload
    {
        public string phase;
        public int nightsSurvived;

        public DayNightPhasePayload(string phase, int nightsSurvived)
        {
            this.phase = phase;
            this.nightsSurvived = nightsSurvived;
        }
    }

    [Serializable]
    public class PlayerStateSnapshotPayload
    {
        public float health, maxHealth;
        public float hunger, thirst, stamina;
        public float posX, posZ;
        public float distanceFromBase;
        public string phase;
        public int nightsSurvived;

        public PlayerStateSnapshotPayload(float health, float maxHealth, float hunger, float thirst, float stamina,
            float posX, float posZ, float distanceFromBase, string phase, int nightsSurvived)
        {
            this.health = health; this.maxHealth = maxHealth;
            this.hunger = hunger; this.thirst = thirst; this.stamina = stamina;
            this.posX = posX; this.posZ = posZ;
            this.distanceFromBase = distanceFromBase;
            this.phase = phase;
            this.nightsSurvived = nightsSurvived;
        }
    }
}