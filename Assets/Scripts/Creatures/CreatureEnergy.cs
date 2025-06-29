using UnityEngine;
using UnityEngine.Events;

namespace EvolutionSimulator.Creature
{
    public class CreatureEnergy : MonoBehaviour
    {
        [Header("Energy Settings")]
        [SerializeField]
        private float maxEnergy = 100f;

        [SerializeField]
        private float currentEnergy = 50f;

        [SerializeField]
        private float basalConstant = 0.1f;

        [SerializeField]
        private float movementConstant = 0.001f;

        [SerializeField]
        private float powerExponent = 1.5f;

        [Header("Age Settings")]
        [SerializeField]
        private float age = 0f;

        [SerializeField]
        private float maxAge = 30f; // 5 minutes = 300 seconds

        [Header("Events")]
        public UnityEvent<float> OnEnergyChanged = new UnityEvent<float>();
        public UnityEvent OnDeath = new UnityEvent();
        public UnityEvent<float> OnFoodConsumed = new UnityEvent<float>();

        private int segmentCount = 0;

        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        public float Age => age;
        public float EnergyRatio => currentEnergy / maxEnergy;
        public bool IsAlive => age < maxAge;

        void Start()
        {
            segmentCount = GetComponentsInChildren<Segment>().Length;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        }

        void Update()
        {
            if (!IsAlive)
                return;

            age += Time.deltaTime;
            ConsumeBasalEnergy();

            CheckDeath();
        }

        void ConsumeBasalEnergy()
        {
            float basalCost = segmentCount * basalConstant * Time.deltaTime;
            ConsumeEnergy(basalCost);
        }

        public void ConsumeMovementEnergy(float angleChange)
        {
            if (!IsAlive)
                return; // Only stop consuming energy if dead from age

            float energyCost =
                Mathf.Pow(1 + angleChange, powerExponent) * movementConstant * Time.deltaTime;
            ConsumeEnergy(energyCost);
        }

        public void ConsumeEnergy(float amount)
        {
            currentEnergy = Mathf.Max(0, currentEnergy - amount);
            OnEnergyChanged?.Invoke(EnergyRatio);
        }

        public void AddEnergy(float amount)
        {
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
            OnEnergyChanged?.Invoke(EnergyRatio);
            OnFoodConsumed?.Invoke(amount);
        }

        void CheckDeath()
        {
            if (age >= maxAge)
            {
                OnDeath?.Invoke();
                enabled = false; // Stop processing
            }
        }

        void OnValidate()
        {
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            basalConstant = Mathf.Max(0, basalConstant);
            movementConstant = Mathf.Max(0, movementConstant);
            powerExponent = Mathf.Clamp(powerExponent, 1f, 5f);
        }
    }
}
