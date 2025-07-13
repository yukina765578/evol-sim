using UnityEngine;
using UnityEngine.Events;

namespace EvolutionSimulator.Creatures.Core
{
    public class Energy : MonoBehaviour
    {
        // Energy Initial Settings
        private float maxEnergy = 100f;
        private float currentEnergy = 50f;
        private float basalConstant = 0.1f;
        private float movementConstant = 0.001f;
        private float powerExponent = 1.5f;
        private float age = 0f;
        private float maxAge = 300f; // 5 minutes
        private float reproductionThreshold = 80f;
        private bool reproductionReady = false;

        private bool isAlive = true;
        private int segmentCount = 0;

        private float lowEnergy = 0.5f;
        private float criticalEnergy = 0.2f;
        private float starvationEnergy = 0f;

        private float normalAgingRate = 1f;
        private float lowEnergyMultiplier = 1.5f;
        private float criticalEnergyMultiplier = 3f;
        private float starvationMultiplier = 5f;

        private Controller controller;

        public UnityEvent OnDeath = new UnityEvent();
        public UnityEvent OnReproductionReadyChanged = new UnityEvent();

        public bool IsAlive => isAlive;
        public bool IsReproductionReady => reproductionReady;

        void Start()
        {
            segmentCount = GetComponentsInChildren<Segment>().Length;
            controller = GetComponent<Controller>();
            currentEnergy = maxEnergy / 2f;
        }

        void Update()
        {
            if (!isAlive)
                return;

            UpdateEnergy();
            UpdateAge();
            CheckDeath();
        }

        void UpdateEnergy()
        {
            float basalEnergy = basalConstant * segmentCount;
            ConsumeEnergy(basalEnergy * Time.deltaTime);
        }

        public void ConsumeMovementEnergy(float angleChange)
        {
            float energyCost =
                Mathf.Pow(1 + angleChange, powerExponent) * movementConstant * Time.deltaTime;
            ConsumeEnergy(energyCost);
        }

        public void ConsumeEnergy(float amount)
        {
            currentEnergy = Mathf.Max(0, currentEnergy - amount);
        }

        public void AddEnergy(float amount)
        {
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        }

        void UpdateAge()
        {
            float agingMultiplier = CalculateAgingMultiplier();
            age += normalAgingRate * agingMultiplier * Time.deltaTime;
        }

        float CalculateAgingMultiplier()
        {
            float energyRatio = currentEnergy / maxEnergy;

            if (energyRatio <= starvationEnergy)
                return starvationMultiplier;
            else if (energyRatio <= criticalEnergy)
                return criticalEnergyMultiplier;
            else if (energyRatio <= lowEnergy)
                return lowEnergyMultiplier;
            else
                return 1f; // Normal aging
        }

        void CheckDeath()
        {
            if (age >= maxAge)
            {
                OnDeath.Invoke();
                isAlive = false;
                controller.HandleDeath("age");
            }
        }

        void CheckReproduction()
        {
            if (currentEnergy >= reproductionThreshold && !reproductionReady)
            {
                reproductionReady = true;
                OnReproductionReadyChanged.Invoke();
            }
            else if (currentEnergy < reproductionThreshold && reproductionReady)
            {
                reproductionReady = false;
                OnReproductionReadyChanged.Invoke();
            }
        }
    }
}
