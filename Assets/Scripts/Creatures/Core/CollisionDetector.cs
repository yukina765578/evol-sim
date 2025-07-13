using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class CollisionDetector : MonoBehaviour
    {
        // Collision detection settings
        private float detectionRadius = 1f;
        private LayerMask foodLayerMask = -1;

        private Energy energy;
        private CircleCollider2D detector;

        void Awake()
        {
            SetupCollider();
            energy = GetComponentInParent<Energy>();

            if (energy == null)
            {
                Debug.LogError("CollisionDetector requires Energy component in parent!");
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
            if (!energy.IsAlive)
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
            float energyGain = foodItem.EnergyValue;
            energy.AddEnergy(energyGain);

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
