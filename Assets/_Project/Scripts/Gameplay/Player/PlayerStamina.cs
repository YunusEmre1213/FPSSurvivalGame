using UnityEngine;

namespace Project.Gameplay.Player
{
    public class PlayerStamina : MonoBehaviour
    {
        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float drainRate = 20f;
        [SerializeField] private float regenRate = 15f;
        [Tooltip("Tukendikten sonra, bu esige ulasmadan kosu tekrar baslamaz.")]
        [SerializeField] private float minStaminaToResumeSprint = 15f;

        public float CurrentStamina { get; private set; }
        public float MaxStamina => maxStamina;

        private bool _exhausted;

        private void Awake()
        {
            CurrentStamina = maxStamina;
        }
        public bool TryDrainForSprint(float deltaTime)
        {
            if (_exhausted)
            {
                if (CurrentStamina >= minStaminaToResumeSprint)
                {
                    _exhausted = false;
                }
                else
                {
                    return false;
                }
            }

            if (CurrentStamina <= 0f)
            {
                _exhausted = true;
                return false;
            }

            CurrentStamina = Mathf.Max(0f, CurrentStamina - drainRate * deltaTime);
            return true;
        }

        public void Regenerate(float deltaTime)
        {
            CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + regenRate * deltaTime);
        }
    }
}