using EvolutionSimulator.Creatures.Core;
using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Detectors
{
    public class FoodDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField]
        private float detectionRadius = 3f;

        private Energy creatureEnergy;
        private FoodManager foodManager;

        void Awake()
        {
            creatureEnergy = GetComponentInParent<Energy>();
            if (creatureEnergy == null)
            {
                Debug.LogError("Energy component not found in parent.");
            }
        }

        void Start()
        {
            foodManager = FindFirstObjectByType<FoodManager>();
            if (foodManager == null)
            {
                Debug.LogError("FoodManager not found in the scene.");
            }
        }

        void Update()
        {
            if (
                creatureEnergy.IsAlive
                && creatureEnergy.CurrentEnergy < creatureEnergy.MaxEnergy - foodManager.FoodEnergy
            )
            {
                DetectFood();
            }
        }

        void DetectFood()
        {
            Vector2 position = transform.position;
            if (foodManager.TryConsumeFood(position, detectionRadius, out float energyGained))
            {
                ConsumeFood(energyGained);
            }
        }

        void ConsumeFood(float energyGained)
        {
            if (creatureEnergy != null)
            {
                creatureEnergy.AddEnergy(energyGained);
            }
        }

        void OnValidate()
        {
            detectionRadius = Mathf.Max(0.1f, detectionRadius);
        }
    }
}
