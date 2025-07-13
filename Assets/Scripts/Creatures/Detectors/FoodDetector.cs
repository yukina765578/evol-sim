using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Detectors
{
    public class FoodDetector : MonoBehaviour
    {
        private float detectionRadius = 1f;

        private LayerMask foodLayerMask = -1;

        private Core.Energy creatureEnergy;
        private CircleCollider2D detector;

        void Awake()
        {
            SetupCollider();
            creatureEnergy = GetComponentInParent<Core.Energy>();

            if (creatureEnergy == null)
            {
                Debug.LogError("FoodDetector requires Energy component in parent!");
            }
        }

        void SetupCollider()
        {
            detector = GetComponent<CircleCollider2D>();
            if (detector == null)
                detector = gameObject.AddComponent<CircleCollider2D>();

            detector.isTrigger = true;
            detector.radius = detectionRadius;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!creatureEnergy.IsAlive)
                return;

            int otherLayer = 1 << other.gameObject.layer;
            if ((foodLayerMask & otherLayer) == 0)
                return;

            FoodItem foodItem = other.GetComponent<FoodItem>();
            if (foodItem != null && !foodItem.IsConsumed)
            {
                ConsumeFood(foodItem);
            }
        }

        void ConsumeFood(FoodItem foodItem)
        {
            if (foodItem.IsConsumed)
                return;

            float energyGained = foodItem.EnergyValue;
            creatureEnergy.AddEnergy(energyGained);
            foodItem.ConsumeFood(gameObject);
        }

        void OnValidate()
        {
            detectionRadius = Mathf.Max(0.1f, detectionRadius);

            if (Application.isPlaying && detector != null)
            {
                detector.radius = detectionRadius;
            }
        }
    }
}
